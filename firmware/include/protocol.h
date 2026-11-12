// HID wire protocol: report descriptor, report IDs, packing and parsing.
//
// This file must match protocol/hid-report.md exactly. That doc is the
// source of truth the Unity layout mirrors. If you change anything here:
//   1. update protocol/hid-report.md (and tools/hid_probe.py)
//   2. bump DEVICE_PID in config.h (hosts cache descriptors per VID/PID)
//   3. run `pio test -e native` (checks descriptor sizes match the constants)
//
// Pure C++ with no Arduino dependencies, so it also compiles in the native tests.
#pragma once

#include <math.h>
#include <stddef.h>
#include <stdint.h>

namespace protocol {

// Bump minor for additive changes, major for layout changes. Mirrors DEVICE_VERSION.
constexpr uint8_t VERSION_MAJOR = 1;
constexpr uint8_t VERSION_MINOR = 0;

constexpr uint8_t REPORT_ID_INPUT = 1;
constexpr uint8_t REPORT_ID_HAPTIC = 2;

// Payload sizes exclude the report ID byte. Over BLE the characteristic value
// carries only the payload; host HID APIs prepend the report ID.
constexpr size_t INPUT_REPORT_SIZE = 15;
constexpr size_t HAPTIC_REPORT_SIZE = 8;

constexpr uint8_t AXIS_COUNT = 6;
constexpr int16_t AXIS_MAX = 32767;   // logical range is symmetric: -32767..32767
constexpr uint8_t HAT_CENTERED = 0;   // 1..8 = N, NE, E, SE, S, SW, W, NW

constexpr uint8_t AXIS_YAW = 0;       // HID usage X
constexpr uint8_t AXIS_PITCH = 1;     // HID usage Y
constexpr uint8_t AXIS_ROLL = 2;      // HID usage Z
                                      // 3..5 (Rx, Ry, Rz) spare, sent as 0

constexpr uint8_t MOTOR_ALL = 0xFF;
constexpr uint8_t PATTERN_CONSTANT = 0;

// clang-format off
constexpr uint8_t HID_REPORT_DESCRIPTOR[] = {
    0x05, 0x01,        // Usage Page (Generic Desktop)
    0x09, 0x05,        // Usage (Game Pad)
    0xA1, 0x01,        // Collection (Application)

    0x85, REPORT_ID_INPUT, //   Report ID (1)

    // Bytes 0-1: 16 buttons, bit i = button i+1
    0x05, 0x09,        //   Usage Page (Button)
    0x19, 0x01,        //   Usage Minimum (Button 1)
    0x29, 0x10,        //   Usage Maximum (Button 16)
    0x15, 0x00,        //   Logical Minimum (0)
    0x25, 0x01,        //   Logical Maximum (1)
    0x75, 0x01,        //   Report Size (1)
    0x95, 0x10,        //   Report Count (16)
    0x81, 0x02,        //   Input (Data, Var, Abs)

    // Byte 2, low nibble: hat switch, 1..8 clockwise from N, 0 = centered (null)
    0x05, 0x01,        //   Usage Page (Generic Desktop)
    0x09, 0x39,        //   Usage (Hat Switch)
    0x15, 0x01,        //   Logical Minimum (1)
    0x25, 0x08,        //   Logical Maximum (8)
    0x35, 0x00,        //   Physical Minimum (0)
    0x46, 0x3B, 0x01,  //   Physical Maximum (315)
    0x65, 0x14,        //   Unit (Eng Rot: Degrees)
    0x75, 0x04,        //   Report Size (4)
    0x95, 0x01,        //   Report Count (1)
    0x81, 0x42,        //   Input (Data, Var, Abs, Null State)
    0x65, 0x00,        //   Unit (None)
    0x45, 0x00,        //   Physical Maximum (0)

    // Byte 2, high nibble: padding
    0x75, 0x04,        //   Report Size (4)
    0x95, 0x01,        //   Report Count (1)
    0x81, 0x03,        //   Input (Const, Var, Abs)

    // Bytes 3-14: 6 signed 16-bit little-endian axes
    0x09, 0x30,        //   Usage (X)   yaw
    0x09, 0x31,        //   Usage (Y)   pitch
    0x09, 0x32,        //   Usage (Z)   roll
    0x09, 0x33,        //   Usage (Rx)  spare
    0x09, 0x34,        //   Usage (Ry)  spare
    0x09, 0x35,        //   Usage (Rz)  spare
    0x16, 0x01, 0x80,  //   Logical Minimum (-32767)
    0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
    0x75, 0x10,        //   Report Size (16)
    0x95, 0x06,        //   Report Count (6)
    0x81, 0x02,        //   Input (Data, Var, Abs)

    0x85, REPORT_ID_HAPTIC, //   Report ID (2)
    0x06, 0x00, 0xFF,  //   Usage Page (Vendor Defined 0xFF00)
    0x09, 0x01,        //   Usage (0x01)
    0x15, 0x00,        //   Logical Minimum (0)
    0x26, 0xFF, 0x00,  //   Logical Maximum (255)
    0x75, 0x08,        //   Report Size (8)
    0x95, HAPTIC_REPORT_SIZE, // Report Count (8)
    0x91, 0x02,        //   Output (Data, Var, Abs)

    0xC0,              // End Collection
};
// clang-format on

struct InputState {
    uint16_t buttons = 0;                  // bit i = button i+1 pressed
    uint8_t hat = HAT_CENTERED;
    int16_t axes[AXIS_COUNT] = {0, 0, 0, 0, 0, 0};

