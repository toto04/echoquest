// Shared I2C bus (Wire on I2C_PIN_SDA/SCL). Used only from the loop task, so
// no locking. Register reads end the write phase with a STOP before reading:
// the AXS5106L touch controller is unreliable with a repeated start.
#pragma once

#include <Arduino.h>
#include <Wire.h>

#include "config.h"

namespace i2c_bus {

inline void begin() {
    static bool started = false;
    if (started) return;
    started = true;
    Wire.begin(I2C_PIN_SDA, I2C_PIN_SCL, 400000);
    Wire.setTimeOut(20);
}

inline bool readRegs(uint8_t addr, uint8_t reg, uint8_t* buf, size_t len) {
    Wire.beginTransmission(addr);
    Wire.write(reg);
    if (Wire.endTransmission(true) != 0) return false;
    if (Wire.requestFrom(addr, len) != len) return false;
    for (size_t i = 0; i < len; i++) buf[i] = (uint8_t)Wire.read();
    return true;
}

inline bool writeReg(uint8_t addr, uint8_t reg, uint8_t value) {
    Wire.beginTransmission(addr);
    Wire.write(reg);
    Wire.write(value);
    return Wire.endTransmission(true) == 0;
}

}  // namespace i2c_bus
