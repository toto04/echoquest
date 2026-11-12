using EchoQuest.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoQuest
{
    public class CritterController : MonoBehaviour
    {
        [Header("Rig")]
        [Tooltip("Rotated to match the controller. Child of this object.")]
        public Transform pivot;
        [Tooltip("Renderers whose colour button A cycles.")]
        public Renderer[] bodyRenderers;

        [Header("Feel")]
        [Tooltip("Higher = follows the controller more tightly. 0 = no smoothing.")]
        public float rotationSharpness = 25f;
        public Color[] palette =
        {
            new Color(1.00f, 0.55f, 0.15f),   // orange
            new Color(0.20f, 0.75f, 1.00f),   // sky blue
            new Color(0.55f, 0.90f, 0.25f),   // lime
            new Color(0.95f, 0.30f, 0.65f),   // pink
            new Color(0.70f, 0.45f, 1.00f),   // lavender
            new Color(1.00f, 0.85f, 0.20f),   // yellow
        };
        public float colorLerpSpeed = 12f;
        public float hopHeight = 0.9f;
        public float hopDuration = 0.45f;

        [Header("Keyboard fallback")]
        public float keyboardDegreesPerSecond = 90f;

        // Read by the HUD.
        public float yawDeg { get; private set; }
        public float pitchDeg { get; private set; }
        public float rollDeg { get; private set; }
        public float yawOffsetDeg { get; private set; }
        public bool usingController { get; private set; }
        public int colorIndex { get; private set; }
        public bool buttonAHeld { get; private set; }
        public bool buttonBHeld { get; private set; }

        private Vector3 m_RestPosition;
        private float m_HopTime = -1f;
        private Color m_CurrentColor;
        private MaterialPropertyBlock m_Props;
        private float m_KeyboardYaw, m_KeyboardPitch, m_KeyboardRoll;
        private float m_BothHeldSince = -1f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            // Test bench convenience: keep reading the controller even when Unity isn't
            // the focused app (e.g. while you look at the serial monitor). Runtime only, not saved.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.runInBackground = true;

            m_RestPosition = transform.localPosition;
            m_Props = new MaterialPropertyBlock();
            m_CurrentColor = palette.Length > 0 ? palette[0] : Color.white;
            ApplyColor(m_CurrentColor);
        }

        private void Update()
        {
            var controller = EchoQuestController.current;
            usingController = controller != null && controller.enabled;
            var keyboard = Keyboard.current;

            bool aPressed, bPressed;
            if (usingController)
            {
                yawDeg = Mathf.DeltaAngle(0f, controller.yawDegrees - yawOffsetDeg);
                pitchDeg = controller.pitchDegrees;
                rollDeg = controller.rollDegrees;
                buttonAHeld = controller.buttonA.isPressed;
                buttonBHeld = controller.buttonB.isPressed;
                aPressed = controller.buttonA.wasPressedThisFrame;
                bPressed = controller.buttonB.wasPressedThisFrame;
            }
            else
            {
                ReadKeyboardOrientation(keyboard);
                yawDeg = Mathf.DeltaAngle(0f, m_KeyboardYaw - yawOffsetDeg);
                pitchDeg = m_KeyboardPitch;
                rollDeg = m_KeyboardRoll;
                buttonAHeld = keyboard != null && keyboard.digit1Key.isPressed;
                buttonBHeld = keyboard != null && keyboard.digit2Key.isPressed;
                aPressed = keyboard != null && keyboard.digit1Key.wasPressedThisFrame;
                bPressed = keyboard != null && keyboard.digit2Key.wasPressedThisFrame;
            }

            // Recenter yaw: R key, or hold A + B together for one second.
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                RecenterYaw();
            if (buttonAHeld && buttonBHeld)
            {
                if (m_BothHeldSince < 0f) m_BothHeldSince = Time.unscaledTime;
                else if (Time.unscaledTime - m_BothHeldSince >= 1f)
                {
                    RecenterYaw();
                    m_BothHeldSince = float.PositiveInfinity;   // once per hold
                }
            }
            else
            {
                m_BothHeldSince = -1f;
            }

            if (aPressed && palette.Length > 0)
                colorIndex = (colorIndex + 1) % palette.Length;
            if (bPressed)
                m_HopTime = 0f;

            UpdateRotation();
            UpdateColor();
            UpdateHop();
        }

        public void RecenterYaw()
        {
            var rawYaw = usingController ? EchoQuestController.current.yawDegrees : m_KeyboardYaw;
            yawOffsetDeg = rawYaw;
            Debug.Log($"[EchoQuest] yaw recentered (offset {yawOffsetDeg:F1} deg)");
        }

        private void ReadKeyboardOrientation(Keyboard keyboard)
        {
            if (keyboard == null) return;
            var step = keyboardDegreesPerSecond * Time.deltaTime;
            if (keyboard.rightArrowKey.isPressed) m_KeyboardYaw += step;
            if (keyboard.leftArrowKey.isPressed) m_KeyboardYaw -= step;
            if (keyboard.upArrowKey.isPressed) m_KeyboardPitch += step;
            if (keyboard.downArrowKey.isPressed) m_KeyboardPitch -= step;
            if (keyboard.eKey.isPressed) m_KeyboardRoll += step;
            if (keyboard.qKey.isPressed) m_KeyboardRoll -= step;
            m_KeyboardYaw = Mathf.DeltaAngle(0f, m_KeyboardYaw);
            m_KeyboardPitch = Mathf.Clamp(m_KeyboardPitch, -90f, 90f);
            m_KeyboardRoll = Mathf.DeltaAngle(0f, m_KeyboardRoll);
        }

        private void UpdateRotation()
        {
            if (pivot == null) return;
            // Protocol: yaw + = turn right, pitch + = nose up, roll + = right side down.
            // Unity (left-handed, Y up, Z forward): +Y rotation turns right, +X tips the nose
            // down, +Z lifts the right side. Unity applies Euler as Z, X, Y = roll, pitch, yaw
            // in the body frame, the same order the firmware's yaw-pitch-roll angles use.
            var target = Quaternion.Euler(-pitchDeg, yawDeg, -rollDeg);
            pivot.localRotation = rotationSharpness <= 0f
                ? target
                : Quaternion.Slerp(pivot.localRotation, target, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }

        private void UpdateColor()
        {
            if (palette.Length == 0) return;
            m_CurrentColor = Color.Lerp(m_CurrentColor, palette[colorIndex], 1f - Mathf.Exp(-colorLerpSpeed * Time.deltaTime));
            ApplyColor(m_CurrentColor);
        }

        private void ApplyColor(Color color)
        {
            if (bodyRenderers == null) return;
            foreach (var r in bodyRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(m_Props);
                m_Props.SetColor(BaseColorId, color);
                r.SetPropertyBlock(m_Props);
            }
        }

        private void UpdateHop()
        {
            if (m_HopTime < 0f)
            {
                transform.localPosition = m_RestPosition;
                transform.localScale = Vector3.one;
                return;
            }
            m_HopTime += Time.deltaTime;
            var t = m_HopTime / hopDuration;
            if (t >= 1f)
            {
                m_HopTime = -1f;
                transform.localPosition = m_RestPosition;
                transform.localScale = Vector3.one;
                return;
            }
            // Parabolic hop with a little squash and stretch.
            var height = 4f * t * (1f - t) * hopHeight;
            transform.localPosition = m_RestPosition + Vector3.up * height;
            var stretch = 1f + 0.25f * Mathf.Sin(t * Mathf.PI) * (t < 0.5f ? 1f : -0.6f);
            transform.localScale = new Vector3(1f / Mathf.Sqrt(stretch), stretch, 1f / Mathf.Sqrt(stretch));
        }
    }
}
