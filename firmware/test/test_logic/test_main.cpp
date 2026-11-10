// Host-side tests for pure logic. Run: pio test -e native
#include <unity.h>

#include <map>

#include "debouncer.h"
#include "gyro_bias.h"
#include "orientation_filter.h"
#include "protocol.h"

using namespace protocol;

void setUp() {}
void tearDown() {}

// ---- Descriptor --------------------------------------------------------------
// Minimal HID descriptor walker: sums report bits per (type, report ID) so we
// can check the descriptor agrees with INPUT_REPORT_SIZE / HAPTIC_REPORT_SIZE.
struct ReportBits {
    std::map<uint8_t, uint32_t> input, output, feature;
};

static ReportBits walkDescriptor(const uint8_t* d, size_t len) {
    ReportBits bits;
    uint32_t reportSize = 0, reportCount = 0;
    uint8_t reportId = 0;
    int depth = 0;
    size_t i = 0;
    while (i < len) {
        uint8_t prefix = d[i];
        uint8_t size = prefix & 0x03;
        if (size == 3) size = 4;
        TEST_ASSERT_TRUE_MESSAGE(i + 1 + size <= len, "descriptor item runs past the end");
        uint32_t value = 0;
        for (uint8_t b = 0; b < size; b++) value |= (uint32_t)d[i + 1 + b] << (8 * b);
        switch (prefix & 0xFC) {
            case 0x74: reportSize = value; break;                 // Report Size
            case 0x94: reportCount = value; break;                // Report Count
            case 0x84: reportId = (uint8_t)value; break;          // Report ID
            case 0x80: bits.input[reportId] += reportSize * reportCount; break;
            case 0x90: bits.output[reportId] += reportSize * reportCount; break;
            case 0xB0: bits.feature[reportId] += reportSize * reportCount; break;
            case 0xA0: depth++; break;                            // Collection
            case 0xC0: depth--; break;                            // End Collection
        }
        i += 1 + size;
    }
    TEST_ASSERT_EQUAL_MESSAGE(0, depth, "unbalanced collections");
    return bits;
}

void test_descriptor_matches_report_sizes() {
    ReportBits b = walkDescriptor(HID_REPORT_DESCRIPTOR, sizeof(HID_REPORT_DESCRIPTOR));
    TEST_ASSERT_EQUAL(1, b.input.size());
    TEST_ASSERT_EQUAL(1, b.output.size());
    TEST_ASSERT_EQUAL(0, b.feature.size());
    TEST_ASSERT_EQUAL(INPUT_REPORT_SIZE * 8, b.input[REPORT_ID_INPUT]);
    TEST_ASSERT_EQUAL(HAPTIC_REPORT_SIZE * 8, b.output[REPORT_ID_HAPTIC]);
}

// ---- Input packing -----------------------------------------------------------
void test_pack_input_layout() {
    InputState s;
    s.buttons = 0x8001;  // button 1 and button 16
    s.hat = 3;           // E
    s.axes[0] = 0x1234;
    s.axes[1] = -2;
    s.axes[2] = AXIS_MAX;
    s.axes[5] = -AXIS_MAX;
    uint8_t out[INPUT_REPORT_SIZE];
    packInputReport(s, out);
    const uint8_t expected[INPUT_REPORT_SIZE] = {
        0x01, 0x80,              // buttons LE
        0x03,                    // hat, padding nibble 0
        0x34, 0x12,              // yaw
        0xFE, 0xFF,              // pitch -2
        0xFF, 0x7F,              // roll +32767
        0x00, 0x00, 0x00, 0x00,  // spare
        0x01, 0x80,              // Rz -32767
    };
    TEST_ASSERT_EQUAL_HEX8_ARRAY(expected, out, INPUT_REPORT_SIZE);
}

void test_degrees_to_axis() {
    TEST_ASSERT_EQUAL_INT16(0, degreesToAxis(0));
    TEST_ASSERT_EQUAL_INT16(AXIS_MAX, degreesToAxis(180));
    TEST_ASSERT_EQUAL_INT16(-AXIS_MAX, degreesToAxis(-180));
    TEST_ASSERT_EQUAL_INT16(16384, degreesToAxis(90));   // 90 * 32767/180 = 16383.5 -> rounds away from 0
    TEST_ASSERT_EQUAL_INT16(degreesToAxis(-170), degreesToAxis(190));  // wraps
    TEST_ASSERT_EQUAL_INT16(degreesToAxis(10), degreesToAxis(370));
    TEST_ASSERT_EQUAL_INT16(0, degreesToAxis(NAN));
    TEST_ASSERT_FLOAT_WITHIN(0.01f, 45.0f, axisToDegrees(degreesToAxis(45)));
}

