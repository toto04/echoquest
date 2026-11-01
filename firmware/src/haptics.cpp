#include "haptics.h"

#include <Arduino.h>

#include "config.h"
#include "log.h"

static_assert(HAPTIC_MOTOR_COUNT >= 1 && HAPTIC_MOTOR_COUNT <= 8, "StubHapticDriver supports 1..8 motors");

void StubHapticDriver::begin() {
    pinMode(HAPTIC_STUB_PIN, OUTPUT);
    digitalWrite(HAPTIC_STUB_PIN, LOW);
    refreshOutputs();
}

void StubHapticDriver::play(const protocol::HapticCommand& cmd, uint32_t nowMs) {
    if (cmd.motorId != protocol::MOTOR_ALL && cmd.motorId >= HAPTIC_MOTOR_COUNT) {
        LOG("haptic", "ignored: motor %u out of range (have %u)", cmd.motorId, HAPTIC_MOTOR_COUNT);
        return;
    }
    if (cmd.patternId != protocol::PATTERN_CONSTANT) {
        LOG("haptic", "pattern %u not implemented, playing constant", cmd.patternId);
    }
    if (cmd.motorId == protocol::MOTOR_ALL) {
        for (uint8_t i = 0; i < HAPTIC_MOTOR_COUNT; i++) startMotor(i, cmd, nowMs);
    } else {
        startMotor(cmd.motorId, cmd, nowMs);
    }
    refreshOutputs();
}

void StubHapticDriver::startMotor(uint8_t id, const protocol::HapticCommand& cmd, uint32_t nowMs) {
    Motor& m = motors_[id];
    if (cmd.isStop()) {
        m.active = false;
        LOG("haptic", "motor %u STOP", id);
        return;
    }
    m.active = true;
    m.intensity = cmd.intensity;
    m.startMs = nowMs;
    m.durationMs = cmd.durationMs;
    LOG("haptic", "motor %u ON  intensity=%u duration=%ums", id, cmd.intensity, cmd.durationMs);
}

void StubHapticDriver::stopAll() {
    bool any = false;
    for (uint8_t i = 0; i < HAPTIC_MOTOR_COUNT; i++) {
        any |= motors_[i].active;
        motors_[i].active = false;
    }
    if (any) LOG("haptic", "all motors STOP");
    refreshOutputs();
}

void StubHapticDriver::update(uint32_t nowMs) {
    bool changed = false;
    for (uint8_t i = 0; i < HAPTIC_MOTOR_COUNT; i++) {
        Motor& m = motors_[i];
        if (m.active && nowMs - m.startMs >= m.durationMs) {
            m.active = false;
            changed = true;
            LOG("haptic", "motor %u done", i);
        }
    }
    if (changed) refreshOutputs();
}

void StubHapticDriver::refreshOutputs() {
    uint8_t maxIntensity = 0;
    bool any = false;
    for (uint8_t i = 0; i < HAPTIC_MOTOR_COUNT; i++) {
        if (motors_[i].active) {
            any = true;
            if (motors_[i].intensity > maxIntensity) maxIntensity = motors_[i].intensity;
        }
    }
    digitalWrite(HAPTIC_STUB_PIN, any ? HIGH : LOW);
#ifdef RGB_BUILTIN
    if (HAPTIC_STUB_USE_RGB_LED) {
        // Magenta, scaled down so full intensity isn't blinding.
        uint8_t v = maxIntensity / 4;
        rgbLedWrite(RGB_BUILTIN, v, 0, v);
    }
#endif
}
