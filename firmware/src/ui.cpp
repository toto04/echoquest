#include "ui.h"

#include "config.h"

#if !HAS_LCD

namespace ui {
bool begin() { return true; }
void update(uint32_t, const View&) {}
void showMessage(const char*) {}
uint16_t buttonMask() { return 0; }
}  // namespace ui

#else

#include <Arduino.h>
#include <Arduino_GFX_Library.h>

#include "log.h"
#include "touch.h"

namespace ui {
namespace {

// JD9853 vendor init sequence, from Waveshare's esp_lcd_jd9853 driver for this
// board. Arduino_ST7789 runs it after a hardware reset, then sends INVON
// (ips = true) and MADCTL for the rotation.
// clang-format off
const uint8_t JD9853_INIT[] = {
    BEGIN_WRITE, WRITE_COMMAND_8, 0x11, END_WRITE,   // sleep out
    DELAY, 120,
    BEGIN_WRITE,
    WRITE_C8_D16, 0xDF, 0x98, 0x53,
    WRITE_C8_D8, 0xB2, 0x23,
    WRITE_COMMAND_8, 0xB7, WRITE_BYTES, 4, 0x00, 0x47, 0x00, 0x6F,
    WRITE_COMMAND_8, 0xBB, WRITE_BYTES, 6, 0x1C, 0x1A, 0x55, 0x73, 0x63, 0xF0,
    WRITE_C8_D16, 0xC0, 0x44, 0xA4,
    WRITE_C8_D8, 0xC1, 0x16,
    WRITE_COMMAND_8, 0xC3, WRITE_BYTES, 8, 0x7D, 0x07, 0x14, 0x06, 0xCF, 0x71, 0x72, 0x77,
    WRITE_COMMAND_8, 0xC4, WRITE_BYTES, 12,
        0x00, 0x00, 0xA0, 0x79, 0x0B, 0x0A, 0x16, 0x79, 0x0B, 0x0A, 0x16, 0x82,
    WRITE_COMMAND_8, 0xC8, WRITE_BYTES, 32,   // gamma
        0x3F, 0x32, 0x29, 0x29, 0x27, 0x2B, 0x27, 0x28, 0x28, 0x26, 0x25, 0x17, 0x12, 0x0D, 0x04, 0x00,
        0x3F, 0x32, 0x29, 0x29, 0x27, 0x2B, 0x27, 0x28, 0x28, 0x26, 0x25, 0x17, 0x12, 0x0D, 0x04, 0x00,
    WRITE_COMMAND_8, 0xD0, WRITE_BYTES, 5, 0x04, 0x06, 0x6B, 0x0F, 0x00,
    WRITE_C8_D16, 0xD7, 0x00, 0x30,
    WRITE_C8_D8, 0xE6, 0x14,
    WRITE_C8_D8, 0xDE, 0x01,
    WRITE_COMMAND_8, 0xB7, WRITE_BYTES, 5, 0x03, 0x13, 0xEF, 0x35, 0x35,
    WRITE_COMMAND_8, 0xC1, WRITE_BYTES, 3, 0x14, 0x15, 0xC0,
    WRITE_C8_D16, 0xC2, 0x06, 0x3A,
    WRITE_C8_D16, 0xC4, 0x72, 0x12,
    WRITE_C8_D8, 0xBE, 0x00,
    WRITE_C8_D8, 0xDE, 0x02,
    WRITE_COMMAND_8, 0xE5, WRITE_BYTES, 3, 0x00, 0x02, 0x00,
    WRITE_COMMAND_8, 0xE5, WRITE_BYTES, 3, 0x01, 0x02, 0x00,
    WRITE_C8_D8, 0xDE, 0x00,
    WRITE_C8_D8, 0x35, 0x00,                   // tearing effect line on
    WRITE_C8_D8, 0x3A, 0x05,                   // RGB565
    WRITE_COMMAND_8, 0x2A, WRITE_BYTES, 4, 0x00, 0x22, 0x00, 0xCD,
    WRITE_COMMAND_8, 0x2B, WRITE_BYTES, 4, 0x00, 0x00, 0x01, 0x3F,
    WRITE_C8_D8, 0xDE, 0x02,
    WRITE_COMMAND_8, 0xE5, WRITE_BYTES, 3, 0x00, 0x02, 0x00,
    WRITE_C8_D8, 0xDE, 0x00,
    WRITE_COMMAND_8, 0x29,                     // display on
    END_WRITE,
};
// clang-format on

// Landscape layout, 320 x 172.
constexpr int16_t W = 320;
constexpr int16_t STATUS_Y = 4;
constexpr int16_t ANGLES_Y = 28;
constexpr int16_t HAPTIC_Y = 50, HAPTIC_H = 8;
constexpr int16_t BTN_Y = 64, BTN_H = 104, BTN_W = 152, BTN_GAP = 8;
constexpr int16_t BTN_A_X = 4, BTN_B_X = BTN_A_X + BTN_W + BTN_GAP;

constexpr uint32_t TEXT_REFRESH_MS = 100;

constexpr uint16_t COLOR_BG = RGB565_BLACK;
constexpr uint16_t COLOR_TEXT = RGB565_WHITE;
constexpr uint16_t COLOR_DIM = RGB565_DARKGREY;
constexpr uint16_t COLOR_A = RGB565_GREEN;
constexpr uint16_t COLOR_B = RGB565_CYAN;
constexpr uint16_t COLOR_HAPTIC = RGB565_MAGENTA;

struct Button {
    int16_t x;
    char label;
    uint16_t color;
    uint8_t hidIndex;
    bool pressed = false;
    bool drawn = false;

