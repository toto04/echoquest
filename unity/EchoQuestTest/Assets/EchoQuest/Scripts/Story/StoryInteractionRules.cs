using System;
using System.Collections.Generic;
using System.Globalization;

namespace EchoQuest.Story
{
    /// <summary>Battle = call and echo over the battle loop (docs/music/04-battle-cues.md).
    /// The story never asks for a timed press in a silence any more (too hard); Note is kept for
    /// configurations that still use it.</summary>
    /// FreePlay = the child plays along with a piece, any button at any time, no wrong answer (stage 0).
    public enum InteractionMode { Confirm, Rhythm, Compare, Note, Decision, Battle, FreePlay }

    public class InteractionSettings
    {
        public InteractionMode Mode;
        /// <summary>Empty Prompt: the story lines before the interaction already asked, so the first
        /// attempt skips the prompt and the examples the story just played.</summary>
        public string Prompt, Success, Retry, Feedback;
        /// <summary>Button sounds, one per note button (suono_pulsante "a;b;c;d"); one entry = every button.</summary>
        public string[] Feedbacks = new string[0];
        /// <summary>Compare: the right answer (risposta "si" or "no"; default No, as in Scene 1).</summary>
        public int ExpectedAnswer = StoryInteractionRules.No;
        /// <summary>Decision: the piano register each button chooses (registro "1=grave;2=acuto").</summary>
        public Dictionary<int, string> Registers = new Dictionary<int, string>();
        /// <summary>Battle: the button of each note of the call (pulsanti "1;2;1"), matching Times.</summary>
        public int[] Buttons = new int[0];
        /// <summary>Battle: voices alternated after a success (Success) and after a failure (Retry).</summary>
        public string[] SuccessVoices = new string[0], RetryVoices = new string[0];
        /// <summary>primo_ascolto "storia": the story just played the examples, so the first attempt
        /// only asks the question (Compare) instead of playing them again.</summary>
        public bool ExamplesPlayedByStory;
        public string[] DeclineVoices;
        public string RetryQuestion;
        public string[] ContinueVoices;
        public int AttemptsPerRound;
        public int ConfirmButton = StoryInteractionRules.Yes;
        public string[] Examples;
        public float[] Times;
        public float Tolerance = 0.3f;

