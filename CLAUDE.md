# EchoQuest

An audio-only Unity game for blind children, played with a custom BLE gamepad.
This repo contains the controller firmware, the HID protocol spec, and host tools.

## Layout

- `firmware/`: PlatformIO project, ESP32-C6 (esp32-c6-devkitc-1), Arduino core 3.x via the pioarduino platform (pinned URL in `platformio.ini`), NimBLE-Arduino 2.x.
  - `include/config.h`: **all** pins, button count, VID/PID, timing. Change hardware settings here only.
  - `include/protocol.h`: HID report descriptor, report IDs, pack/parse. Pure C++, no Arduino deps.
  - `src/main.cpp`: wiring only (setup/loop). Keep it thin.
  - `src/ble_hid.cpp`: HID-over-GATT service on NimBLE, advertising, bonding, output-report queue.
  - `src/buttons.cpp`: debounced GPIO buttons and hat. `src/imu.cpp`: `ImuSource` and `FakeImu`. `src/haptics.cpp`: `HapticDriver` and the stub.
  - `test/test_logic/`: native unit tests (descriptor sizes, packing, parsing, debouncer).
- `protocol/hid-report.md`: **single source of truth** for report byte layouts. The Unity C# layout mirrors it.
- `tools/hid_probe.py`: hidapi tool to list, monitor, send haptics, dump the descriptor, and run a selftest.

## Commands (run from `firmware/`)

```sh
pio run                              # build (default env, DEBUG logging on)
pio run -t upload                    # flash over the board's "USB" port (USB-Serial/JTAG)
pio device monitor                   # serial log (Ctrl+C to quit)
pio run -t upload -t monitor         # flash, then monitor
pio run -e release -t upload         # flash with logging compiled out (ECHO_DEBUG=0)
pio test -e native                   # host unit tests, no board needed
pio run -t clean                     # clean build
```

If several boards are connected, add `--upload-port /dev/cu.usbmodemXXXX` / `--port ...`.
If the board doesn't enumerate (e.g. the firmware crashed early), hold BOOT, tap RESET, release BOOT, then upload.

Host tools (from repo root):

```sh
pip install -r tools/requirements.txt
python tools/hid_probe.py selftest          # offline, no device
python tools/hid_probe.py monitor           # live decoded input reports
python tools/hid_probe.py haptic 0 200 500  # motor 0, intensity 200, 500 ms
python tools/hid_probe.py descriptor        # descriptor as the host sees it vs. the doc
```

## Verify a change

1. `pio run`: must build with no warnings.
2. `pio test -e native`: must pass.
3. `python tools/hid_probe.py selftest`: must pass.
4. On hardware: follow the manual test steps in `README.md`.

## Changing the HID descriptor or report layout: re-pair required

Hosts cache the report descriptor; Windows caches it per VID/PID.
After **any** descriptor change:

1. Update `protocol/hid-report.md` first, then `include/protocol.h`, then `tools/hid_probe.py`.
2. **Bump `DEVICE_PID` in `include/config.h`.** Bump the protocol version (`protocol.h` `VERSION_*` and `config.h` `DEVICE_VERSION`) too.
3. `pio test -e native` checks that the descriptor bit counts match `INPUT_REPORT_SIZE` / `HAPTIC_REPORT_SIZE`.
4. Flash, then on every host **remove the controller from Bluetooth settings and pair again**.
   To also wipe the controller's side, hold BOOT for 3 s while it's running. That erases all bonds and reboots.
5. `python tools/hid_probe.py descriptor` should then report a match (on macOS/Linux).

## Conventions

- NimBLE callbacks run in the NimBLE host task. Do not block there or call into other modules; parse and push onto the queue, then let `loop()` consume it.
- Logging goes through the `LOG(tag, fmt, ...)` macro in `include/log.h`, gated by `ECHO_DEBUG`.
- Pure logic (anything testable without hardware) goes in header-only files under `include/` with tests in `test/test_logic/`.
- Haptics must stop on disconnect (failsafe in `main.cpp`). Keep that behaviour when replacing the stub driver.
- The real IMU driver is a TODO: implement `ImuSource`, add an `IMU_DRIVER_*` value in `config.h`, and add a case in `createImu()`.
