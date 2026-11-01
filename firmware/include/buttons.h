// Debounced GPIO buttons and hat switch. Pins come from config.h.
#pragma once

#include <stdint.h>

namespace buttons {

void begin();

// Call every loop iteration; samples all pins and updates debounced state.
void update(uint32_t nowMs);

// Bit i = button i+1 pressed (debounced).
uint16_t mask();

// Hat value per protocol.h (0 = centered, 1..8 = N..NW).
uint8_t hat();

// True once BOND_CLEAR_PIN has been held for BOND_CLEAR_HOLD_MS. Fires once per hold.
bool bondClearRequested();

}  // namespace buttons
