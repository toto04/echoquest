// Haptic output. ble_hid parses output reports into protocol::HapticCommand;
// main.cpp feeds them to a HapticDriver.
#pragma once

#include <stdint.h>

#include "protocol.h"

class HapticDriver {
   public:
    virtual ~HapticDriver() = default;
    virtual void begin() = 0;
    // Starts (or replaces) the effect on cmd.motorId; a stop command stops it.
    virtual void play(const protocol::HapticCommand& cmd, uint32_t nowMs) = 0;
    virtual void stopAll() = 0;
    // Call every loop iteration; ends effects whose duration has elapsed.
    virtual void update(uint32_t nowMs) = 0;
};

// Placeholder until motors exist: logs every command, drives HAPTIC_STUB_PIN
// HIGH while any motor is active, and optionally lights the onboard RGB LED
// with brightness = intensity.
class StubHapticDriver : public HapticDriver {
   public:
    void begin() override;
    void play(const protocol::HapticCommand& cmd, uint32_t nowMs) override;
    void stopAll() override;
    void update(uint32_t nowMs) override;

   private:
    struct Motor {
        bool active = false;
        uint8_t intensity = 0;
        uint32_t startMs = 0;
        uint16_t durationMs = 0;
    };
    void startMotor(uint8_t id, const protocol::HapticCommand& cmd, uint32_t nowMs);
    void refreshOutputs();

    static constexpr uint8_t MAX_MOTORS = 8;
    Motor motors_[MAX_MOTORS];
};
