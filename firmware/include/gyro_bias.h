// Stillness detection and continuous gyro-bias tracking. Pure C++, unit-tested
// in test/test_logic.
//
// Why: yaw has no absolute reference on a 6-axis IMU, so any gyro bias left
// after the boot calibration integrates into a steady yaw rotation (0.5 dps of
// bias = 30 deg/min). The bias also shifts as the board warms up. While the
// board is still, this tracker re-learns the bias and tells the caller to stop
// integrating the gyro, so the reported orientation doesn't creep at rest.
//
// "Still" means, sustained for STILL_TIME_S: gyro samples stay close to their
// own short-term average (no rotation beyond noise), the average itself is a
// plausible bias (not a slow steady turn), and the accelerometer reads about
// 1 g and isn't changing.
#pragma once

#include <math.h>

namespace orientation {

class GyroBiasTracker {
   public:
    struct Config {
        float gyroNoiseDps = 1.5f;      // max |sample - short-term mean| per axis while still
        float maxBiasErrorDps = 6.0f;   // short-term mean further than this from the bias = turning, not bias
        float accelTolG = 0.06f;        // | |a| - 1 g | while still
        float accelNoiseG = 0.04f;      // max |a - short-term mean| per axis while still
        float averageTauS = 0.25f;      // short-term mean time constant
        float stillTimeS = 0.4f;        // stillness must last this long before it counts
        float biasTauS = 1.5f;          // bias learning time constant while still
    };

    GyroBiasTracker() = default;
    explicit GyroBiasTracker(const Config& cfg) : cfg_(cfg) {}

    // Starts from a known bias (e.g. the boot calibration).
    void reset(const float biasDps[3]) {
        for (int i = 0; i < 3; i++) {
            bias_[i] = biasDps[i];
            gyroMean_[i] = biasDps[i];
            accelMean_[i] = 0;
        }
        accelMean_[2] = 1;
        primed_ = false;
        stillFor_ = 0;
        still_ = false;
    }

    // gyroDps: raw rates (bias not removed); accelG: acceleration in g; dt in s.
    // Returns true while the board is still (bias is being learned).
    bool update(const float gyroDps[3], const float accelG[3], float dt) {
        if (dt <= 0) return still_;
        if (!primed_) {
            for (int i = 0; i < 3; i++) {
                gyroMean_[i] = gyroDps[i];
                accelMean_[i] = accelG[i];
            }
            primed_ = true;
        }
        const float k = alpha(dt, cfg_.averageTauS);
        bool quiet = true;
        float an2 = 0;
        for (int i = 0; i < 3; i++) {
            gyroMean_[i] += k * (gyroDps[i] - gyroMean_[i]);
            accelMean_[i] += k * (accelG[i] - accelMean_[i]);
            if (fabsf(gyroDps[i] - gyroMean_[i]) > cfg_.gyroNoiseDps) quiet = false;
            if (fabsf(gyroMean_[i] - bias_[i]) > cfg_.maxBiasErrorDps) quiet = false;
            if (fabsf(accelG[i] - accelMean_[i]) > cfg_.accelNoiseG) quiet = false;
            an2 += accelG[i] * accelG[i];
        }
        if (fabsf(sqrtf(an2) - 1.0f) > cfg_.accelTolG) quiet = false;

        stillFor_ = quiet ? stillFor_ + dt : 0;
        still_ = stillFor_ >= cfg_.stillTimeS;
        if (still_) {
            const float kb = alpha(dt, cfg_.biasTauS);
            for (int i = 0; i < 3; i++) bias_[i] += kb * (gyroDps[i] - bias_[i]);
        }
        return still_;
    }

    bool still() const { return still_; }
    const float* bias() const { return bias_; }

   private:
    static float alpha(float dt, float tau) { return dt >= tau ? 1.0f : dt / tau; }

    Config cfg_;
    float bias_[3] = {0, 0, 0};
    float gyroMean_[3] = {0, 0, 0};
    float accelMean_[3] = {0, 0, 1};
    bool primed_ = false;
    float stillFor_ = 0;
    bool still_ = false;
};

}  // namespace orientation
