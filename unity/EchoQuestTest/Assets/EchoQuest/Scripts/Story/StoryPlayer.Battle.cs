// Scene 3's battle (docs/music/04-battle-cues.md, 06-worked-example.md): a backing loop on the audio
// clock that never stops, long exchanges aligned to its chords (call, call again, the child's echo,
// a reply), the TOC on beat 4 as "your turn", a verdict on beat 4 of the echo bar, layers that grow
// with each success and lose only the top one on a failure, and a comic taunt instead of a buzzer.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoQuest.Story
{
    public partial class StoryPlayer
    {
        [Header("Battle (docs/music/04-battle-cues.md)")]
        [Tooltip("Battle music level under a voice line.")]
        [Range(0f, 1f)] public float battleDuckGain = 0.55f;
        [Tooltip("Bars per exchange: call, call again, echo, reply.")]
        public int exchangeBars = 4;
        [Tooltip("Pad level during call bars, when it sits below the call (03-backing.md: about -9 dB).")]
        [Range(0f, 1f)] public float callBarPadGain = 0.35f;

        private sealed class Battle
        {
            public double Start, Bar;
            public int Bars;
            public string Until = "";
            public SpatialSource L0, L1, L3;
            /// <summary>0 = groove only (L0); 1 adds the pad (L1); 2 adds the hits (L3).</summary>
            public int Layers;
            /// <summary>With the low register the pad is voiced above the call: silent in call bars.</summary>
            public bool HighPad;
        }

        private Battle m_Battle;
        private int m_BattleSuccessIndex, m_BattleTauntIndex;
        public bool BattleRunning => m_Battle != null;
        public int BattleLayers => m_Battle?.Layers ?? 0;
        public double BattleBarSeconds => m_Battle?.Bar ?? 0;
        /// <summary>Audio-clock time of the loop's first downbeat, and of the current exchange (tests, overlay).</summary>
        public double BattleStart => m_Battle?.Start ?? 0;
        public double ExchangeStart { get; private set; }

        private void StartBattle(StagingEntry staging, double start)
        {
            StopBattle();
            string prefix = staging.BattleBase;
            if (!InteractionClip(prefix + "_l0", out var l0))
            {
                Debug.LogError("[Story] Missing battle loop " + prefix + "_l0");
                return;
            }
            int bars = staging.BattleBars > 0 ? staging.BattleBars : 8;
            var battle = new Battle
            {
                Start = start, Bars = bars, Bar = (double)l0.samples / l0.frequency / bars,
                Until = staging.Until, HighPad = Register == "grave",
            };
            battle.L0 = BattleStem(prefix + "_l0", l0, start, 1f);
            if (InteractionClip(prefix + "_l1", out var l1)) battle.L1 = BattleStem(prefix + "_l1", l1, start, 0f);
            if (InteractionClip(prefix + "_l3", out var l3)) battle.L3 = BattleStem(prefix + "_l3", l3, start, 0f);
            m_Battle = battle;
            LogEvent($"battle loop {prefix}: {bars} bars of {battle.Bar:F2} s, register {Register}");
        }

        private SpatialSource BattleStem(string id, AudioClip clip, double start, float gain)
        {
            var staging = StagingFor(new StoryItem { Id = id, Kind = StoryItemKind.Sound });
            var go = new GameObject(clip.name + " (battaglia)", typeof(AudioSource), typeof(SpatialSource));
            go.transform.SetParent(transform, true);
            var src = go.GetComponent<SpatialSource>();
            src.Init(this, listener, clip, staging, id, soundMarker, loop: true, startGain: gain, scheduledStart: start);
            src.TargetGain = gain;
            src.ReleaseSpeed = 4f;   // a layer comes in within about one beat
            return src;
        }

        /// <summary>Stops the loop now (fade) or exactly at a bar line on the audio clock.</summary>
        private void StopBattle(double? at = null)
        {
            if (m_Battle == null) return;
            foreach (var stem in new[] { m_Battle.L0, m_Battle.L1, m_Battle.L3 })
            {
                if (stem == null) continue;
                if (at.HasValue) stem.Source.SetScheduledEndTime(at.Value);
                else stem.FadeOut(0.5f);
            }
            m_Battle = null;
        }

        /// <summary>When the item that ends the battle starts, the loop stops on the next bar line and a
        /// sound (the fanfare) starts exactly there. Returns that time for sounds, null otherwise.</summary>
        private double? EndBattleAt(StoryItem item)
        {
            if (m_Battle == null || m_Battle.Until != item.Id) return null;
            double at = NextBar(0.15);
            StopBattle(at);
            LogEvent("battle ends on the bar line");
            return item.Kind == StoryItemKind.Sound ? at : (double?)null;
        }

        private double NextBar(double lead)
        {
            var b = m_Battle;
            double k = System.Math.Ceiling((AudioSettings.dspTime + lead - b.Start) / b.Bar);
            return b.Start + System.Math.Max(0, k) * b.Bar;
        }

        /// <summary>The next exchange boundary: a bar where the chord changes, so call and echo share it.</summary>
        private double NextExchange(double lead)
        {
            var b = m_Battle;
            double k = System.Math.Ceiling((AudioSettings.dspTime + lead - b.Start) / b.Bar);
            k = System.Math.Ceiling(System.Math.Max(0, k) / exchangeBars) * exchangeBars;
            return b.Start + k * b.Bar;
        }

        private void UpdateBattleLayers(bool voicePlaying)
        {
            var b = m_Battle;
            if (b == null) return;
            double now = AudioSettings.dspTime;
            int bar = now < b.Start ? 0 : (int)((now - b.Start) / b.Bar) % b.Bars;
            bool callBar = bar % exchangeBars < 2;
            float duck = voicePlaying ? battleDuckGain : 1f;
            if (b.L0 != null) b.L0.TargetGain = duck;
            if (b.L1 != null) b.L1.TargetGain = (b.Layers >= 1 ? (callBar ? (b.HighPad ? 0f : callBarPadGain) : 1f) : 0f) * duck;
            if (b.L3 != null) b.L3.TargetGain = (b.Layers >= 2 ? 1f : 0f) * duck;
        }

        private void ScheduleBattleClip(string id, double at)
        {
            if (!InteractionClip(id, out var clip)) { Debug.LogError("[Story] Missing battle clip " + id); return; }
            var staging = StagingFor(new StoryItem { Id = id, Kind = StoryItemKind.Sound });
            Spawn(clip, staging, id, soundMarker, scheduledStart: System.Math.Max(at, AudioSettings.dspTime + 0.02));
        }

        private IEnumerator WaitUntilDsp(double time)
        {
            while (!Interrupted && AudioSettings.dspTime < time) yield return null;
        }

        private IEnumerator PlayBattleRound(StoryItem item, InteractionSettings settings)
        {
            if (m_Battle == null)
            {
                Status = "La sfida ha bisogno della base: in regia manca il suono con la colonna 'base'";
                Debug.LogError("[Story] " + Status + " (" + item.Id + ")");
                while (!Interrupted) yield return null;
                yield break;
            }
            CurrentInteraction = settings;
            LastAttempt = "";
            int failed = 0;
            try
            {
                while (!Interrupted)
                {
                    m_Foreground.RemoveAll(source => source == null);
                    double exchange = NextExchange(0.3);
                    ExchangeStart = exchange;
                    if (settings.Mode == InteractionMode.Battle) yield return EchoExchange(settings, exchange);
                    else yield return AnswerExchange(settings, exchange);
                    EndInteractionCapture();
                    if (Interrupted) yield break;
                    LogEvent(item.Id + (m_InteractionPassed ? " passed" : " not passed"));
                    if (m_InteractionPassed)
                    {
                        string voice = settings.SuccessVoices[m_BattleSuccessIndex++ % settings.SuccessVoices.Length];
                        yield return InteractionVoice(voice);
                        yield break;
                    }
                    // Failure: the taunt is already playing in the reply bar; Sbuffo laughs, then the same
                    // call comes back on the next exchange. Never slower (01-palette.md).
                    string taunt = settings.RetryVoices[m_BattleTauntIndex++ % settings.RetryVoices.Length];
                    yield return InteractionVoice(taunt, pauseAfter: false);
                    if (Interrupted) yield break;
                    if (settings.AttemptsPerRound > 0 && ++failed >= settings.AttemptsPerRound)
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
                        failed = 0;
                    }
                }
            }
            finally
            {
                EndInteractionCapture();
                CurrentInteraction = null;
            }
        }

        /// <summary>Bars 1-2: the call twice, the TOC on beat 4 of bar 2. Bar 3: the child's echo, the verdict
        /// on its beat 4. Bar 4: on success the piano plays the phrase back (the companion), on a failure
        /// Sbuffo's taunt. Judged on order and relative rhythm.</summary>
        private IEnumerator EchoExchange(InteractionSettings settings, double ex)
        {
            var b = m_Battle;
            double beat = b.Bar / 4;
            string call = settings.Examples[0];
            ScheduleBattleClip(call, ex);
            ScheduleBattleClip(call, ex + b.Bar);
            ScheduleBattleClip("x_s03_toc", ex + b.Bar + 3 * beat);
            double open = ex + b.Bar + 3 * beat - 0.15;     // from the TOC on, presses count
            double close = ex + 3 * b.Bar + 2 * beat;       // until the middle of the reply bar
            BeginInteractionCapture(false);
            Status = "Ascolta la frase due volte; al TOC ripetila";
            var taps = new List<(int button, double time)>();
            bool forced = false;
            while (!Interrupted)
            {
                if (Paused) { yield return null; continue; }
                if (m_ForcePass) { forced = true; break; }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (button < 1 || button > 4) continue;
                    InteractionTapFeedback(settings, button);   // wrong presses always sound
                    if (time < open || taps.Count >= settings.Buttons.Length) continue;
                    taps.Add((button, time));
                    m_InteractionAttempted = true;
                }
                if (taps.Count >= settings.Buttons.Length || AudioSettings.dspTime >= close) break;
                yield return null;
            }
            EndInteractionCapture();
            if (Interrupted) yield break;
            m_InteractionAttempted = true;
            m_InteractionPassed = forced || StoryInteractionRules.MatchesPhrase(taps, settings.Buttons, settings.Times);
            LastAttempt = forced ? "passed from the overlay" : DescribeTaps(taps, settings) + (m_InteractionPassed ? ": matches" : ": does not match");
            double verdict = System.Math.Max(ex + 2 * b.Bar + 3 * beat, NextBeat(0.05));
            yield return Verdict(settings, verdict, System.Math.Max(ex + 3 * b.Bar, NextBar(0.1)), call);
        }

        /// <summary>Bar 1: the head without its last note, the TOC on beat 4. Bar 2: the piano plays it again
        /// and the child supplies the missing note with the right button. Bar 3: the whole phrase.</summary>
        private IEnumerator AnswerExchange(InteractionSettings settings, double ex)
        {
            var b = m_Battle;
            double beat = b.Bar / 4;
            int last = settings.Times.Length - 1;
            ScheduleBattleClip(settings.Examples[0], ex);
            ScheduleBattleClip("x_s03_toc", ex + 3 * beat);
            ScheduleBattleClip(settings.Examples[0], ex + b.Bar);
            double answerBar = ex + b.Bar;
            double close = answerBar + settings.Times[last] + 0.45;
            BeginInteractionCapture(false);
            Status = "Al momento della nota mancante premi il pulsante " + settings.Buttons[last];
            bool hit = false;
            while (!Interrupted)
            {
                if (Paused) { yield return null; continue; }
                if (m_ForcePass) { hit = true; LastAttempt = "passed from the overlay"; break; }
                while (m_InteractionInput.Count > 0)
                {
                    var (button, time) = m_InteractionInput.Dequeue();
                    if (button < 1 || button > 4) continue;
                    InteractionTapFeedback(settings, button);
                    if (time < answerBar) continue;
                    bool ok = StoryInteractionRules.HitsAnswer(button, time - answerBar, settings.Buttons[last], settings.Times[last]);
                    LastAttempt = $"button {button} at {time - answerBar:F2} s; expected {settings.Buttons[last]} at {settings.Times[last]:F2} s: " + (ok ? "hit" : "missed");
                    hit |= ok;
                }
                if (hit || AudioSettings.dspTime >= close) break;
                yield return null;
            }
            EndInteractionCapture();
            if (Interrupted) yield break;
            m_InteractionAttempted = true;
            m_InteractionPassed = hit;
            if (string.IsNullOrEmpty(LastAttempt)) LastAttempt = "no press in the window";
            double verdict = System.Math.Max(answerBar + 3 * beat, NextBeat(0.05));
            yield return Verdict(settings, verdict, System.Math.Max(ex + 2 * b.Bar, NextBar(0.1)), settings.Examples[1]);
        }

        private IEnumerator Verdict(InteractionSettings settings, double stingAt, double replyBar, string reply)
        {
            var b = m_Battle;
            ScheduleBattleClip(m_InteractionPassed ? "x_s03_ok" : "x_s03_notyet", stingAt);
            if (m_InteractionPassed)
            {
                ScheduleBattleClip(reply, replyBar);                  // the companion plays the phrase back
                yield return WaitUntilDsp(replyBar + b.Bar);
                if (m_Battle != null) m_Battle.Layers = System.Math.Min(2, m_Battle.Layers + 1);   // on the bar line
            }
            else
            {
                ScheduleBattleClip("x_s03_taunt", replyBar);           // locked bar: Sbuffo laughs
                yield return WaitUntilDsp(replyBar);
                if (m_Battle != null) m_Battle.Layers = System.Math.Max(0, m_Battle.Layers - 1);   // only the top layer
                yield return WaitUntilDsp(replyBar + b.Bar * 0.6);
            }
        }

        private double NextBeat(double lead)
        {
            var b = m_Battle;
            double beat = b.Bar / 4;
            double k = System.Math.Ceiling((AudioSettings.dspTime + lead - b.Start) / beat);
            return b.Start + k * beat;
        }

        private static string DescribeTaps(List<(int button, double time)> taps, InteractionSettings settings)
        {
            if (taps.Count == 0) return "no press after the TOC";
            var sb = new System.Text.StringBuilder("presses ");
            for (int i = 0; i < taps.Count; i++)
                sb.Append(i == 0 ? "" : ", ").Append($"{taps[i].button}@{taps[i].time - taps[0].time:F2}");
            sb.Append(" s; expected ");
            for (int i = 0; i < settings.Buttons.Length; i++)
                sb.Append(i == 0 ? "" : ", ").Append($"{settings.Buttons[i]}@{settings.Times[i]:F2}");
            return sb.Append(" s").ToString();
        }
    }
}
