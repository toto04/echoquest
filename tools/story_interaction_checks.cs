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
        values["prosegui"] = "continue";
        Check(InteractionSettings.Parse(Get).AttemptsPerRound == 2 && InteractionSettings.Parse(Get).RetryQuestion == "",
            "without a question the story goes on after the attempts (chapter 2)");
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

        // Keeping the beat (chapter 2, scene 2).
        var beats = new[] { 10d, 10.6, 11.2, 11.8 };
        Check(StoryInteractionRules.BeatHits(new[] { 10.05, 10.55, 11.3, 11.8 }, beats, 0.15) == 4, "presses near every beat");
        Check(StoryInteractionRules.BeatHits(new[] { 10.05, 10.4, 11.2 }, beats, 0.15) == 2, "a press 200 ms early does not count");
        Check(StoryInteractionRules.BeatHits(new[] { 10d, 10.01, 10.02, 10.6 }, beats, 0.15) == 2, "one press counts for one beat only");
        Check(StoryInteractionRules.BeatHits(new[] { 10.3 }, beats, 0.15) == 0, "a press between two beats hits neither");
        Check(StoryInteractionRules.BeatHits(new[] { 9.9, 10d, 10.1, 10.6, 10.9, 11.2, 11.5, 11.8 }, beats, 0.15) == 4,
            "extra presses never count against");
        Check(StoryInteractionRules.BeatHits(new[] { double.NaN, 10d }, beats, 0.15) == 1, "an invalid timestamp is ignored");
        values.Clear();
        values["modalita"] = "battito"; values["esempi"] = "x_t02_battito"; values["suono_pulsante"] = "re";
        values["tempi"] = "2.4;3;3.6;4.2"; values["quota"] = "3"; values["successo"] = "ok"; values["riprova"] = "again";
        var beat = InteractionSettings.Parse(Get);
        Check(beat.Mode == InteractionMode.Beat && beat.Quota == 3 && Math.Abs(beat.Tolerance - 0.15f) < 1e-6,
            "battito: quota, default tolerance 150 ms");
        values["tolleranza"] = "0.2"; values["aiuto"] = "uno;due";
        beat = InteractionSettings.Parse(Get);
        Check(Math.Abs(beat.Tolerance - 0.2f) < 1e-6 && beat.Help.Length == 2, "battito: tolerance and the counting help");
        values["quota"] = "5";
        Invalid(() => InteractionSettings.Parse(Get), "the quota cannot exceed the beats");
        values["quota"] = "3"; values["tempi"] = "2.4;2.4;3";
        Invalid(() => InteractionSettings.Parse(Get), "beats must increase");
        values["tempi"] = "2.4;3;3.6;4.2"; values["tolleranza"] = "0.6";
        Invalid(() => InteractionSettings.Parse(Get), "a tolerance over half a second is not keeping the beat");
        values["tolleranza"] = ""; values["successo"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "keeping the beat says when it worked");

        // Practice echoes (chapter 2, scene 3): a battle round without Sbuffo's taunt.
        values.Clear();
        values["modalita"] = "eco"; values["esempi"] = "x_t03_colibri"; values["tempi"] = "0;0.3;0.6";
        values["pulsanti"] = "1;1;1"; values["successo"] = "ok";
        var echo = InteractionSettings.Parse(Get);
        Check(echo.Mode == InteractionMode.Battle && echo.Practice && echo.RetryVoices.Length == 0, "eco: a round without a taunt");
        values["modalita"] = "sfida";
        Invalid(() => InteractionSettings.Parse(Get), "a real battle round still needs the taunt");

        // Chapter 2 battle options: short exchanges, two-bar calls, the attack round, play together.
        values["riprova"] = "laugh"; values["scambio"] = "corto"; values["battute_frase"] = "2";
        var shortRound = InteractionSettings.Parse(Get);
        Check(shortRound.ShortExchange && shortRound.CallBars == 2 && !shortRound.Attack && !shortRound.Together, "short exchange, two-bar call");
        values["scambio"] = "medio";
        Invalid(() => InteractionSettings.Parse(Get), "scambio is lungo or corto");
        values["scambio"] = ""; values["battute_frase"] = "3";
        Invalid(() => InteractionSettings.Parse(Get), "calls are one or two bars");
        values["battute_frase"] = ""; values["attacco"] = "si";
        Check(InteractionSettings.Parse(Get).Attack, "the attack round");
        values["attacco"] = ""; values["insieme"] = "si";
        Invalid(() => InteractionSettings.Parse(Get), "play together needs the question after the attempts");
        values["tentativi"] = "3"; values["domanda_riprova"] = "together?"; values["prosegui"] = "go on";
        Check(InteractionSettings.Parse(Get).Together, "play together after three attempts");

        // The final stroke: one press on the beat after the count.
        values.Clear();
        values["modalita"] = "colpo"; values["esempi"] = "x_t05_bacchetta"; values["suono_pulsante"] = "re";
        values["tempi"] = "1.2;1.8"; values["quota"] = "1"; values["successo"] = "bum"; values["riprova"] = "again";
        values["suono_successo"] = "x_t05_colpo";
        var strike = InteractionSettings.Parse(Get);
        Check(strike.Mode == InteractionMode.Beat && strike.Strike && Math.Abs(strike.Tolerance - 0.2f) < 1e-6,
            "colpo: a single stroke, 200 ms by default");
        values["suono_successo"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "colpo needs the big drum");

        // Chapter 3: recognize (answer with a finger) and count.
        values.Clear();
        values["modalita"] = "riconosci"; values["esempi"] = "x_c01_lungo"; values["consegna"] = "lungo o corto?";
        values["successo"] = "ok"; values["riprova"] = "again"; values["pulsanti"] = "1;2"; values["risposta"] = "indice";
        var recognize = InteractionSettings.Parse(Get);
        Check(recognize.Mode == InteractionMode.Recognize && recognize.ExpectedAnswer == 1 && !recognize.AskFirst &&
              recognize.Buttons.Length == 2, "riconosci: the index finger is the answer, the question from the second listen");
        values["risposta"] = "medio"; values["chiedi"] = "sempre";
        recognize = InteractionSettings.Parse(Get);
        Check(recognize.ExpectedAnswer == 2 && recognize.AskFirst, "riconosci: the middle finger, asked every time");
        values["risposta"] = "anulare";
        Invalid(() => InteractionSettings.Parse(Get), "the answer must be one of the answer fingers");
        values["risposta"] = "si";
        Invalid(() => InteractionSettings.Parse(Get), "riconosci is answered with a finger, not yes or no");
        values["risposta"] = "indice"; values["chiedi"] = "forse";
        Invalid(() => InteractionSettings.Parse(Get), "chiedi is sempre or empty");
        values["chiedi"] = ""; values["consegna"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "riconosci needs its question");
        values["modalita"] = "confronto"; values["consegna"] = "q"; values["esempi"] = "a;b"; values["pulsanti"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "a finger answer is only for riconosci");
        values.Clear();
        values["modalita"] = "conta"; values["esempi"] = "x_c02_due"; values["consegna"] = "quante?"; values["quota"] = "2";
        values["successo"] = "ok"; values["riprova"] = "again"; values["suono_pulsante"] = "re;fa;la;do";
        var count = InteractionSettings.Parse(Get);
        Check(count.Mode == InteractionMode.Count && count.Quota == 2, "conta: how many rings");
        values["quota"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "conta needs the number of rings");

        // Chapter 3: answers. The call is the question, replica the answer the bell plays back.
        values.Clear();
        values["modalita"] = "eco"; values["esempi"] = "x_c03_ciao"; values["replica"] = "x_c03_altalena";
        values["tempi"] = "0;0.6;1.2"; values["pulsanti"] = "2;3;2"; values["successo"] = "ok";
        var fixedAnswer = InteractionSettings.Parse(Get);
        Check(fixedAnswer.Reply == "x_c03_altalena" && fixedAnswer.Examples[0] == "x_c03_ciao" && fixedAnswer.Pieces.Count == 1,
            "the fixed answer: the question plays, the answer is judged and played back");
        values.Clear();
        values["modalita"] = "risposta"; values["esempi"] = "x_c05_domanda_libera"; values["tempi"] = "0"; values["pulsanti"] = "2";
        values["successo"] = "ok"; values["riprova"] = "home"; values["scambio"] = "corto";
        var free = InteractionSettings.Parse(Get);
        Check(free.Mode == InteractionMode.Battle && free.FreeAnswer && free.Quota == 2 && free.Buttons[0] == 2,
            "risposta: at least two notes, ending on the middle finger");
        values["pulsanti"] = "2;3"; values["tempi"] = "0;0.6";
        Invalid(() => InteractionSettings.Parse(Get), "risposta has only the home button");
        values["pulsanti"] = "2"; values["tempi"] = "0"; values["riprova"] = "";
        Invalid(() => InteractionSettings.Parse(Get), "an answer with nothing wrong needs who answers when the child does not");
        values["replica"] = "x_c03_altalena"; values["successo"] = ""; values["quota"] = "1";
        var noWrong = InteractionSettings.Parse(Get);
        Check(noWrong.FreeAnswer && noWrong.RetryVoices.Length == 0 && noWrong.Quota == 1, "the answer to Sbuffo: nothing is wrong");
        values["tentativi"] = "2"; values["prosegui"] = "go";
        Invalid(() => InteractionSettings.Parse(Get), "nothing is wrong, so no attempts");

        // Chapter 3's lullaby: a call in pieces, one exchange each.
        values.Clear();
        values["modalita"] = "eco"; values["esempi"] = "x_c05_ninna_a;x_c05_ninna_b"; values["successo"] = "ok";
        values["pulsanti"] = "3;2;3;4|3;2;1"; values["tempi"] = "0;1.2;1.8;3.0|0;1.2;1.8"; values["battute_frase"] = "2";
        var lullaby = InteractionSettings.Parse(Get);
        Check(lullaby.Pieces.Count == 2 && lullaby.Pieces[1].buttons.Length == 3 && lullaby.Pieces[1].call == "x_c05_ninna_b" &&
              Math.Abs(lullaby.Pieces[0].times[3] - 3.0f) < 1e-6, "the lullaby in two pieces");
        values["esempi"] = "x_c05_ninna_a";
        Invalid(() => InteractionSettings.Parse(Get), "one call clip per piece");
        values["esempi"] = "x_c05_ninna_a;x_c05_ninna_b"; values["tempi"] = "0;1.2;1.8;3.0";
        Invalid(() => InteractionSettings.Parse(Get), "pulsanti and tempi have the same pieces");
        return checks;
    }
}