void test_hat_from_directions() {
    TEST_ASSERT_EQUAL(HAT_CENTERED, hatFromDirections(false, false, false, false));
    TEST_ASSERT_EQUAL(1, hatFromDirections(true, false, false, false));   // N
    TEST_ASSERT_EQUAL(2, hatFromDirections(true, true, false, false));    // NE
    TEST_ASSERT_EQUAL(3, hatFromDirections(false, true, false, false));   // E
    TEST_ASSERT_EQUAL(4, hatFromDirections(false, true, true, false));    // SE
    TEST_ASSERT_EQUAL(5, hatFromDirections(false, false, true, false));   // S
    TEST_ASSERT_EQUAL(6, hatFromDirections(false, false, true, true));    // SW
    TEST_ASSERT_EQUAL(7, hatFromDirections(false, false, false, true));   // W
    TEST_ASSERT_EQUAL(8, hatFromDirections(true, false, false, true));    // NW
    TEST_ASSERT_EQUAL(HAT_CENTERED, hatFromDirections(true, false, true, false));  // opposing cancel
    TEST_ASSERT_EQUAL(3, hatFromDirections(true, true, true, false));     // up+down cancel, right remains
}

// ---- Haptic parsing ----------------------------------------------------------
void test_parse_haptic_payload() {
    const uint8_t r[] = {1, 200, 0xF4, 0x01, 0, 0, 0, 0};  // motor 1, 200, 500 ms
    HapticCommand c;
    TEST_ASSERT_TRUE(parseHapticReport(r, sizeof(r), c) == ParseResult::Ok);
    TEST_ASSERT_EQUAL(1, c.motorId);
    TEST_ASSERT_EQUAL(200, c.intensity);
    TEST_ASSERT_EQUAL(500, c.durationMs);
    TEST_ASSERT_EQUAL(0, c.patternId);
    TEST_ASSERT_FALSE(c.isStop());
}

void test_parse_haptic_with_report_id() {
    const uint8_t r[] = {REPORT_ID_HAPTIC, 0xFF, 10, 0x10, 0x27, 3, 9, 9, 9};  // all motors, 10000 ms, pattern 3
    HapticCommand c;
    TEST_ASSERT_TRUE(parseHapticReport(r, sizeof(r), c) == ParseResult::Ok);
    TEST_ASSERT_EQUAL(MOTOR_ALL, c.motorId);
    TEST_ASSERT_EQUAL(10000, c.durationMs);
    TEST_ASSERT_EQUAL(3, c.patternId);
}

void test_parse_haptic_rejects_bad_input() {
    HapticCommand c;
    const uint8_t shortR[] = {0, 1, 2};
    TEST_ASSERT_TRUE(parseHapticReport(shortR, sizeof(shortR), c) == ParseResult::BadLength);
    const uint8_t wrongId[] = {7, 0, 0, 0, 0, 0, 0, 0, 0};
    TEST_ASSERT_TRUE(parseHapticReport(wrongId, sizeof(wrongId), c) == ParseResult::BadReportId);
}

void test_haptic_stop_semantics() {
    HapticCommand c;
    c.intensity = 0;
    c.durationMs = 100;
    TEST_ASSERT_TRUE(c.isStop());
    c.intensity = 100;
    c.durationMs = 0;
    TEST_ASSERT_TRUE(c.isStop());
}

// ---- Debouncer ---------------------------------------------------------------
void test_debouncer_ignores_bounce() {
    Debouncer db(10);
    TEST_ASSERT_FALSE(db.update(true, 0));
    TEST_ASSERT_FALSE(db.update(false, 3));  // bounce
    TEST_ASSERT_FALSE(db.update(true, 5));
    TEST_ASSERT_FALSE(db.update(true, 14));  // only 9 ms stable
    TEST_ASSERT_TRUE(db.update(true, 15));   // 10 ms stable
    TEST_ASSERT_TRUE(db.update(false, 20));  // release not yet accepted
    TEST_ASSERT_FALSE(db.update(false, 30));
}

void test_debouncer_millis_wraparound() {
    Debouncer db(10);
    db.update(true, 0xFFFFFFFA);
    TEST_ASSERT_TRUE(db.update(true, 4));  // 10 ms later across the wrap
}

// ---- Orientation filter ------------------------------------------------------
// Body frame: X forward, Y left, Z up. A still accelerometer reads "up".
static const float D2R = orientation::DEG_TO_RAD_F;

void test_orientation_flat_is_zero() {
    orientation::MahonyFilter f;
    f.initFromAccel(0, 0, 1);
    orientation::Euler e = f.euler();
    TEST_ASSERT_FLOAT_WITHIN(0.01f, 0, e.yawDeg);
    TEST_ASSERT_FLOAT_WITHIN(0.01f, 0, e.pitchDeg);
    TEST_ASSERT_FLOAT_WITHIN(0.01f, 0, e.rollDeg);
}

