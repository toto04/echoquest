// Run in the connected Editor with:
// unity command 'script run' --file ../../tools/story_interaction_checks.cs --entry StoryInteractionChecks.Run
using System;
using System.Collections.Generic;
using EchoQuest.Story;

public static class StoryInteractionChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception(name);
            checks++;
        }
        void Invalid(Action action, string name)
        {
            bool rejected = false;
            try { action(); } catch (FormatException) { rejected = true; }
            Check(rejected, name);
        }
        var regular = new[] { 0f, 1f, 2f };
        var maestro = new[] { 0f, .32f, .95f };
        Check(StoryInteractionRules.MatchesRhythm(new[] { 8d, 9d, 10d }, regular), "regular rhythm");
        Check(StoryInteractionRules.MatchesRhythm(new[] { 8d, 9.5, 11d }, regular), "slower regular rhythm");
        Check(StoryInteractionRules.MatchesRhythm(new[] { 8d, 8.6, 9.2 }, regular), "faster regular rhythm");
        Check(StoryInteractionRules.MatchesRhythm(new[] { 8d, 9.05, 10.03 }, regular), "small timing errors");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 8d, 8.2, 10d }, regular), "unequal gaps");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 8d, 8.1, 8.2 }, regular), "button mashing");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 8d, 9d }, regular), "incomplete rhythm");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 8d, 8d, 9d }, regular), "held or duplicate press");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 8d, 9d, 10d, 11d }, regular), "extra press");
        Check(StoryInteractionRules.MatchesRhythm(new[] { 10d, 10.32, 10.95 }, maestro), "Maestro rhythm");
        Check(StoryInteractionRules.MatchesRhythm(new[] { 10d, 10.48, 11.425 }, maestro), "slower Maestro rhythm");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 10d, 10.475, 10.95 }, maestro), "regular is not Maestro");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 10d, 10.63, 10.95 }, maestro), "reversed pattern");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 10d, double.NaN, 10.95 }, maestro), "invalid timestamp");
        Check(!StoryInteractionRules.MatchesRhythm(new[] { 10d, 10.32, 10.95 }, new[] { 0f, float.NaN, .95f }), "invalid pattern");

        var theme = new[] { 0f, 1.2f, 1.5f };
        Check(StoryInteractionRules.HitsMissingNote(101.5, 100, theme, .3f), "missing note on time");
        Check(StoryInteractionRules.HitsMissingNote(101.3, 100, theme, .3f), "forgiving early response");
        Check(StoryInteractionRules.HitsMissingNote(101.79, 100, theme, .3f), "forgiving late response");
        Check(!StoryInteractionRules.HitsMissingNote(101.2, 100, theme, .3f), "second note does not pass");
        Check(!StoryInteractionRules.HitsMissingNote(99, 100, theme, .3f), "press before playback");
        Check(!StoryInteractionRules.HitsMissingNote(101.81, 100, theme, .3f), "response outside window");
        Check(!StoryInteractionRules.HitsMissingNote(double.NaN, 100, theme, .3f), "NaN press");
        Check(!StoryInteractionRules.HitsMissingNote(101.5, 100, theme, float.NaN), "NaN tolerance");
        Check(!StoryInteractionRules.HitsMissingNote(101.5, 100, new[] { 0f, 2f, 1f }, .3f), "unordered theme");

        var values = new Dictionary<string, string>
        {
            ["modalita"] = "ritmo", ["consegna"] = "prompt", ["successo"] = "success",
            ["riprova"] = "retry", ["esempi"] = "s01_sfx_13", ["tempi"] = "0;1;2",
        };
        string Get(string key) => values.TryGetValue(key, out var value) ? value : "";
        var settings = InteractionSettings.Parse(Get);
        Check(settings.Mode == InteractionMode.Rhythm && settings.Times.Length == 3 && settings.Times[2] == 2,
            "parse timed interaction");
        values["tempi"] = "0;1;NaN";
        Invalid(() => InteractionSettings.Parse(Get), "reject invalid configuration times");
        values["tempi"] = "0;1;1";
        Invalid(() => InteractionSettings.Parse(Get), "reject nonincreasing times");
        values["tempi"] = "0;1;2"; values["esempi"] = "a;b";
        Invalid(() => InteractionSettings.Parse(Get), "rhythm requires one example");
        values["modalita"] = "confronto";
        Check(InteractionSettings.Parse(Get).Examples.Length == 2, "comparison examples");
        values["esempi"] = "a";
        Invalid(() => InteractionSettings.Parse(Get), "comparison requires two examples");
        values["modalita"] = "conferma"; values["riprova"] = ""; values["esempi"] = ""; values["tempi"] = "";
        Check(InteractionSettings.Parse(Get).Mode == InteractionMode.Confirm, "confirmation needs no retry or example");
        values["consegna"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "instruction is required");
        values["modalita"] = "unknown";
        Invalid(() => InteractionSettings.Parse(Get), "reject unknown mode");

        var legacy = StoryData.ParseStaging("id,scelte\ns01_int_02,1=B;2=A\n")["s01_int_02"];
        Check(legacy.Interaction == null && legacy.Choices[1] == "B" && legacy.Choices[2] == "A",
            "legacy narrative choice is unchanged");
        var parsed = StoryData.ParseStaging("id,modalita,consegna,successo\ns01_int_01,conferma,prompt,success\n");
        Check(parsed["s01_int_01"].Interaction.Mode == InteractionMode.Confirm, "new staging fields");
        values["modalita"] = "conferma"; values["consegna"] = "prompt";
        Check(InteractionSettings.Parse(Get).ConfirmButton == StoryInteractionRules.Yes, "confirmations use thumb Yes");
        values["pulsante"] = "indice";
        Check(InteractionSettings.Parse(Get).ConfirmButton == 1, "musical tutorial can require index");
        values["pulsante"] = "medio";
        Invalid(() => InteractionSettings.Parse(Get), "do not silently map an unknown confirmation button");
        values["pulsante"] = ""; values["modalita"] = "confronto";
        values["esempi"] = "a;b"; values["riprova"] = "retry";
        Check(InteractionSettings.Parse(Get).AttemptsPerRound == 0, "old staging keeps its existing retry behavior");
        values["tentativi"] = "2";
        Invalid(() => InteractionSettings.Parse(Get), "a retry limit requires spoken choice and continuation");
        values["domanda_riprova"] = "question"; values["prosegui"] = "continue";
        Check(InteractionSettings.Parse(Get).AttemptsPerRound == 2, "two attempts per round");
        values["prosegui"] = "continue;continue_alt";
        Check(InteractionSettings.Parse(Get).ContinueVoices.Length == 2, "alternate the recorded encouragements");
        values["prosegui"] = ";;";
        Invalid(() => InteractionSettings.Parse(Get), "empty encouragement entries cannot enable a retry limit");
        values["prosegui"] = "continue;continue_alt";
        foreach (string bad in new[] { "0", "4", "-1", "two", "2.5" })
        {
            values["tentativi"] = bad;
            Invalid(() => InteractionSettings.Parse(Get), "reject invalid attempt limit " + bad);
        }
        values["tentativi"] = "2"; values["modalita"] = "conferma";
        Invalid(() => InteractionSettings.Parse(Get), "narrative confirmation cannot be skipped by an exercise limit");
        values["modalita"] = "confronto"; values["tentativi"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "retry voices need an explicit limit");
        return checks;
    }
}
