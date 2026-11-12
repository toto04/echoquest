using System;
using System.Collections.Generic;
using System.Globalization;

namespace EchoQuest.Story
{
    public enum InteractionMode { Confirm, Rhythm, Compare, Note, Decision }

    public class InteractionSettings
    {
        public InteractionMode Mode;
        public string Prompt, Success, Retry, Feedback;
        public string[] DeclineVoices;
        public string RetryQuestion;
        public string[] ContinueVoices;
        public int AttemptsPerRound;
        public int ConfirmButton = StoryInteractionRules.Yes;
        public string[] Examples;
        public float[] Times;
        public float Tolerance = 0.3f;

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
                    _ => throw new FormatException("Unknown interaction mode: " + mode),
                },
                Prompt = get("consegna"), Success = get("successo"), Retry = get("riprova"),
                Feedback = get("suono_pulsante"),
                DeclineVoices = get("attesa_no").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                RetryQuestion = get("domanda_riprova"),
                ContinueVoices = get("prosegui").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                Examples = get("esempi").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
            };
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
                _ => throw new FormatException("Unknown confirmation button: " + get("pulsante")),
            };
            if (string.IsNullOrEmpty(settings.Prompt))
                throw new FormatException("Interactions require a consegna voice ID.");
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