    bool operator==(const InputState& o) const {
        if (buttons != o.buttons || hat != o.hat) return false;
        for (uint8_t i = 0; i < AXIS_COUNT; i++)
            if (axes[i] != o.axes[i]) return false;
        return true;
    }
    bool operator!=(const InputState& o) const { return !(*this == o); }
};

struct HapticCommand {
    uint8_t motorId = 0;       // 0..N-1, or MOTOR_ALL
    uint8_t intensity = 0;     // 0 = stop, 255 = max
    uint16_t durationMs = 0;   // 0 = stop
    uint8_t patternId = PATTERN_CONSTANT;

    bool isStop() const { return intensity == 0 || durationMs == 0; }
};

enum class ParseResult { Ok, BadLength, BadReportId };

inline const char* toString(ParseResult r) {
    switch (r) {
        case ParseResult::Ok: return "ok";
        case ParseResult::BadLength: return "bad length";
        case ParseResult::BadReportId: return "bad report id";
    }
    return "?";
}

// Converts an angle in degrees to the axis range. Wraps into [-180, 180]
// first, so yaw sweeping past 180 wraps to -180. 1 LSB = 180/32767 degrees.
inline int16_t degreesToAxis(float deg) {
    if (!isfinite(deg)) return 0;
    deg = fmodf(deg, 360.0f);
    if (deg > 180.0f) deg -= 360.0f;
    if (deg < -180.0f) deg += 360.0f;
    long v = lroundf(deg * (float)AXIS_MAX / 180.0f);
    if (v > AXIS_MAX) v = AXIS_MAX;
    if (v < -AXIS_MAX) v = -AXIS_MAX;
    return (int16_t)v;
}

inline float axisToDegrees(int16_t v) { return (float)v * 180.0f / (float)AXIS_MAX; }

// Opposing directions cancel out (up+down = neither).
inline uint8_t hatFromDirections(bool up, bool right, bool down, bool left) {
    int y = (up ? 1 : 0) - (down ? 1 : 0);
    int x = (right ? 1 : 0) - (left ? 1 : 0);
    // Indexed by [y+1][x+1]
    static constexpr uint8_t table[3][3] = {
        {6, 5, 4},             // down:  SW, S, SE
        {7, HAT_CENTERED, 3},  // mid:   W, center, E
        {8, 1, 2},             // up:    NW, N, NE
    };
    return table[y + 1][x + 1];
}

// Writes the input report payload (no report ID) into out.
inline void packInputReport(const InputState& s, uint8_t out[INPUT_REPORT_SIZE]) {
    out[0] = (uint8_t)(s.buttons & 0xFF);
    out[1] = (uint8_t)(s.buttons >> 8);
    out[2] = (uint8_t)(s.hat & 0x0F);
    for (uint8_t i = 0; i < AXIS_COUNT; i++) {
        uint16_t u = (uint16_t)s.axes[i];
        out[3 + i * 2] = (uint8_t)(u & 0xFF);
        out[4 + i * 2] = (uint8_t)(u >> 8);
    }
}

// Parses a haptic output report. Accepts the 8-byte payload, or 9 bytes with
// a leading report ID (some host stacks include it). Reserved bytes 5-7 are
// ignored in protocol 1.0.
inline ParseResult parseHapticReport(const uint8_t* data, size_t len, HapticCommand& out) {
    if (len == HAPTIC_REPORT_SIZE + 1) {
        if (data[0] != REPORT_ID_HAPTIC) return ParseResult::BadReportId;
        data++;
        len--;
    }
    if (len != HAPTIC_REPORT_SIZE) return ParseResult::BadLength;
    out.motorId = data[0];
    out.intensity = data[1];
    out.durationMs = (uint16_t)(data[2] | (data[3] << 8));
    out.patternId = data[4];
    return ParseResult::Ok;
}

}  // namespace protocol
