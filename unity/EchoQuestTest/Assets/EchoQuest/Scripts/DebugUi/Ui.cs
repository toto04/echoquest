// Immediate-mode widgets drawn with IMGUI but clicked through the Input System.
// IMGUI gets no input events in a player when only the Input System is active, so the
// overlay passes the pointer in and every widget does its own hit test.
using UnityEngine;

namespace EchoQuest.DebugUi
{
    public static class Ui
    {
        // Pointer state in GUI units (after the overlay's scale), set by DebugOverlay.Update.
        public static Vector2 Pointer;
        public static bool Pressed, Held;
        // Where the current layout area starts, and the region that accepts clicks.
        public static Vector2 Origin;
        public static Rect Clip;

        public static GUIStyle Label, Small, Head, ButtonText, RowText, Big;

        private static readonly Color Normal = new Color(0.22f, 0.24f, 0.30f, 1f);
        private static readonly Color Hover = new Color(0.30f, 0.33f, 0.42f, 1f);
        private static readonly Color Down = new Color(0.16f, 0.17f, 0.22f, 1f);
        private static readonly Color Accent = new Color(0.16f, 0.45f, 0.75f, 1f);

        // A click found during Repaint is delivered on the next Layout pass, where changing
        // what gets drawn is safe.
        private static int s_Counter, s_FireFrame;
        private static string s_Fire, s_SliderKey, s_PendingKey;
        private static float s_PendingValue;

        private static bool Repainting => Event.current.type == EventType.Repaint;
        private static bool LayingOut => Event.current.type == EventType.Layout;

        public static void EnsureStyles()
        {
            if (Label != null) return;
            Label = new GUIStyle(GUI.skin.label) { wordWrap = true, richText = true, fontSize = 13 };
            Label.normal.textColor = new Color(0.92f, 0.93f, 0.95f);
            Small = new GUIStyle(Label) { fontSize = 11 };
            Head = new GUIStyle(Label) { fontStyle = FontStyle.Bold, fontSize = 13 };
            Head.normal.textColor = new Color(1f, 0.85f, 0.45f);
            ButtonText = new GUIStyle(Label)
            {
                alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Clip,
                padding = new RectOffset(8, 8, 4, 4), margin = new RectOffset(2, 2, 2, 2),
            };
            RowText = new GUIStyle(ButtonText) { alignment = TextAnchor.MiddleLeft, fontSize = 11 };
            Big = new GUIStyle(Label) { fontSize = 24, alignment = TextAnchor.LowerCenter };
        }

        public static void BeginPass() => s_Counter = 0;

        public static void EndRepaint()
        {
            Pressed = false;
            if (s_Fire != null && Time.frameCount > s_FireFrame + 2) s_Fire = null;
        }

        public static void Release() => s_SliderKey = null;

        private static bool Over(Rect r) => Clip.Contains(Pointer) && r.Contains(Pointer - Origin);

        public static void Fill(Rect r, Color c)
        {
            if (!Repainting) return;
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }

        public static bool ButtonAt(Rect r, string label, bool active = false, GUIStyle style = null)
        {
            string key = s_Counter++ + ":" + label;
            if (Repainting)
            {
                bool over = Over(r);
                Fill(r, active ? Accent : over ? (Held ? Down : Hover) : Normal);
                GUI.Label(r, label, style ?? ButtonText);
                if (over && Pressed)
                {
                    s_Fire = key;
                    s_FireFrame = Time.frameCount;
                    Pressed = false;
                }
                return false;
            }
            if (LayingOut && s_Fire == key)
            {
                s_Fire = null;
                return true;
            }
            return false;
        }

        public static bool Button(string label, bool active = false, params GUILayoutOption[] options)
        {
            var r = GUILayoutUtility.GetRect(new GUIContent(label), ButtonText, options);
            return ButtonAt(r, label, active);
        }

        public static bool Toggle(string label, bool value)
        {
            if (Button((value ? "[x] " : "[  ] ") + label, value)) return !value;
            return value;
        }

        public static float Slider(string label, float value, float min, float max, string format = "F2")
        {
            string key = s_Counter++ + ":" + label;
            var r = GUILayoutUtility.GetRect(100f, 22f, GUILayout.ExpandWidth(true));
            if (LayingOut && s_PendingKey == key)
            {
                value = s_PendingValue;
                s_PendingKey = null;
            }
            if (!Repainting) return value;

            var track = new Rect(r.x + 170f, r.y + 5f, Mathf.Max(20f, r.width - 230f), r.height - 10f);
            GUI.Label(new Rect(r.x, r.y, 168f, r.height), label, Small);
            Fill(track, Normal);
            float t = Mathf.InverseLerp(min, max, value);
            Fill(new Rect(track.x, track.y, track.width * t, track.height), Accent);
            GUI.Label(new Rect(track.xMax + 6f, r.y, 54f, r.height), value.ToString(format), Small);

            var grab = new Rect(track.x - 6f, r.y, track.width + 12f, r.height);
            if (Pressed && Over(grab))
            {
                s_SliderKey = key;
                Pressed = false;
            }
            if (Held && s_SliderKey == key)
            {
                s_PendingKey = key;
                s_PendingValue = Mathf.Lerp(min, max, Mathf.Clamp01((Pointer.x - Origin.x - track.x) / track.width));
            }
            return value;
        }

        public static void Header(string text)
        {
            GUILayout.Space(8f);
            GUILayout.Label(text, Head);
        }

        public static void Text(string text) => GUILayout.Label(text, Label);
        public static void Note(string text) => GUILayout.Label(text, Small);
    }
}
