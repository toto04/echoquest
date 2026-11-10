// QMI8658(A) driver: register-level, polled, on the shared I2C bus.
// Register values follow Waveshare's ESP32-C6-Touch-LCD-1.47 factory firmware
// and the QMI8658A datasheet.
#include <Arduino.h>

#include "config.h"
#include "i2c_bus.h"
#include "imu.h"
#include "log.h"

namespace {

constexpr uint8_t REG_WHO_AM_I = 0x00;   // reads 0x05
constexpr uint8_t REG_CTRL1 = 0x02;      // serial interface
constexpr uint8_t REG_CTRL2 = 0x03;      // accel full scale + ODR
constexpr uint8_t REG_CTRL3 = 0x04;      // gyro full scale + ODR
constexpr uint8_t REG_CTRL7 = 0x08;      // sensor enable
constexpr uint8_t REG_STATUS0 = 0x2E;    // bit0 accel ready, bit1 gyro ready
constexpr uint8_t REG_AX_L = 0x35;       // AX..AZ, GX..GZ: 12 bytes little-endian
constexpr uint8_t REG_RESET = 0x60;

constexpr uint8_t WHO_AM_I_VALUE = 0x05;
constexpr uint8_t CTRL1_ADDR_AUTO_INCREMENT = 0x40;   // little-endian, auto-increment
constexpr uint8_t CTRL2_ACCEL_4G_224HZ = 0x15;        // aFS = +-4 g, ODR ~224 Hz
constexpr uint8_t CTRL3_GYRO_512DPS_224HZ = 0x55;     // gFS = +-512 dps, ODR ~224 Hz
constexpr uint8_t CTRL7_ACCEL_GYRO_ON = 0x03;
constexpr uint8_t RESET_COMMAND = 0xB0;

constexpr float ACCEL_LSB_PER_G = 8192.0f;   // +-4 g
constexpr float GYRO_LSB_PER_DPS = 64.0f;    // +-512 dps

constexpr uint32_t MAX_DT_US = 50000;   // ignore longer gaps (e.g. after a stall)
// The gyro output settles after power-up; samples in this window are discarded.
constexpr uint32_t GYRO_SETTLE_MS = 300;

}  // namespace

bool Qmi8658Imu::readSample(float accel[3], float gyroDps[3]) {
    uint8_t raw[12];
    if (!i2c_bus::readRegs(QMI8658_I2C_ADDR, REG_AX_L, raw, sizeof(raw))) return false;
    float sensorAccel[3], sensorGyro[3];
    for (int i = 0; i < 3; i++) {
        sensorAccel[i] = (int16_t)(raw[i * 2] | (raw[i * 2 + 1] << 8)) / ACCEL_LSB_PER_G;
        sensorGyro[i] = (int16_t)(raw[6 + i * 2] | (raw[7 + i * 2] << 8)) / GYRO_LSB_PER_DPS;
    }
    orientation::remapAxes(IMU_AXIS_MAP, sensorAccel, accel);
    orientation::remapAxes(IMU_AXIS_MAP, sensorGyro, gyroDps);
    return true;
}