    bool contains(const touch::Point& p) const {
        return p.x >= x && p.x < x + BTN_W && p.y >= BTN_Y && p.y < BTN_Y + BTN_H;
    }
};

Arduino_DataBus* bus = nullptr;
Arduino_GFX* gfx = nullptr;
bool touchOk = false;

Button buttons[2] = {
    {BTN_A_X, 'A', COLOR_A, TOUCH_BUTTON_A_INDEX},
    {BTN_B_X, 'B', COLOR_B, TOUCH_BUTTON_B_INDEX},
};
uint16_t mask = 0;

uint32_t lastTouchMs = 0;
uint32_t lastTextMs = 0;
uint8_t touchErrors = 0;
int lastStatus = -1;
uint8_t lastHaptic = 255;
bool messageShown = false;

void drawButton(const Button& b) {
    if (b.pressed) {
        gfx->fillRoundRect(b.x, BTN_Y, BTN_W, BTN_H, 12, b.color);
        gfx->setTextColor(COLOR_BG, b.color);
    } else {
        gfx->fillRoundRect(b.x, BTN_Y, BTN_W, BTN_H, 12, COLOR_BG);
        gfx->drawRoundRect(b.x, BTN_Y, BTN_W, BTN_H, 12, b.color);
        gfx->drawRoundRect(b.x + 1, BTN_Y + 1, BTN_W - 2, BTN_H - 2, 11, b.color);
        gfx->setTextColor(b.color, COLOR_BG);
    }
    gfx->setTextSize(6);
    gfx->setCursor(b.x + BTN_W / 2 - 15, BTN_Y + BTN_H / 2 - 21);   // 6x8 font * 6
    gfx->print(b.label);
    gfx->setTextSize(1);
    gfx->setCursor(b.x + 8, BTN_Y + BTN_H - 14);
    gfx->printf("HID button %u", b.hidIndex + 1);
}

void drawStatus(const View& v) {
    int status = v.ready ? 2 : (v.connected ? 1 : 0);
    if (status == lastStatus) return;
    lastStatus = status;
    static const char* const LABELS[] = {"ADVERTISING", "PAIRING", "CONNECTED"};
    static const uint16_t COLORS[] = {RGB565_YELLOW, RGB565_ORANGE, RGB565_GREEN};
    gfx->fillRect(0, STATUS_Y, W, 16, COLOR_BG);
    gfx->setTextSize(2);
    gfx->setTextColor(COLOR_TEXT, COLOR_BG);
    gfx->setCursor(4, STATUS_Y);
    gfx->print(v.deviceName);
    gfx->setTextSize(1);
    gfx->setTextColor(COLORS[status], COLOR_BG);
    gfx->setCursor(W - 4 - 6 * (int16_t)strlen(LABELS[status]), STATUS_Y + 4);
    gfx->print(LABELS[status]);
}

void drawAngles(const Orientation& o) {
    if (messageShown) {
        gfx->fillRect(0, ANGLES_Y, W, 16, COLOR_BG);
        messageShown = false;
    }
    char line[40];
    snprintf(line, sizeof(line), "Y%7.1f P%6.1f R%7.1f", o.yawDeg, o.pitchDeg, o.rollDeg);
    gfx->setTextSize(2);
    gfx->setTextColor(COLOR_TEXT, COLOR_BG);   // opaque background: no flicker
    gfx->setCursor(4, ANGLES_Y);
    gfx->print(line);
    // Green dot: the IMU sees the board at rest (gyro bias being refined, yaw held).
    gfx->fillCircle(W - 8, ANGLES_Y + 7, 4, o.still ? RGB565_GREEN : COLOR_DIM);
}

void drawHaptic(uint8_t intensity) {
    if (intensity == lastHaptic) return;
    lastHaptic = intensity;
    int16_t w = (int16_t)((int32_t)(W - 8) * intensity / 255);
    gfx->fillRect(4, HAPTIC_Y, W - 8, HAPTIC_H, COLOR_DIM);
    if (w > 0) gfx->fillRect(4, HAPTIC_Y, w, HAPTIC_H, COLOR_HAPTIC);
}

void pollTouch() {
    touch::Point pts[touch::MAX_POINTS];
    uint8_t n = 0;
    if (!touch::read(pts, n)) {
        // Keep the last state through a glitch; release after repeated failures.
        if (++touchErrors < 3) return;
        n = 0;
    } else {
        touchErrors = 0;
    }
    uint16_t newMask = 0;
    for (Button& b : buttons) {
        bool pressed = false;
        for (uint8_t i = 0; i < n; i++) pressed |= b.contains(pts[i]);
        if (pressed) newMask |= (uint16_t)(1u << b.hidIndex);
        if (pressed != b.pressed || !b.drawn) {
            b.pressed = pressed;
            b.drawn = true;
            drawButton(b);
            LOG("ui", "button %c (HID %u) %s", b.label, b.hidIndex + 1, pressed ? "down" : "up");
        }
    }
    mask = newMask;
}

}  // namespace

bool begin() {
    pinMode(LCD_PIN_BL, OUTPUT);
    analogWrite(LCD_PIN_BL, 0);   // dark until the first frame is drawn

    bus = new Arduino_HWSPI(LCD_PIN_DC, LCD_PIN_CS, LCD_PIN_SCK, LCD_PIN_MOSI, GFX_NOT_DEFINED);
    gfx = new Arduino_ST7789(bus, LCD_PIN_RST, LCD_ROTATION, /*ips=*/true, 172, 320,
                             /*col_offset1=*/34, /*row_offset1=*/0, /*col_offset2=*/34, /*row_offset2=*/0,
                             JD9853_INIT, sizeof(JD9853_INIT));
    if (!gfx->begin(LCD_SPI_HZ)) {
        LOG("ui", "LCD init failed");
        return false;
    }
    gfx->fillScreen(COLOR_BG);
    for (Button& b : buttons) {
        drawButton(b);
        b.drawn = true;
    }
    drawHaptic(0);
    analogWrite(LCD_PIN_BL, LCD_BACKLIGHT);

    touchOk = touch::begin();
    LOG("ui", "LCD ready (%dx%d), touch %s", gfx->width(), gfx->height(), touchOk ? "ok" : "NOT FOUND");
    return touchOk;
}

void update(uint32_t nowMs, const View& view) {
    if (!gfx) return;
    if (touchOk && nowMs - lastTouchMs >= TOUCH_POLL_MS) {
        lastTouchMs = nowMs;
        pollTouch();
    }
    drawStatus(view);
    drawHaptic(view.hapticIntensity);
    if (nowMs - lastTextMs >= TEXT_REFRESH_MS) {
        lastTextMs = nowMs;
        drawAngles(view.orientation);
    }
}

void showMessage(const char* text) {
    if (!gfx) return;
    gfx->fillRect(0, ANGLES_Y, W, 16, COLOR_BG);
    gfx->setTextSize(2);
    gfx->setTextColor(RGB565_YELLOW, COLOR_BG);
    gfx->setCursor(4, ANGLES_Y);
    gfx->print(text);
    messageShown = true;
}

uint16_t buttonMask() { return mask; }

}  // namespace ui

#endif  // HAS_LCD
