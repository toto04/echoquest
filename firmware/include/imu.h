// Orientation source. Pick the implementation with IMU_DRIVER in config.h.
//   FakeImu     slow sine sweep, so the HID path works without hardware
//   Qmi8658Imu  QMI8658(A) accel + gyro over I2C, fused with a Mahony filter
#pragma once

#include <stdint.h>

#include "orientation_filter.h"

struct Orientation {
    float yawDeg = 0;     // -180..180, positive = turn right (clockwise seen from above)
    float pitchDeg = 0;   // -90..90, positive = nose up
    float rollDeg = 0;    // -180..180, positive = right side down
};

class ImuSource {
   public:
    virtual ~ImuSource() = default;
    virtual bool begin() = 0;
    // Call every loop iteration. Returns false if no fresh sample was
    // available (out keeps the last value).
    virtual bool read(uint32_t nowMs, Orientation& out) = 0;
    virtual const char* name() const = 0;
};

class FakeImu : public ImuSource {
   public:
    bool begin() override { return true; }
    bool read(uint32_t nowMs, Orientation& out) override;
    const char* name() const override { return "FakeImu (sine sweep)"; }
};

class Qmi8658Imu : public ImuSource {
   public:
    // Resets and configures the sensor, then measures gyro bias. The board
    // must be still for ~1 s during begin().
    bool begin() override;
    bool read(uint32_t nowMs, Orientation& out) override;
    const char* name() const override { return "QMI8658 + Mahony"; }

   private:
    // Raw sample converted to body frame: accel in g, gyro in deg/s.
    bool readSample(float accel[3], float gyroDps[3]);

    orientation::MahonyFilter filter_;
    float gyroBiasDps_[3] = {0, 0, 0};
    uint32_t lastSampleUs_ = 0;
    bool ok_ = false;
    uint32_t errorCount_ = 0;
};

// Returns the implementation selected by IMU_DRIVER. Never returns null.
ImuSource& createImu();
