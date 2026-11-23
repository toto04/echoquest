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
            /// <summary>The loop's clip prefix (staging "base"), to know when a round asks for another loop.</summary>
            public string Prefix = "";
            /// <summary>Bars per exchange: 4 for long exchanges (call twice), 2 for short ones (call once).</summary>
            public int ExchangeBars = 4;
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
                Until = staging.Until, HighPad = Register == "grave", Prefix = prefix, ExchangeBars = exchangeBars,
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
                if (at.HasValue) stem.ScheduleEnd(at.Value);
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
            double k = System.Math.Ceiling((Now + lead - b.Start) / b.Bar);
            return b.Start + System.Math.Max(0, k) * b.Bar;
        }

        /// <summary>The next exchange boundary: a bar where the chord changes, so call and echo share it.</summary>
        private double NextExchange(double lead)
        {
            var b = m_Battle;
            double k = System.Math.Ceiling((Now + lead - b.Start) / b.Bar);
            k = System.Math.Ceiling(System.Math.Max(0, k) / b.ExchangeBars) * b.ExchangeBars;
            return b.Start + k * b.Bar;
        }

        /// <summary>Changes the loop on an exchange boundary (chapter 2: rounds 3-4 move to the short-exchange
        /// loop, then home to F). The old loop stops exactly where the new one starts; the layers won so far
        /// stay. Returns the time of the change.</summary>
        private double SwitchBattle(StagingEntry staging, double at)
        {
            int layers = m_Battle?.Layers ?? 0;
            string from = m_Battle?.Prefix ?? "";
            StopBattle(at);
            StartBattle(staging, at);
            if (m_Battle != null) m_Battle.Layers = layers;
            LogEvent($"battle loop changes from {from} to {staging.BattleBase} on the exchange boundary");
            return at;
        }

        private void UpdateBattleLayers(bool voicePlaying)
        {
            var b = m_Battle;
            if (b == null) return;
            double now = Now;
            int bar = now < b.Start ? 0 : (int)((now - b.Start) / b.Bar) % b.Bars;
            bool callBar = bar % b.ExchangeBars < b.ExchangeBars / 2;
            float duck = voicePlaying ? battleDuckGain : 1f;
            if (b.L0 != null) b.L0.TargetGain = duck;
            if (b.L1 != null) b.L1.TargetGain = (b.Layers >= 1 ? (callBar ? (b.HighPad ? 0f : callBarPadGain) : 1f) : 0f) * duck;
            if (b.L3 != null) b.L3.TargetGain = (b.Layers >= 2 ? 1f : 0f) * duck;
        }

        private void ScheduleBattleClip(string id, double at)
        {
            if (!InteractionClip(id, out var clip)) { Debug.LogError("[Story] Missing battle clip " + id); return; }
            var staging = StagingFor(new StoryItem { Id = id, Kind = StoryItemKind.Sound });
            Spawn(clip, staging, id, soundMarker, scheduledStart: System.Math.Max(at, Now + 0.02));
        }

        private IEnumerator WaitUntilDsp(double time)
        {
            while (!Interrupted && Now < time) yield return null;
        }

        private IEnumerator PlayBattleRound(StoryItem item, InteractionSettings settings)
        {
            // A round can start its own loop (practice echoes have no count-in sound) or change to another one
            // on the next exchange boundary: staging "base" on the round.
            var roundStaging = StagingFor(item);
            if (!string.IsNullOrEmpty(roundStaging.BattleBase))
            {
                if (m_Battle == null) StartBattle(roundStaging, Now + 0.4);   // the first exchange starts with the loop
                else if (m_Battle.Prefix != roundStaging.BattleBase) SwitchBattle(roundStaging, NextExchange(0.3));
            }
            if (m_Battle == null)
            {
                Status = "La sfida ha bisogno della base: in regia manca il suono con la colonna 'base'";
                Debug.LogError("[Story] " + Status + " (" + item.Id + ")");
                while (!Interrupted) yield return null;
                yield break;
            }
            CurrentInteraction = settings;
            LastAttempt = "";
            int failed = 0, missesInARow = 0, failedTries = 0;
            try
            {
                while (!Interrupted)
                {
                    m_Foreground.RemoveAll(source => source == null);
                    if (m_Battle == null) yield break;
                    // Short exchanges from stage 3 (07, D3); the boundary is counted on the current loop.
                    double exchange = NextExchange(0.3);
                    m_Battle.ExchangeBars = ExchangeBarsFor(settings);
                    ExchangeStart = exchange;
                    // After two misses in a row: the help (04, Failure): a barer loop and the Maestro saying the
                    // syllables over the call.
                    bool help = settings.Help.Length > 0 && missesInARow >= 2;
                    if (help && m_Battle.Layers > 0) m_Battle.Layers = 0;
                    // The attack round: the first try has no call at all; after a miss the call comes once.
                    bool withCall = !(settings.Attack && failedTries == 0);
                    yield return EchoExchange(settings, exchange, help, withCall);
                    failedTries++;
                    EndInteractionCapture();
                    if (Interrupted) yield break;
                    LogEvent(item.Id + (m_InteractionPassed ? " passed" : " not passed"));
                    if (m_InteractionPassed)
                    {
                        string voice = settings.SuccessVoices[m_BattleSuccessIndex++ % settings.SuccessVoices.Length];
                        yield return InteractionVoice(voice);
                        yield break;
                    }
                    missesInARow++;
                    // Failure: the taunt is already playing in the reply bar; Sbuffo laughs, then the same
                    // call comes back on the next exchange. Never slower (01-palette.md). Practice: no taunt.
                    if (settings.RetryVoices.Length > 0)
                    {
                        string taunt = settings.RetryVoices[m_BattleTauntIndex++ % settings.RetryVoices.Length];
                        yield return InteractionVoice(taunt, pauseAfter: false);
                        if (Interrupted) yield break;
                    }
                    if (settings.AttemptsPerRound > 0 && ++failed >= settings.AttemptsPerRound)
                    {
                        if (string.IsNullOrEmpty(settings.RetryQuestion))
                        {
                            LogEvent(item.Id + " given after " + failed + " tries");
                            yield return InteractionVoice(settings.ContinueVoices[0]);
                            yield break;
                        }
                        yield return RetryChoice(settings);
                        if (Interrupted) yield break;
                        if (settings.Together)
                        {
                            // "Shall we play it together, slowly?" Yes: the drum plays the call along with the
                            // child and the round is won. No: the child tries again.
                            if (m_InteractionSelectedButton == StoryInteractionRules.Yes)
                            {
                                if (m_Battle == null) yield break;
                                double together = NextExchange(0.3);
                                ExchangeStart = together;
                                yield return TogetherExchange(settings, together);
                                if (Interrupted) yield break;
                                LogEvent(item.Id + " passed together");
                                string voice = settings.SuccessVoices[m_BattleSuccessIndex++ % settings.SuccessVoices.Length];
                                yield return InteractionVoice(voice);
                                yield break;
                            }
                            failed = 0;
                            continue;
                        }
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

        /// <summary>Long exchange (4 bars): bars 1-2 the call twice, the TOC on beat 4 of bar 2; bar 3 the
        /// child's echo, the verdict on its beat 4; bar 4 on success the companion plays the phrase back, on a
        /// failure Sbuffo's taunt. Short exchange (scambio corto, 2 bars + reply): the call once with the TOC
        /// on its beat 4, the echo in bar 2. Judged on order and relative rhythm.</summary>
        /// <summary>Bars of one exchange: the call (once or twice), the echo, and for long exchanges the reply.
        /// The attack round has the shape of a short exchange.</summary>
        private int ExchangeBarsFor(InteractionSettings settings) =>
            settings.CallBars * (settings.ShortExchange || settings.Attack ? 2 : 4);

        private IEnumerator EchoExchange(InteractionSettings settings, double ex, bool help = false, bool withCall = true)
        {
            var b = m_Battle;
            double beat = b.Bar / 4;
            double phrase = settings.CallBars * b.Bar;
            string call = settings.Examples[0];
            int calls = settings.ShortExchange || settings.Attack ? 1 : 2;
            double echoBar = ex + calls * phrase;
            for (int i = 0; i < calls && withCall; i++)
            {
                ScheduleBattleClip(call, ex + i * phrase);
                if (help) ScheduleBattleVoice(settings.Help[0], ex + i * phrase);
            }
            ScheduleBattleClip("x_s03_toc", echoBar - beat);
            double open = echoBar - beat - 0.15;                                       // from the TOC on, presses count
            double close = settings.ShortExchange || settings.Attack ? echoBar + phrase : echoBar + phrase + 2 * beat;
            BeginInteractionCapture(false);
            Status = (!withCall ? "Nessuno la suona prima: al TOC suonala a memoria" :
                      calls == 1 ? "Ascolta la frase una volta; al TOC ripetila" : "Ascolta la frase due volte; al TOC ripetila") +
                     (help ? " (aiuto: il Maestro dice le sillabe)" : "");
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
                if (taps.Count >= settings.Buttons.Length || Now >= close) break;
                yield return null;
            }
            EndInteractionCapture();
            if (Interrupted) yield break;
            m_InteractionAttempted = true;
            m_InteractionPassed = forced || StoryInteractionRules.MatchesPhrase(taps, settings.Buttons, settings.Times);
            LastAttempt = forced ? "passed from the overlay" : DescribeTaps(taps, settings) + (m_InteractionPassed ? ": matches" : ": does not match");
            double verdict = System.Math.Max(echoBar + phrase - beat, NextBeat(0.05));
            yield return Verdict(settings, verdict, System.Math.Max(echoBar + phrase, NextBar(0.1)), call);
        }

        /// <summary>"Let's play it together": the call as usual, then in the echo bar the drum plays it again
        /// with the child. Every press sounds; nothing is judged.</summary>
        private IEnumerator TogetherExchange(InteractionSettings settings, double ex)
        {
            var b = m_Battle;
            int calls = settings.ShortExchange ? 1 : 2;
            double echoBar = ex + calls * b.Bar;
            string call = settings.Examples[0];
            for (int i = 0; i < calls; i++) ScheduleBattleClip(call, ex + i * b.Bar);
            ScheduleBattleClip("x_s03_toc", echoBar - b.Bar / 4);
            ScheduleBattleClip(call, echoBar);
            BeginInteractionCapture(false);
            Status = "Suoniamola insieme";
            while (!Interrupted && Now < echoBar + b.Bar)
            {
                while (m_InteractionInput.Count > 0)
                {
                    var (button, _) = m_InteractionInput.Dequeue();
                    if (button >= 1 && button <= 4) InteractionTapFeedback(settings, button);
                }
                yield return null;
            }
            EndInteractionCapture();
            m_InteractionAttempted = m_InteractionPassed = !Interrupted;
            if (m_Battle != null && !Interrupted) m_Battle.Layers = System.Math.Min(2, m_Battle.Layers + 1);
        }

        /// <summary>A Maestro line on the audio clock (the help syllables over the call).</summary>
        private void ScheduleBattleVoice(string id, double at)
        {
            var parts = InteractionVoiceClips(id);
            if (parts == null || parts.Count == 0) { Debug.LogError("[Story] Missing battle voice " + id); return; }
            var staging = m_Staging.TryGetValue(id, out var own) ? own :
                m_Staging.TryGetValue("maes", out var maestro) ? maestro : StagingEntry.Default;
            Spawn(parts[0], staging, id, characterMarker, isVoice: true, scheduledStart: System.Math.Max(at, Now + 0.02));
        }

        private IEnumerator Verdict(InteractionSettings settings, double stingAt, double replyBar, string reply)
        {
            var b = m_Battle;
            ScheduleBattleClip(m_InteractionPassed ? "x_s03_ok" : "x_s03_notyet", stingAt);
            if (m_InteractionPassed)
            {
                ScheduleBattleClip(reply, replyBar);                  // the companion plays the phrase back
                yield return WaitUntilDsp(replyBar + settings.CallBars * b.Bar);
                if (m_Battle != null) m_Battle.Layers = System.Math.Min(2, m_Battle.Layers + 1);   // on the bar line
            }
            else
            {
                if (!settings.Practice && !settings.Attack) ScheduleBattleClip("x_s03_taunt", replyBar);   // locked bar: Sbuffo laughs
                yield return WaitUntilDsp(replyBar);
                if (m_Battle != null) m_Battle.Layers = System.Math.Max(0, m_Battle.Layers - 1);   // only the top layer
                yield return WaitUntilDsp(replyBar + b.Bar * 0.6);
            }
        }

        private double NextBeat(double lead)
        {
            var b = m_Battle;
            double beat = b.Bar / 4;
            double k = System.Math.Ceiling((Now + lead - b.Start) / beat);
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
