#!/usr/bin/env python3
"""EchoQuest controller probe: find the controller over HID, decode input
reports live, and send haptic output reports.

Byte layout: protocol/hid-report.md (keep this file in sync with it).

Setup:
    pip install hidapi          # provides the `hid` module (cython-hidapi)

Usage:
    python tools/hid_probe.py list                      # controllers matching VID/PID
    python tools/hid_probe.py monitor [--raw]           # live decoded input reports
    python tools/hid_probe.py haptic 0 200 500          # motor 0, intensity 200, 500 ms
    python tools/hid_probe.py haptic all 255 1000 --pattern 0
    python tools/hid_probe.py haptic 0 0 0              # stop motor 0
    python tools/hid_probe.py descriptor                # descriptor as the host sees it vs. the doc
    python tools/hid_probe.py selftest                  # offline encode/decode checks, no device

VID/PID default to the values in firmware/include/config.h; override with --vid/--pid.
"""

from __future__ import annotations

import argparse
import re
import struct
import sys
import time
from pathlib import Path

REPORT_ID_INPUT = 1
REPORT_ID_HAPTIC = 2
INPUT_PAYLOAD_SIZE = 15
HAPTIC_PAYLOAD_SIZE = 8
AXIS_MAX = 32767
MOTOR_ALL = 0xFF
HAT_NAMES = ["--", "N", "NE", "E", "SE", "S", "SW", "W", "NW"]

CONFIG_H = Path(__file__).resolve().parent.parent / "firmware" / "include" / "config.h"


# Protocol encode/decode (mirrors protocol/hid-report.md)

def decode_input(buf: bytes) -> dict:
    """Decode a host-side input report buffer (report ID first)."""
    if len(buf) < 1 + INPUT_PAYLOAD_SIZE or buf[0] != REPORT_ID_INPUT:
        raise ValueError(f"not an input report: {buf.hex(' ')}")
    buttons, hat_byte, *axes = struct.unpack_from("<HB6h", buf, 1)
    hat = hat_byte & 0x0F
    return {
        "buttons": buttons,
        "pressed": [i + 1 for i in range(16) if buttons & (1 << i)],
        "hat": hat,
        "hat_name": HAT_NAMES[hat] if hat < len(HAT_NAMES) else f"?{hat}",
        "axes": axes,
        "yaw": axes[0] * 180.0 / AXIS_MAX,
        "pitch": axes[1] * 180.0 / AXIS_MAX,
        "roll": axes[2] * 180.0 / AXIS_MAX,
    }


def encode_haptic(motor: int, intensity: int, duration_ms: int, pattern: int = 0) -> bytes:
    """Encode a host-side haptic output report buffer (report ID first)."""
    for name, v, hi in (("motor", motor, 255), ("intensity", intensity, 255),
                        ("duration_ms", duration_ms, 65535), ("pattern", pattern, 255)):
        if not 0 <= v <= hi:
            raise ValueError(f"{name} must be 0..{hi}, got {v}")
    payload = struct.pack("<BBHB3x", motor, intensity, duration_ms, pattern)
    assert len(payload) == HAPTIC_PAYLOAD_SIZE
    return bytes([REPORT_ID_HAPTIC]) + payload


def selftest() -> int:
    # Example from protocol/hid-report.md
    assert encode_haptic(0, 200, 500).hex(" ") == "02 00 c8 f4 01 00 00 00 00"
    assert encode_haptic(MOTOR_ALL, 255, 65535, 3).hex(" ") == "02 ff ff ff ff 03 00 00 00"
    # Same vector as firmware test_pack_input_layout
    buf = bytes([REPORT_ID_INPUT, 0x01, 0x80, 0x03, 0x34, 0x12, 0xFE, 0xFF, 0xFF, 0x7F,
                 0, 0, 0, 0, 0x01, 0x80])
    d = decode_input(buf)
    assert d["pressed"] == [1, 16], d
    assert d["hat_name"] == "E", d
    assert d["axes"] == [0x1234, -2, 32767, 0, 0, -32767], d
    assert abs(d["roll"] - 180.0) < 1e-9
    desc = expected_descriptor()
    assert desc is not None and desc[:2] == b"\x05\x01" and desc[-1] == 0xC0, "doc descriptor block unreadable"
    print("selftest OK")
    return 0


