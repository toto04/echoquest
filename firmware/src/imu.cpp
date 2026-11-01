#include "imu.h"

#include <math.h>

#include "config.h"

namespace {

constexpr float TWO_PI_F = 6.28318530718f;

// Different, non-harmonic periods so all three axes are distinguishable in a log.
constexpr float YAW_PERIOD_S = 20.0f, YAW_AMPLITUDE_DEG = 180.0f;
constexpr float PITCH_PERIOD_S = 7.0f, PITCH_AMPLITUDE_DEG = 45.0f;
constexpr float ROLL_PERIOD_S = 11.0f, ROLL_AMPLITUDE_DEG = 30.0f;

float sweep(float tSeconds, float periodS, float amplitudeDeg) {
    return amplitudeDeg * sinf(TWO_PI_F * tSeconds / periodS);
}

}  // namespace

bool FakeImu::read(uint32_t nowMs, Orientation& out) {
    float t = (float)nowMs / 1000.0f;
    out.yawDeg = sweep(t, YAW_PERIOD_S, YAW_AMPLITUDE_DEG);
    out.pitchDeg = sweep(t, PITCH_PERIOD_S, PITCH_AMPLITUDE_DEG);
    out.rollDeg = sweep(t, ROLL_PERIOD_S, ROLL_AMPLITUDE_DEG);
    return true;
}

ImuSource& createImu() {
#if IMU_DRIVER == IMU_DRIVER_FAKE
    static FakeImu imu;
    return imu;
#else
#error "Unknown IMU_DRIVER in config.h"
#endif
}
