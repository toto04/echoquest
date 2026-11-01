// Orientation source. FakeImu produces a slow sine sweep so the HID path can be
// tested before hardware exists. Pick the implementation with IMU_DRIVER in config.h.
#pragma once

#include <stdint.h>

struct Orientation {
    float yawDeg = 0;     // -180..180, positive = turn right (clockwise seen from above)
    float pitchDeg = 0;   // -90..90, positive = nose up
    float rollDeg = 0;    // -180..180, positive = right side down
};

class ImuSource {
   public:
    virtual ~ImuSource() = default;
    virtual bool begin() = 0;
    // Returns false if no fresh sample is available (out keeps the last value).
    virtual bool read(uint32_t nowMs, Orientation& out) = 0;
    virtual const char* name() const = 0;
};

class FakeImu : public ImuSource {
   public:
    bool begin() override { return true; }
    bool read(uint32_t nowMs, Orientation& out) override;
    const char* name() const override { return "FakeImu (sine sweep)"; }
};

// TODO(imu): add the real driver once the part is chosen, e.g.
//   class Bno085Imu : public ImuSource { ... };   // I2C on IMU_PIN_SDA/IMU_PIN_SCL
// then add an IMU_DRIVER_* value in config.h and a case in createImu().

// Returns the implementation selected by IMU_DRIVER. Never returns null.
ImuSource& createImu();
