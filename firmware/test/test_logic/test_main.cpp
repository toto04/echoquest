// Host-side tests for pure logic. Run: pio test -e native
#include <unity.h>

#include <map>

#include "debouncer.h"
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
    return UNITY_END();
}
