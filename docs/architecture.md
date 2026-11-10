# Architecture

How the EchoQuest pieces fit together, and why they are built this way. For the
byte-level report format see [`protocol/hid-report.md`](../protocol/hid-report.md);
for flashing and hardware tests see the [root README](../README.md).

## System overview

```
 ┌──────────────────────────── Controller (ESP32-C6) ───────────────────────────┐
 │                                                                               │
 │  Buttons / touch ──┐                                                          │
 │                    ├──► input state ──► Input report ID 1 (15 B) ──┐          │
 │  IMU ─► Mahony ────┘    (buttons, hat,  every 10 ms, sent if it     │          │
 │         filter          yaw/pitch/roll)  changed                    │          │
 │                                                                     ▼          │
 │                                                       NimBLE HID-over-GATT     │
 │  Haptic driver ◄── queue ◄── parse ◄── Output report ID 2 (8 B) ◄──┘          │
 │  (stub today)                                                                  │
 └────────────────────────────────────────────┬──────────────────────────────────┘
                                              │ Bluetooth LE (bonded, encrypted)
 ┌────────────────────────────────────────────▼──────────────────────────────────┐
 │  Host OS (Windows / macOS): standard HID gamepad, no driver                   │
 │        │                                         │                            │
 │        ▼                                         ▼                            │
 │  Unity (Input System, game)              tools/hid_probe.py (hidapi)          │
 │  reads input, sends haptics              list / monitor / haptic / descriptor │
 └───────────────────────────────────────────────────────────────────────────────┘
```

The **contract** between the controller and everything on the host is the HID
report descriptor plus the two reports. As long as both sides follow
[`protocol/hid-report.md`](../protocol/hid-report.md), firmware and game can
change independently.

## The controller

### Hardware

The target MCU is the **Espressif ESP32-C6** (RISC-V, BLE 5, Wi-Fi 6, 802.15.4).
Two development boards are supported today. Both produce identical HID reports,
so the host can't tell them apart.

| Board | PlatformIO env | Inputs | Orientation | Extras |
|---|---|---|---|---|
| **Waveshare ESP32-C6-Touch-LCD-1.47** (current test unit) | `waveshare-c6-lcd147` (default) | 2 on-screen touch buttons → HID buttons 1–2 | Onboard **QMI8658A** 6-axis IMU (I2C) | 1.47" JD9853 LCD (172×320) showing BLE state, live angles and a haptic bar |
| **Espressif ESP32-C6-DevKitC-1** | `esp32-c6-devkitc-1` | 8 GPIO buttons + 4-way hat | `FakeImu` sine sweep (no sensor) | RGB LED as a haptic stand-in |

