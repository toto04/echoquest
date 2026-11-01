// Serial logging over USB-Serial/JTAG, compiled out when ECHO_DEBUG is 0.
// Usage: LOG("ble", "connected to %s", addr);  ->  "[  12.345] ble: connected to ..."
#pragma once

#include <Arduino.h>

#include "config.h"

#if ECHO_DEBUG
#define LOG(tag, fmt, ...) \
    Serial.printf("[%4lu.%03lu] " tag ": " fmt "\n", millis() / 1000, millis() % 1000, ##__VA_ARGS__)
#else
#define LOG(tag, fmt, ...) \
    do {                   \
    } while (0)
#endif

inline void logBegin() {
#if ECHO_DEBUG
    Serial.begin(LOG_BAUD);
    // Don't block the firmware when no USB host is reading the port.
    Serial.setTxTimeoutMs(0);
#endif
}
