// Board configuration: pins, identity, timing. Everything a teammate is likely
// to change lives here.
//
// Two board profiles, selected by -DBOARD=... in platformio.ini:
//   BOARD_DEVKITC1             Espressif ESP32-C6-DevKitC-1, external buttons, FakeImu
//   BOARD_WAVESHARE_C6_LCD147  Waveshare ESP32-C6-Touch-LCD-1.47: onboard QMI8658 IMU,
//                              JD9853 LCD with two on-screen buttons (AXS5106L touch)
// Shared settings come first; per-board pins are in the #if blocks below.
#pragma once

#include <stdint.h>

#include <array>

#define BOARD_DEVKITC1 1
#define BOARD_WAVESHARE_C6_LCD147 2
#ifndef BOARD
#define BOARD BOARD_DEVKITC1
#endif

constexpr uint8_t NO_PIN = 0xFF;   // for optional pins

// Logging
// Set by platformio.ini (1 in the debug envs, 0 in env:release).
#ifndef ECHO_DEBUG
#define ECHO_DEBUG 1
#endif

constexpr uint32_t LOG_BAUD = 115200;               // ignored by USB-Serial/JTAG, kept for UART fallback
constexpr uint32_t LOG_INPUT_INTERVAL_MS = 1000;    // how often the current input report is logged

// BLE identity
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

// Shared tuning
// A button pin must hold the same level this long before a change is accepted.
constexpr uint32_t DEBOUNCE_MS = 10;
// Hold BOND_CLEAR_PIN low this long to erase all pairings and reboot.
constexpr uint32_t BOND_CLEAR_HOLD_MS = 3000;

constexpr uint8_t HAPTIC_MOTOR_COUNT = 2;           // valid motor_id: 0..HAPTIC_MOTOR_COUNT-1, or 0xFF = all
constexpr uint8_t HAPTIC_QUEUE_LEN = 16;            // output reports buffered between BLE task and loop()

// The input report is rebuilt at this rate and sent only if it changed.
constexpr uint32_t INPUT_REPORT_INTERVAL_MS = 10;   // 100 Hz

// IMU driver selection, see createImu() in src/imu.cpp.
#define IMU_DRIVER_FAKE 0       // slow sine sweep, no hardware
#define IMU_DRIVER_QMI8658 1    // QMI8658(A) 6-axis over I2C, fused with a Mahony filter

#if BOARD == BOARD_DEVKITC1
// Espressif ESP32-C6-DevKitC-1
// Pins to avoid: 8 (onboard RGB LED), 9 (BOOT), 12/13 (USB), 16/17 (UART0),
// 24-30 (flash). 6/7 are reserved for an external IMU's I2C bus.

// Buttons: active low, internal pull-up; wire each between pin and GND.
// Button N in the HID report (bit N-1) is BUTTON_PINS[N-1]. Max 16.
constexpr std::array<uint8_t, 8> BUTTON_PINS = {0, 1, 2, 3, 10, 11, 18, 19};

// Hat switch from 4 direction buttons (active low, pull-up).
// Set HAT_ENABLED to false if the hardware has no D-pad; the hat then reads centered.
constexpr bool HAT_ENABLED = true;
constexpr uint8_t HAT_PIN_UP = 20;
constexpr uint8_t HAT_PIN_RIGHT = 21;
constexpr uint8_t HAT_PIN_DOWN = 22;
constexpr uint8_t HAT_PIN_LEFT = 23;

constexpr uint8_t BOND_CLEAR_PIN = 9;               // BOOT button

#define IMU_DRIVER IMU_DRIVER_FAKE
constexpr uint8_t I2C_PIN_SDA = 6;
constexpr uint8_t I2C_PIN_SCL = 7;

constexpr uint8_t HAPTIC_STUB_PIN = 5;              // stub drives this HIGH while any motor is active
constexpr bool HAPTIC_STUB_USE_RGB_LED = true;      // stub also lights the onboard RGB LED (GPIO 8)

#define HAS_LCD 0

#elif BOARD == BOARD_WAVESHARE_C6_LCD147
// Waveshare ESP32-C6-Touch-LCD-1.47 (ESP32-C6FH8, 8 MB flash)
// https://docs.waveshare.com/ESP32-C6-Touch-LCD-1.47
// Onboard: LCD SPI 1/2/14/15/22, backlight 23; I2C 18/19 shared by IMU and
// touch; touch RST 20 / INT 21; IMU INT 5/6; SD card 3/4 (shares LCD SPI);
// battery ADC 0. GPIO 7 is free on the header. No RGB LED.