# Device access

def default_ids() -> tuple[int, int]:
    vid, pid = 0x1209, 0x0001
    try:
        text = CONFIG_H.read_text()
        m = re.search(r"DEVICE_VID\s*=\s*(0x[0-9A-Fa-f]+)", text)
        if m:
            vid = int(m.group(1), 16)
        m = re.search(r"DEVICE_PID\s*=\s*(0x[0-9A-Fa-f]+)", text)
        if m:
            pid = int(m.group(1), 16)
    except OSError:
        pass
    return vid, pid


def import_hid():
    try:
        import hid  # type: ignore
    except ImportError:
        sys.exit("Missing module: run `pip install hidapi`")
    if not hasattr(hid, "device"):
        sys.exit("Wrong `hid` module installed (the ctypes 'hid' package). "
                 "Run `pip uninstall hid && pip install hidapi`.")
    return hid


def find_devices(hid, vid: int, pid: int) -> list[dict]:
    devs = hid.enumerate(vid, pid)
    # Prefer the gamepad collection if the OS exposes several.
    devs.sort(key=lambda d: (d.get("usage_page"), d.get("usage")) != (0x01, 0x05))
    return devs


def open_device(hid, vid: int, pid: int):
    devs = find_devices(hid, vid, pid)
    if not devs:
        sys.exit(f"No controller with VID 0x{vid:04X} PID 0x{pid:04X}. "
                 "Is it paired and connected? (`list --all` shows every HID device)")
    info = devs[0]
    dev = hid.device()
    dev.open_path(info["path"])
    print(f"Opened {info.get('product_string') or '?'} "
          f"(VID 0x{vid:04X} PID 0x{pid:04X}, usage {info.get('usage_page', 0):#06x}/{info.get('usage', 0):#04x})")
    return dev


def cmd_list(args) -> int:
    hid = import_hid()
    devs = hid.enumerate() if args.all else find_devices(hid, args.vid, args.pid)
    if not devs:
        print(f"No devices found for VID 0x{args.vid:04X} PID 0x{args.pid:04X}")
        return 1
    for d in devs:
        print(f"{d['vendor_id']:04X}:{d['product_id']:04X}  usage {d.get('usage_page', 0):#06x}/{d.get('usage', 0):#04x}  "
              f"{d.get('manufacturer_string') or ''} / {d.get('product_string') or ''}  path={d['path']!r}")
    return 0


def cmd_monitor(args) -> int:
    hid = import_hid()
    dev = open_device(hid, args.vid, args.pid)
    print("Reading input reports, Ctrl+C to stop.")
    count, t0 = 0, time.monotonic()
    rate = 0.0
    try:
        while True:
            data = dev.read(64, 1000)
            if not data:
                continue
            buf = bytes(data)
            count += 1
            now = time.monotonic()
            if now - t0 >= 1.0:
                rate, count, t0 = count / (now - t0), 0, now
            if args.raw:
                print(buf.hex(" "))
                continue
            try:
                d = decode_input(buf)
            except ValueError as e:
                print(f"\n{e}")
                continue
            pressed = " ".join(f"B{b}" for b in d["pressed"]) or "-"
            line = (f"btn {pressed:<24} hat {d['hat_name']:<2}  yaw {d['yaw']:8.2f}  "
                    f"pitch {d['pitch']:7.2f}  roll {d['roll']:8.2f}  spare {d['axes'][3:]}  {rate:5.1f} rep/s")
            print(("\r" + line) if not args.lines else line, end="" if not args.lines else "\n", flush=True)
    except KeyboardInterrupt:
        print()
    finally:
        dev.close()
    return 0


