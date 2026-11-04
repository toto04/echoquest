# EchoQuest controller: HID report protocol

**Protocol version 1.0.** This document is the single source of truth for the
controller's HID reports. The firmware (`firmware/include/protocol.h`), the
probe tool (`tools/hid_probe.py`) and the Unity C# layout
(`unity/EchoQuestTest/Assets/EchoQuest/Scripts/EchoQuestController.cs`) all mirror it.

## Rules for changing this protocol

1. Change this document first, then `protocol.h`, `hid_probe.py`, and the Unity layout
   (`EchoQuestController.cs`: struct offsets and `ProductId`).
2. **Bump `DEVICE_PID`** in `firmware/include/config.h` whenever the report
   descriptor changes in any way. Windows caches descriptors per VID/PID and
   will keep decoding with the old layout otherwise.
3. Bump the protocol version (`VERSION_MAJOR/MINOR` in `protocol.h` and
   `DEVICE_VERSION` in `config.h`): minor for additive changes, major for
   anything that moves or reinterprets existing bytes.
4. Run `pio test -e native`. It fails if the descriptor and the size constants disagree.
5. On every host: remove the controller from Bluetooth settings and pair again.

## Transport and identity

| Item | Value |
|---|---|
| Transport | Bluetooth LE, HID over GATT (HOGP) |
| Advertised name | `EchoQuest-XXXX`. `XXXX` = last 2 bytes of the BT MAC, uppercase hex |
| Appearance | `0x03C4` (Gamepad) |
| PnP ID: VID source | `0x02` (USB-IF namespace) |
| VID | `0x1209` (pid.codes) |
| PID | `0x0001` (test range; bump on every descriptor change) |
| Product version | `0x0100` = protocol 1.0 |
| Pairing | Just Works, LE Secure Connections, bonded |
| HID usage | Generic Desktop / Game Pad (`0x01`/`0x05`), one application collection |

## Reports

| Report ID | Type | Payload size | Purpose |
|---|---|---|---|
| `1` | Input | 15 bytes | Buttons, hat, orientation axes |
| `2` | Output | 8 bytes | Haptic command (vendor-defined, usage page `0xFF00`) |

There are no feature reports.

### Report ID framing

- **Over BLE (inside the firmware):** characteristic values carry only the
  payload, with no report ID byte. The report ID is in the GATT Report Reference descriptor.
- **On the host (hidapi, Windows HID, macOS IOHID, Unity raw HID):** buffers
  start with the report ID byte, so **host offset = payload offset + 1**.
  The tables below give both. Use `tools/hid_probe.py --raw` to see exactly
  what your host delivers.

All multi-byte fields are **little-endian**.

## Input report (ID 1), 15-byte payload

| Payload offset | Host offset | Size | Type | Field | Range | Notes |
|---|---|---|---|---|---|---|
| — | 0 | 1 | u8 | report ID | `0x01` | host buffers only |
| 0 | 1 | 2 | u16 bitfield | buttons | bit *i* = button *i*+1 | 1 = pressed. Buttons 1–16. Unwired buttons read 0 |
| 2 | 3 | 1 (low nibble) | u4 | hat | 0–8 | 0 = centered, 1 = N, 2 = NE, 3 = E, 4 = SE, 5 = S, 6 = SW, 7 = W, 8 = NW |
| 2 | 3 | 1 (high nibble) | — | padding | 0 | always 0 |
| 3 | 4 | 2 | i16 | axis 0, **yaw** (HID X) | −32767..32767 | angle, see below |
| 5 | 6 | 2 | i16 | axis 1, **pitch** (HID Y) | −32767..32767 | angle, see below |
| 7 | 8 | 2 | i16 | axis 2, **roll** (HID Z) | −32767..32767 | angle, see below |
| 9 | 10 | 2 | i16 | axis 3, spare (HID Rx) | −32767..32767 | currently always 0 |
| 11 | 12 | 2 | i16 | axis 4, spare (HID Ry) | −32767..32767 | currently always 0 |
| 13 | 14 | 2 | i16 | axis 5, spare (HID Rz) | −32767..32767 | currently always 0 |

Host-side total: 16 bytes.

### Orientation axes

- **Unit:** all three angles use the same scale, `degrees = value × 180 / 32767`
  (1 LSB ≈ 0.0055°). `+32767` = +180°, `−32767` = −180°. The value −32768 is never sent.
- **Yaw:** −180°..+180°, wraps (crossing +180° jumps to −180°). Positive = turning right (clockwise seen from above).
- **Pitch:** −90°..+90° (−16384..16384). Positive = nose up.
- **Roll:** −180°..+180°. Positive = right side down.
- **Body frame:** the controller lying flat, screen up, in the LCD's landscape
  orientation. "Forward"/"nose" is the top edge of the screen.
- **Zero reference:** pitch and roll are absolute (measured against gravity; 0 = flat).
  Yaw is relative: 0 at power-on, and it drifts slowly because there is no magnetometer.
  Hosts that need a stable heading should recenter yaw themselves.
- With `FakeImu` (DevKitC-1 build), all three axes are slow sine waves: yaw ±180°
  with a 20 s period, pitch ±45° with a 7 s period, roll ±30° with an 11 s period.

### Send behaviour

The firmware rebuilds the report every 10 ms (`INPUT_REPORT_INTERVAL_MS`) and
sends a notification only when it changed. It always sends once right after the host
subscribes. Hosts should treat the last received report as current state.

## Output report (ID 2): haptic command, 8-byte payload

| Payload offset | Host offset | Size | Type | Field | Range | Notes |
|---|---|---|---|---|---|---|
| — | 0 | 1 | u8 | report ID | `0x02` | host buffers only |
| 0 | 1 | 1 | u8 | motor_id | 0..N−1, or `0xFF` | `0xFF` = all motors. N = `HAPTIC_MOTOR_COUNT` (currently 2). Out-of-range IDs are ignored |
| 1 | 2 | 1 | u8 | intensity | 0–255 | 0 = stop, 255 = maximum |
| 2 | 3 | 2 | u16 | duration_ms | 0–65535 | milliseconds. 0 = stop |
| 4 | 5 | 1 | u8 | pattern_id | 0–255 | 0 = constant. 1–255 reserved; v1.0 firmware plays them as constant |
| 5 | 6 | 3 | — | reserved | 0 | send 0; ignored by v1.0 firmware |

Host-side total: 9 bytes.

### Haptic semantics

- A command on a motor **replaces** whatever that motor is doing. There is no queueing per motor.
- `intensity == 0` or `duration_ms == 0` stops the motor immediately.
- The effect ends by itself after `duration_ms`. To play longer than 65.5 s, re-send.
- Every command is delivered in order. The firmware queues up to 16 commands
  between the BLE stack and the main loop, so bursts are not coalesced.
- **Failsafe:** all motors stop when the host disconnects.
- Example, motor 0 at intensity 200 for 500 ms: payload `00 C8 F4 01 00 00 00 00`,
  host buffer `02 00 C8 F4 01 00 00 00 00`.

## Report descriptor (98 bytes)

Annotated source: `firmware/include/protocol.h`.

```
05 01 09 05 A1 01 85 01 05 09 19 01 29 10 15 00
25 01 75 01 95 10 81 02 05 01 09 39 15 01 25 08
35 00 46 3B 01 65 14 75 04 95 01 81 42 65 00 45
00 75 04 95 01 81 03 09 30 09 31 09 32 09 33 09
34 09 35 16 01 80 26 FF 7F 75 10 95 06 81 02 85
02 06 00 FF 09 01 15 00 26 FF 00 75 08 95 08 91
02 C0
```
