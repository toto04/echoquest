#include "buttons.h"

#include <Arduino.h>

#include "config.h"
#include "debouncer.h"
#include "protocol.h"

namespace buttons {
namespace {

enum HatDir : uint8_t { HAT_UP, HAT_RIGHT, HAT_DOWN, HAT_LEFT, HAT_DIRS };
constexpr uint8_t HAT_PINS[HAT_DIRS] = {HAT_PIN_UP, HAT_PIN_RIGHT, HAT_PIN_DOWN, HAT_PIN_LEFT};

Debouncer buttonDb[BUTTON_COUNT];
Debouncer hatDb[HAT_DIRS];
Debouncer bondClearDb;

uint16_t currentMask = 0;
uint8_t currentHat = protocol::HAT_CENTERED;

uint32_t bondPressedSinceMs = 0;
bool bondClearFired = false;
bool bondClearPending = false;

// Active low: pressed when the pin reads LOW.
bool readPressed(uint8_t pin) { return digitalRead(pin) == LOW; }

}  // namespace

void begin() {
    for (uint8_t i = 0; i < BUTTON_COUNT; i++) {
        pinMode(BUTTON_PINS[i], INPUT_PULLUP);
        buttonDb[i] = Debouncer(DEBOUNCE_MS);
    }
    if (HAT_ENABLED) {
        for (uint8_t i = 0; i < HAT_DIRS; i++) {
            pinMode(HAT_PINS[i], INPUT_PULLUP);
            hatDb[i] = Debouncer(DEBOUNCE_MS);
        }
    }
    pinMode(BOND_CLEAR_PIN, INPUT_PULLUP);
    bondClearDb = Debouncer(DEBOUNCE_MS);
}

void update(uint32_t nowMs) {
    uint16_t m = 0;
    for (uint8_t i = 0; i < BUTTON_COUNT; i++) {
        if (buttonDb[i].update(readPressed(BUTTON_PINS[i]), nowMs)) m |= (uint16_t)(1u << i);
    }
    currentMask = m;

    if (HAT_ENABLED) {
        bool d[HAT_DIRS];
        for (uint8_t i = 0; i < HAT_DIRS; i++) d[i] = hatDb[i].update(readPressed(HAT_PINS[i]), nowMs);
        currentHat = protocol::hatFromDirections(d[HAT_UP], d[HAT_RIGHT], d[HAT_DOWN], d[HAT_LEFT]);
    }

    bool bondHeld = bondClearDb.update(readPressed(BOND_CLEAR_PIN), nowMs);
    if (!bondHeld) {
        bondPressedSinceMs = 0;
        bondClearFired = false;
    } else if (bondPressedSinceMs == 0) {
        bondPressedSinceMs = nowMs ? nowMs : 1;
    } else if (!bondClearFired && nowMs - bondPressedSinceMs >= BOND_CLEAR_HOLD_MS) {
        bondClearFired = true;
        bondClearPending = true;
    }
}

uint16_t mask() { return currentMask; }

uint8_t hat() { return currentHat; }

bool bondClearRequested() {
    bool r = bondClearPending;
    bondClearPending = false;
    return r;
}

}  // namespace buttons