// No physical buttons by default; the two on-screen buttons are the inputs.
// Add header GPIOs here (e.g. {7}) to wire external buttons.
constexpr std::array<uint8_t, 0> BUTTON_PINS = {};

constexpr bool HAT_ENABLED = false;
constexpr uint8_t HAT_PIN_UP = NO_PIN;
constexpr uint8_t HAT_PIN_RIGHT = NO_PIN;
constexpr uint8_t HAT_PIN_DOWN = NO_PIN;
constexpr uint8_t HAT_PIN_LEFT = NO_PIN;

// The ESP32-C6 boot strap pin is GPIO 9. Waveshare's page says "GPIO8"; if
// pressing BOOT doesn't log "bond-clear button down", try 8.
constexpr uint8_t BOND_CLEAR_PIN = 9;

#define IMU_DRIVER IMU_DRIVER_QMI8658
constexpr uint8_t I2C_PIN_SDA = 18;
constexpr uint8_t I2C_PIN_SCL = 19;

constexpr uint8_t HAPTIC_STUB_PIN = 7;              // header pin; the LCD also shows haptic activity
constexpr bool HAPTIC_STUB_USE_RGB_LED = false;     // this board has no RGB LED

#define HAS_LCD 1
// LCD: JD9853, 172x320 IPS, 4-wire SPI.
constexpr uint8_t LCD_PIN_SCK = 1;
constexpr uint8_t LCD_PIN_MOSI = 2;
constexpr uint8_t LCD_PIN_CS = 14;
constexpr uint8_t LCD_PIN_DC = 15;
constexpr uint8_t LCD_PIN_RST = 22;
constexpr uint8_t LCD_PIN_BL = 23;
constexpr uint32_t LCD_SPI_HZ = 80000000;
constexpr uint8_t LCD_BACKLIGHT = 200;              // 0-255
// 1 or 3: landscape (320x172). 3 flips it 180 degrees. Touch mapping follows.
constexpr uint8_t LCD_ROTATION = 1;

// Touch: AXS5106L on the shared I2C bus.
constexpr uint8_t TOUCH_I2C_ADDR = 0x63;
constexpr uint8_t TOUCH_PIN_RST = 20;
constexpr uint8_t TOUCH_PIN_INT = 21;
constexpr uint32_t TOUCH_POLL_MS = 20;

// HID buttons driven by the on-screen buttons (0 = button 1).
constexpr uint8_t TOUCH_BUTTON_A_INDEX = 0;
constexpr uint8_t TOUCH_BUTTON_B_INDEX = 1;

#else
#error "Unknown BOARD in config.h"
#endif

constexpr uint8_t BUTTON_COUNT = BUTTON_PINS.size();
static_assert(BUTTON_COUNT <= 16, "input report has room for 16 buttons");

// QMI8658 IMU (used when IMU_DRIVER == IMU_DRIVER_QMI8658)
constexpr uint8_t QMI8658_I2C_ADDR = 0x6B;
// Maps sensor axes onto the controller's body frame, which is defined with
// the board lying flat, screen up, in the LCD's landscape orientation:
//   body X = forward (toward the top edge of the screen)
//   body Y = left
//   body Z = up (out of the screen)
// Entry i is the sensor axis feeding body axis i: +1/+2/+3 = sensor X/Y/Z,
// negative = inverted. Check with the "IMU axes" steps in README.md and fix
// here if, say, tilting the top edge up doesn't give positive pitch.
constexpr int8_t IMU_AXIS_MAP[3] = {+1, +2, +3};
// Gyro bias is measured at boot over this many samples (~0.9 s) once the board
// is still; up to IMU_CALIBRATION_ATTEMPTS windows are tried. After boot the
// bias keeps being re-learned whenever the board rests (include/gyro_bias.h).
constexpr uint16_t IMU_CALIBRATION_SAMPLES = 200;
constexpr uint8_t IMU_CALIBRATION_ATTEMPTS = 6;
// Max gyro spread (max - min, any axis) accepted for a still calibration window.
constexpr float IMU_CALIBRATION_MAX_SPREAD_DPS = 3.0f;
// Mahony filter gain: higher = pitch/roll follow the accelerometer faster
// (less drift, more sensitive to shaking).
constexpr float IMU_FILTER_KP = 1.0f;
