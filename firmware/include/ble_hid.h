// BLE HID-over-GATT gamepad built directly on NimBLE-Arduino, using the
// report descriptor in protocol.h.
//
// Threading: NimBLE callbacks run in the NimBLE host task. Output reports are
// parsed there and pushed onto a FreeRTOS queue; loop() drains it with
// popHapticCommand(). Nothing else is shared across tasks except atomics.
#pragma once

#include <stdint.h>

#include "protocol.h"

namespace ble_hid {

// Initialises NimBLE, the HID/DIS/battery services and starts advertising.
bool begin();

// "<DEVICE_NAME_PREFIX>-XXXX", valid after begin().
const char* deviceName();

// A host is connected (may not be encrypted or subscribed yet).
bool isConnected();

// Connected and the host has enabled notifications on the input report.
bool isReady();

// Sends the input report if isReady(). Returns false if not sent (not ready or
// the BLE stack is out of buffers); the caller should retry next tick.
bool sendInput(const protocol::InputState& state);

// Pops the next received haptic command. Returns false if the queue is empty.
bool popHapticCommand(protocol::HapticCommand& out);

// Output reports dropped because the queue was full (should stay 0).
uint32_t droppedOutputReports();

// Erases all stored pairings and reboots. Hosts must re-pair afterwards.
[[noreturn]] void clearBondsAndRestart();

}  // namespace ble_hid
