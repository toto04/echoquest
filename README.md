# EchoQuest controller

A Bluetooth LE gamepad for an audio-only Unity game for blind children.
It appears to the OS as a standard HID gamepad (HID over GATT) and accepts
vendor-defined haptic commands from the game.

- Project docs (idea, architecture, roadmap, workflow): [`docs/`](docs/README.md)
- Firmware: [`firmware/`](firmware/) (ESP32-C6, PlatformIO, Arduino core 3.x, NimBLE)
- Report layout: [`protocol/hid-report.md`](protocol/hid-report.md)
- Host probe tool: [`tools/hid_probe.py`](tools/hid_probe.py)
- Unity test scene: [`unity/EchoQuestTest/`](unity/EchoQuestTest/), see [`docs/unity-test-scene.md`](docs/unity-test-scene.md#running-it)
- Build, flash and dev notes: [`CLAUDE.md`](CLAUDE.md)

## Supported boards

| PlatformIO env | Board | Inputs | IMU |
|---|---|---|---|
| `waveshare-c6-lcd147` (default) | Waveshare ESP32-C6-Touch-LCD-1.47 | 2 on-screen touch buttons (HID buttons 1, 2) | onboard QMI8658A |
| `esp32-c6-devkitc-1` | Espressif ESP32-C6-DevKitC-1 | 8 GPIO buttons + 4-way hat | `FakeImu` sine sweep |

Both boards send the same HID reports, so hosts and Unity can't tell them apart.

## Quick start

```sh
cd firmware
pio run -t upload -t monitor                          # Waveshare board (default env)
pio run -e esp32-c6-devkitc-1 -t upload -t monitor    # DevKitC-1 ("USB" port)
```

`pio run` downloads the pinned pioarduino platform and libraries on the first run.

## Waveshare ESP32-C6-Touch-LCD-1.47

Everything is onboard; nothing needs wiring. The screen (landscape) shows:

```
EchoQuest-55A2                         CONNECTED   <- name, BLE state
Y   12.3 P   -4.5 R    0.8                         <- yaw / pitch / roll (deg)
[==========haptic bar==========                ]   <- magenta while a haptic command plays
+-----------------+  +-----------------+
|        A        |  |        B        |           <- touch buttons = HID buttons 1 and 2
+-----------------+  +-----------------+
```

The BLE state reads ADVERTISING (no host), PAIRING (connected, input not yet
subscribed) or CONNECTED (reports flowing).

- **Keep the board still for about 1 s after reset**, while the screen says
  `Hold still: calibrating`. The firmware measures the gyro bias then, and retries
  for a few seconds if the board moves.
- **Yaw is 0 at boot.** Pitch and roll are absolute, measured against gravity.
  There's no magnetometer, so yaw has no absolute reference. Whenever the board
  rests (the dot right of the angles turns **green**), the firmware re-learns the
  gyro bias and holds yaw, so it doesn't creep at rest. Some drift can still
  build up while the board is moving.
- **Pins in use:** LCD SPI 1/2/14/15/22, backlight 23, I2C 18/19 (IMU 0x6B, touch 0x63), touch RST/INT 20/21.
  GPIO 7 on the header is the haptic stub output (HIGH while active).
- **Clear pairings:** hold BOOT for 3 s. The serial log prints `bond-clear button down`
  when you press it. If it doesn't, change `BOND_CLEAR_PIN` to 8 in `config.h`
  (see the comment there).

### IMU axes check (once per board design)

The sensor's mounting orientation on the PCB isn't documented, so check the
signs once. Lay the board flat, screen up, text readable:

| Motion | Expected on screen / in `hid_probe.py monitor` |
|---|---|
| Rotate clockwise (seen from above) | yaw increases |
| Lift the top edge of the screen | pitch goes positive |
| Lower the right edge | roll goes positive |

If an axis is wrong, edit `IMU_AXIS_MAP` in `firmware/include/config.h`.
For example, `{-2, +1, +3}` means body X = −sensor Y and body Y = +sensor X.
After changing it, check all three rows again. No re-pairing is needed; the HID layout is unchanged.

## ESP32-C6-DevKitC-1 wiring

Buttons connect between the pin and GND (internal pull-ups).

| Function | GPIO |
|---|---|
| Buttons 1–8 | 0, 1, 2, 3, 10, 11, 18, 19 |
| Hat up / right / down / left | 20, 21, 22, 23 |
| Haptic stub output (HIGH while active) | 5 |
| Haptic stub LED | onboard RGB LED (GPIO 8), magenta |
| Clear pairings (hold 3 s) | 9 (BOOT button) |

With nothing wired, it still works: buttons read released, the hat reads
centered, and `FakeImu` sweeps yaw, pitch and roll as slow sine waves.

## Manual test steps

### 0. Flash and watch the log

```sh
cd firmware
pio run -t upload            # add -e esp32-c6-devkitc-1 for the DevKitC-1
pio device monitor
```

Expected output on the Waveshare board (the name suffix depends on the MAC):

```
[   0.200] boot: EchoQuest controller, protocol 1.0, VID 0x1209 PID 0x0001
[   0.950] touch: AXS5106L ready
[   0.950] ui: LCD ready (320x172), touch ok
[   2.420] imu: calibration 1: bias 0.31 -0.12 0.05 dps (spread 0.9 dps)
[   2.120] boot: IMU: QMI8658 + Mahony
[   2.190] ble: advertising as "EchoQuest-55A2" (VID 0x1209 PID 0x0001, 0 bond(s) stored)
[   3.200] input: btn=0x0000 hat=0 yaw=   0.02 pitch=  0.41 roll=  -0.33 | tx=0 (no host) | ...
```

On the DevKitC-1 there are no `touch`/`ui`/`imu` lines, and the IMU line reads `FakeImu (sine sweep)`.

Keep the monitor open for the steps below.

### 1a. Pair on Windows 10/11

1. Settings → Bluetooth & devices → Add device → Bluetooth.
2. Select `EchoQuest-XXXX`. There's no PIN; pairing completes on its own.
3. The serial log shows `ble: connected`, `ble: secured: encrypted=yes bonded=yes`,
   and `ble: host enabled input notifications`. The screen status turns green
   (CONNECTED), and the `input:` lines show a `tx=` count instead of `(no host)`.
4. Optional: run `joy.cpl` (Game Controllers) → Properties. Buttons 1/2 light up
   when you touch A/B, and X/Y/Z follow yaw/pitch/roll.

### 1b. Pair on macOS

1. System Settings → Bluetooth. `EchoQuest-XXXX` appears under Nearby Devices → Connect.
2. Same serial and screen feedback as on Windows.

Only one host can be connected at a time. To move the controller to another
computer, disconnect it on the first one (or switch that computer's Bluetooth off).

### 2. Check input with hid_probe.py

From the repo root, on the paired computer (see `CLAUDE.md` for venv setup):

```sh
python tools/hid_probe.py list          # should list 1209:0001 usage 0x0001/0x05
python tools/hid_probe.py monitor       # touch A/B -> B1/B2; tilt and turn the board -> yaw/pitch/roll
python tools/hid_probe.py descriptor    # macOS: should say MATCHES protocol/hid-report.md
```

Pressing A and B together should show `B1 B2`; the touch controller reports two points.
Compare a `monitor --raw` line with the hex at the end of a serial `input:` line.
They should match, except that the host line starts with the report ID `01`.

On macOS, if `monitor` can't open the device, give your terminal app
**Input Monitoring** permission (System Settings → Privacy & Security).

### 3. Send a haptic command

```sh
python tools/hid_probe.py haptic 0 200 500      # motor 0, intensity 200, 500 ms
python tools/hid_probe.py haptic all 255 2000   # all motors, 2 s
python tools/hid_probe.py haptic 1 0 0          # stop motor 1
```

### 4. Observe it on the board

For `haptic 0 200 500` the serial log shows:

```
[  42.101] output: haptic report: motor=0 intensity=200 duration=500ms pattern=0
[  42.101] haptic: motor 0 ON  intensity=200 duration=500ms
[  42.601] haptic: motor 0 done
```

Waveshare: the haptic bar fills magenta to about 80% (200/255) for 500 ms, and GPIO 7 goes HIGH.
DevKitC-1: GPIO 5 goes HIGH and the RGB LED glows magenta.
Disconnecting the host during a long command stops all motors (`haptic: all motors STOP`).

## Troubleshooting

- **Garbage or missing axes after a firmware update:** the host cached the old
  descriptor. Bump `DEVICE_PID` (see `CLAUDE.md`), remove the device from
  Bluetooth settings, and pair again.
- **The host won't reconnect, or pairing fails, after reflashing:** hold BOOT for 3 s to
  clear the controller's bonds, remove the device on the host, and pair again.
- **Yaw keeps creeping:** put the board down for a couple of seconds; the dot
  next to the angles should turn green and yaw should stop. If the dot never turns
  green on a still board, check the once-per-second `imu:` log line (bias,
  residual rate, |a|). Drift during motion is expected without a magnetometer;
  recenter yaw on the host.
- **`touch: AXS5106L not responding` or `imu: QMI8658 not responding`:** both share
  I2C on GPIO 18/19. Check that nothing external is connected to those header pins.
- **Screen colors look inverted or red/blue are swapped:** the panel init follows
  Waveshare's driver; see `JD9853_INIT` and the `ips` flag in `firmware/src/ui.cpp`.
- **Serial port missing:** use the board's native USB port. To force download
  mode, hold BOOT and tap RESET.
