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
        Check(StoryInteractionRules.HitsMissingNote(101.5, 100, theme, .3f, 101.8), "missing note on time");
        Check(StoryInteractionRules.HitsMissingNote(101.3, 100, theme, .3f, 101.8), "forgiving early response");
        Check(StoryInteractionRules.HitsMissingNote(101.79, 100, theme, .3f, 101.8), "forgiving late response");
        Check(!StoryInteractionRules.HitsMissingNote(101.2, 100, theme, .3f, 101.8), "second note does not pass");
        Check(!StoryInteractionRules.HitsMissingNote(99, 100, theme, .3f, 101.8), "press before playback");
        Check(!StoryInteractionRules.HitsMissingNote(101.81, 100, theme, .3f, 101.8), "response outside window");
        Check(!StoryInteractionRules.HitsMissingNote(double.NaN, 100, theme, .3f, 101.8), "NaN press");
        Check(!StoryInteractionRules.HitsMissingNote(101.5, 100, theme, float.NaN, 101.8), "NaN tolerance");
        Check(!StoryInteractionRules.HitsMissingNote(101.5, 100, new[] { 0f, 2f, 1f }, .3f, 101.8), "unordered theme");
        Check(StoryInteractionRules.HitsMissingNote(103.7, 100, theme, .3f, 106.2), "response in silence after the piano finishes");
        Check(!StoryInteractionRules.HitsMissingNote(106.21, 100, theme, .3f, 106.2), "response after the listening window closes");
        Check(!StoryInteractionRules.HitsMissingNote(103.7, 100, theme, .3f, double.NaN), "invalid response deadline");
        Check(!StoryInteractionRules.HitsMissingNote(101.5, 100, theme, .3f, 101.4), "deadline cannot precede the missing note");

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
        Check(InteractionSettings.Parse(Get).Prompt == "", "an empty instruction means the story lines already asked (Scenes 2-3)");
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
        Check(InteractionSettings.Parse(Get).ConfirmButton == 2, "Scene 3 introduces the middle button");
        values["pulsante"] = "anulare";
        Check(InteractionSettings.Parse(Get).ConfirmButton == 3, "the ring finger can be required");
        values["pulsante"] = "mignolo";
        Check(InteractionSettings.Parse(Get).ConfirmButton == 4, "chapter 2 introduces the little finger");
        values["pulsante"] = "pollice";
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
        var invitations = StoryData.ParseStaging("id,modalita,consegna,attesa_no\ns01_int_06,conferma,prompt,first;second\n");
        Check(invitations["s01_int_06"].Interaction.DeclineVoices.Length == 2, "parse multiple invitations after No");
        var calmWait = StoryData.ParseStaging("id,modalita,consegna,attesa_no\ns01_int_06,conferma,prompt,wait\n");
        Check(calmWait["s01_int_06"].Interaction.DeclineVoices.Length == 1, "single calm-wait recording remains supported");

        // Scenes 2-3: the expected answer of a comparison, registers, per-button sounds, battle rounds.
        values.Clear();
        values["modalita"] = "confronto"; values["riprova"] = "retry"; values["esempi"] = "a;b";
        Check(InteractionSettings.Parse(Get).ExpectedAnswer == StoryInteractionRules.No, "Scene 1 comparisons still expect No");
        values["risposta"] = "si";
        Check(InteractionSettings.Parse(Get).ExpectedAnswer == StoryInteractionRules.Yes, "piano and forte: the same notes, Yes");
        values["risposta"] = "forse";
        Invalid(() => InteractionSettings.Parse(Get), "reject an unknown expected answer");
        values.Clear();
        values["modalita"] = "scelta"; values["registro"] = "1=grave;2=acuto";
        var choice = InteractionSettings.Parse(Get);
        Check(choice.Registers[1] == "grave" && choice.Registers[2] == "acuto", "the register choice");
        values["registro"] = "1=basso";
        Invalid(() => InteractionSettings.Parse(Get), "reject an unknown register");
        values.Clear();
        values["modalita"] = "conferma"; values["suono_pulsante"] = "re;fa;la;do";
        Check(InteractionSettings.Parse(Get).Feedbacks.Length == 4, "one sound per note button");
        values.Clear();
        values["modalita"] = "sfida"; values["esempi"] = "x_s03_r2"; values["tempi"] = "0;1.2;1.5"; values["pulsanti"] = "1;2;1";
        values["successo"] = "ok;ok_alt"; values["riprova"] = "laugh;laugh_alt";
        var round = InteractionSettings.Parse(Get);
        Check(round.Mode == InteractionMode.Battle && round.Buttons[1] == 2 && round.SuccessVoices.Length == 2 && round.RetryVoices.Length == 2,
            "parse a battle round");
        values["pulsanti"] = "1;2";
        Invalid(() => InteractionSettings.Parse(Get), "one button per note of the call");
        values["pulsanti"] = "1;5;1";
        Invalid(() => InteractionSettings.Parse(Get), "battle buttons are note buttons");
        values["pulsanti"] = "1;2;1"; values["riprova"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "a battle round needs Sbuffo's taunt voices");
        values["riprova"] = "laugh"; values["modalita"] = "sfida_nota";
        Invalid(() => InteractionSettings.Parse(Get), "no round asks to fill a silence on time any more");
        values.Clear();
        values["modalita"] = "confronto"; values["consegna"] = "question"; values["riprova"] = "retry"; values["esempi"] = "memory;today";
        Check(!InteractionSettings.Parse(Get).ExamplesPlayedByStory, "examples are played by the interaction by default");
        values["primo_ascolto"] = "storia";
        Check(InteractionSettings.Parse(Get).ExamplesPlayedByStory, "Scene 1: the story already played the memory and today's piano");
        values["primo_ascolto"] = "sempre";
        Invalid(() => InteractionSettings.Parse(Get), "reject an unknown primo_ascolto");

        var r2 = new[] { 1, 2, 1 };
        var call = new[] { 0f, 1.2f, 1.5f };
        Check(StoryInteractionRules.MatchesPhrase(new[] { (1, 10d), (2, 11.2), (1, 11.5) }, r2, call), "echo on time");
        Check(StoryInteractionRules.MatchesPhrase(new[] { (1, 10d), (2, 11.45), (1, 11.8) }, r2, call), "a little slower");
        Check(!StoryInteractionRules.MatchesPhrase(new[] { (1, 10d), (1, 11.2), (1, 11.5) }, r2, call), "wrong middle button");
        Check(!StoryInteractionRules.MatchesPhrase(new[] { (1, 10d), (2, 10.3), (1, 11.5) }, r2, call), "TA-TA... TA is not TA... TA-TA");
        Check(!StoryInteractionRules.MatchesPhrase(new[] { (1, 10d), (2, 11.2) }, r2, call), "incomplete echo");
        Check(StoryInteractionRules.MatchesPhrase(new[] { (2, 5d) }, new[] { 2 }, new[] { 0f }), "a single note matches by button");
        values.Clear();
        values["modalita"] = "libero"; values["esempi"] = "x_s04_libero"; values["suono_pulsante"] = "re;fa;la;do";
        Check(InteractionSettings.Parse(Get).Mode == InteractionMode.FreePlay, "Scene 4: free play");
        values["suono_pulsante"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "free play needs the button sounds");
        values["suono_pulsante"] = "re;fa;la;do"; values["tentativi"] = "2"; values["domanda_riprova"] = "q"; values["prosegui"] = "c";
        Invalid(() => InteractionSettings.Parse(Get), "free play has no attempts: there is no wrong note");
        return checks;
    }
}
