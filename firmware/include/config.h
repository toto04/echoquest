// Board configuration: pins, identity, timing. Everything a teammate is likely
// to change lives here. Default pins target the ESP32-C6-DevKitC-1.
//
// Pins to avoid on the DevKitC-1:
//   8      onboard RGB LED (WS2812), used by the haptic stub
//   9      BOOT button (strapping pin); used as the "clear bonds" button below
//   12,13  USB D-/D+ (USB-Serial/JTAG logging and flashing)
//   16,17  UART0 TX/RX
//   24-30  internal SPI flash
//   6,7    reserved for the IMU's I2C bus (SDA/SCL), not used yet
#pragma once

#include <stdint.h>

// ---------------------------------------------------------------------------
// Logging
// ---------------------------------------------------------------------------
// Set by platformio.ini (env:esp32-c6-devkitc-1 = 1, env:release = 0).
#ifndef ECHO_DEBUG
#define ECHO_DEBUG 1
#endif

constexpr uint32_t LOG_BAUD = 115200;               // ignored by USB-Serial/JTAG, kept for UART fallback
constexpr uint32_t LOG_INPUT_INTERVAL_MS = 1000;    // how often the current input report is logged

// ---------------------------------------------------------------------------
// BLE identity
// ---------------------------------------------------------------------------
// Advertised name is "<prefix>-XXXX"; XXXX = last 2 bytes of the BT MAC in hex.
#define DEVICE_NAME_PREFIX "EchoQuest"
#define DEVICE_MANUFACTURER "EchoQuest Team"

// USB-IF style VID/PID reported in the BLE PnP ID characteristic.
// 0x1209 is the pid.codes VID; PIDs 0x0001-0x0010 are its test range for
// private prototypes. Apply for a real pid.codes PID before any distribution.
//
// !!! Bump DEVICE_PID whenever the HID report descriptor changes !!!
// Windows (and to a lesser extent macOS) cache the descriptor per VID/PID.
// If the descriptor changes but the PID does not, the host keeps using the old
// layout and reports decode as garbage. After bumping: remove the device from
// the OS Bluetooth settings and pair again. See protocol/hid-report.md.
constexpr uint16_t DEVICE_VID = 0x1209;
constexpr uint16_t DEVICE_PID = 0x0001;
// Reported as the PnP "product version": 0xMMmm = protocol major.minor.
// Must match PROTOCOL_VERSION in protocol/hid-report.md.
constexpr uint16_t DEVICE_VERSION = 0x0100;

// ---------------------------------------------------------------------------
// Buttons (active low, internal pull-up; wire each button between pin and GND)
// ---------------------------------------------------------------------------
// Button N in the HID report (bit N-1) is BUTTON_PINS[N-1]. Max 16.
constexpr uint8_t BUTTON_PINS[] = {0, 1, 2, 3, 10, 11, 18, 19};
constexpr uint8_t BUTTON_COUNT = sizeof(BUTTON_PINS) / sizeof(BUTTON_PINS[0]);
static_assert(BUTTON_COUNT <= 16, "input report has room for 16 buttons");

// Hat switch from 4 direction buttons (active low, pull-up).
// Set HAT_ENABLED to false if the hardware has no D-pad; the hat then reads centered.
constexpr bool HAT_ENABLED = true;
constexpr uint8_t HAT_PIN_UP = 20;
constexpr uint8_t HAT_PIN_RIGHT = 21;
constexpr uint8_t HAT_PIN_DOWN = 22;
constexpr uint8_t HAT_PIN_LEFT = 23;

// A pin must hold the same level this long before a change is accepted.
constexpr uint32_t DEBOUNCE_MS = 10;

// Hold this pin low for BOND_CLEAR_HOLD_MS to erase all pairings and reboot.
// Useful after a descriptor change. 9 = BOOT button on the DevKitC-1.
constexpr uint8_t BOND_CLEAR_PIN = 9;
constexpr uint32_t BOND_CLEAR_HOLD_MS = 3000;

// ---------------------------------------------------------------------------
// IMU
// ---------------------------------------------------------------------------
// Which ImuSource implementation createImu() returns.
#define IMU_DRIVER_FAKE 0
// #define IMU_DRIVER_xxx 1   // TODO: add once the IMU part is chosen
#define IMU_DRIVER IMU_DRIVER_FAKE

constexpr uint8_t IMU_PIN_SDA = 6;
constexpr uint8_t IMU_PIN_SCL = 7;

// ---------------------------------------------------------------------------
// Haptics
// ---------------------------------------------------------------------------
constexpr uint8_t HAPTIC_MOTOR_COUNT = 2;           // valid motor_id: 0..HAPTIC_MOTOR_COUNT-1, or 0xFF = all
constexpr uint8_t HAPTIC_STUB_PIN = 5;              // stub drives this HIGH while any motor is active
constexpr bool HAPTIC_STUB_USE_RGB_LED = true;      // stub also lights the onboard RGB LED (GPIO 8)
constexpr uint8_t HAPTIC_QUEUE_LEN = 16;            // output reports buffered between BLE task and loop()

// ---------------------------------------------------------------------------
// Timing
// ---------------------------------------------------------------------------
// The input report is rebuilt at this rate and sent only if it changed.
constexpr uint32_t INPUT_REPORT_INTERVAL_MS = 10;   // 100 Hz
