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

// A still board shows less spread than this on every gyro axis.
constexpr float CALIBRATION_MAX_SPREAD_DPS = 3.0f;
constexpr uint32_t MAX_DT_US = 50000;   // ignore longer gaps (e.g. after a stall)

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
    delay(50);  // let the gyro settle after power-up

    // Gyro bias: average over IMU_CALIBRATION_SAMPLES while still.
    float sumG[3] = {0, 0, 0}, sumA[3] = {0, 0, 0};
    float minG[3] = {1e9f, 1e9f, 1e9f}, maxG[3] = {-1e9f, -1e9f, -1e9f};
    uint16_t n = 0;
    uint32_t deadline = millis() + 3000;
    while (n < IMU_CALIBRATION_SAMPLES && (int32_t)(millis() - deadline) < 0) {
        uint8_t status = 0;
        if (!i2c_bus::readRegs(QMI8658_I2C_ADDR, REG_STATUS0, &status, 1) || !(status & 0x02)) {
            delayMicroseconds(500);
            continue;
        }
        float a[3], g[3];
        if (!readSample(a, g)) continue;
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
    for (int i = 0; i < 3; i++) {
        gyroBiasDps_[i] = sumG[i] / n;
        if (maxG[i] - minG[i] > spread) spread = maxG[i] - minG[i];
    }
    filter_.initFromAccel(sumA[0] / n, sumA[1] / n, sumA[2] / n);
    LOG("imu", "gyro bias %.2f %.2f %.2f dps over %u samples (spread %.1f dps)", gyroBiasDps_[0], gyroBiasDps_[1],
        gyroBiasDps_[2], n, spread);
    if (spread > CALIBRATION_MAX_SPREAD_DPS) {
        LOG("imu", "WARNING: board moved during calibration; yaw will drift. Reset it while still.");
    }
    lastSampleUs_ = micros();
    ok_ = true;
    return true;
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

    filter_.update((g[0] - gyroBiasDps_[0]) * orientation::DEG_TO_RAD_F,
                   (g[1] - gyroBiasDps_[1]) * orientation::DEG_TO_RAD_F,
                   (g[2] - gyroBiasDps_[2]) * orientation::DEG_TO_RAD_F, a[0], a[1], a[2], dtUs * 1e-6f);
    orientation::Euler e = filter_.euler();
    out.yawDeg = e.yawDeg;
    out.pitchDeg = e.pitchDeg;
    out.rollDeg = e.rollDeg;
    return true;
}
