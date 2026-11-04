# Roadmap and status

Last updated: 2026-11-04.

## Where we are

### Done

| Area | What exists | How it's verified |
|---|---|---|
| Firmware: BLE HID gamepad | Bonded HID-over-GATT gamepad on NimBLE, input report (buttons, hat, yaw/pitch/roll), vendor haptic output report, haptics stop on disconnect, bond clearing | Builds with no warnings; native unit tests; **confirmed working on hardware** (pairing, `hid_probe.py monitor` and `haptic`) |
| Protocol v1.0 | [`protocol/hid-report.md`](../protocol/hid-report.md), 98-byte descriptor, PID `0x0001` | Native tests check that descriptor bit counts match the report sizes; probe self-test |
| Host probe tool | `tools/hid_probe.py`: list, monitor, haptic, descriptor, selftest | Self-test passes; used against real hardware |
| DevKitC-1 profile | 8 GPIO buttons, 4-way hat, `FakeImu`, RGB LED haptic stand-in | Hardware-tested |
| Waveshare ESP32-C6-Touch-LCD-1.47 profile | Real QMI8658A IMU with Mahony fusion, LCD status screen, 2 touch buttons, default env | Builds; orientation signs unit-tested. Hardware checklist in the [README](../README.md#imu-axes-check-once-per-board-design) |

### Known gaps and things to check on hardware

- **Waveshare board checklist** (from the README): IMU axis signs
  (`IMU_AXIS_MAP`), touch-to-screen mapping, LCD colours and orientation, and
  whether the BOOT button is GPIO 9 or 8.
- **Yaw drifts** (no magnetometer). Acceptable for now; revisit when the game's
  use of yaw is known.
- **Haptics are a stub.** No motor driver yet; the stub logs, toggles a GPIO and
  shows a bar on the LCD. Pattern IDs 1–255 are reserved but play as constant.
- **VID/PID are in the pid.codes test range.** A real PID is needed before any
  distribution.
- **Repo `.gitignore` is not ready for a Unity project.** It ignores `*.meta`
  and `**/Packages/*`, which Unity projects must commit, and its Unity rules are
  anchored to the repo root. Must be fixed before the Unity project is
  committed (see [unity-test-scene.md](unity-test-scene.md#repository-setup)).

## Next milestones

### 1. Unity test scene (next)

A small, playful Unity scene that proves the controller works end to end
inside Unity: an object that rotates with the controller's orientation and
reacts to the two buttons. Details: [unity-test-scene.md](unity-test-scene.md).

Exit criteria:
- The controller is detected by the Unity Input System on macOS (and ideally Windows).
- Buttons 1–2 and yaw/pitch/roll are read with correct signs and scale.
- Findings about how Unity exposes the device are written down, since they
  shape the real game's input layer.

### 2. Haptics from Unity

Send output report ID 2 from Unity (through the Input System if it can send
HID output reports on our platforms, otherwise through a small native or
hidapi-based plugin). Exit: a Unity button press makes the controller's haptic
stub fire, as `hid_probe.py haptic` does today.

### 3. Unity input layer for the game

A reusable C# module that mirrors `protocol/hid-report.md`: device detection,
decoded state (buttons, orientation in degrees), yaw recentering, haptic
commands, and connection/disconnection events that the game can turn into sound.

### 4. Real haptics hardware

Choose motors and a driver, implement a `HapticDriver` for them, and define
the first vibration patterns (pattern IDs 1+). Keep the disconnect failsafe.

### 5. Game prototype

Once the game design questions in [vision.md](vision.md#open-questions) are
answered: a first playable audio-only prototype.

### Later

- Controller form factor, battery and power management, and how battery state
  is shown to a blind player.
- A custom PCB as a new `BOARD_*` profile.
- Possibly a magnetometer for absolute heading, if the game needs it.
- A real USB VID/PID allocation.
