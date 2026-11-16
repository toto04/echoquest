// The debug overlay: a docked panel with tabs, present in every scene.
// F1 or ` hides it. Plan and panel list: docs/demo/debug-ui.md.
using System.Collections.Generic;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoQuest.DebugUi
{
    public abstract class DebugPanel
    {
        public abstract string Title { get; }
        public float Scroll;
        /// <summary>Called from LateUpdate while the panel is shown. Read Input System controls here, not in Draw.</summary>
        public virtual void Tick() { }
        /// <summary>Draws the panel's content with GUILayout and the <see cref="Ui"/> widgets.</summary>
        public abstract void Draw();
        /// <summary>Draws over the whole screen, even when the overlay is hidden (e.g. subtitles).</summary>
        public virtual void DrawScreen(float width, float height) { }
    }

    public class DebugOverlay : MonoBehaviour
    {
        public static DebugOverlay Instance { get; private set; }

        public bool visible = true;
        [Tooltip("Black screen behind the overlay: what the player is meant to see.")]
        public bool blackout;

        private readonly List<DebugPanel> m_Panels = new List<DebugPanel>();
        private int m_Tab;
        private StoryPlayer m_Player;
        private bool m_Built, m_HadPlayer, m_Shown;
        private float m_NextSearch, m_ContentHeight, m_ViewHeight;
        private Rect m_Dock;

        private static readonly Color Background = new Color(0.08f, 0.09f, 0.12f, 0.94f);
        private static readonly Color ScrollTrack = new Color(1f, 1f, 1f, 0.06f);
        private static readonly Color ScrollThumb = new Color(1f, 1f, 1f, 0.30f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("Debug overlay");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<DebugOverlay>();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AudioListener.pause = false;
            m_Built = false;
            m_NextSearch = 0f;
        }

        private static float Scale => Mathf.Clamp(Screen.height / 820f, 0.7f, 2.5f);

        private void Rebuild(StoryPlayer player)
        {
            m_Player = player;
            m_HadPlayer = player != null;
            m_Built = true;
            m_Panels.Clear();
            if (player != null)
            {
                m_Panels.Add(new StoryPanel(player));
                m_Panels.Add(new InputPanel(player));
                m_Panels.Add(new AudioPanel(player));
                m_Panels.Add(new RoomPanel(player));
            }
            m_Panels.Add(new PadPanel(player));
            if (player != null) m_Panels.Add(new LogPanel(player));
            m_Panels.Add(new DemoPanel(this));
            m_Tab = 0;
        }

        private void Update()
        {
            // Scenes can create their story player after loading (the check scripts do).
            bool lostPlayer = m_HadPlayer && m_Player == null;
            if (!m_Built || lostPlayer || (m_Player == null && Time.unscaledTime >= m_NextSearch))
            {
                m_NextSearch = Time.unscaledTime + 0.5f;
                var player = FindAnyObjectByType<StoryPlayer>();
                if (!m_Built || lostPlayer || player != m_Player) Rebuild(player);
            }

            var kb = Keyboard.current;
            if (kb != null && (kb.f1Key.wasPressedThisFrame || kb.backquoteKey.wasPressedThisFrame))
                visible = !visible;

            float s = Scale;
            var pointer = Pointer.current;
            if (pointer != null)
            {
                var p = pointer.position.ReadValue();
                Ui.Pointer = new Vector2(p.x / s, (Screen.height - p.y) / s);
                Ui.Held = pointer.press.isPressed;
                if (pointer.press.wasPressedThisFrame) Ui.Pressed = true;
            }
            else Ui.Held = false;
            if (!Ui.Held) Ui.Release();

            var mouse = Mouse.current;
            if (visible && mouse != null && m_Panels.Count > 0 && m_Dock.Contains(Ui.Pointer))
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (wheel != 0f) ScrollBy(-Mathf.Clamp(wheel * 4f, -80f, 80f));
            }
        }

        private void ScrollBy(float delta)
        {
            var panel = m_Panels[m_Tab];
            panel.Scroll = Mathf.Clamp(panel.Scroll + delta, 0f, Mathf.Max(0f, m_ContentHeight - m_ViewHeight));
        }

        private void LateUpdate()
        {
            if (visible && m_Tab < m_Panels.Count) m_Panels[m_Tab].Tick();
        }

        private void OnGUI()
        {
            Ui.EnsureStyles();
            GUI.depth = -100;
            float s = Scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            float width = Screen.width / s, height = Screen.height / s;
            bool repaint = Event.current.type == EventType.Repaint;

            Ui.BeginPass();
            Ui.Origin = Vector2.zero;
            Ui.Clip = new Rect(0f, 0f, width, height);
            if (blackout) Ui.Fill(Ui.Clip, Color.black);
            foreach (var panel in m_Panels) panel.DrawScreen(width, height);

            // Other code may flip "visible" between the Layout and Repaint passes of one frame.
            if (Event.current.type == EventType.Layout) m_Shown = visible && m_Panels.Count > 0;
            if (!m_Shown)
            {
                m_Dock = new Rect(8f, 8f, 100f, 24f);
                if (Ui.ButtonAt(m_Dock, "Debug (F1)")) visible = true;
                if (repaint) Ui.EndRepaint();
                return;
            }

            float dockWidth = Mathf.Min(520f, width - 16f);
            m_Dock = new Rect(8f, 8f, dockWidth, height - 16f);
            Ui.Fill(m_Dock, Background);

            const float hideWidth = 50f;
            float tabWidth = (dockWidth - 12f - hideWidth) / m_Panels.Count;
            for (int i = 0; i < m_Panels.Count; i++)
            {
                var r = new Rect(m_Dock.x + 4f + i * tabWidth, m_Dock.y + 4f, tabWidth - 2f, 26f);
                if (Ui.ButtonAt(r, m_Panels[i].Title, i == m_Tab)) m_Tab = i;
            }
            if (Ui.ButtonAt(new Rect(m_Dock.xMax - hideWidth - 4f, m_Dock.y + 4f, hideWidth, 26f), "Hide"))
                visible = false;

            var current = m_Panels[Mathf.Min(m_Tab, m_Panels.Count - 1)];
            const float barWidth = 12f;
            var view = new Rect(m_Dock.x + 8f, m_Dock.y + 38f, dockWidth - 20f - barWidth, m_Dock.height - 46f);
            m_ViewHeight = view.height;
            current.Scroll = Mathf.Clamp(current.Scroll, 0f, Mathf.Max(0f, m_ContentHeight - m_ViewHeight));

            GUI.BeginGroup(view);
            Ui.Origin = new Vector2(view.x, view.y - current.Scroll);
            Ui.Clip = view;
            GUILayout.BeginArea(new Rect(0f, -current.Scroll, view.width, 30000f));
            GUILayout.BeginVertical();
            current.Draw();
            GUILayout.EndVertical();
            if (repaint) m_ContentHeight = GUILayoutUtility.GetLastRect().yMax + 12f;
            GUILayout.EndArea();
            GUI.EndGroup();

            // Scroll bar: click or drag anywhere on it.
            Ui.Origin = Vector2.zero;
            Ui.Clip = new Rect(0f, 0f, width, height);
            float max = m_ContentHeight - m_ViewHeight;
            if (max > 0f)
            {
                var bar = new Rect(view.xMax + 4f, view.y, barWidth, view.height);
                Ui.Fill(bar, ScrollTrack);
                float thumb = Mathf.Max(24f, bar.height * m_ViewHeight / m_ContentHeight);
                Ui.Fill(new Rect(bar.x, bar.y + (bar.height - thumb) * current.Scroll / max, bar.width, thumb), ScrollThumb);
                if (repaint && Ui.Held && bar.Contains(Ui.Pointer))
                    current.Scroll = max * Mathf.Clamp01((Ui.Pointer.y - bar.y - thumb / 2f) / (bar.height - thumb));
            }
            if (repaint) Ui.EndRepaint();
        }
    }
}
