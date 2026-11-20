// The overlay's tabs. Each one only reads public state and calls public methods.
using System.Collections.Generic;
using System.Text;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoQuest.DebugUi
{
    public class StoryPanel : DebugPanel
    {
        public override string Title => "Story";
        private readonly StoryPlayer m_Player;
        private readonly List<string> m_Sections;
        private readonly List<string> m_Rows = new List<string>();
        private string m_RowsSection;
        private int m_Missing;
        private bool m_Subtitles;

        public StoryPanel(StoryPlayer player)
        {
            m_Player = player;
            m_Sections = player.Sections();
        }

        private void BuildRows()
        {
            m_RowsSection = m_Player.section;
            m_Rows.Clear();
            m_Missing = 0;
            foreach (var item in m_Player.SectionItems)
            {
                bool audio = m_Player.HasAudio(item);
                if (!audio) m_Missing++;
                string what = item.Kind switch
                {
                    StoryItemKind.Voice => item.Character,
                    StoryItemKind.Sound => "sound",
                    StoryItemKind.Interaction => "INTERACTION",
                    _ => "(branches join)",
                };
                string text = item.Text.Length > 44 ? item.Text.Substring(0, 44) + "..." : item.Text;
                m_Rows.Add((audio ? "" : "(no audio) ") + (item.Branch.Length > 0 ? "[" + item.Branch + "] " : "") +
                           item.Id + "  " + what + ": " + text);
            }
        }

        public override void Draw()
        {
            var p = m_Player;
            if (m_RowsSection != p.section) BuildRows();

            GUILayout.BeginHorizontal();
            if (Ui.Button(p.Paused ? "Play" : "Pause")) p.TogglePause();
            if (Ui.Button("Previous")) p.Previous();
            if (Ui.Button("Next")) p.Skip();
            if (Ui.Button("Repeat")) p.Repeat();
            if (Ui.Button("Restart")) p.Restart();
            GUILayout.EndHorizontal();

            // Pick a scene; "Play on" decides whether the chapter continues into the next scene.
            GUILayout.BeginHorizontal();
            foreach (var s in m_Sections)
                if (Ui.Button(StoryPlayer.SectionName(s), s == p.section)) p.SetSection(s);
            GUILayout.EndHorizontal();
            p.continueToNextSection = Ui.Toggle("Play on into the next scene (off = this scene only)", p.continueToNextSection);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Voice take", Ui.Small, GUILayout.Width(70f));
            if (Ui.Button("a", p.take == "a")) p.take = "a";
            if (Ui.Button("b", p.take == "b")) p.take = "b";
            GUILayout.Label("Branch", Ui.Small, GUILayout.Width(50f));
            if (Ui.Button("A", p.BranchChoice == "A")) p.ForceBranch("A");
            if (Ui.Button("B", p.BranchChoice == "B")) p.ForceBranch("B");
            m_Subtitles = Ui.Toggle("Subtitles", m_Subtitles);
            GUILayout.EndHorizontal();

            Ui.Header($"Now playing  ({p.ItemIndex + 1} of {p.ItemCount})");
            Ui.Text((p.Paused ? "<color=#ffcc66>PAUSED</color>  " : "") + p.Status);
            var item = p.Current;
            if (item != null)
            {
                Ui.Text("<b>" + (string.IsNullOrEmpty(item.Character) ? item.Id : item.Character + "  (" + item.Id + ")") + "</b>");
                if (!p.WaitingRetryChoice) Ui.Text(item.Text);
                if (!string.IsNullOrEmpty(p.CurrentClip)) Ui.Note("File: " + p.CurrentClip);
            }

            Ui.Header($"Timeline: {m_Rows.Count} items, {m_Missing} without audio. Click to jump.");
            var items = p.SectionItems;
            for (int i = 0; i < m_Rows.Count; i++)
            {
                var r = GUILayoutUtility.GetRect(100f, 20f, GUILayout.ExpandWidth(true));
                if (Ui.ButtonAt(new Rect(r.x, r.y, r.width, r.height - 1f), m_Rows[i], items[i] == item, Ui.RowText))
                    p.JumpTo(items[i].Id);
            }
        }

        public override void DrawScreen(float width, float height)
        {
            if (!m_Subtitles || m_Player.Current == null || m_Player.Current.Kind == StoryItemKind.Sound) return;
            var item = m_Player.Current;
            float w = Mathf.Min(900f, width - 40f);
            var r = new Rect((width - w) / 2f, height - 230f, w, 210f);
            GUI.Label(r, (string.IsNullOrEmpty(item.Character) ? "" : "<b>" + item.Character + "</b>\n") + item.Text, Ui.Big);
        }
    }

    public class InputPanel : DebugPanel
    {
        public override string Title => "Input";
        private readonly StoryPlayer m_Player;
        public InputPanel(StoryPlayer player) => m_Player = player;

        public override void Draw()
        {
            var p = m_Player;
            Ui.Header("Controller buttons");
            Ui.Note("The four note buttons (index to little finger) and the two thumb buttons.");
            GUILayout.BeginHorizontal();
            for (int b = 1; b <= 4; b++)
                if (Ui.Button(b.ToString(), false, GUILayout.Height(48f))) p.PressLogical(b);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Ui.Button("Yes", false, GUILayout.Height(48f))) p.PressLogical(StoryInteractionRules.Yes);
            if (Ui.Button("No", false, GUILayout.Height(48f))) p.PressLogical(StoryInteractionRules.No);
            GUILayout.EndHorizontal();

            Ui.Header("Waiting for");
            if (p.WaitingRetryChoice) Ui.Text("Try again? Yes repeats the exercise, No continues the story.");
            else if (p.WaitingInteractionInput) Ui.Text(p.WaitingChoice ? "An answer: Yes or No (or the button the Maestro asked for)." : "Taps on button 1.");
            else if (p.WaitingChoice) Ui.Text("A choice: buttons 1 to 4.");
            else Ui.Text("Nothing: the story is playing.");

            var it = p.CurrentInteraction;
            if (it != null)
            {
                Ui.Header("Current interaction: " + (p.Current != null ? p.Current.Id : ""));
                var sb = new StringBuilder();
                sb.Append("Mode: ").Append(it.Mode switch
                {
                    InteractionMode.Confirm => "confirm with one button",
                    InteractionMode.Rhythm => "repeat a three-tap rhythm",
                    InteractionMode.Compare => "same or different? (correct answer: " + StoryPlayer.ButtonName(it.ExpectedAnswer) + ")",
                    InteractionMode.Note => "press at the missing third note",
                    InteractionMode.Battle => "battle: echo the call after the TOC (buttons " + string.Join(" ", it.Buttons) + ")",
                    InteractionMode.FreePlay => "free play: any button, any time, no wrong note",
                    _ => "a choice picks a branch",
                }).AppendLine();
                sb.Append("Prompt: ").Append(it.Prompt);
                if (it.Examples.Length > 0) sb.AppendLine().Append("Examples: ").Append(string.Join(", ", it.Examples));
                if (it.Times.Length > 0)
                    sb.AppendLine().Append("Expected times: ").Append(string.Join(", ", System.Array.ConvertAll(it.Times, t => t.ToString("F2")))).Append(" s");
                if (p.BattleRunning) sb.AppendLine().Append("Battle layers: ").Append(p.BattleLayers).Append(" · register ").Append(p.Register);
                if (it.Mode == InteractionMode.Note) sb.AppendLine().Append($"Tolerance: {it.Tolerance:F2} s before the note");
                if (it.Mode == InteractionMode.Rhythm) sb.AppendLine().Append("Tolerance: each gap within 0.10 s or 30%; any speed from half to double");
                if (it.AttemptsPerRound > 0) sb.AppendLine().Append("Attempts before \"try again?\": ").Append(it.AttemptsPerRound);
                Ui.Text(sb.ToString());
                if (!string.IsNullOrEmpty(p.LastAttempt)) Ui.Text("<b>Last attempt:</b> " + p.LastAttempt);
                GUILayout.BeginHorizontal();
                if (Ui.Button("Pass it for me")) p.ForcePass();
                if (Ui.Button("Skip it")) p.Skip();
                if (Ui.Button("Hear it again")) p.Repeat();
                GUILayout.EndHorizontal();
            }

            Ui.Header("Keyboard");
            Ui.Note("1-4 note buttons · S or Y yes · N no (N skips a line while someone speaks) · Space pause · " +
                    "R repeat · Backspace restart · Left/Right turn the head · Home face forward · F9 record · F1 hide this panel");
        }
    }

    public class AudioPanel : DebugPanel
    {
        public override string Title => "Audio";
        private readonly StoryPlayer m_Player;
        private readonly float[] m_Defaults;
        private AudioReverbZone m_Reverb;

        public AudioPanel(StoryPlayer player)
        {
            m_Player = player;
            m_Defaults = new[]
            {
                player.pauseAfterLine, player.pauseOnSpeakerChange, player.pauseAroundInlineSound, player.pauseAfterSound,
                player.pauseAfterQuestion, player.duckGain, player.soundDuckGain, player.bedFadeSeconds,
            };
            m_Reverb = Object.FindAnyObjectByType<AudioReverbZone>(FindObjectsInactive.Include);
        }

        public override void Draw()
        {
            var p = m_Player;
            Ui.Header("Output");
            AudioListener.volume = Ui.Slider("Master volume", AudioListener.volume, 0f, 1f);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                Ui.Note("In a browser the reverb and the muffling are expected to have no effect.");
            GUILayout.BeginHorizontal();
            if (m_Reverb != null) m_Reverb.enabled = Ui.Toggle("Hall reverb", m_Reverb.enabled);
            p.distanceLowpass = Ui.Toggle("Distance muffling", p.distanceLowpass);
            p.ducking = Ui.Toggle("Ducking", p.ducking);
            GUILayout.EndHorizontal();
#if !UNITY_WEBGL || UNITY_EDITOR
            var recorder = p.listener != null ? p.listener.GetComponent<ListenerRecorder>() : null;
            if (recorder != null)
            {
                if (Ui.Button(recorder.IsRecording ? "Stop recording" : "Record what you hear to a WAV", recorder.IsRecording))
                    recorder.Toggle();
                if (!string.IsNullOrEmpty(recorder.LastFile)) Ui.Note("Last recording: " + recorder.LastFile);
            }
#endif

            Ui.Header("Pacing (seconds)");
            p.pauseAfterLine = Ui.Slider("After a line", p.pauseAfterLine, 0f, 3f);
            p.pauseOnSpeakerChange = Ui.Slider("When the speaker changes", p.pauseOnSpeakerChange, 0f, 3f);
            p.pauseAfterQuestion = Ui.Slider("After a question", p.pauseAfterQuestion, 0f, 4f);
            p.pauseAfterSound = Ui.Slider("After a sound", p.pauseAfterSound, 0f, 2f);
            p.pauseAroundInlineSound = Ui.Slider("Around a sound in a line", p.pauseAroundInlineSound, 0f, 2f);
            Ui.Header("Music beds");
            p.duckGain = Ui.Slider("Bed level under a voice", p.duckGain, 0f, 1f);
            p.soundDuckGain = Ui.Slider("Sound tail under a voice", p.soundDuckGain, 0f, 1f);
            p.bedFadeSeconds = Ui.Slider("Bed fade-out", p.bedFadeSeconds, 0f, 5f);
            if (Ui.Button("Reset pacing and beds"))
            {
                p.pauseAfterLine = m_Defaults[0]; p.pauseOnSpeakerChange = m_Defaults[1];
                p.pauseAroundInlineSound = m_Defaults[2]; p.pauseAfterSound = m_Defaults[3];
                p.pauseAfterQuestion = m_Defaults[4]; p.duckGain = m_Defaults[5];
                p.soundDuckGain = m_Defaults[6]; p.bedFadeSeconds = m_Defaults[7];
            }

            Ui.Header("Playing now: " + p.Active.Count);
            foreach (var s in p.Active)
            {
                if (s == null || s.Source == null || s.Source.clip == null) continue;
                var src = s.Source;
                string where = s.Is3D
                    ? $"{Vector3.Distance(s.transform.position, p.listener.position):F1} m, low-pass {s.CutoffHz / 1000f:F1} kHz"
                    : "in the head";
                Ui.Text($"<b>{s.Label}</b>  {src.clip.name}\n" +
                        $"<size=11>{(s.IsVoice ? "voice" : src.loop ? "loop" : "sound")} · volume {src.volume:F2} (gain {s.Gain:F2}) · " +
                        $"{where} · {src.time:F1} / {src.clip.length:F1} s</size>");
            }
        }
    }

    public class RoomPanel : DebugPanel
    {
        public override string Title => "Room";
        private readonly StoryPlayer m_Player;
        public RoomPanel(StoryPlayer player) => m_Player = player;

        public override void Draw()
        {
            var p = m_Player;
            Ui.Header("Listener");
            Ui.Note("The view behind this panel is the room from above. The white head is the listener; the nose points forward.");
            if (p.listener != null)
            {
                float yaw = Mathf.DeltaAngle(0f, p.listener.eulerAngles.y);
                float changed = Ui.Slider("Head turned (degrees)", yaw, -180f, 180f, "F0");
                if (!Mathf.Approximately(changed, yaw) && !p.headFollowsController)
                    p.listener.rotation = Quaternion.Euler(0f, changed, 0f);
            }
            GUILayout.BeginHorizontal();
            if (Ui.Button("Face forward")) p.RecenterHead();
            p.headFollowsController = Ui.Toggle("Head follows the controller", p.headFollowsController);
            GUILayout.EndHorizontal();
            if (p.headFollowsController && EchoQuest.Input.EchoQuestController.current == null)
                Ui.Note("No controller connected.");

            var item = p.Current;
            if (item == null) return;
            var st = p.StagingFor(item);
            Ui.Header("Staging of " + item.Id);
            var sb = new StringBuilder();
            sb.Append("Position: ").Append(st.Position.HasValue ? Describe(st.Position.Value) : "not spatial (inside the head)");
            if (st.MoveTo.HasValue) sb.AppendLine().Append("Moves to: ").Append(Describe(st.MoveTo.Value));
            sb.AppendLine().Append("Volume: ").Append(st.Volume.ToString("F2"));
            if (st.LowpassHz > 0) sb.AppendLine().Append("Fixed low-pass: ").Append(st.LowpassHz.ToString("F0")).Append(" Hz");
            if (st.Loop) sb.AppendLine().Append("Loops until: ").Append(string.IsNullOrEmpty(st.Until) ? "the end" : st.Until);
            if (st.Wait.HasValue) sb.AppendLine().Append("Next item starts after: ").Append(st.Wait.Value.ToString("F2")).Append(" s");
            if (st.PauseAfter.HasValue) sb.AppendLine().Append("Pause after: ").Append(st.PauseAfter.Value.ToString("F2")).Append(" s");
            if (!string.IsNullOrEmpty(st.Bed)) sb.AppendLine().Append("Bed underneath: ").Append(st.Bed);
            Ui.Text(sb.ToString());
            Ui.Note("Staging comes from capitolo_01_regia.csv. x is right, y is up, z is in front, in metres.");
        }

        private static string Describe(Vector3 v) => $"x {v.x:F1}, y {v.y:F1}, z {v.z:F1}";
    }

    public class PadPanel : DebugPanel
    {
        public override string Title => "Pad";
        private readonly StoryPlayer m_Player;
        private readonly CritterController m_Critter;
        private readonly StringBuilder m_Sb = new StringBuilder(1024);
        private string m_Text = "";

        public PadPanel(StoryPlayer player)
        {
            m_Player = player;
            m_Critter = Object.FindAnyObjectByType<CritterController>();
        }

        public override void Tick() => m_Text = ControllerDebugHud.BuildText(m_Sb, m_Critter, true);

        public override void Draw()
        {
            Ui.Text(m_Text);
            if (m_Player != null)
            {
                Ui.Header("Thumb buttons in the story");
                Ui.Note("Which HID buttons answer Yes and No. Buttons 1-4 are the notes, so both must be 5 or higher and different. 0 = not assigned.");
                m_Player.yesThumbButton = Stepper("Yes", m_Player.yesThumbButton);
                m_Player.noThumbButton = Stepper("No", m_Player.noThumbButton);
                bool valid = m_Player.yesThumbButton > 4 && m_Player.noThumbButton > 4 &&
                             m_Player.yesThumbButton != m_Player.noThumbButton;
                Ui.Note(valid ? "The controller can answer questions." : "Not valid yet: the controller cannot answer questions.");
            }
            Ui.Header("Haptics");
            Ui.Note("Not implemented in Unity yet (roadmap milestone 2).");
        }

        private static int Stepper(string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: HID button {value}", Ui.Label, GUILayout.Width(160f));
            if (Ui.Button("-", false, GUILayout.Width(40f))) value = Mathf.Max(0, value - 1);
            if (Ui.Button("+", false, GUILayout.Width(40f))) value = Mathf.Min(16, value + 1);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return value;
        }
    }

    public class LogPanel : DebugPanel
    {
        public override string Title => "Log";
        private readonly StoryPlayer m_Player;
        public LogPanel(StoryPlayer player) => m_Player = player;

        public override void Draw()
        {
            var p = m_Player;
            var missing = new StringBuilder();
            int count = 0;
            foreach (var item in p.SectionItems)
            {
                if (p.HasAudio(item)) continue;
                if (count++ < 12) missing.Append(count > 1 ? ", " : "").Append(item.Id);
            }
            Ui.Header($"Missing audio in {p.section}: {count}");
            if (count > 0) Ui.Note(missing + (count > 12 ? ", ..." : ""));

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Events, newest first ({p.Log.Count})", Ui.Head);
            if (Ui.Button("Clear", false, GUILayout.Width(60f))) p.Log.Clear();
            GUILayout.EndHorizontal();
            var sb = new StringBuilder();
            for (int i = p.Log.Count - 1, shown = 0; i >= 0 && shown < 120; i--, shown++) sb.AppendLine(p.Log[i]);
            Ui.Note(sb.ToString());
        }
    }

    public class DemoPanel : DebugPanel
    {
        public override string Title => "Demo";
        private readonly DebugOverlay m_Overlay;
        private float m_Fps;
        public DemoPanel(DebugOverlay overlay) => m_Overlay = overlay;

        public override void Tick() => m_Fps = Mathf.Lerp(m_Fps, 1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime), 0.05f);

        public override void Draw()
        {
            Ui.Header("Scenes");
            int active = SceneManager.GetActiveScene().buildIndex;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (Ui.Button(name, i == active) && i != active) SceneManager.LoadScene(i);
            }

            Ui.Header("Screen");
            Ui.Note("EchoQuest is an audio-only game. Everything on screen is for the people around the player.");
            m_Overlay.blackout = Ui.Toggle("Dark screen, as the player has it", m_Overlay.blackout);
            if (Ui.Button("Hide this panel (F1 brings it back)")) m_Overlay.visible = false;

            Ui.Header("Build");
            Ui.Text($"{Application.productName} {Application.version}\n" +
                    $"Unity {Application.unityVersion} · {Application.platform}\n" +
                    $"{m_Fps:F0} frames/s · audio {AudioSettings.outputSampleRate} Hz · audio clock {AudioSettings.dspTime:F1} s");
        }
    }
}