        private static string[] Split(string text) =>
            (text ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        public static InteractionSettings Parse(Func<string, string> get)
        {
            string mode = get("modalita");
            if (string.IsNullOrEmpty(mode)) return null;
            var settings = new InteractionSettings
            {
                Mode = mode switch
                {
                    "conferma" => InteractionMode.Confirm,
                    "ritmo" => InteractionMode.Rhythm,
                    "confronto" => InteractionMode.Compare,
                    "nota" => InteractionMode.Note,
                    "scelta" => InteractionMode.Decision,
                    "sfida" => InteractionMode.Battle,
                    "libero" => InteractionMode.FreePlay,
                    _ => throw new FormatException("Unknown interaction mode: " + mode),
                },
                Prompt = get("consegna"), Success = get("successo"), Retry = get("riprova"),
                Feedback = get("suono_pulsante"),
                DeclineVoices = get("attesa_no").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                RetryQuestion = get("domanda_riprova"),
                ContinueVoices = get("prosegui").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                Examples = get("esempi").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
            };
            settings.Feedbacks = Split(settings.Feedback);
            settings.ExamplesPlayedByStory = get("primo_ascolto") switch
            {
                "" => false,
                "storia" => true,
                _ => throw new FormatException("primo_ascolto can only be storia: " + get("primo_ascolto")),
            };
            settings.SuccessVoices = Split(settings.Success);
            settings.RetryVoices = Split(settings.Retry);
            if (settings.SuccessVoices.Length > 1) settings.Success = settings.SuccessVoices[0];
            if (settings.RetryVoices.Length > 1) settings.Retry = settings.RetryVoices[0];
            settings.ExpectedAnswer = get("risposta") switch
            {
                "" or "no" => StoryInteractionRules.No,
                "si" or "sì" => StoryInteractionRules.Yes,
                _ => throw new FormatException("risposta must be si or no: " + get("risposta")),
            };
            foreach (var pair in get("registro").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=');
                if (kv.Length != 2 || !int.TryParse(kv[0].Trim(), out int button) || kv[1].Trim() is not ("grave" or "acuto"))
                    throw new FormatException("registro must look like 1=grave;2=acuto: " + pair);
                settings.Registers[button] = kv[1].Trim();
            }
            var buttons = new List<int>();
            foreach (var value in Split(get("pulsanti")))
            {
                if (!int.TryParse(value, out int b) || b < 1 || b > 4) throw new FormatException("pulsanti are note buttons 1-4: " + value);
                buttons.Add(b);
            }
            settings.Buttons = buttons.ToArray();
            var times = new List<float>();
            foreach (var value in get("tempi").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float time) ||
                    float.IsNaN(time) || float.IsInfinity(time) || time < 0)
                    throw new FormatException("Invalid interaction time: " + value);
                times.Add(time);
            }
            settings.Times = times.ToArray();
            string attempts = get("tentativi");
            if (!string.IsNullOrEmpty(attempts) &&
                (!int.TryParse(attempts, NumberStyles.Integer, CultureInfo.InvariantCulture, out settings.AttemptsPerRound) ||
                 settings.AttemptsPerRound < 1 || settings.AttemptsPerRound > 3))
                throw new FormatException("Practice rounds require one to three attempts.");
            if (settings.AttemptsPerRound > 0)
            {
                if (settings.Mode is InteractionMode.Confirm or InteractionMode.Decision)
                    throw new FormatException("Attempt limits apply only to practice exercises.");
                if (string.IsNullOrEmpty(settings.RetryQuestion) || settings.ContinueVoices.Length == 0)
                    throw new FormatException("Attempt limits require domanda_riprova and prosegui voices.");
            }
            else if (!string.IsNullOrEmpty(settings.RetryQuestion) || settings.ContinueVoices.Length > 0)
                throw new FormatException("Retry choice voices require a tentativi limit.");
            settings.ConfirmButton = get("pulsante") switch
            {
                "" or "si" => StoryInteractionRules.Yes,
                "indice" => 1,
                "medio" => 2,
                "anulare" => 3,
                "mignolo" => 4,
                _ => throw new FormatException("Unknown confirmation button: " + get("pulsante")),
            };
            if (settings.Mode == InteractionMode.Battle)
            {
                if (times.Count < 1 || times[0] != 0 || times.Count != settings.Buttons.Length)
                    throw new FormatException("Battle rounds need tempi starting at zero and one pulsanti entry per note.");
                for (int i = 1; i < times.Count; i++)
                    if (times[i] <= times[i - 1]) throw new FormatException("Battle tempi must increase.");
                if (settings.Examples.Length != 1)
                    throw new FormatException("A battle round needs exactly one call clip.");
                if (settings.SuccessVoices.Length == 0 || settings.RetryVoices.Length == 0)
                    throw new FormatException("Battle rounds need successo and riprova voices.");
                return settings;
            }
            if (settings.Mode == InteractionMode.FreePlay)
            {
                if (settings.Examples.Length != 1 || settings.Feedbacks.Length == 0)
                    throw new FormatException("Free play needs the piece to play along with (esempi) and the button sounds (suono_pulsante).");
                if (settings.AttemptsPerRound > 0) throw new FormatException("Free play has no attempts: there is no wrong note.");
                return settings;
            }
            if (settings.Mode is not (InteractionMode.Confirm or InteractionMode.Decision) &&
                (string.IsNullOrEmpty(settings.Retry) || settings.Examples.Length == 0))
                throw new FormatException("This interaction requires riprova and esempi.");
            if (settings.Mode is InteractionMode.Rhythm or InteractionMode.Note)
            {
                if (times.Count != 3 || times[0] != 0 || times[1] <= times[0] || times[2] <= times[1])
                    throw new FormatException("Timed interactions require three increasing times starting at zero.");
                if (settings.Examples.Length != 1)
                    throw new FormatException("Timed interactions require exactly one example.");
            }
            if (settings.Mode == InteractionMode.Compare && settings.Examples.Length != 2)
                throw new FormatException("Rhythm comparisons require two examples.");
            return settings;
        }
    }

    public static class StoryInteractionRules
    {
        // Logical controls; these are not physical HID button numbers.
        public const int Yes = 5, No = 6;
        public static bool MatchesRhythm(IReadOnlyList<double> taps, IReadOnlyList<float> expected)
        {
            if (taps.Count != 3 || expected.Count != 3) return false;
            foreach (float time in expected) if (float.IsNaN(time) || float.IsInfinity(time)) return false;
            double duration = taps[2] - taps[0];
            double reference = expected[2] - expected[0];
            if (reference <= 0 || duration <= 0 || double.IsNaN(duration) || double.IsInfinity(duration)) return false;
            double speed = duration / reference;
            if (speed < 0.5 || speed > 2) return false;
            for (int i = 1; i < 3; i++)
            {
                double gap = taps[i] - taps[i - 1];
                double target = (expected[i] - expected[i - 1]) * speed;
                if (gap <= 0 || target <= 0 || double.IsNaN(gap) || double.IsInfinity(gap) ||
                    Math.Abs(gap - target) > Math.Max(0.10, target * 0.30)) return false;
            }
            return true;
        }

        /// <summary>A battle echo: the same buttons in the same order, and the same relative rhythm
        /// (any speed from half to double; each gap within 0.12 s or 35%). Judged on structure, not
        /// on absolute precision (06-worked-example.md, stage 2).</summary>
        public static bool MatchesPhrase(IReadOnlyList<(int button, double time)> taps, IReadOnlyList<int> buttons, IReadOnlyList<float> expected)
        {
            if (taps.Count != buttons.Count || buttons.Count != expected.Count || taps.Count == 0) return false;
            for (int i = 0; i < taps.Count; i++)
                if (taps[i].button != buttons[i] || double.IsNaN(taps[i].time) || double.IsInfinity(taps[i].time)) return false;
            if (taps.Count == 1) return true;
            double duration = taps[taps.Count - 1].time - taps[0].time;
            double reference = expected[expected.Count - 1] - expected[0];
            if (reference <= 0 || duration <= 0) return false;
            double speed = duration / reference;
            if (speed < 0.5 || speed > 2) return false;
            for (int i = 1; i < taps.Count; i++)
            {
                double gap = taps[i].time - taps[i - 1].time;
                double target = (expected[i] - expected[i - 1]) * speed;
                if (gap <= 0 || Math.Abs(gap - target) > Math.Max(0.12, target * 0.35)) return false;
            }
            return true;
        }


        public static bool HitsMissingNote(double press, double start, IReadOnlyList<float> times, float tolerance, double deadline)
        {
            if (times.Count != 3 || double.IsNaN(press) || double.IsInfinity(press) ||
                double.IsNaN(start) || double.IsInfinity(start) || double.IsNaN(deadline) || double.IsInfinity(deadline) || tolerance < 0 ||
                float.IsNaN(tolerance) || float.IsInfinity(tolerance)) return false;
            foreach (float time in times) if (float.IsNaN(time) || float.IsInfinity(time)) return false;
            if (times[0] != 0 || times[1] <= 0 || times[2] <= times[1] || deadline < start + times[2]) return false;
            double relative = press - start;
            double earliest = Math.Max(times[1] + 0.05, times[2] - tolerance);
            return relative >= earliest && press <= deadline;
        }
    }
}
