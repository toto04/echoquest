// On-board LCD + touch test UI (boards with HAS_LCD). Shows BLE state,
// orientation and haptic activity, and provides two on-screen buttons that
// act as HID buttons. On boards without an LCD every function is a no-op.
#pragma once

#include <stdint.h>

#include "imu.h"

namespace ui {

struct View {
    const char* deviceName = "";
    bool connected = false;
    bool ready = false;              // host subscribed to input reports
    Orientation orientation;
    uint8_t hapticIntensity = 0;     // 0 = idle
};

bool begin();

// Call every loop iteration; polls touch and redraws what changed (rate-limited).
void update(uint32_t nowMs, const View& view);

// HID button bits currently held via the on-screen buttons.
uint16_t buttonMask();

}  // namespace ui
