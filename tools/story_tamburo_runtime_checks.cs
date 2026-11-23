// Play-mode checks of chapter 2 (il tamburo) with the real scene data (capitolo_02_regia.csv, clips, timing).
// Open any story scene, set run-in-background, enter Play mode, then evaluate this file followed by
//   return await TamburoRuntimeChecks.RunScene1();   (one Run method per scene)
// (unity command script.eval --file <combined file>). Presses go through the on-screen buttons
// (StoryPlayer.PressLogical). Parts that only wait for the story run at a higher playback speed.
using System;
using System.Linq;
using System.Threading.Tasks;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TamburoRuntimeChecks
{
    const string Scenes = "Assets/EchoQuest/Story/Scenes/Capitolo2_Tamburo/";
    static int checks;
    static StoryPlayer s_Player;

    static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label + Tail());
        checks++;
    }

    static string Tail() => s_Player == null ? "" : "\n  last log: " + string.Join(" | ", s_Player.Log.Skip(Math.Max(0, s_Player.Log.Count - 12)));

    static async Task Until(Func<bool> condition, string label, double seconds)
    {
        double end = Time.realtimeSinceStartupAsDouble + seconds;
        while (!condition())
        {
            if (Time.realtimeSinceStartupAsDouble > end) throw new Exception("Timeout: " + label + Tail());
            await Task.Delay(5);
        }
    }

    /// <summary>Waits until story time <paramref name="time"/> (StoryPlayer.Now).</summary>
    static async Task At(double time)
    {
        while (s_Player.Now < time - 0.003) await Task.Delay(1);
    }

    static async Task<StoryPlayer> Load(string file, string section)
    {
        string path = Scenes + file + ".unity";
        bool loaded = false;
        void OnLoaded(Scene s, LoadSceneMode mode) { if (s.path == path) loaded = true; }
        SceneManager.sceneLoaded += OnLoaded;
        SceneManager.LoadScene(path);
        StoryPlayer p = null;
        try
        {
            await Until(() => loaded && (p = UnityEngine.Object.FindAnyObjectByType<StoryPlayer>()) != null && p.section == section,
                "scene " + file, 20);
        }
        finally { SceneManager.sceneLoaded -= OnLoaded; }
        s_Player = p;
        p.SetSpeed(1f);
        await Task.Delay(300);
        p.Log.Clear();
        return p;
    }

    static bool Logged(StoryPlayer p, string text) => p.Log.Any(line => line.Contains(text));

    /// <summary>Prologue and Scene 1: the chapter's own scenes and section names, the prologue playing on
    /// into Scene 1, free play on the drums (the child's tempo is measured), the little finger, and the
    /// "is it regular?" comparison.</summary>
    public static async Task<string> RunScene1()
    {
        checks = 0;
        Check(StoryPlayer.SectionName("t00") == "Prologo" && StoryPlayer.SectionName("t01") == "Scena 1" &&
              StoryPlayer.SectionName("t99") == "Epilogo" && StoryPlayer.SectionName("s03") == "Scena 3" &&
              StoryPlayer.SectionName("p00") == "Intro", "section names for chapter 2");

        var p = await Load("C2_00_Prologo", "t00");
        Check(p.Sections().First() == "t00" && p.Sections().Contains("t99"), "the scene holds chapter 2's script");
        await Until(() => p.Current?.Id == "t00_sfx_01", "the prologue starts", 5);
        Check(p.CurrentClip == "t00_sfx_01_" + p.Register, "the prologue opens on the piano's piece, in the chosen register");

        // The prologue plays on into Scene 1 without a scene load (fast, only waiting here).
        p.JumpTo("t00_sfx_02");
        p.SetSpeed(3f);
        await Until(() => p.section == "t01" && p.Current?.Id == "t01_maes_01", "the prologue continues into Scene 1", 20);
        Check(Logged(p, "section t01"), "the section change is logged");

        // Free play: every button sounds its drum, nothing is judged, the march ends it; the mean gap is measured.
        p.JumpTo("t01_int_01");
        await Until(() => p.WaitingInteractionInput, "free play listens", 5);
        p.SetSpeed(1f);
        Check(p.CurrentClip == "x_t01_marcia", "free play runs over the march");
        double t0 = p.Now + 0.2;
        for (int i = 0; i < 4; i++)
        {
            await At(t0 + 0.6 * i);
            p.PressLogical(1 + i);
        }
        await Task.Delay(100);
        Check(p.Active.Any(s => s != null && s.Label == "x_t01_tamburo_do"), "the little finger plays the Do drum");
        p.SetSpeed(3f);
        await Until(() => Logged(p, "t01_int_01 passed (4 notes"), "free play ends with the march", 15);
        Check(Math.Abs(p.SpontaneousBeatSeconds - 0.6f) < 0.08f, $"the child's tempo is measured (one press every {p.SpontaneousBeatSeconds:F2} s)");

        // The little finger: the index does not answer, the little finger does.
        await Until(() => p.Current?.Id == "t01_int_02" && p.WaitingInteractionInput, "the little-finger test", 20);
        p.SetSpeed(1f);
        p.PressLogical(1);
        await Task.Delay(300);
        Check(!Logged(p, "t01_int_02 passed"), "the index does not pass the little-finger test");
        p.PressLogical(4);
        await Until(() => Logged(p, "t01_int_02 passed"), "the little finger passes", 5);

        // "Is the second one regular like the first?": Yes is wrong (the examples come back), No is right.
        p.SetSpeed(3f);
        await Until(() => p.Current?.Id == "t01_int_03" && p.WaitingInteractionInput, "the comparison asks", 30);
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => Logged(p, "t01_int_03 not passed"), "Yes is not the answer", 5);
        await Until(() => p.WaitingInteractionInput, "the examples come back and the question is asked again", 30);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => Logged(p, "t01_int_03 passed"), "No is the answer", 5);
        await Until(() => p.Current?.Id == "t01_maes_06", "the scene ends with the Maestro", 20);
        p.SetSpeed(1f);
        return $"Tamburo Scene 1: {checks} checks passed";
    }

    /// <summary>Presses on the beats of a keep-the-beat try, a little early or late like a child.</summary>
    static async Task TapBeats(StoryPlayer p, double first, int count, double[] jitter)
    {
        await Until(() => p.WaitingInteractionInput && p.BeatStart > 0, "the beat try starts", 10);
        double start = p.BeatStart;
        for (int k = 0; k < count; k++)
        {
            double time = start + first + 0.6 * k + jitter[k % jitter.Length];
            await At(time);
            p.PressLogicalAt(1, time);   // exact timestamps: the check frames must not decide a 150 ms window
        }
    }

    /// <summary>Scene 2: keep the beat with the orchestra (6 of 8), then alone when the music thins
    /// (5 of 8, with the Maestro counting after a miss), then the two tempo comparisons.</summary>
    public static async Task<string> RunScene2()
    {
        checks = 0;
        var p = await Load("C2_02_Scena2", "t02");

        // Level 1: on the beat, within the 150 ms window.
        p.JumpTo("t02_int_01");
        await TapBeats(p, 2.4, 8, new[] { 0.04, -0.06, 0.1, 0.0 });
        await Until(() => Logged(p, "t02_int_01 passed"), "level 1 passes on the beat", 10);
        Check(p.LastAttempt.StartsWith("8 of 8 beats hit"), "every beat is hit: " + p.LastAttempt);
        await Until(() => p.Active.All(s => s == null || s.Label != "x_t02_battito"), "the piece stops after the judged bars", 2);

        // Level 2: nothing in the bars where the music thins is not a pass; then the Maestro counts.
        await Until(() => p.Current?.Id == "t02_int_02" && p.WaitingInteractionInput, "level 2", 40);
        await TapBeats(p, 2.4, 4, new[] { 0.0 });   // only with the kick, then the child stops
        await Until(() => Logged(p, "t02_int_02 not passed"), "stopping when the music thins does not pass", 15);
        Check(p.LastAttempt.StartsWith("0 of 8 beats hit"), "the thin bars are judged: " + p.LastAttempt);
        await Until(() => p.WaitingInteractionInput && p.Active.Any(s => s != null && s.Label == "x_t02_conta_1"),
            "after the retry voice the Maestro counts along", 20);
        await TapBeats(p, 7.2, 8, new[] { 0.12, -0.12, 0.2, 0.05 });   // two presses 200 ms late: 6 of 8 hit
        await Until(() => Logged(p, "t02_int_02 passed"), "level 2 passes alone", 15);
        Check(p.LastAttempt.StartsWith("6 of 8 beats hit"), "late presses do not count: " + p.LastAttempt);

        // Tempo: the second is faster (Yes), then slower (No).
        p.SetSpeed(3f);
        await Until(() => p.Current?.Id == "t02_int_03" && p.WaitingInteractionInput, "is the second faster?", 40);
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => Logged(p, "t02_int_03 passed"), "faster: Yes", 5);
        await Until(() => p.Current?.Id == "t02_int_04" && p.WaitingInteractionInput, "and now?", 30);
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => Logged(p, "t02_int_04 not passed"), "slower: Yes is wrong", 5);
        await Until(() => p.WaitingInteractionInput, "the examples come back", 30);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => Logged(p, "t02_int_04 passed"), "slower: No", 5);
        await Until(() => p.Current?.Id == "t02_maes_05", "the scene ends with the Maestro's secret", 20);
        p.SetSpeed(1f);
        return $"Tamburo Scene 2: {checks} checks passed";
    }

    /// <summary>The child's echo in bar 3 of the current exchange, at exact story times.</summary>
    static async Task Echo(StoryPlayer p, int[] buttons, double[] times, int calls = 2, int callBars = 1)
    {
        double at = p.ExchangeStart + calls * callBars * p.BattleBarSeconds;   // the echo bar: after the call(s)
        for (int i = 0; i < buttons.Length; i++)
        {
            await At(at + times[i]);
            p.PressLogicalAt(buttons[i], at + times[i]);
        }
    }

    static async Task Round(StoryPlayer p, string id)
    {
        await Until(() => p.Current?.Id == id && p.WaitingInteractionInput && p.BattleRunning, id + " listens", 60);
    }

    /// <summary>Scene 3: the drum's words as practice echoes over the practice loop, which the first word
    /// starts by itself; a miss has no taunt and the word comes back; the loop stops on the bar line of
    /// the memory of the gallop.</summary>
    public static async Task<string> RunScene3()
    {
        checks = 0;
        var p = await Load("C2_03_Scena3", "t03");
        p.SetSpeed(2f);

        p.JumpTo("t03_int_01");
        await Round(p, "t03_int_01");
        Check(p.BattleRunning && Math.Abs(p.BattleBarSeconds - 2.4) < 1e-3, "me-la starts the practice loop at 100 BPM");
        Check(Math.Abs(p.ExchangeStart - p.BattleStart) < 1e-6, "the first word starts with the loop");
        await Echo(p, new[] { 1, 1 }, new[] { 0, 0.6 });
        await Until(() => Logged(p, "t03_int_01 passed"), "me-la", 15);

        await Round(p, "t03_int_02");
        double ex = p.ExchangeStart;
        await Echo(p, new[] { 1, 2, 1 }, new[] { 0, 0.3, 0.6 });   // wrong middle drum
        await Until(() => Logged(p, "t03_int_02 not passed"), "a wrong drum is not co-li-brì", 15);
        Check(p.Active.All(s => s == null || s.Label != "x_s03_taunt"), "practice: Sbuffo does not laugh");
        await Until(() => p.WaitingInteractionInput && p.ExchangeStart > ex, "the word comes back on the next exchange", 20);
        Check(Math.Abs((p.ExchangeStart - ex) / p.BattleBarSeconds % 4) < 1e-6, "on an exchange boundary");
        await Echo(p, new[] { 1, 1, 1 }, new[] { 0, 0.3, 0.6 });
        await Until(() => Logged(p, "t03_int_02 passed"), "co-li-brì", 15);

        await Round(p, "t03_int_03");
        await Echo(p, new[] { 1, 1, 1, 1 }, new[] { 0, 0.3, 0.6, 1.2 });
        await Until(() => Logged(p, "t03_int_03 passed"), "tar-ta-ru-ga", 15);

        await Round(p, "t03_int_04");
        await Echo(p, new[] { 3, 3, 1 }, new[] { 0, 0.3, 0.6 });
        await Until(() => Logged(p, "t03_int_04 passed"), "two drums", 15);

        await Until(() => p.Current?.Id == "t03_sfx_01", "the memory of the gallop", 60);
        Check(!p.BattleRunning && Logged(p, "battle ends on the bar line"), "the practice loop stops on the bar line of the memory");
        p.SetSpeed(1f);
        return $"Tamburo Scene 3: {checks} checks passed";
    }

    /// <summary>Scene 4: "are they the same?" (the gallop backwards), then the battle: two long rounds on
    /// loop A, round 3 moves to loop B with short exchanges, help after two misses, "together" after
    /// three; the bass walks home to F and round 4 (the gallop) is a short exchange on the home loop.</summary>
    public static async Task<string> RunScene4()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "grave";
        var p = await Load("C2_04_Scena4", "t04");
        p.SetSpeed(2f);

        p.JumpTo("t04_int_01");
        await Until(() => p.WaitingInteractionInput, "are they the same?", 10);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => Logged(p, "t04_int_01 passed"), "No: Sbuffo turned it backwards", 5);

        // Rounds 1-2: long exchanges on loop A, started by the count-in.
        await Round(p, "t04_int_02");
        Check(Logged(p, "battle loop x_t04_base_a: 8 bars"), "the count-in starts loop A");
        await Echo(p, new[] { 1, 1, 1 }, new[] { 0, 0.3, 0.6 });
        await Until(() => Logged(p, "t04_int_02 passed"), "round 1", 15);
        await Round(p, "t04_int_03");
        await Echo(p, new[] { 1, 1, 1, 1, 1, 1 }, new[] { 0, 0.3, 0.6, 1.2, 1.5, 1.8 });
        await Until(() => Logged(p, "t04_int_03 passed"), "round 2", 15);
        Check(p.BattleLayers == 2, "two rounds won, two layers");

        // Round 3: loop B on the exchange boundary, short exchanges; three misses lead to help and "together".
        await Round(p, "t04_int_04");
        Check(Logged(p, "battle loop changes from x_t04_base_a to x_t04_base_b"), "round 3 moves to loop B");
        Check(Math.Abs(p.ExchangeStart - p.BattleStart) < 1e-6, "loop B starts on the exchange boundary");
        Check(p.BattleLayers == 2, "the layers won so far stay");
        for (int miss = 1; miss <= 3; miss++)
        {
            double ex = p.ExchangeStart;
            if (miss == 3)
                Check(p.Active.Any(s => s != null && s.Label == "eq_t04_extra_02_colibri"), "after two misses the Maestro says the syllables");
            await Echo(p, new[] { 1, 1, 1 }, new[] { 0, 0.3, 0.6 }, calls: 1);   // index only: the wrong drums
            await Until(() => p.Log.Count(l => l.Contains("t04_int_04 not passed")) == miss, "miss " + miss, 15);
            if (miss < 3)
            {
                await Until(() => p.WaitingInteractionInput && p.ExchangeStart > ex, "the call comes back", 20);
                double bars = (p.ExchangeStart - ex) / p.BattleBarSeconds;
                Check(Math.Abs(bars - 2 * Math.Round(bars / 2)) < 1e-6, $"short exchanges: every two bars ({bars:F6} bars later)");
            }
        }
        Check(p.BattleLayers == 0, "the help loop is the bare one");
        await Until(() => p.WaitingRetryChoice, "shall we play it together?", 20);
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => Logged(p, "t04_int_04 passed together"), "together wins the round", 20);

        // Home: the bass walks down on the boundary and round 4 plays the gallop on the home loop.
        await Round(p, "t04_int_05");
        Check(Logged(p, "battle loop changes from x_t04_base_b to x_t04_base_casa after t04_sfx_05"), "the music comes home");
        await Echo(p, new[] { 3, 3, 4, 3, 3, 2 }, new[] { 0, 0.3, 0.6, 1.2, 1.5, 1.8 }, calls: 1);
        await Until(() => Logged(p, "t04_int_05 passed"), "the gallop", 15);
        await Until(() => p.Current?.Id == "t04_vent_03", "Sbuffo loses his favourite gallop", 30);
        Check(p.BattleRunning, "the home loop plays on into Scene 5");
        p.SetSpeed(1f);
        return $"Tamburo Scene 4: {checks} checks passed";
    }

    /// <summary>Scene 5: the attack round (no call, then a hint and the call once), the final stroke (a miss,
    /// then BUM), Sbuffo's gusts drifting onto the child's beat, and the extra level (the 3+3+2 rhythm, a
    /// two-bar call).</summary>
    public static async Task<string> RunScene5()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "grave";
        var p = await Load("C2_05_Scena5", "t05");
        p.SetSpeed(2f);

        // The attack round starts its own home loop when the chapter starts here.
        p.JumpTo("t05_int_01");
        await Round(p, "t05_int_01");
        Check(Logged(p, "battle loop x_t04_base_casa"), "the attack round plays over the home loop");
        double ex = p.ExchangeStart;
        await At(ex + 0.2);
        Check(p.Active.All(s => s == null || s.Label != "x_s03_r2"), "the first try has no call: from memory");
        await Until(() => Logged(p, "t05_int_01 not passed"), "silence is a miss", 20);
        await Until(() => p.WaitingInteractionInput && p.ExchangeStart > ex, "after the hint the next try", 20);
        Check(p.Active.Any(s => s != null && s.Label == "x_s03_r2"), "the piano plays the phrase once");
        await Echo(p, new[] { 1, 2, 1 }, new[] { 0, 1.2, 1.5 }, calls: 1);
        await Until(() => Logged(p, "t05_int_01 passed"), "the attack round", 15);

        // The final stroke: the loop stops for the Maestro, the count, one stroke on four.
        await Until(() => p.Current?.Id == "t05_int_02" && p.WaitingInteractionInput && p.BeatStart > 0, "the count", 40);
        Check(!p.BattleRunning, "the loop stopped before the final stroke");
        double start = p.BeatStart;
        await At(start + 1.45);
        p.PressLogicalAt(1, start + 1.45);   // 350 ms early
        await Until(() => Logged(p, "t05_int_02 not passed"), "an early stroke", 10);
        Check(p.LastAttempt.Contains("not on the beat"), "the early stroke is described: " + p.LastAttempt);
        await Until(() => p.WaitingInteractionInput && p.BeatStart > start, "the Maestro counts again", 20);
        start = p.BeatStart;
        await At(start + 1.85);
        p.PressLogicalAt(1, start + 1.85);
        await Until(() => Logged(p, "t05_int_02 passed"), "BUM", 10);
        Check(p.LastAttempt.StartsWith("stroke +50 ms") && p.LastAttempt.EndsWith("BUM"), "on the beat: " + p.LastAttempt);

        // Free play with Sbuffo: gusts move onto the child's beat.
        p.JumpTo("t05_int_03");
        await Until(() => p.WaitingInteractionInput, "free play with Sbuffo", 10);
        double t0 = p.Now + 0.3;
        for (int k = 0; k < 20; k++)
        {
            await At(t0 + 0.7 * k);
            p.PressLogicalAt(1 + k % 2, t0 + 0.7 * k);
        }
        var offs = p.Log.Where(l => l.Contains("ms off the child's beat")).ToList();
        Check(offs.Count >= 4, "gusts follow the child's beat: " + string.Join(" | ", offs));
        Check(offs.Last().Contains(", 0 ms off") || offs.Last().Contains(", -0 ms off"), "in the end Sbuffo blows on the beat: " + offs.Last());
        await Until(() => Logged(p, "t05_int_03 passed"), "free play ends with the wind", 30);

        // The extra level: Yes, then the secret rhythm, a two-bar call over the home loop.
        await Until(() => p.Current?.Id == "t05_int_04" && p.WaitingInteractionInput, "extra level?", 60);
        p.PressLogical(StoryInteractionRules.Yes);
        await Round(p, "t05_int_05");
        await Echo(p, new[] { 2, 2, 2, 1 }, new[] { 0, 0.9, 1.8, 2.4 }, calls: 2, callBars: 2);
        await Until(() => Logged(p, "t05_int_05 passed"), "the secret rhythm", 20);
        await Until(() => p.section == "t99", "the chapter goes on into the epilogue", 60);
        p.SetSpeed(1f);
        return $"Tamburo Scene 5: {checks} checks passed";
    }

    /// <summary>Chapter 1 to chapter 2: choosing the drum in chapter 1's Scene 5 opens the drum's prologue
    /// right after chapter 1's epilogue, in the piano register the child chose (choosing the bell is
    /// remembered as the bell's prologue: CampanellaRuntimeChecks.RunChapterHandoffs).</summary>
    public static async Task<string> RunChapterHandoff()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "acuto";
        StoryPlayer.PlayedChapters.Clear();
        string path = "Assets/EchoQuest/Story/Scenes/Capitolo1_Pianoforte/C1_05_Scena5.unity";
        SceneManager.LoadScene(path);
        StoryPlayer p = null;
        await Until(() => (p = UnityEngine.Object.FindAnyObjectByType<StoryPlayer>()) != null && p.section == "s05" &&
                          SceneManager.GetActiveScene().path == path, "chapter 1, Scene 5", 20);
        s_Player = p;
        await Task.Delay(300);
        p.JumpTo("s05_int_01");
        await Until(() => p.WaitingInteractionInput, "drum or bell?", 10);
        p.PressLogical(2);   // the bell first
        await Until(() => Logged(p, "after this chapter: C3_00_Prologo"), "the bell is remembered", 5);
        p.JumpTo("s05_int_01");
        await Until(() => p.WaitingInteractionInput, "drum or bell, again", 10);
        p.PressLogical(1);   // the drum
        await Until(() => Logged(p, "after this chapter: C2_00_Prologo"), "the drum is remembered", 5);
        p.SetSpeed(3f);
        p.JumpTo("e00_narr_02");   // the last line of chapter 1
        await Until(() => SceneManager.GetActiveScene().name == "C2_00_Prologo", "chapter 1 goes on into the drum's prologue", 40);
        await Until(() => (p = UnityEngine.Object.FindAnyObjectByType<StoryPlayer>()) != null && p.section == "t00" &&
                          p.Current?.Id == "t00_sfx_01", "the prologue starts", 10);
        s_Player = p;
        Check(p.CurrentClip == "t00_sfx_01_acuto", "the piano keeps the register chosen in chapter 1");
        Check(StoryPlayer.NextChapterScene == "", "the hand-off is used once");
        p.SetSpeed(1f);
        return $"Chapter hand-off: {checks} checks passed";
    }

    /// <summary>Epilogue: the choice at the end (No = the menu, which does not exist yet: the chapter ends).</summary>
    public static async Task<string> RunEpilogue()
    {
        checks = 0;
        var p = await Load("C2_99_Epilogo", "t99");
        p.SetSpeed(3f);
        await Until(() => p.Current?.Id == "t99_sfx_01", "the epilogue opens with the calm drum", 5);
        Check(p.CurrentClip == "t99_sfx_01_" + p.Register, "in the chosen register");
        await Until(() => p.Current?.Id == "t99_int_01" && p.WaitingInteractionInput, "go on to the next instrument?", 40);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => Logged(p, "branch B"), "No is the menu", 5);
        await Until(() => p.Current == null && p.Status.StartsWith("Fine del capitolo"), "the chapter ends", 10);
        p.SetSpeed(1f);
        return $"Tamburo Epilogue: {checks} checks passed";
    }
}