bool Qmi8658Imu::begin() {
    i2c_bus::begin();
    filter_ = orientation::MahonyFilter(IMU_FILTER_KP);

    uint8_t id = 0;
    if (!i2c_bus::readRegs(QMI8658_I2C_ADDR, REG_WHO_AM_I, &id, 1)) {
        LOG("imu", "QMI8658 not responding at 0x%02X", QMI8658_I2C_ADDR);
        return false;
    }
    if (id != WHO_AM_I_VALUE) {
        LOG("imu", "unexpected WHO_AM_I 0x%02X (want 0x%02X)", id, WHO_AM_I_VALUE);
        return false;
    }
    i2c_bus::writeReg(QMI8658_I2C_ADDR, REG_RESET, RESET_COMMAND);
    delay(20);
    bool ok = i2c_bus::writeReg(QMI8658_I2C_ADDR, REG_CTRL1, CTRL1_ADDR_AUTO_INCREMENT) &&
              i2c_bus::writeReg(QMI8658_I2C_ADDR, REG_CTRL2, CTRL2_ACCEL_4G_224HZ) &&
              i2c_bus::writeReg(QMI8658_I2C_ADDR, REG_CTRL3, CTRL3_GYRO_512DPS_224HZ) &&
              i2c_bus::writeReg(QMI8658_I2C_ADDR, REG_CTRL7, CTRL7_ACCEL_GYRO_ON);
    if (!ok) {
        LOG("imu", "QMI8658 configuration write failed");
        return false;
    }
    // Discard samples while the gyro settles after power-up.
    float a[3], g[3];
    for (uint32_t t0 = millis(); millis() - t0 < GYRO_SETTLE_MS;) waitSample(a, g, 20);

    // Gyro bias: average a window of samples while the board is still. Retry a
    // few windows if it moved; fall back to the stillest window. Any residual
    // error is removed later by the bias tracker whenever the board rests.
    float bestBias[3] = {0, 0, 0}, bestAccel[3] = {0, 0, 1};
    float bestSpread = 1e9f;
    for (uint8_t attempt = 0; attempt < IMU_CALIBRATION_ATTEMPTS; attempt++) {
        float sumG[3] = {0, 0, 0}, sumA[3] = {0, 0, 0};
        float minG[3] = {1e9f, 1e9f, 1e9f}, maxG[3] = {-1e9f, -1e9f, -1e9f};
        uint16_t n = 0;
        for (uint16_t tries = 0; n < IMU_CALIBRATION_SAMPLES && tries < IMU_CALIBRATION_SAMPLES * 2; tries++) {
            if (!waitSample(a, g, 20)) continue;
            for (int i = 0; i < 3; i++) {
                sumA[i] += a[i];
                sumG[i] += g[i];
                if (g[i] < minG[i]) minG[i] = g[i];
                if (g[i] > maxG[i]) maxG[i] = g[i];
            }
            n++;
        }
        if (n < IMU_CALIBRATION_SAMPLES / 2) {
            LOG("imu", "QMI8658 produced only %u samples during calibration", n);
            return false;
        }
        float spread = 0;
        for (int i = 0; i < 3; i++)
            if (maxG[i] - minG[i] > spread) spread = maxG[i] - minG[i];
        if (spread < bestSpread) {
            bestSpread = spread;
            for (int i = 0; i < 3; i++) {
                bestBias[i] = sumG[i] / n;
                bestAccel[i] = sumA[i] / n;
            }
        }
        LOG("imu", "calibration %u: bias %.2f %.2f %.2f dps (spread %.1f dps)%s", attempt + 1, sumG[0] / n,
            sumG[1] / n, sumG[2] / n, spread, spread <= IMU_CALIBRATION_MAX_SPREAD_DPS ? "" : ", moving: retrying");
        if (spread <= IMU_CALIBRATION_MAX_SPREAD_DPS) break;
    }
    if (bestSpread > IMU_CALIBRATION_MAX_SPREAD_DPS) {
        LOG("imu", "WARNING: board never still during calibration; bias will be refined once it rests");
    }
    biasTracker_.reset(bestBias);
    filter_.initFromAccel(bestAccel[0], bestAccel[1], bestAccel[2]);
    lastSampleUs_ = micros();
    ok_ = true;
    return true;
}

bool Qmi8658Imu::waitSample(float accel[3], float gyroDps[3], uint32_t timeoutMs) {
    for (uint32_t t0 = millis(); millis() - t0 < timeoutMs;) {
        uint8_t status = 0;
        if (!i2c_bus::readRegs(QMI8658_I2C_ADDR, REG_STATUS0, &status, 1)) return false;
        if (status & 0x02) return readSample(accel, gyroDps);
        delayMicroseconds(500);
    }
    return false;
}

bool Qmi8658Imu::read(uint32_t, Orientation& out) {
    if (!ok_) return false;
    uint8_t status = 0;
    if (!i2c_bus::readRegs(QMI8658_I2C_ADDR, REG_STATUS0, &status, 1)) {
        if (errorCount_++ % 1000 == 0) LOG("imu", "I2C read error (%lu so far)", (unsigned long)errorCount_);
        return false;
    }
    if (!(status & 0x02)) return false;   // no new gyro sample yet

    float a[3], g[3];
    if (!readSample(a, g)) return false;

    uint32_t nowUs = micros();
    uint32_t dtUs = nowUs - lastSampleUs_;
    lastSampleUs_ = nowUs;
    if (dtUs > MAX_DT_US) dtUs = MAX_DT_US;

    const float dt = dtUs * 1e-6f;
    // At rest: learn the bias and don't integrate the gyro, so yaw can't creep.
    // The accelerometer still corrects pitch/roll.
    const bool still = biasTracker_.update(g, a, dt);
    const float* bias = biasTracker_.bias();
    float rate[3];
    for (int i = 0; i < 3; i++) rate[i] = still ? 0.0f : (g[i] - bias[i]) * orientation::DEG_TO_RAD_F;
    filter_.update(rate[0], rate[1], rate[2], a[0], a[1], a[2], dt);

    orientation::Euler e = filter_.euler();
    out.yawDeg = e.yawDeg;
    out.pitchDeg = e.pitchDeg;
    out.rollDeg = e.rollDeg;
    out.still = still;

#if ECHO_DEBUG
    const uint32_t nowMs = millis();
    if (nowMs - lastLogMs_ >= LOG_INPUT_INTERVAL_MS) {
        lastLogMs_ = nowMs;
        LOG("imu", "%s bias %.3f %.3f %.3f dps | gyro-bias %.2f %.2f %.2f dps | |a| %.3f g",
            still ? "STILL " : "moving", bias[0], bias[1], bias[2], g[0] - bias[0], g[1] - bias[1], g[2] - bias[2],
            sqrtf(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]));
    }
#endif
    return true;
}