The Waveshare board is a self-contained test unit: plug in USB, pair, and you
have buttons, real orientation and visible haptic feedback, with no wiring.
Neither board is the final controller; the form factor is an
[open question](vision.md#open-questions).

### Firmware

PlatformIO project in [`firmware/`](../firmware/), Arduino core 3.x on the
pinned **pioarduino** platform (55.03.312-1 → Arduino 3.3.12 / ESP-IDF 5.5.5),
**NimBLE-Arduino 2.5.2** for Bluetooth, Arduino_GFX for the LCD.

One module per concern; `main.cpp` only wires them together:

| Module | Responsibility |
|---|---|
| `include/config.h` | **All** board-specific settings: pins, button count, VID/PID, timing, IMU axis map. Per-board profiles selected by `-DBOARD=` |
| `include/protocol.h` | HID report descriptor, report IDs, pack/parse functions. Pure C++, unit-tested on the host |
| `src/ble_hid.cpp` | HID-over-GATT service on NimBLE: advertising, Just Works bonding with LE Secure Connections, input notifications, output-report queue |
| `src/buttons.cpp` + `include/debouncer.h` | Debounced GPIO buttons and hat; hold BOOT 3 s to erase pairings |
| `src/imu.cpp`, `src/imu_qmi8658.cpp` | `ImuSource` interface; `FakeImu`; QMI8658 driver with gyro-bias calibration at boot |
| `include/gyro_bias.h` | Stillness detection and continuous gyro-bias tracking (holds yaw at rest). Pure, unit-tested |
| `include/orientation_filter.h` | Mahony 6-axis sensor fusion, axis remapping, protocol sign conventions. Pure, unit-tested |
| `src/haptics.cpp` | `HapticDriver` interface and a stub that logs, drives a GPIO and shows activity |
| `src/ui.cpp`, `src/touch_axs5106.cpp` | LCD status screen and two touch buttons (only on boards with an LCD; no-ops otherwise) |
| `include/log.h` | `LOG()` macro over USB-Serial/JTAG, compiled out in the `release` env |

Threading rule: NimBLE callbacks run in the BLE host task and only parse and
enqueue. Everything else (I2C, LCD, haptics) runs in the Arduino `loop()` task.

### Orientation

- The IMU's gyro and accelerometer are fused with a **Mahony filter** into a
  quaternion, then converted to yaw/pitch/roll in the protocol's conventions:
  yaw + = turning right, pitch + = nose up, roll + = right side down.
- **Pitch and roll are absolute** (referenced to gravity). **Yaw is relative**:
  0 at power-on, with no absolute reference, because a 6-axis IMU has no magnetometer.
  Any gyro bias error integrates straight into yaw (0.5 °/s of bias = 30° per
  minute), so `include/gyro_bias.h` detects when the controller is at rest,
  re-learns the bias there and stops integrating the gyro. Yaw then holds still
  at rest; it can still drift slowly during motion.
  The game must recenter yaw itself if it needs a stable heading. Whether this
  is acceptable depends on game design (see [vision.md](vision.md#open-questions)).
- Gyro bias is measured during the first ~1 s after boot, so the controller should
  be still then.
- The mapping from the sensor's axes to the controller's body frame is a
  one-line table (`IMU_AXIS_MAP` in `config.h`), checked once per board design.

### Bluetooth and identity

- Standard **HID over GATT** gamepad (Appearance 0x03C4), name `EchoQuest-XXXX`
  (from the MAC), VID `0x1209` (pid.codes) / PID `0x0001` (test range).
- **Just Works** pairing with bonding and LE Secure Connections: no PIN, and it
  reconnects automatically after the first pairing.
- Hosts cache the report descriptor per VID/PID, so **any descriptor change
  requires a PID bump and re-pairing**. The procedure is in
  [`CLAUDE.md`](../CLAUDE.md) and [`protocol/hid-report.md`](../protocol/hid-report.md).

## The protocol (summary)

| Report | Direction | Payload | Content |
|---|---|---|---|
| ID 1, input | controller → host | 15 bytes | 16 buttons (bitfield), hat (4 bits), 6 × int16 axes: yaw, pitch, roll, 3 spare |
| ID 2, output | host → controller | 8 bytes | haptic command: motor_id, intensity, duration_ms (u16), pattern_id, 3 reserved |

All angles share one scale: `degrees = value × 180 / 32767`. Host buffers start
with the report ID byte, so host offset = payload offset + 1. Full tables,
semantics and the 98-byte descriptor: [`protocol/hid-report.md`](../protocol/hid-report.md).

## Host side

- **`tools/hid_probe.py`** (Python + hidapi): lists the controller, decodes input
  live, sends haptic commands, compares the descriptor the host sees with the
  documented one, and runs an offline self-test. It lets us test the controller
  without Unity.
- **Unity** (not started yet): will read the controller through the Unity Input
  System and send haptic output reports. The first step is a test scene, see
  [unity-test-scene.md](unity-test-scene.md). The Unity C# report layout must
  mirror `protocol/hid-report.md`.

## Design decisions

Recorded so they aren't relitigated without a reason. Newest last.

| # | Decision | Why | Alternatives considered |
|---|---|---|---|
| 1 | ESP32-C6 + Arduino core 3.x via a **pinned** pioarduino release | The official PlatformIO espressif32 platform doesn't support Arduino on the C6. Pinning a release makes builds reproducible from a clean checkout | ESP-IDF directly (more boilerplate, slower to iterate with AI); "stable" platform tag (non-reproducible) |
| 2 | **Standard BLE HID gamepad** (HID over GATT) | Works on Windows/macOS with no driver or companion app; Unity sees it as a normal input device | Custom GATT service (needs host-side BLE code in Unity on every platform) |
| 3 | HID service written **directly on NimBLE-Arduino**, not ESP32-BLE-Gamepad | We need a vendor-defined **output report with a callback** for haptics and full control over the descriptor. Chosen for reliability over convenience | ESP32-BLE-Gamepad library (fixed descriptor, no suitable output-report hook) |
| 4 | Orientation as **16-bit signed axes** with one shared degree scale | Plenty of resolution (≈0.0055°/LSB), standard HID axis type, trivial to decode | Floats in a vendor report (not a standard gamepad axis) |
| 5 | **Vendor output report** (usage page 0xFF00) for haptics, 8 bytes with reserved fields | Room for motor selection, duration and patterns without changing the descriptor later | HID PID (force feedback) usage page (complex, poorly supported over BLE) |
| 6 | **Haptics stop on disconnect** | A controller must never keep vibrating after the game is gone | — |
| 7 | **Interfaces** for IMU (`ImuSource`) and haptics (`HapticDriver`) with stub/fake implementations | Develop and test the full link before the final sensors and motors are chosen | — |
| 8 | **Per-board profiles** in one `config.h`, one PlatformIO env per board | Several dev boards now, a custom PCB later, with identical HID output | Separate firmware per board |
| 9 | **Mahony 6-axis filter**, no magnetometer; yaw relative to boot | The test board has a 6-axis IMU; magnetometers are unreliable indoors near electronics. Revisit when the game's use of yaw is known | Madgwick (similar); 9-axis IMU (absolute heading, needs calibration) |
| 10 | Pure logic in header-only files with **native unit tests** | Descriptor sizes, packing, parsing, debouncing and orientation signs are checked on every change without hardware | Testing only on the board |
| 11 | **Python probe tool** independent of Unity | Isolates "controller problem" from "game problem"; gives AI agents a scriptable check | Testing only inside Unity |
