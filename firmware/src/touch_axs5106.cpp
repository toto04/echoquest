// AXS5106L touch reader. Frame layout from Waveshare's esp_lcd_touch_axs5106
// driver: read 14 bytes from register 0x01; byte 1 low nibble = touch count;
// point i at byte 2 + 6i: XH(low nibble) XL YH(low nibble) YL.
#include "config.h"

#if HAS_LCD

#include <Arduino.h>

#include "i2c_bus.h"
#include "log.h"
#include "touch.h"

namespace touch {
namespace {

constexpr uint8_t REG_TOUCH_DATA = 0x01;
constexpr uint8_t FRAME_LEN = 14;
// Native panel is 172 (raw X) x 320 (raw Y), portrait.
constexpr uint16_t PANEL_W = 172;
constexpr uint16_t PANEL_H = 320;

static_assert(LCD_ROTATION == 1 || LCD_ROTATION == 3, "touch mapping supports landscape rotations 1 and 3");

Point mapToScreen(uint16_t rawX, uint16_t rawY) {
    Point p;
    if (LCD_ROTATION == 1) {
        p.x = rawY;
        p.y = rawX;
    } else {
        p.x = (PANEL_H - 1) - rawY;
        p.y = (PANEL_W - 1) - rawX;
    }
    return p;
}

}  // namespace

bool begin() {
    i2c_bus::begin();
    pinMode(TOUCH_PIN_INT, INPUT_PULLUP);
    pinMode(TOUCH_PIN_RST, OUTPUT);
    digitalWrite(TOUCH_PIN_RST, LOW);
    delay(200);
    digitalWrite(TOUCH_PIN_RST, HIGH);
    delay(300);

    uint8_t frame[FRAME_LEN];
    if (!i2c_bus::readRegs(TOUCH_I2C_ADDR, REG_TOUCH_DATA, frame, FRAME_LEN)) {
        LOG("touch", "AXS5106L not responding at 0x%02X", TOUCH_I2C_ADDR);
        return false;
    }
    LOG("touch", "AXS5106L ready");
    return true;
}

bool read(Point points[MAX_POINTS], uint8_t& count) {
    uint8_t frame[FRAME_LEN];
    if (!i2c_bus::readRegs(TOUCH_I2C_ADDR, REG_TOUCH_DATA, frame, FRAME_LEN)) return false;
    uint8_t n = frame[1] & 0x0F;
    if (n > MAX_POINTS) n = MAX_POINTS;
    uint8_t valid = 0;
    for (uint8_t i = 0; i < n; i++) {
        const uint8_t* d = frame + 2 + i * 6;
        uint16_t rawX = ((d[0] & 0x0F) << 8) | d[1];
        uint16_t rawY = ((d[2] & 0x0F) << 8) | d[3];
        if (rawX >= PANEL_W || rawY >= PANEL_H) continue;   // ignore glitches
        points[valid++] = mapToScreen(rawX, rawY);
    }
    count = valid;
    return true;
}

}  // namespace touch

#endif  // HAS_LCD
