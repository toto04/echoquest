using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace EchoQuest.Story
{
    public partial class StoryPlayer
    {
        [Header("Physical thumb buttons (0 = unassigned; keyboard S/Y and N available)")]
        [Range(0, 16)] public int yesThumbButton;
        [Range(0, 16)] public int noThumbButton;

        private readonly Queue<(int button, double time)> m_InteractionInput = new();
        private bool m_CapturingInteraction, m_InteractionPassed, m_InteractionAttempted;
        private double m_CaptureStart, m_CaptureInputStart;
        private int m_InteractionSelectedButton, m_InteractionConfirmButton;
        private int m_EncouragementIndex;
        private const float MissingNoteResponseSeconds = 3f;
        public bool WaitingInteractionInput => m_CapturingInteraction;
        public bool WaitingRetryChoice { get; private set; }
        /// <summary>The mean time between the child's presses in the last free play with at least three
        /// presses (0 = not measured yet): the child's spontaneous tempo.</summary>
        public float SpontaneousBeatSeconds { get; private set; }
        /// <summary>Story time at which the piece of the current keep-the-beat try starts (tests, overlay).</summary>
        public double BeatStart { get; private set; }

        private void OnEnable()
        {
            InputSystem.onEvent += CaptureInteractionInput;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.pauseStateChanged += OnEditorPauseChanged;
#endif
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= CaptureInteractionInput;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.pauseStateChanged -= OnEditorPauseChanged;
#endif
            EndInteractionCapture();
        }

#if UNITY_EDITOR
        private void OnEditorPauseChanged(UnityEditor.PauseState state)
        {
            if (state == UnityEditor.PauseState.Unpaused) RebaseInteractionClock();
        }
#endif

        private void CaptureInteractionInput(InputEventPtr ev, InputDevice device)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPaused) return;
#endif
            if (!m_CapturingInteraction || Paused ||
                (!ev.IsA<StateEvent>() && !ev.IsA<DeltaStateEvent>())) return;
            if (ev.time < m_CaptureInputStart) return;
            double time = m_CaptureStart + (ev.time - m_CaptureInputStart) * Speed;
            void Read(ButtonControl button, int number)
            {
                if (button != null && button.ReadValueFromEvent(ev, out float value) &&
                    value >= button.pressPointOrDefault && !button.isPressed)
                {
                    m_InteractionInput.Enqueue((number, time));
                    LogEvent("input " + ButtonName(number));
                }
            }
            if (device is Keyboard keyboard)
            {
                Read(keyboard.digit1Key, 1); Read(keyboard.digit2Key, 2);
                Read(keyboard.digit3Key, 3); Read(keyboard.digit4Key, 4);
                Read(keyboard.numpad1Key, 1); Read(keyboard.numpad2Key, 2);
                Read(keyboard.numpad3Key, 3); Read(keyboard.numpad4Key, 4);
                Read(keyboard.yKey, StoryInteractionRules.Yes);
                Read(keyboard.sKey, StoryInteractionRules.Yes);
                Read(keyboard.nKey, StoryInteractionRules.No);
                if (WaitingChoice) Read(keyboard.spaceKey, m_InteractionConfirmButton);
            }
            else if (device is EchoQuest.Input.EchoQuestController)
            {
                for (int i = 1; i <= 4; i++) Read(device.TryGetChildControl<ButtonControl>("button" + i), i);
                if (yesThumbButton > 4 && noThumbButton > 4 && yesThumbButton != noThumbButton)
                {
                    Read(device.TryGetChildControl<ButtonControl>("button" + yesThumbButton), StoryInteractionRules.Yes);
                    Read(device.TryGetChildControl<ButtonControl>("button" + noThumbButton), StoryInteractionRules.No);
                }
            }
        }

        private void BeginInteractionCapture(bool choice, int confirmButton = StoryInteractionRules.Yes)
        {
            m_InteractionInput.Clear();
            m_CapturingInteraction = true;
            RebaseInteractionClock();
            WaitingChoice = choice;
            m_InteractionConfirmButton = confirmButton;
            m_InteractionSelectedButton = 0;
            m_InteractionPassed = m_InteractionAttempted = false;
            m_ForcePass = false;
        }

        private void EndInteractionCapture()
        {
            m_CapturingInteraction = false;
            WaitingRetryChoice = false;
            WaitingChoice = false;
            m_InteractionInput.Clear();
        }

        private void RebaseInteractionClock()
        {
            if (!m_CapturingInteraction) return;
            // A fixed offset avoids rounding a fresh press back into the preceding DSP buffer.
            m_CaptureStart = Now;
            m_CaptureInputStart = InputState.currentTime;
        }

        /// <summary>An example or button sound, in the chosen register when it has variants (x_s02_ritmo_grave).</summary>
        private bool InteractionClip(string id, out AudioClip clip)
        {
            if (m_ClipsByName.TryGetValue(id + "_" + Register, out clip)) return true;
            if (SoundClip(id, out clip) || m_ClipsByName.TryGetValue(id, out clip)) return true;
            return false;
        }

        private List<AudioClip> InteractionVoiceClips(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (m_ClipsByName.TryGetValue(id, out var clip)) return new List<AudioClip> { clip };
            return VoiceParts(id, out _);
        }

        private bool InteractionVoiceExists(string id) => InteractionVoiceClips(id) is { Count: > 0 };

        /// <summary>The first voice or clip the interaction needs that has no recording, or null when all exist.</summary>
        private string MissingInteractionAsset(InteractionSettings settings)
        {
            var voices = new List<string>();
            if (!string.IsNullOrEmpty(settings.Prompt)) voices.Add(settings.Prompt);
            voices.AddRange(settings.SuccessVoices);
            if (settings.Mode == InteractionMode.Battle) voices.AddRange(settings.RetryVoices);
            voices.AddRange(settings.DeclineVoices);
            if (settings.AttemptsPerRound > 0)
            {
                if (!string.IsNullOrEmpty(settings.RetryQuestion)) voices.Add(settings.RetryQuestion);
                voices.AddRange(settings.ContinueVoices);
            }
            // Every exercise that can fail says so, except practice echoes (no taunt, the call just comes back).
            if (settings.Mode is not (InteractionMode.Confirm or InteractionMode.Decision or InteractionMode.FreePlay) &&
                !(settings.Mode == InteractionMode.Battle && settings.Practice))
                voices.Add(settings.Retry ?? "");
            foreach (var voice in voices) if (!InteractionVoiceExists(voice)) return voice.Length > 0 ? voice : "(riprova)";
            var clips = new List<string>(settings.Examples);
            clips.AddRange(settings.Feedbacks);
            clips.AddRange(settings.Help);
            if (!string.IsNullOrEmpty(settings.StrikeSound)) clips.Add(settings.StrikeSound);
            if (!string.IsNullOrEmpty(settings.Gust)) clips.Add(settings.Gust);
            foreach (var clip in clips) if (!InteractionClip(clip, out _)) return clip;
            return null;
        }

        private IEnumerator InteractionVoice(string id, bool pauseAfter = true)
        {
            var item = m_Items.Find(i => i.Id == id) ?? new StoryItem
            {
                Id = id, Character = "MAESTRO BATTUTA", Kind = StoryItemKind.Voice,
            };
            var staging = m_Staging.TryGetValue(id, out var explicitStaging) ? explicitStaging :
                m_Staging.TryGetValue("maes", out var maestroStaging) ? maestroStaging : StagingEntry.Default;
            foreach (var clip in InteractionVoiceClips(id))
            {
                if (Interrupted) yield break;
                CurrentClip = clip.name;
                Status = "Consegna o risposta del Maestro";
                var source = Spawn(clip, staging, item.Character, MarkerFor(item), isVoice: true);
                yield return WaitForPlayback(source);
            }
            if (pauseAfter) yield return Wait(pauseAfterLine);
        }

        private IEnumerator InteractionExample(string id, bool pauseAfter = true)
        {
            if (Interrupted) yield break;
            InteractionClip(id, out var clip);
            var item = new StoryItem { Id = id, Kind = StoryItemKind.Sound };
            CurrentClip = clip.name;
            Status = "Ascolta l'esempio";
            var source = Spawn(clip, StagingFor(item), id, soundMarker);
            yield return WaitForPlayback(source);
            if (pauseAfter) yield return Wait(pauseAfterSound);
        }

        /// <summary>The sound of a button: suono_pulsante lists one sound per note button, or one for all.</summary>
        private string FeedbackFor(InteractionSettings settings, int button)
        {
            var list = settings.Feedbacks;
            if (list.Length == 0) return null;
            return button >= 1 && button <= list.Length ? list[button - 1] : list[0];
        }

        private void InteractionTapFeedback(InteractionSettings settings, int button = 1)
        {
            string id = FeedbackFor(settings, button);
            if (string.IsNullOrEmpty(id) || !InteractionClip(id, out var clip)) return;
            var item = new StoryItem { Id = id, Kind = StoryItemKind.Sound };
            Spawn(clip, StagingFor(item), item.Id, soundMarker);
        }

        private float FeedbackLength(InteractionSettings settings, int button) =>
            FeedbackFor(settings, button) is { } id && InteractionClip(id, out var clip) ? clip.length : 0f;

        private IEnumerator PlayInteraction(StoryItem item, InteractionSettings settings)
        {
            if (MissingInteractionAsset(settings) is { } missing)
            {
                Status = $"Mancano gli audio dell'interazione {item.Id}: {missing}";
                Debug.LogError("[Story] " + Status);
                while (!Interrupted) yield return null;
                yield break;
            }
            if (settings.Mode == InteractionMode.Battle)
            {
                yield return PlayBattleRound(item, settings);
                yield break;
            }
            if (settings.Mode == InteractionMode.FreePlay)
            {
                yield return PlayFreePlay(item, settings);
                yield break;
            }
            if (settings.Mode == InteractionMode.Decision && StagingFor(item).Choices.Count < 2)
            {
                Status = "Mancano i rami della scelta per " + item.Id;
                Debug.LogError("[Story] " + Status);
                while (!Interrupted) yield return null;
                yield break;
            }
            CurrentInteraction = settings;
            LastAttempt = "";
            try
            {
                bool firstAttempt = true;
                int failedAttempts = 0, tries = 0;
                while (!Interrupted)
                {
                    m_Foreground.RemoveAll(source => source == null);
                    // No prompt: the story lines and sounds just before the interaction asked and played the
                    // examples, so the first attempt starts listening straight away.
                    bool introducedByStory = firstAttempt && string.IsNullOrEmpty(settings.Prompt);
                    if (firstAttempt && !introducedByStory && settings.Mode != InteractionMode.Compare)
                    {
                        yield return InteractionVoice(settings.Prompt,
                            settings.Mode is InteractionMode.Rhythm or InteractionMode.Note);
                        if (Interrupted) yield break;
                    }
                    bool examplesJustPlayed = introducedByStory || (firstAttempt && settings.ExamplesPlayedByStory);
                    if ((settings.Mode is InteractionMode.Rhythm or InteractionMode.Compare) && !examplesJustPlayed)
                        foreach (var example in settings.Examples)
                        {
                            yield return InteractionExample(example, settings.Mode != InteractionMode.Rhythm);
                            if (Interrupted) yield break;
                        }
                    if (firstAttempt && !introducedByStory && settings.Mode == InteractionMode.Compare)
                    {
                        yield return InteractionVoice(settings.Prompt, pauseAfter: false);
                        if (Interrupted) yield break;
                    }
                    firstAttempt = false;
                    if (settings.Mode == InteractionMode.Note) yield return CaptureMissingNote(settings);
                    else if (settings.Mode == InteractionMode.Beat) yield return CaptureBeat(settings, help: tries > 0);
                    else yield return CaptureButtons(settings, StagingFor(item).Choices);
                    tries++;
                    EndInteractionCapture();
                    if (Interrupted) yield break;
                    LogEvent(item.Id + (m_InteractionPassed ? " passed" : m_InteractionAttempted ? " not passed" : " no attempt"));
                    if (m_InteractionPassed)
                    {
                        if ((settings.Mode is InteractionMode.Confirm or InteractionMode.Decision) &&
                            StagingFor(item).Choices.TryGetValue(m_InteractionSelectedButton, out var branch))
                        {
                            BranchChoice = branch;
                            LogEvent("branch " + branch);
                            RememberNextChapter(item, branch);
                        }
                        if (settings.Registers.TryGetValue(m_InteractionSelectedButton, out var register)) SetRegister(register);
                        if (!string.IsNullOrEmpty(settings.Success)) yield return InteractionVoice(settings.Success);
                        yield break;
                    }
                    if (m_InteractionAttempted)
                    {
                        if (settings.AttemptsPerRound > 0 && ++failedAttempts >= settings.AttemptsPerRound)
                        {
                            if (string.IsNullOrEmpty(settings.RetryQuestion))
                            {
                                // No question: after the attempts the story goes on (the drum gives the stroke).
                                LogEvent(item.Id + " given after " + failedAttempts + " tries");
                                yield return InteractionVoice(settings.ContinueVoices[0]);
                                yield break;
                            }
                            yield return RetryChoice(settings);
                            if (Interrupted) yield break;
                            if (m_InteractionSelectedButton == StoryInteractionRules.No)
                            {
                                string encouragement = settings.ContinueVoices[m_EncouragementIndex % settings.ContinueVoices.Length];
                                m_EncouragementIndex = (m_EncouragementIndex + 1) % settings.ContinueVoices.Length;
                                yield return InteractionVoice(encouragement);
                                yield break;
                            }
                            failedAttempts = 0;
                        }
                        yield return InteractionVoice(settings.Retry);
                    }
                }
            }
            finally
            {
                EndInteractionCapture();
                CurrentInteraction = null;
            }
        }

        /// <summary>Free play: the piece plays and every press sounds its piano note, in the chosen register.
        /// Every button fits the harmony (docs/music/01-palette.md), so nothing is judged; the story goes on
        /// when the piece ends. "Pass it for me" ends it early.</summary>
        private IEnumerator PlayFreePlay(StoryItem item, InteractionSettings settings)
        {
            CurrentInteraction = settings;
            LastAttempt = "";
            try
            {
                if (!string.IsNullOrEmpty(settings.Prompt))
                {
                    yield return InteractionVoice(settings.Prompt);
                    if (Interrupted) yield break;
                }
                InteractionClip(settings.Examples[0], out var piece);
                double start = Now + 0.1;
                var source = Spawn(piece, StagingFor(new StoryItem { Id = settings.Examples[0], Kind = StoryItemKind.Sound }),
                    settings.Examples[0], soundMarker, scheduledStart: start);
                BeginInteractionCapture(false);
                Status = "Suona quando vuoi: ogni nota sta bene con la musica";
                CurrentClip = piece.name;
                int notes = 0;
                double firstTap = 0, lastTap = 0;
                var taps = new List<double>();
                int gusts = 0;
                double nextGust = start + 1.5;
                while (!Interrupted && Now < start + piece.length)
                {
                    if (m_ForcePass) { source.FadeOut(0.5f); break; }
                    if (!string.IsNullOrEmpty(settings.Gust) && Now >= nextGust - 0.1 && Now < start + piece.length - 2.0)
                    {
                        // Chapter 2, scene 5: Sbuffo's gusts drift onto the child's beat, a little more each time.
                        double at = GustTime(taps, nextGust, ++gusts, out double offBeat);
                        ScheduleBattleClip(settings.Gust, at);
                        LogEvent(double.IsNaN(offBeat) ? $"gust {gusts}, no beat yet" : $"gust {gusts}, {offBeat * 1000:F0} ms off the child's beat");
                        nextGust = at + 1.2 + (taps.Count >= 2 ? 0.0 : 0.6);
                    }
                    while (m_InteractionInput.Count > 0)
                    {
                        var (button, time) = m_InteractionInput.Dequeue();
                        if (button < 1 || button > 4) continue;
                        InteractionTapFeedback(settings, button);
                        if (notes == 0) firstTap = time;
                        lastTap = time;
                        taps.Add(time);
                        notes++;
                        LastAttempt = notes + (notes == 1 ? " note played" : " notes played");
                    }
                    yield return null;
                }
                EndInteractionCapture();
                if (Interrupted) yield break;
                // The child's own pace (chapter 2, scene 1): the mean time between presses.
                if (notes >= 3) SpontaneousBeatSeconds = (float)((lastTap - firstTap) / (notes - 1));
                LogEvent(item.Id + " passed (" + notes + " notes" +
                         (notes >= 3 ? $", one every {SpontaneousBeatSeconds:F2} s" : "") + ")");
                if (!string.IsNullOrEmpty(settings.Success)) yield return InteractionVoice(settings.Success);
            }
            finally
            {
                EndInteractionCapture();
                CurrentInteraction = null;
            }
        }

        /// <summary>When Sbuffo's next gust blows: with no beat from the child, a little after
        /// <paramref name="planned"/>, never on a pulse; once the child taps, it moves towards the child's next
        /// beat (last press + the mean gap), closer with every gust, from the fourth on exactly on it.</summary>
        private double GustTime(List<double> taps, double planned, int gust, out double offBeat)
        {
            double wander = planned + 0.37 * ((gust * 7) % 5) / 4.0;
            offBeat = double.NaN;
            if (taps.Count < 2) return System.Math.Max(Now + 0.1, wander);
            int n = System.Math.Min(5, taps.Count - 1);
            double period = (taps[taps.Count - 1] - taps[taps.Count - 1 - n]) / n;
            period = System.Math.Clamp(period, 0.3, 1.5);
            double beat = taps[taps.Count - 1];
            while (beat < Now + 0.1) beat += period;
            double pull = System.Math.Min(1.0, gust / 4.0);
            double at = System.Math.Max(Now + 0.1, wander + (beat - wander) * pull);
            offBeat = at - beat;
            return at;
        }

        private IEnumerator RetryChoice(InteractionSettings settings)
        {
            yield return InteractionVoice(settings.RetryQuestion, pauseAfter: false);
            if (Interrupted) yield break;
            BeginInteractionCapture(true);
            WaitingRetryChoice = true;
            Status = "Ti va di riprovare? S/Y: sì · N: continuiamo la storia";
            try
            {
                while (!Interrupted)
                {
                    if (Paused) { yield return null; continue; }
                    while (m_InteractionInput.Count > 0)
                    {
                        var (button, _) = m_InteractionInput.Dequeue();
                        if (button != StoryInteractionRules.Yes && button != StoryInteractionRules.No) continue;
                        m_InteractionSelectedButton = button;
                        yield break;
                    }
                    yield return null;
                }
            }
            finally { EndInteractionCapture(); }
        }

        private IEnumerator CaptureButtons(InteractionSettings settings, Dictionary<int, string> choices)
        {
            // A choice made with the note buttons (Scene 2: index = low, middle = high): Space picks the first.
            bool noteChoice = settings.Mode == InteractionMode.Decision && !choices.ContainsKey(StoryInteractionRules.Yes);
            int firstChoice = int.MaxValue;
            foreach (var key in choices.Keys) firstChoice = Mathf.Min(firstChoice, key);
            BeginInteractionCapture(settings.Mode is InteractionMode.Confirm or InteractionMode.Compare or InteractionMode.Decision,
                noteChoice ? firstChoice : settings.ConfirmButton);
            var taps = new List<double>();
            bool declined = false;
            int declineIndex = 0;
            Status = "Aspetto la tua risposta";
            double lastActivity = Now;
            while (!Interrupted)
            {
                if (Paused) { yield return null; continue; }
                if (m_ForcePass)
                {
                    m_InteractionSelectedButton = settings.Mode == InteractionMode.Compare ? settings.ExpectedAnswer :
                        noteChoice ? firstChoice : settings.ConfirmButton;
                    m_InteractionAttempted = m_InteractionPassed = true;
                    LastAttempt = "passed from the overlay";
                    yield break;
                }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (settings.Mode is InteractionMode.Compare or InteractionMode.Decision)
                    {
                        bool valid = noteChoice ? choices.ContainsKey(button)
                            : button == StoryInteractionRules.Yes || button == StoryInteractionRules.No;
                        if (!valid) continue;
                        m_InteractionAttempted = true;
                        m_InteractionSelectedButton = button;
                        m_InteractionPassed = settings.Mode == InteractionMode.Decision || button == settings.ExpectedAnswer;
                        yield break;
                    }
                    if (settings.Mode == InteractionMode.Confirm)
                    {
                        if (button == settings.ConfirmButton)
                        {
                            m_InteractionSelectedButton = button;
                            m_InteractionAttempted = m_InteractionPassed = true;
                            InteractionTapFeedback(settings, button);
                            float length = FeedbackLength(settings, button);
                            if (length > 0) yield return Wait(length + pauseAfterSound);
                            yield break;
                        }
                        if (button == StoryInteractionRules.No && settings.ConfirmButton == StoryInteractionRules.Yes &&
                            settings.DeclineVoices.Length > 0 && (!declined || settings.DeclineVoices.Length > 1))
                        {
                            declined = true;
                            EndInteractionCapture();
                            yield return InteractionVoice(settings.DeclineVoices[declineIndex]);
                            declineIndex = (declineIndex + 1) % settings.DeclineVoices.Length;
                            if (Interrupted) yield break;
                            BeginInteractionCapture(true, settings.ConfirmButton);
                            Status = "Aspetto la tua risposta: S/Y sì · N no";
                        }
                        continue;
                    }
                    if (button != 1) continue;
                    lastActivity = Now;
                    m_InteractionAttempted = true;
                    InteractionTapFeedback(settings, button);
                    taps.Add(time);
                    if (taps.Count == 3)
                    {
                        m_InteractionPassed = StoryInteractionRules.MatchesRhythm(taps, settings.Times);
                        LastAttempt = $"taps at 0.00, {taps[1] - taps[0]:F2}, {taps[2] - taps[0]:F2} s; " +
                            $"example {settings.Times[0]:F2}, {settings.Times[1]:F2}, {settings.Times[2]:F2} s: " +
                            (m_InteractionPassed ? "matches" : "does not match");
                        // Let the last tap finish before the response voice.
                        float length = FeedbackLength(settings, button);
                        if (length > 0) yield return Wait(length + pauseAfterSound);
                        yield break;
                    }
                }
                double wait = Now - lastActivity;
                if (taps.Count > 0 && wait >= settings.Times[2] * 2 + 1) yield break;
                yield return null;
            }
        }

        /// <summary>Keep the beat (chapter 2, scene 2): the piece plays, every press sounds its drum, and the
        /// beats listed in tempi are judged; the try passes when at least quota of them got a press within
        /// tolleranza. After a failed try the Maestro counts the first beats aloud (aiuto). The piece stops
        /// one beat after the last judged beat.</summary>
        private IEnumerator CaptureBeat(InteractionSettings settings, bool help)
        {
            string id = settings.Examples[0];
            InteractionClip(id, out var piece);
            double start = Now + 0.15;
            BeatStart = start;
            var source = Spawn(piece, StagingFor(new StoryItem { Id = id, Kind = StoryItemKind.Sound }), id, soundMarker,
                scheduledStart: start);
            var times = settings.Times;
            double beat = times[1] - times[0];
            double end = start + times[times.Length - 1] + beat;
            bool cut = end < start + piece.length - 0.05;
            if (help)
                for (int i = 0; i < settings.Help.Length; i++)
                {
                    InteractionClip(settings.Help[i], out var word);
                    var wordItem = new StoryItem { Id = settings.Help[i], Kind = StoryItemKind.Sound };
                    Spawn(word, StagingFor(wordItem), settings.Help[i], characterMarker, scheduledStart: start + times[i]);
                }
            BeginInteractionCapture(false);
            Status = help ? "Batti il tempo: il Maestro conta con te" : "Batti il tempo insieme alla musica";
            CurrentClip = piece.name;
            var presses = new List<double>();
            bool forced = false;
            while (!Interrupted && Now < end)
            {
                if (m_ForcePass) { forced = true; break; }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (button < 1 || button > 4) continue;
                    if (settings.Strike)
                    {
                        // One stroke only: on the beat it is the biggest drum of all, otherwise an ordinary one.
                        presses.Add(time);
                        bool onTime = System.Math.Abs(time - (start + times[times.Length - 1])) <= settings.Tolerance;
                        if (onTime && InteractionClip(settings.StrikeSound, out var big))
                            Spawn(big, StagingFor(new StoryItem { Id = settings.StrikeSound, Kind = StoryItemKind.Sound }),
                                settings.StrikeSound, soundMarker);
                        else InteractionTapFeedback(settings, button);
                        break;
                    }
                    InteractionTapFeedback(settings, button);
                    presses.Add(time);
                }
                if (settings.Strike && presses.Count > 0) break;
                yield return null;
            }
            EndInteractionCapture();
            if ((cut || forced) && source != null) source.FadeOut(0.4f);
            if (Interrupted) yield break;
            // A whole listen counts as a try, even with no press.
            m_InteractionAttempted = true;
            var beats = new List<double>(times.Length);
            if (settings.Strike) beats.Add(start + times[times.Length - 1]);   // only the stroke's beat is judged
            else foreach (float t in times) beats.Add(start + t);
            int hits = StoryInteractionRules.BeatHits(presses, beats, settings.Tolerance);
            m_InteractionPassed = forced || hits >= settings.Quota;
            LastAttempt = forced ? "passed from the overlay" :
                settings.Strike ? (presses.Count == 0 ? "no stroke" :
                    $"stroke {(presses[0] - beats[0]) * 1000:+0;-0} ms from the beat (within {settings.Tolerance * 1000:F0} ms)" +
                    (m_InteractionPassed ? ": BUM" : ": not on the beat")) :
                $"{hits} of {times.Length} beats hit within {settings.Tolerance * 1000:F0} ms (needed {settings.Quota}); " +
                $"{presses.Count} presses" + (m_InteractionPassed ? ": passes" : ": not yet");
            LogEvent("beat " + LastAttempt);
            if (settings.Strike) yield return Wait(m_InteractionPassed ? 1.5f : 0.6f);   // let the big drum ring
            else if (!cut && !forced) while (!Interrupted && Now < start + piece.length) yield return null;
            else yield return Wait(0.4f);
        }

        private IEnumerator CaptureMissingNote(InteractionSettings settings)
        {
            InteractionClip(settings.Examples[0], out var clip);
            var sound = new StoryItem { Id = settings.Examples[0], Kind = StoryItemKind.Sound };
            double start = Now + 0.15;
            var source = Spawn(clip, StagingFor(sound), sound.Id, soundMarker, scheduledStart: start);
            source.Source.bypassReverbZones = true;
            BeginInteractionCapture(false);
            Status = "Premi nel momento della terza nota";
            CurrentClip = clip.name;
            double deadline = start + clip.length + MissingNoteResponseSeconds;
            while (!Interrupted)
            {
                if (Paused) { yield return null; continue; }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (button != 1 || time < start) continue;
                    m_InteractionAttempted = true;
                    bool hit = StoryInteractionRules.HitsMissingNote(time, start, settings.Times, settings.Tolerance, deadline);
                    m_InteractionPassed |= hit;
                    LastAttempt = $"press at {time - start:F2} s; third note at {settings.Times[2]:F2} s, " +
                        $"tolerance {settings.Tolerance:F2} s: " + (hit ? "hit" : "missed");
                }
                if (m_ForcePass)
                {
                    m_InteractionAttempted = m_InteractionPassed = true;
                    LastAttempt = "passed from the overlay";
                }
                if (m_InteractionPassed) break;
                if (Now >= deadline) break;
                if (Now >= start + clip.length) Status = "Ora premi 1 nel silenzio";
                yield return null;
            }
            // A complete listening window counts as a try even if no button was pressed.
            if (!Interrupted) m_InteractionAttempted = true;
            EndInteractionCapture();
            if (!Interrupted) Status = m_InteractionPassed ? "Nota riconosciuta" : "Ascolta la fine dell’esempio";
            while (!Interrupted && Now < start + clip.length + pauseAfterSound) yield return null;
        }
    }
}
