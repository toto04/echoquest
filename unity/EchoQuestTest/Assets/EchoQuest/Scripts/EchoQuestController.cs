// Input System device for the EchoQuest controller.
//
// Byte layout: protocol/hid-report.md (repo root). Keep this file in sync with it.
// Unity's generic HID fallback would turn X/Y into a "stick" with a radial
// deadzone and an inverted Y, which distorts orientation angles. This custom
// layout reads the report exactly as documented instead.
//
// The Input System delivers the raw HID input report as device state, report ID
// byte first, so offsets below are the "host offsets" from the protocol doc.
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EchoQuest.Input
{
    /// <summary>Input report ID 1, protocol 1.0: 1 B report ID + 15 B payload.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct EchoQuestInputReport : IInputStateTypeInfo
    {
        public static readonly FourCC Format = new FourCC('H', 'I', 'D');
        public FourCC format => Format;

        [FieldOffset(0)] public byte reportId;

        // Buttons 1-16, bit i = button i+1. On the Waveshare test board, touch A = button 1, B = button 2.
        [InputControl(name = "button1", layout = "Button", bit = 0, displayName = "Button 1 (A)")]
        [InputControl(name = "button2", layout = "Button", bit = 1, displayName = "Button 2 (B)")]
        [InputControl(name = "button3", layout = "Button", bit = 2)]
        [InputControl(name = "button4", layout = "Button", bit = 3)]
        [InputControl(name = "button5", layout = "Button", bit = 4)]
        [InputControl(name = "button6", layout = "Button", bit = 5)]
        [InputControl(name = "button7", layout = "Button", bit = 6)]
        [InputControl(name = "button8", layout = "Button", bit = 7)]
        [InputControl(name = "button9", layout = "Button", bit = 8)]
        [InputControl(name = "button10", layout = "Button", bit = 9)]
        [InputControl(name = "button11", layout = "Button", bit = 10)]
        [InputControl(name = "button12", layout = "Button", bit = 11)]
        [InputControl(name = "button13", layout = "Button", bit = 12)]
        [InputControl(name = "button14", layout = "Button", bit = 13)]
        [InputControl(name = "button15", layout = "Button", bit = 14)]
        [InputControl(name = "button16", layout = "Button", bit = 15)]
        [FieldOffset(1)] public ushort buttons;

        // Hat: low nibble, 0 = centered, 1 = N, 2 = NE ... 8 = NW.
        [InputControl(name = "hat", layout = "Integer", format = "BIT", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad", layout = "Dpad", format = "BIT", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/up", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4,
            parameters = "minValue=8,maxValue=2,nullValue=0,wrapAtValue=8")]
        [InputControl(name = "dpad/right", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4,
            parameters = "minValue=2,maxValue=4")]
        [InputControl(name = "dpad/down", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4,
            parameters = "minValue=4,maxValue=6")]
        [InputControl(name = "dpad/left", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4,
            parameters = "minValue=6,maxValue=8")]
        [FieldOffset(3)] public byte hat;

        // Axes: int16, -32767..32767 = -180..+180 degrees. AxisControl normalises SHRT to -1..1,
        // so degrees = value * 180 (error < 0.003 deg).
        [InputControl(name = "yaw", layout = "Axis", format = "SHRT", displayName = "Yaw")]
        [FieldOffset(4)] public short yaw;
        [InputControl(name = "pitch", layout = "Axis", format = "SHRT", displayName = "Pitch")]
        [FieldOffset(6)] public short pitch;
        [InputControl(name = "roll", layout = "Axis", format = "SHRT", displayName = "Roll")]
        [FieldOffset(8)] public short roll;
        [InputControl(name = "axis3", layout = "Axis", format = "SHRT", displayName = "Spare axis 3")]
        [FieldOffset(10)] public short axis3;
        [InputControl(name = "axis4", layout = "Axis", format = "SHRT", displayName = "Spare axis 4")]
        [FieldOffset(12)] public short axis4;
        [InputControl(name = "axis5", layout = "Axis", format = "SHRT", displayName = "Spare axis 5")]
        [FieldOffset(14)] public short axis5;
    }

    /// <summary>The EchoQuest BLE controller (VID 0x1209, PID 0x0001).</summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(stateType = typeof(EchoQuestInputReport), displayName = "EchoQuest Controller")]
    public class EchoQuestController : InputDevice
    {
        // Must match firmware/include/config.h. The PID is bumped on every descriptor
        // change, and so must this layout be updated.
        public const int VendorId = 0x1209;
        public const int ProductId = 0x0001;
        public const int InputReportId = 1;
        public const int InputReportSize = 16;   // host side, including the report ID

        public ButtonControl buttonA { get; private set; }
        public ButtonControl buttonB { get; private set; }
        public ButtonControl[] buttons { get; private set; }
        public IntegerControl hat { get; private set; }
        public DpadControl dpad { get; private set; }
        public AxisControl yaw { get; private set; }
        public AxisControl pitch { get; private set; }
        public AxisControl roll { get; private set; }

        /// <summary>Yaw in degrees, -180..180. Positive = turned right. Relative to controller power-on; drifts.</summary>
        public float yawDegrees => yaw.ReadValue() * 180f;
        /// <summary>Pitch in degrees, -90..90. Positive = nose up. Absolute (gravity).</summary>
        public float pitchDegrees => pitch.ReadValue() * 180f;
        /// <summary>Roll in degrees, -180..180. Positive = right side down. Absolute (gravity).</summary>
        public float rollDegrees => roll.ReadValue() * 180f;

        /// <summary>Button bitfield exactly as in the report (bit i = button i+1).</summary>
        public int buttonMask
        {
            get
            {
                var mask = 0;
                for (var i = 0; i < buttons.Length; i++)
                    if (buttons[i].isPressed) mask |= 1 << i;
                return mask;
            }
        }

        // Debug info, updated from raw state events.
        public int reportCount { get; private set; }
        public float reportsPerSecond { get; private set; }
        public byte[] lastRawReport { get; } = new byte[InputReportSize];
        public int lastRawReportSize { get; private set; }

        public static EchoQuestController current { get; private set; }

        static EchoQuestController()
        {
            InputSystem.RegisterLayout<EchoQuestController>(
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", VendorId)
                    .WithCapability("productId", ProductId));
            InputSystem.onEvent += OnAnyEvent;
        }

        // Triggers the static constructor in players and in Play mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() { }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            buttons = new ButtonControl[16];
            for (var i = 0; i < buttons.Length; i++)
                buttons[i] = GetChildControl<ButtonControl>("button" + (i + 1));
            buttonA = buttons[0];
            buttonB = buttons[1];
            hat = GetChildControl<IntegerControl>("hat");
            dpad = GetChildControl<DpadControl>("dpad");
            yaw = GetChildControl<AxisControl>("yaw");
            pitch = GetChildControl<AxisControl>("pitch");
            roll = GetChildControl<AxisControl>("roll");
        }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            if (current == null) current = this;
            Debug.Log($"[EchoQuest] controller added: {description.product} ({name})");
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            if (current == this) current = null;
            Debug.Log($"[EchoQuest] controller removed: {description.product} ({name})");
        }

        private float m_RateWindowStart;
        private int m_RateWindowCount;

        private static unsafe void OnAnyEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!(device is EchoQuestController controller) || !eventPtr.IsA<StateEvent>())
                return;
            var stateEvent = StateEvent.From(eventPtr);
            var size = (int)Math.Min(stateEvent->stateSizeInBytes, (uint)InputReportSize);
            var src = (byte*)stateEvent->state;
            for (var i = 0; i < size; i++)
                controller.lastRawReport[i] = src[i];
            controller.lastRawReportSize = (int)stateEvent->stateSizeInBytes;
            controller.reportCount++;
            controller.UpdateRate();
        }

        private void UpdateRate()
        {
            var now = Time.realtimeSinceStartup;
            m_RateWindowCount++;
            if (now - m_RateWindowStart >= 1f)
            {
                reportsPerSecond = m_RateWindowCount / (now - m_RateWindowStart);
                m_RateWindowStart = now;
                m_RateWindowCount = 0;
            }
        }

        /// <summary>Last raw report as hex, for comparing with the firmware's serial log.</summary>
        public string LastRawReportHex()
        {
            if (lastRawReportSize == 0) return "(none yet)";
            var n = Math.Min(lastRawReportSize, InputReportSize);
            return BitConverter.ToString(lastRawReport, 0, n).Replace('-', ' ') +
                   (lastRawReportSize != InputReportSize ? $"  (size {lastRawReportSize}, expected {InputReportSize}!)" : "");
        }
    }
}
