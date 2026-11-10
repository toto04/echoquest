// Wiring only: buttons + touch UI + IMU -> input report -> BLE; BLE output report -> haptics.
#include <Arduino.h>

#include "ble_hid.h"
#include "buttons.h"
#include "config.h"
#include "haptics.h"
#include "imu.h"
#include "log.h"
#include "protocol.h"
#include "ui.h"

namespace {

ImuSource& imu = createImu();
StubHapticDriver hapticDriver;
HapticDriver& haptics = hapticDriver;

Orientation imuOrientation;
protocol::InputState lastSent;
bool lastSentValid = false;   // false forces a send on the next tick
bool wasConnected = false;
uint32_t lastReportMs = 0;
uint32_t lastLogMs = 0;
uint32_t reportsSent = 0;

protocol::InputState buildInputState() {
    protocol::InputState s;
    s.buttons = buttons::mask() | ui::buttonMask();
    s.hat = buttons::hat();
    s.axes[protocol::AXIS_YAW] = protocol::degreesToAxis(imuOrientation.yawDeg);
    s.axes[protocol::AXIS_PITCH] = protocol::degreesToAxis(imuOrientation.pitchDeg);
    s.axes[protocol::AXIS_ROLL] = protocol::degreesToAxis(imuOrientation.rollDeg);
    return s;
}

void logInput(const protocol::InputState& s) {
#if ECHO_DEBUG
    uint8_t raw[protocol::INPUT_REPORT_SIZE];
    protocol::packInputReport(s, raw);
    char hex[protocol::INPUT_REPORT_SIZE * 3 + 1];
    for (size_t i = 0; i < sizeof(raw); i++) snprintf(hex + i * 3, 4, "%02X ", raw[i]);
    LOG("input", "btn=0x%04X hat=%u yaw=%7.2f pitch=%6.2f roll=%7.2f | tx=%lu %s| %s", s.buttons, s.hat,
        protocol::axisToDegrees(s.axes[0]), protocol::axisToDegrees(s.axes[1]), protocol::axisToDegrees(s.axes[2]),
        (unsigned long)reportsSent, ble_hid::isReady() ? "" : "(no host) ", hex);
#endif
}

}  // namespace

void setup() {
    logBegin();
    delay(200);  // give the USB-Serial/JTAG host a moment to attach
    LOG("boot", "EchoQuest controller, protocol %u.%u, VID 0x%04X PID 0x%04X", protocol::VERSION_MAJOR,
        protocol::VERSION_MINOR, DEVICE_VID, DEVICE_PID);

    buttons::begin();
    haptics.begin();
    if (!ui::begin()) LOG("boot", "UI started without touch");
    ui::showMessage("Hold still: calibrating");
    if (!imu.begin()) LOG("boot", "IMU %s failed to start", imu.name());
    else LOG("boot", "IMU: %s", imu.name());

    if (!ble_hid::begin()) {
        LOG("boot", "BLE init failed, restarting in 2 s");
        delay(2000);
        ESP.restart();
    }
}

void loop() {
    const uint32_t now = millis();

    buttons::update(now);
    if (buttons::bondClearRequested()) {
        haptics.stopAll();
        ble_hid::clearBondsAndRestart();
    }

    protocol::HapticCommand cmd;
    while (ble_hid::popHapticCommand(cmd)) {
        LOG("output", "haptic report: motor=%u intensity=%u duration=%ums pattern=%u", cmd.motorId, cmd.intensity,
            cmd.durationMs, cmd.patternId);
        haptics.play(cmd, now);
    }
    haptics.update(now);

    // Safety: never leave a motor running without a host to stop it.
    const bool connected = ble_hid::isConnected();
    if (wasConnected && !connected) haptics.stopAll();
    wasConnected = connected;

    imu.read(now, imuOrientation);   // every iteration, so the filter sees every sample

    ui::View view;
    view.deviceName = ble_hid::deviceName();
    view.connected = connected;
    view.ready = ble_hid::isReady();
    view.orientation = imuOrientation;
    view.hapticIntensity = haptics.activeIntensity();
    ui::update(now, view);

    if (now - lastReportMs >= INPUT_REPORT_INTERVAL_MS) {
        lastReportMs = now;
        const protocol::InputState state = buildInputState();
        if (!ble_hid::isReady()) {
            lastSentValid = false;
        } else if ((!lastSentValid || state != lastSent) && ble_hid::sendInput(state)) {
            lastSent = state;
            lastSentValid = true;
            reportsSent++;
        }
        if (now - lastLogMs >= LOG_INPUT_INTERVAL_MS) {
            lastLogMs = now;
            logInput(state);
            reportsSent = 0;
        }
    }

    delay(1);  // yield to the idle task (task watchdog) and the BLE host task
}
