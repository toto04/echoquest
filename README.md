# EchoQuest controller

A Bluetooth LE gamepad for an audio-only Unity game for blind children.
It appears to the OS as a standard HID gamepad (HID over GATT) and accepts
vendor-defined haptic commands from the game.

- Firmware: [`firmware/`](firmware/) (ESP32-C6, PlatformIO, Arduino core 3.x, NimBLE)
- Report layout: [`protocol/hid-report.md`](protocol/hid-report.md)
- Host probe tool: [`tools/hid_probe.py`](tools/hid_probe.py)
- Build, flash and dev notes: [`CLAUDE.md`](CLAUDE.md)

## Quick start

```sh
cd firmware
pio run -t upload -t monitor     # plug into the DevKitC-1 port labelled "USB"
```

`pio run` downloads the pinned pioarduino platform and NimBLE-Arduino on the first run.

## Default wiring (ESP32-C6-DevKitC-1)

All pins live in `firmware/include/config.h`. Buttons connect between the pin
and GND; internal pull-ups are used.

| Function | GPIO |
|---|---|
| Buttons 1–8 | 0, 1, 2, 3, 10, 11, 18, 19 |
| Hat up / right / down / left | 20, 21, 22, 23 |
| Haptic stub output (HIGH while active) | 5 |
| Haptic stub LED | onboard RGB LED (GPIO 8), magenta |
| Clear pairings (hold 3 s) | 9 (BOOT button) |
| IMU I2C SDA / SCL (reserved) | 6 / 7 |

With nothing wired, the controller still works: buttons read released, the
hat reads centered, and `FakeImu` sweeps yaw, pitch and roll as slow sine waves.

## Manual test steps

### 0. Flash and watch the log

```sh
cd firmware
pio run -t upload
pio device monitor
```

Expected output (the name suffix depends on the board's MAC):

```
[   0.200] boot: EchoQuest controller, protocol 1.0, VID 0x1209 PID 0x0001
[   0.201] boot: IMU: FakeImu (sine sweep)
[   0.260] ble: advertising as "EchoQuest-55A2" (VID 0x1209 PID 0x0001, 0 bond(s) stored)
[   1.200] input: btn=0x0000 hat=0 yaw=  56.10 pitch= 36.33 roll=  17.07 | tx=0 (no host) | 00 00 00 ...
```

Keep the monitor open for the steps below.

### 1a. Pair on Windows 10/11

1. Settings → Bluetooth & devices → Add device → Bluetooth.
2. Select `EchoQuest-XXXX`. There's no PIN; pairing completes on its own.
3. The serial log shows `ble: connected`, `ble: secured: encrypted=yes bonded=yes`,
   and `ble: host enabled input notifications`. The `input:` lines change from
   `(no host)` to a `tx=` count of about 100/s.
4. Optional: run `joy.cpl` (Game Controllers) → Properties. Buttons light up,
   the POV hat follows the D-pad, and the X/Y/Z axes sweep slowly.

### 1b. Pair on macOS

1. System Settings → Bluetooth. `EchoQuest-XXXX` appears under Nearby Devices → Connect.
2. The serial log shows the same three `ble:` lines as on Windows.

Only one host can be connected at a time. To move the controller to another
computer, disconnect it on the first one (or switch that computer's Bluetooth off).

### 2. Check input with hid_probe.py

From the repo root, on the paired computer:

```sh
pip install -r tools/requirements.txt
python tools/hid_probe.py list          # should list 1209:0001 usage 0x0001/0x05
python tools/hid_probe.py monitor       # yaw/pitch/roll sweep; press buttons to see B1..B8, hat N..NW
python tools/hid_probe.py descriptor    # macOS: should say MATCHES protocol/hid-report.md
```

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

GPIO 5 goes HIGH and the onboard RGB LED glows magenta for 500 ms.
Disconnecting the host during a long command stops all motors (`haptic: all motors STOP`).

## Troubleshooting

- **Garbage or missing axes after a firmware update:** the host cached the old
  descriptor. Bump `DEVICE_PID` (see `CLAUDE.md`), remove the device from
  Bluetooth settings, and pair again.
- **The host won't reconnect, or pairing fails, after reflashing:** hold BOOT for 3 s to
  clear the controller's bonds, remove the device on the host, and pair again.
- **Serial port missing:** use the "USB" port (not "UART"). To force download mode, hold BOOT and tap RESET.