void test_orientation_nose_up_is_positive_pitch() {
    // Nose up 30 deg: "up" gains a +X (forward) component.
    orientation::MahonyFilter f;
    f.initFromAccel(sinf(30 * D2R), 0, cosf(30 * D2R));
    orientation::Euler e = f.euler();
    TEST_ASSERT_FLOAT_WITHIN(0.1f, 30, e.pitchDeg);
    TEST_ASSERT_FLOAT_WITHIN(0.1f, 0, e.rollDeg);
}

void test_orientation_right_down_is_positive_roll() {
    // Right side down 20 deg: the left side rises, "up" gains a +Y (left) component.
    orientation::MahonyFilter f;
    f.initFromAccel(0, sinf(20 * D2R), cosf(20 * D2R));
    orientation::Euler e = f.euler();
    TEST_ASSERT_FLOAT_WITHIN(0.1f, 20, e.rollDeg);
    TEST_ASSERT_FLOAT_WITHIN(0.1f, 0, e.pitchDeg);
}

void test_orientation_turn_right_is_positive_yaw() {
    // Turning right = clockwise from above = negative rotation about Z-up.
    orientation::MahonyFilter f(1.0f);
    f.initFromAccel(0, 0, 1);
    for (int i = 0; i < 100; i++) f.update(0, 0, -90 * D2R, 0, 0, 1, 0.01f);   // 1 s at -90 deg/s
    TEST_ASSERT_FLOAT_WITHIN(0.5f, 90, f.euler().yawDeg);
}

void test_orientation_converges_to_accel() {
    // Start flat, then hold the board nose-up 30 deg with no rotation reported:
    // the accelerometer correction must pull pitch to 30.
    orientation::MahonyFilter f(2.0f);
    f.initFromAccel(0, 0, 1);
    for (int i = 0; i < 1000; i++) f.update(0, 0, 0, sinf(30 * D2R), 0, cosf(30 * D2R), 0.01f);
    TEST_ASSERT_FLOAT_WITHIN(0.5f, 30, f.euler().pitchDeg);
}

void test_orientation_gyro_pitch_matches_accel_sign() {
    // Gyro and accel must agree on direction: rotating nose-up via the gyro
    // (negative rate about Y-left) gives positive pitch, same as the accel test.
    orientation::MahonyFilter f(0.0f);   // gyro only
    f.initFromAccel(0, 0, 1);
    for (int i = 0; i < 100; i++) f.update(0, -30 * D2R, 0, 0, 0, 0, 0.01f);
    TEST_ASSERT_FLOAT_WITHIN(0.5f, 30, f.euler().pitchDeg);
}

void test_remap_axes() {
    const int8_t map[3] = {-2, +1, +3};
    const float in[3] = {1, 2, 3};
    float out[3];
    orientation::remapAxes(map, in, out);
    TEST_ASSERT_EQUAL_FLOAT(-2, out[0]);
    TEST_ASSERT_EQUAL_FLOAT(1, out[1]);
    TEST_ASSERT_EQUAL_FLOAT(3, out[2]);
}

// ---- Gyro bias tracking ------------------------------------------------------
// Deterministic pseudo-noise in [-amp, amp].
static float noise(uint32_t& seed, float amp) {
    seed = seed * 1664525u + 1013904223u;
    return ((seed >> 8) / 16777215.0f * 2.0f - 1.0f) * amp;
}

static const float DT = 1.0f / 224;   // QMI8658 ODR

void test_bias_tracker_learns_bias_when_still() {
    const float zero[3] = {0, 0, 0};
    const float trueBias[3] = {0.8f, -0.5f, 1.2f};
    orientation::GyroBiasTracker t;
    t.reset(zero);
    uint32_t seed = 1;
    for (int i = 0; i < 224 * 8; i++) {   // 8 s at rest, gyro noise 0.3 dps
        float g[3] = {trueBias[0] + noise(seed, 0.3f), trueBias[1] + noise(seed, 0.3f), trueBias[2] + noise(seed, 0.3f)};
        float a[3] = {noise(seed, 0.01f), noise(seed, 0.01f), 1 + noise(seed, 0.01f)};
        t.update(g, a, DT);
    }
    TEST_ASSERT_TRUE(t.still());
    for (int i = 0; i < 3; i++) TEST_ASSERT_FLOAT_WITHIN(0.05f, trueBias[i], t.bias()[i]);
}

