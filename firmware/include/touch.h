// AXS5106L capacitive touch controller (Waveshare ESP32-C6-Touch-LCD-1.47).
// Polled over the shared I2C bus; coordinates are mapped to the LCD's
// landscape rotation (LCD_ROTATION), origin top-left, 320x172.
#pragma once

#include <stdint.h>

namespace touch {

constexpr uint8_t MAX_POINTS = 2;

struct Point {
    uint16_t x = 0, y = 0;
};

// Resets the controller and checks it answers on I2C.
bool begin();

// Reads the current touches. Returns false on an I2C error (count is then
// left unchanged); returns true with count = 0 when nothing is touching.
bool read(Point points[MAX_POINTS], uint8_t& count);

}  // namespace touch
