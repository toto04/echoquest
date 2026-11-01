// 6-axis orientation fusion (Mahony complementary filter) and conversion to
// the protocol's yaw/pitch/roll conventions. Pure C++, unit-tested in
// test/test_logic.
//
// Body frame (see IMU_AXIS_MAP in config.h): X forward, Y left, Z up.
// The quaternion rotates body-frame vectors into the earth frame.
// With no magnetometer, yaw is integrated gyro only: 0 at boot, drifts slowly.
#pragma once

#include <math.h>
#include <stdint.h>

namespace orientation {

constexpr float DEG_TO_RAD_F = 0.017453292519943f;
constexpr float RAD_TO_DEG_F = 57.295779513082f;

struct Quaternion {
    float w = 1, x = 0, y = 0, z = 0;
};

// Angles in degrees, using protocol/hid-report.md conventions:
// yaw + = turn right, pitch + = nose (top edge) up, roll + = right side down.
struct Euler {
    float yawDeg = 0, pitchDeg = 0, rollDeg = 0;
};

// out[i] = sign(map[i]) * in[|map[i]| - 1]. map entries are +-1..3.
inline void remapAxes(const int8_t map[3], const float in[3], float out[3]) {
    for (int i = 0; i < 3; i++) {
        int8_t m = map[i];
        int idx = (m < 0 ? -m : m) - 1;
        if (idx < 0 || idx > 2) {
            out[i] = 0;
            continue;
        }
        out[i] = m < 0 ? -in[idx] : in[idx];
    }
}

inline Quaternion fromEulerRad(float yaw, float pitch, float roll) {
    float cy = cosf(yaw * 0.5f), sy = sinf(yaw * 0.5f);
    float cp = cosf(pitch * 0.5f), sp = sinf(pitch * 0.5f);
    float cr = cosf(roll * 0.5f), sr = sinf(roll * 0.5f);
    Quaternion q;
    q.w = cr * cp * cy + sr * sp * sy;
    q.x = sr * cp * cy - cr * sp * sy;
    q.y = cr * sp * cy + sr * cp * sy;
    q.z = cr * cp * sy - sr * sp * cy;
    return q;
}

// Converts the body->earth quaternion (FLU body frame, ZYX Euler) into the
// protocol's sign conventions.
inline Euler toProtocolEuler(const Quaternion& q) {
    float yaw = atan2f(2.0f * (q.w * q.z + q.x * q.y), 1.0f - 2.0f * (q.y * q.y + q.z * q.z));
    float s = 2.0f * (q.w * q.y - q.z * q.x);
    if (s > 1.0f) s = 1.0f;
    if (s < -1.0f) s = -1.0f;
    float pitch = asinf(s);
    float roll = atan2f(2.0f * (q.w * q.x + q.y * q.z), 1.0f - 2.0f * (q.x * q.x + q.y * q.y));
    Euler e;
    // Math convention: +yaw = CCW from above (turn left), +pitch about Y-left
    // = nose down, +roll about X-forward = right side down.
    e.yawDeg = -yaw * RAD_TO_DEG_F;
    e.pitchDeg = -pitch * RAD_TO_DEG_F;
    e.rollDeg = roll * RAD_TO_DEG_F;
    return e;
}

class MahonyFilter {
   public:
    explicit MahonyFilter(float kp = 1.0f) : kp_(kp) {}

    // Sets pitch/roll from a still accelerometer reading (any unit), yaw = 0.
    void initFromAccel(float ax, float ay, float az) {
        float roll = atan2f(ay, az);
        float pitch = atan2f(-ax, sqrtf(ay * ay + az * az));
        q_ = fromEulerRad(0, pitch, roll);
    }

    // gx/gy/gz in rad/s, accel in any consistent unit, dt in seconds.
    void update(float gx, float gy, float gz, float ax, float ay, float az, float dt) {
        float n = sqrtf(ax * ax + ay * ay + az * az);
        if (n > 1e-6f) {
            ax /= n;
            ay /= n;
            az /= n;
            // "Up" as predicted by the current orientation, in body frame.
            float vx = 2.0f * (q_.x * q_.z - q_.w * q_.y);
            float vy = 2.0f * (q_.w * q_.x + q_.y * q_.z);
            float vz = q_.w * q_.w - q_.x * q_.x - q_.y * q_.y + q_.z * q_.z;
            // Error = measured x predicted; steer the gyro toward the accelerometer.
            gx += kp_ * (ay * vz - az * vy);
            gy += kp_ * (az * vx - ax * vz);
            gz += kp_ * (ax * vy - ay * vx);
        }
        gx *= 0.5f * dt;
        gy *= 0.5f * dt;
        gz *= 0.5f * dt;
        Quaternion p = q_;
        q_.w += -p.x * gx - p.y * gy - p.z * gz;
        q_.x += p.w * gx + p.y * gz - p.z * gy;
        q_.y += p.w * gy - p.x * gz + p.z * gx;
        q_.z += p.w * gz + p.x * gy - p.y * gx;
        float qn = sqrtf(q_.w * q_.w + q_.x * q_.x + q_.y * q_.y + q_.z * q_.z);
        if (qn > 1e-9f) {
            q_.w /= qn;
            q_.x /= qn;
            q_.y /= qn;
            q_.z /= qn;
        } else {
            q_ = Quaternion{};
        }
    }

    const Quaternion& quaternion() const { return q_; }
    Euler euler() const { return toProtocolEuler(q_); }

   private:
    float kp_;
    Quaternion q_;
};

}  // namespace orientation