void test_bias_tracker_ignores_motion() {
    const float bias0[3] = {0.2f, 0.2f, 0.2f};
    orientation::GyroBiasTracker t;
    t.reset(bias0);
    for (int i = 0; i < 224 * 5; i++) {   // waving the controller: +-40 dps at 1.5 Hz
        float w = 40 * sinf(2 * 3.14159f * 1.5f * i * DT);
        float g[3] = {w, 0.5f * w, 0.2f};
        float a[3] = {0.1f * sinf(i * 0.05f), 0, 1};
        TEST_ASSERT_FALSE(t.update(g, a, DT));
    }
    for (int i = 0; i < 3; i++) TEST_ASSERT_EQUAL_FLOAT(bias0[i], t.bias()[i]);
}

void test_bias_tracker_ignores_slow_steady_turn() {
    // A steady 15 dps turn is smooth but far from any plausible bias: not "still".
    const float zero[3] = {0, 0, 0};
    orientation::GyroBiasTracker t;
    t.reset(zero);
    for (int i = 0; i < 224 * 3; i++) {
        float g[3] = {0, 0, -15};
        float a[3] = {0, 0, 1};
        t.update(g, a, DT);
    }
    TEST_ASSERT_FALSE(t.still());
    TEST_ASSERT_EQUAL_FLOAT(0, t.bias()[2]);
}

void test_bias_tracker_needs_one_g() {
    // Gyro quiet but accel far from 1 g (e.g. in free fall or being swung): not still.
    const float zero[3] = {0, 0, 0};
    orientation::GyroBiasTracker t;
    t.reset(zero);
    for (int i = 0; i < 224 * 2; i++) {
        float g[3] = {0.5f, 0, 0};
        float a[3] = {0, 0, 1.3f};
        t.update(g, a, DT);
    }
    TEST_ASSERT_FALSE(t.still());
}

void test_still_board_does_not_drift_in_yaw() {
    // End to end, as the QMI8658 driver does it: a still board, tilted, whose boot
    // calibration left a 0.5 dps error on every axis. Without tracking this drifts
    // ~30 deg/min in yaw; with it, yaw must stay put.
    const float bootBias[3] = {0, 0, 0};
    const float trueBias[3] = {0.5f, -0.5f, 0.5f};
    const float a[3] = {sinf(20 * D2R), 0, cosf(20 * D2R)};   // nose up 20 deg
    orientation::GyroBiasTracker t;
    t.reset(bootBias);
    orientation::MahonyFilter f(1.0f);
    f.initFromAccel(a[0], a[1], a[2]);
    uint32_t seed = 7;
    for (int i = 0; i < 224 * 60; i++) {
        float g[3] = {trueBias[0] + noise(seed, 0.3f), trueBias[1] + noise(seed, 0.3f), trueBias[2] + noise(seed, 0.3f)};
        bool still = t.update(g, a, DT);
        float r[3];
        for (int k = 0; k < 3; k++) r[k] = still ? 0 : (g[k] - t.bias()[k]) * D2R;
        f.update(r[0], r[1], r[2], a[0], a[1], a[2], DT);
    }
    TEST_ASSERT_FLOAT_WITHIN(0.5f, 0, f.euler().yawDeg);
    TEST_ASSERT_FLOAT_WITHIN(0.5f, 20, f.euler().pitchDeg);
}

int main() {
    UNITY_BEGIN();
    RUN_TEST(test_descriptor_matches_report_sizes);
    RUN_TEST(test_pack_input_layout);
    RUN_TEST(test_degrees_to_axis);
    RUN_TEST(test_hat_from_directions);
    RUN_TEST(test_parse_haptic_payload);
    RUN_TEST(test_parse_haptic_with_report_id);
    RUN_TEST(test_parse_haptic_rejects_bad_input);
    RUN_TEST(test_haptic_stop_semantics);
    RUN_TEST(test_debouncer_ignores_bounce);
    RUN_TEST(test_debouncer_millis_wraparound);
    RUN_TEST(test_orientation_flat_is_zero);
    RUN_TEST(test_orientation_nose_up_is_positive_pitch);
    RUN_TEST(test_orientation_right_down_is_positive_roll);
    RUN_TEST(test_orientation_turn_right_is_positive_yaw);
    RUN_TEST(test_orientation_converges_to_accel);
    RUN_TEST(test_orientation_gyro_pitch_matches_accel_sign);
    RUN_TEST(test_remap_axes);
    RUN_TEST(test_bias_tracker_learns_bias_when_still);
    RUN_TEST(test_bias_tracker_ignores_motion);
    RUN_TEST(test_bias_tracker_ignores_slow_steady_turn);
    RUN_TEST(test_bias_tracker_needs_one_g);
    RUN_TEST(test_still_board_does_not_drift_in_yaw);
    return UNITY_END();
}
