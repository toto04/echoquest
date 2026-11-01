// Time-based debouncer: a new level is accepted only after it has been stable
// for `debounceMs`. Pure logic, unit-tested in test/test_logic.
#pragma once

#include <stdint.h>

class Debouncer {
   public:
    explicit Debouncer(uint32_t debounceMs = 10) : debounceMs_(debounceMs) {}

    // Feed the raw level (true = pressed) and the current time. Returns the debounced level.
    bool update(bool raw, uint32_t nowMs) {
        if (raw != lastRaw_) {
            lastRaw_ = raw;
            changedAtMs_ = nowMs;
        } else if (raw != stable_ && (uint32_t)(nowMs - changedAtMs_) >= debounceMs_) {
            stable_ = raw;
        }
        return stable_;
    }

    bool pressed() const { return stable_; }

   private:
    uint32_t debounceMs_;
    uint32_t changedAtMs_ = 0;
    bool lastRaw_ = false;
    bool stable_ = false;
};