def cmd_haptic(args) -> int:
    try:
        motor = MOTOR_ALL if args.motor.lower() == "all" else int(args.motor, 0)
        report = encode_haptic(motor, args.intensity, args.duration, args.pattern)
    except ValueError as e:
        sys.exit(f"error: {e}")
    hid = import_hid()
    dev = open_device(hid, args.vid, args.pid)
    try:
        n = dev.write(list(report))
        if n < 0:
            # Some Windows BLE stacks reject WriteFile; fall back to HidD_SetOutputReport if available.
            send = getattr(dev, "send_output_report", None)
            n = send(list(report)) if send else n
        if n < 0:
            print(f"write failed: {dev.error()}")
            return 1
        print(f"sent {report.hex(' ')}  (motor={'all' if motor == MOTOR_ALL else motor} "
              f"intensity={args.intensity} duration={args.duration}ms pattern={args.pattern})")
    finally:
        dev.close()
    return 0


def expected_descriptor() -> bytes | None:
    doc = Path(__file__).resolve().parent.parent / "protocol" / "hid-report.md"
    try:
        m = re.search(r"## Report descriptor.*?```\n(.*?)```", doc.read_text(), re.S)
    except OSError:
        return None
    return bytes.fromhex(m.group(1)) if m else None


def cmd_descriptor(args) -> int:
    hid = import_hid()
    dev = open_device(hid, args.vid, args.pid)
    try:
        got = bytes(dev.get_report_descriptor())
    finally:
        dev.close()
    print(f"host descriptor ({len(got)} bytes):")
    for i in range(0, len(got), 16):
        print("  " + got[i:i + 16].hex(" "))
    want = expected_descriptor()
    if want is None:
        print("could not read the expected descriptor from protocol/hid-report.md")
    elif got == want:
        print("MATCHES protocol/hid-report.md")
    else:
        print("DIFFERS from protocol/hid-report.md. On Windows, hidapi rebuilds the descriptor from parsed "
              "data, so small differences are normal. On macOS/Linux a difference means a stale pairing "
              "or an old firmware: remove the device, re-pair, and check the PID.")
    return 0


def main() -> int:
    vid, pid = default_ids()
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--vid", type=lambda s: int(s, 0), default=vid, help=f"default 0x{vid:04X} (from config.h)")
    p.add_argument("--pid", type=lambda s: int(s, 0), default=pid, help=f"default 0x{pid:04X} (from config.h)")
    sub = p.add_subparsers(dest="cmd", required=True)

    s = sub.add_parser("list", help="list matching HID devices")
    s.add_argument("--all", action="store_true", help="list every HID device")
    s.set_defaults(func=cmd_list)

    s = sub.add_parser("monitor", help="print decoded input reports live")
    s.add_argument("--raw", action="store_true", help="print raw bytes instead of decoding")
    s.add_argument("--lines", action="store_true", help="one line per report instead of refreshing")
    s.set_defaults(func=cmd_monitor)

    s = sub.add_parser("haptic", help="send a haptic output report")
    s.add_argument("motor", help="motor id (0..N-1) or 'all'")
    s.add_argument("intensity", type=int, help="0-255 (0 = stop)")
    s.add_argument("duration", type=int, help="milliseconds 0-65535 (0 = stop)")
    s.add_argument("--pattern", type=int, default=0, help="pattern id (0 = constant)")
    s.set_defaults(func=cmd_haptic)

    s = sub.add_parser("descriptor", help="dump the report descriptor the host sees and compare with the doc")
    s.set_defaults(func=cmd_descriptor)

    s = sub.add_parser("selftest", help="offline encode/decode checks (no device needed)")
    s.set_defaults(func=lambda a: selftest())

    args = p.parse_args()
    return args.func(args)


if __name__ == "__main__":
    sys.exit(main())
