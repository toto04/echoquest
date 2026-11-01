#include "ble_hid.h"

#include <Arduino.h>
#include <NimBLEDevice.h>
#include <NimBLEHIDDevice.h>
#include <esp_mac.h>
#include <freertos/FreeRTOS.h>
#include <freertos/queue.h>

#include <atomic>

#include "config.h"
#include "log.h"

namespace ble_hid {
namespace {

// The advertisement (31 bytes max) carries flags, appearance, the HID UUID and
// the full name; "<prefix>-XXXX" must stay <= 17 chars to fit.
static_assert(sizeof(DEVICE_NAME_PREFIX) - 1 <= 12, "DEVICE_NAME_PREFIX too long for the advertisement");
char nameBuf[32];

NimBLEServer* server = nullptr;
NimBLEHIDDevice* hid = nullptr;
NimBLECharacteristic* inputChr = nullptr;
NimBLECharacteristic* outputChr = nullptr;
QueueHandle_t hapticQueue = nullptr;

std::atomic<uint16_t> connHandle{BLE_HS_CONN_HANDLE_NONE};
std::atomic<bool> subscribed{false};
std::atomic<uint32_t> dropped{0};

void buildDeviceName() {
    uint8_t mac[6] = {0};
    esp_read_mac(mac, ESP_MAC_BT);
    snprintf(nameBuf, sizeof(nameBuf), "%s-%02X%02X", DEVICE_NAME_PREFIX, mac[4], mac[5]);
}

class ServerCallbacks : public NimBLEServerCallbacks {
    void onConnect(NimBLEServer*, NimBLEConnInfo& info) override {
        connHandle = info.getConnHandle();
        subscribed = false;
        LOG("ble", "connected: %s (handle %u, interval %.2f ms)", info.getAddress().toString().c_str(),
            info.getConnHandle(), info.getConnInterval() * 1.25f);
    }

    void onDisconnect(NimBLEServer*, NimBLEConnInfo& info, int reason) override {
        connHandle = BLE_HS_CONN_HANDLE_NONE;
        subscribed = false;
        LOG("ble", "disconnected: %s reason=0x%03X (%s); advertising again", info.getAddress().toString().c_str(),
            reason, NimBLEUtils::returnCodeToString(reason));
    }

    void onAuthenticationComplete(NimBLEConnInfo& info) override {
        if (!info.isEncrypted()) {
            LOG("ble", "pairing failed, disconnecting %s", info.getAddress().toString().c_str());
            NimBLEDevice::getServer()->disconnect(info.getConnHandle());
            return;
        }
        LOG("ble", "secured: encrypted=yes bonded=%s", info.isBonded() ? "yes" : "no");
    }

    void onConnParamsUpdate(NimBLEConnInfo& info) override {
        LOG("ble", "conn params: interval %.2f ms, latency %u, timeout %u ms", info.getConnInterval() * 1.25f,
            info.getConnLatency(), info.getConnTimeout() * 10);
    }
};

class InputCallbacks : public NimBLECharacteristicCallbacks {
    void onSubscribe(NimBLECharacteristic*, NimBLEConnInfo&, uint16_t subValue) override {
        subscribed = (subValue & 0x0001) != 0;
        LOG("ble", "host %s input notifications", subscribed ? "enabled" : "disabled");
    }
};

// Runs in the NimBLE host task: parse, enqueue, return. No blocking.
class OutputCallbacks : public NimBLECharacteristicCallbacks {
    void onWrite(NimBLECharacteristic* chr, NimBLEConnInfo&) override {
        NimBLEAttValue v = chr->getValue();
        protocol::HapticCommand cmd;
        protocol::ParseResult r = protocol::parseHapticReport(v.data(), v.size(), cmd);
        if (r != protocol::ParseResult::Ok) {
            LOG("ble", "output report rejected (%s, %u bytes)", protocol::toString(r), (unsigned)v.size());
            return;
        }
        if (xQueueSend(hapticQueue, &cmd, 0) != pdTRUE) {
            dropped++;
            LOG("ble", "output report dropped: queue full");
        }
    }
};

ServerCallbacks serverCallbacks;
InputCallbacks inputCallbacks;
OutputCallbacks outputCallbacks;

}  // namespace

bool begin() {
    buildDeviceName();

    hapticQueue = xQueueCreate(HAPTIC_QUEUE_LEN, sizeof(protocol::HapticCommand));
    if (!hapticQueue) return false;

    if (!NimBLEDevice::init(nameBuf)) return false;

    // "Just Works" bonding with LE Secure Connections: no PIN on the host.
    NimBLEDevice::setSecurityAuth(/*bonding=*/true, /*mitm=*/false, /*sc=*/true);
    NimBLEDevice::setSecurityIOCap(BLE_HS_IO_NO_INPUT_OUTPUT);

    server = NimBLEDevice::createServer();
    server->setCallbacks(&serverCallbacks);
    server->advertiseOnDisconnect(true);

    hid = new NimBLEHIDDevice(server);
    hid->setManufacturer(DEVICE_MANUFACTURER);
    hid->setPnp(0x02 /* VID source: USB-IF */, DEVICE_VID, DEVICE_PID, DEVICE_VERSION);
    hid->setHidInfo(0x00 /* country */, 0x01 /* RemoteWake */);
    hid->setReportMap(const_cast<uint8_t*>(protocol::HID_REPORT_DESCRIPTOR),
                      sizeof(protocol::HID_REPORT_DESCRIPTOR));

    inputChr = hid->getInputReport(protocol::REPORT_ID_INPUT);
    inputChr->setCallbacks(&inputCallbacks);
    uint8_t initial[protocol::INPUT_REPORT_SIZE];
    protocol::packInputReport(protocol::InputState{}, initial);
    inputChr->setValue(initial, sizeof(initial));

    outputChr = hid->getOutputReport(protocol::REPORT_ID_HAPTIC);
    outputChr->setCallbacks(&outputCallbacks);

    hid->setBatteryLevel(100);  // TODO: real battery measurement

    NimBLEAdvertising* adv = NimBLEDevice::getAdvertising();
    adv->setAppearance(HID_GAMEPAD);
    adv->addServiceUUID(hid->getHidService()->getUUID());
    adv->setName(nameBuf);
    if (!adv->start()) return false;

    LOG("ble", "advertising as \"%s\" (VID 0x%04X PID 0x%04X, %d bond(s) stored)", nameBuf, DEVICE_VID, DEVICE_PID,
        NimBLEDevice::getNumBonds());
    return true;
}

const char* deviceName() { return nameBuf; }

bool isConnected() { return connHandle != BLE_HS_CONN_HANDLE_NONE; }

bool isReady() { return isConnected() && subscribed; }

bool sendInput(const protocol::InputState& state) {
    if (!isReady()) return false;
    uint8_t buf[protocol::INPUT_REPORT_SIZE];
    protocol::packInputReport(state, buf);
    inputChr->setValue(buf, sizeof(buf));
    return inputChr->notify();
}

bool popHapticCommand(protocol::HapticCommand& out) {
    return hapticQueue && xQueueReceive(hapticQueue, &out, 0) == pdTRUE;
}

uint32_t droppedOutputReports() { return dropped; }

void clearBondsAndRestart() {
    LOG("ble", "clearing %d bond(s) and restarting", NimBLEDevice::getNumBonds());
    NimBLEDevice::deleteAllBonds();
    delay(100);  // let the log flush
    ESP.restart();
    while (true) {
    }
}

}  // namespace ble_hid
