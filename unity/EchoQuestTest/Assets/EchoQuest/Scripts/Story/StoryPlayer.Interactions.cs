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
        [Header("Physical thumb buttons (0 = unassigned; keyboard Y/N always available)")]
        [Range(0, 16)] public int yesThumbButton;
        [Range(0, 16)] public int noThumbButton;

        private readonly Queue<(int button, double time)> m_InteractionInput = new();
        private bool m_CapturingInteraction, m_InteractionPassed, m_InteractionAttempted;
        private double m_CaptureStart, m_CaptureInputStart;
        private int m_InteractionSelectedButton, m_InteractionConfirmButton;
        private int m_EncouragementIndex;
        public bool WaitingInteractionInput => m_CapturingInteraction;
        public bool WaitingRetryChoice { get; private set; }

        private void OnEnable() => InputSystem.onEvent += CaptureInteractionInput;

        private void OnDisable()
        {
            InputSystem.onEvent -= CaptureInteractionInput;
            EndInteractionCapture();
        }

        private void CaptureInteractionInput(InputEventPtr ev, InputDevice device)
        {
            if (!m_CapturingInteraction || Paused ||
                (!ev.IsA<StateEvent>() && !ev.IsA<DeltaStateEvent>())) return;
            if (ev.time < m_CaptureInputStart) return;
            double time = m_CaptureStart + (ev.time - m_CaptureInputStart);
            void Read(ButtonControl button, int number)
            {
                if (button != null && button.ReadValueFromEvent(ev, out float value) &&
                    value >= button.pressPointOrDefault && !button.isPressed)
                    m_InteractionInput.Enqueue((number, time));
            }
            if (device is Keyboard keyboard)
            {
                Read(keyboard.digit1Key, 1); Read(keyboard.digit2Key, 2);
                Read(keyboard.digit3Key, 3); Read(keyboard.digit4Key, 4);
                Read(keyboard.numpad1Key, 1); Read(keyboard.numpad2Key, 2);
                Read(keyboard.numpad3Key, 3); Read(keyboard.numpad4Key, 4);
                Read(keyboard.yKey, StoryInteractionRules.Yes);
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
            m_CaptureStart = AudioSettings.dspTime;
            m_CaptureInputStart = InputState.currentTime;
        }

        private bool InteractionClip(string id, out AudioClip clip)
        {
            if (m_Sounds.TryGetValue(id, out clip) || m_ClipsByName.TryGetValue(id, out clip)) return true;
            return false;
        }

        private List<AudioClip> InteractionVoiceClips(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (m_ClipsByName.TryGetValue(id, out var clip)) return new List<AudioClip> { clip };
            return VoiceParts(id, out _);
        }

        private bool InteractionVoiceExists(string id) => InteractionVoiceClips(id) is { Count: > 0 };

        private bool InteractionAssetsReady(InteractionSettings settings)
        {
            if (!InteractionVoiceExists(settings.Prompt)) return false;
            if (!string.IsNullOrEmpty(settings.Success) && !InteractionVoiceExists(settings.Success)) return false;
            if (!string.IsNullOrEmpty(settings.Decline) && !InteractionVoiceExists(settings.Decline)) return false;
            if (settings.AttemptsPerRound > 0)
            {
                if (!InteractionVoiceExists(settings.RetryQuestion)) return false;
                foreach (var voice in settings.ContinueVoices) if (!InteractionVoiceExists(voice)) return false;
            }
            if (settings.Mode is not (InteractionMode.Confirm or InteractionMode.Decision) && !InteractionVoiceExists(settings.Retry)) return false;
            foreach (var example in settings.Examples) if (!InteractionClip(example, out _)) return false;
            return string.IsNullOrEmpty(settings.Feedback) || InteractionClip(settings.Feedback, out _);
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

        private void InteractionTapFeedback(InteractionSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Feedback)) return;
            InteractionClip(settings.Feedback, out var clip);
            var item = new StoryItem { Id = settings.Feedback, Kind = StoryItemKind.Sound };
            Spawn(clip, StagingFor(item), item.Id, soundMarker);
        }

        private IEnumerator PlayInteraction(StoryItem item, InteractionSettings settings)
        {
            if (!InteractionAssetsReady(settings))
            {
                Status = "Mancano gli audio dell'interazione " + item.Id;
                Debug.LogError("[Story] " + Status);
                while (!Interrupted) yield return null;
                yield break;
            }
            if (settings.Mode == InteractionMode.Decision &&
                (!StagingFor(item).Choices.ContainsKey(StoryInteractionRules.Yes) ||
                 !StagingFor(item).Choices.ContainsKey(StoryInteractionRules.No)))
            {
                Status = "Mancano i rami Sì/No per " + item.Id;
                Debug.LogError("[Story] " + Status);
                while (!Interrupted) yield return null;
                yield break;
            }
            try
            {
                bool firstAttempt = true;
                int failedAttempts = 0;
                while (!Interrupted)
                {
                    m_Foreground.RemoveAll(source => source == null);
                    if (firstAttempt && settings.Mode != InteractionMode.Compare)
                    {
                        yield return InteractionVoice(settings.Prompt,
                            settings.Mode is InteractionMode.Rhythm or InteractionMode.Note);
                        if (Interrupted) yield break;
                    }
                    if (settings.Mode is InteractionMode.Rhythm or InteractionMode.Compare)
                        foreach (var example in settings.Examples)
                        {
                            yield return InteractionExample(example, settings.Mode != InteractionMode.Rhythm);
                            if (Interrupted) yield break;
                        }
                    if (firstAttempt && settings.Mode == InteractionMode.Compare)
                    {
                        yield return InteractionVoice(settings.Prompt, pauseAfter: false);
                        if (Interrupted) yield break;
                    }
                    firstAttempt = false;
                    if (settings.Mode == InteractionMode.Note) yield return CaptureMissingNote(settings);
                    else yield return CaptureButtons(settings);
                    EndInteractionCapture();
                    if (Interrupted) yield break;
                    if (m_InteractionPassed)
                    {
                        if ((settings.Mode is InteractionMode.Confirm or InteractionMode.Decision) &&
                            StagingFor(item).Choices.TryGetValue(m_InteractionSelectedButton, out var branch))
                            BranchChoice = branch;
                        if (!string.IsNullOrEmpty(settings.Success)) yield return InteractionVoice(settings.Success);
                        yield break;
                    }
                    if (m_InteractionAttempted)
                    {
                        if (settings.AttemptsPerRound > 0 && ++failedAttempts >= settings.AttemptsPerRound)
                        {
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
            }
        }

        private IEnumerator RetryChoice(InteractionSettings settings)
        {
            yield return InteractionVoice(settings.RetryQuestion, pauseAfter: false);
            if (Interrupted) yield break;
            BeginInteractionCapture(true);
            WaitingRetryChoice = true;
            Status = "Ti va di riprovare? Y: sì · N: continuiamo la storia";
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

        private IEnumerator CaptureButtons(InteractionSettings settings)
        {
            BeginInteractionCapture(settings.Mode is InteractionMode.Confirm or InteractionMode.Compare or InteractionMode.Decision,
                settings.ConfirmButton);
            var taps = new List<double>();
            bool declined = false;
            Status = "Aspetto la tua risposta";
            double lastActivity = AudioSettings.dspTime;
            while (!Interrupted)
            {
                if (Paused) { yield return null; continue; }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (settings.Mode is InteractionMode.Compare or InteractionMode.Decision)
                    {
                        if (button != StoryInteractionRules.Yes && button != StoryInteractionRules.No) continue;
                        m_InteractionAttempted = true;
                        m_InteractionSelectedButton = button;
                        m_InteractionPassed = settings.Mode == InteractionMode.Decision || button == StoryInteractionRules.No;
                        yield break;
                    }
                    if (settings.Mode == InteractionMode.Confirm)
                    {
                        if (button == settings.ConfirmButton)
                        {
                            m_InteractionSelectedButton = button;
                            m_InteractionAttempted = m_InteractionPassed = true;
                            InteractionTapFeedback(settings);
                            if (!string.IsNullOrEmpty(settings.Feedback) && InteractionClip(settings.Feedback, out var feedback))
                                yield return Wait(feedback.length + pauseAfterSound);
                            yield break;
                        }
                        if (button == StoryInteractionRules.No && settings.ConfirmButton == StoryInteractionRules.Yes && !declined)
                        {
                            declined = true;
                            EndInteractionCapture();
                            if (!string.IsNullOrEmpty(settings.Decline)) yield return InteractionVoice(settings.Decline);
                            if (Interrupted) yield break;
                            BeginInteractionCapture(true, settings.ConfirmButton);
                        }
                        continue;
                    }
                    if (button != 1) continue;
                    lastActivity = AudioSettings.dspTime;
                    m_InteractionAttempted = true;
                    InteractionTapFeedback(settings);
                    taps.Add(time);
                    if (taps.Count == 3)
                    {
                        m_InteractionPassed = StoryInteractionRules.MatchesRhythm(taps, settings.Times);
                        // Let the last tap finish before the response voice.
                        if (!string.IsNullOrEmpty(settings.Feedback) && InteractionClip(settings.Feedback, out var feedback))
                            yield return Wait(feedback.length + pauseAfterSound);
                        yield break;
                    }
                }
                double wait = AudioSettings.dspTime - lastActivity;
                if (taps.Count > 0 && wait >= settings.Times[2] * 2 + 1) yield break;
                yield return null;
            }
        }

        private IEnumerator CaptureMissingNote(InteractionSettings settings)
        {
            InteractionClip(settings.Examples[0], out var clip);
            var sound = new StoryItem { Id = settings.Examples[0], Kind = StoryItemKind.Sound };
            double start = AudioSettings.dspTime + 0.15;
            Spawn(clip, StagingFor(sound), sound.Id, soundMarker, scheduledStart: start);
            BeginInteractionCapture(false);
            Status = "Premi nel momento della terza nota";
            CurrentClip = clip.name;
            double deadline = start + settings.Times[2] + settings.Tolerance;
            while (!Interrupted)
            {
                if (Paused) { yield return null; continue; }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (button != 1 || time < start) continue;
                    m_InteractionAttempted = true;
                    m_InteractionPassed |= StoryInteractionRules.HitsMissingNote(time, start, settings.Times, settings.Tolerance);
                }
                if (AudioSettings.dspTime >= deadline) break;
                yield return null;
            }
            // A complete listening window counts as a try even if no button was pressed.
            if (!Interrupted) m_InteractionAttempted = true;
            EndInteractionCapture();
            while (!Interrupted && AudioSettings.dspTime < start + clip.length + pauseAfterSound) yield return null;
        }
    }
}
