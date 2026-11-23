// Play-mode checks of Scenes 2-5 and the epilogue with the real scene data (regia, clips, timing).
// Open any story scene, set run-in-background, enter Play mode, then evaluate this file followed by
//   return await StoryBattleRuntimeChecks.RunScene2();   (or RunScene3, RunScene4, RunScene5, RunEpilogue, RunContinuity)
// (unity command script.eval --file <combined file>). Set StoryBattleRuntimeChecks.Speed = 2f first to run them at x2
// (flaky there: the 0.12 s rhythm tolerance is only 60 ms of real time, close to the frame-timed presses). Presses go through the on-screen buttons
// (StoryPlayer.PressLogical), timed on the audio clock like a child playing on the beat.
using System;
using System.Linq;
using System.Threading.Tasks;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StoryBattleRuntimeChecks
{
    static int checks;
    static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
        checks++;
    }

    static async Task Until(Func<bool> condition, string label, double seconds)
    {
        double end = Time.realtimeSinceStartupAsDouble + seconds;
        while (!condition())
        {
            if (Time.realtimeSinceStartupAsDouble > end) throw new Exception("Timeout: " + label);
            await Task.Delay(5);
        }
    }

    /// <summary>Playback speed for the checks (StoryPlayer.Clock.cs); every time below is story time.</summary>
    public static float Speed = 1f;
    static StoryPlayer s_Player;
    static double Clock => s_Player != null ? s_Player.Now : AudioSettings.dspTime;

    static async Task At(double time)
    {
        while (Clock < time - 0.003 * Speed) await Task.Delay(1);
    }

    static async Task<StoryPlayer> Load(string scene, string section)
    {
        bool loaded = false;
        void OnLoaded(Scene s, LoadSceneMode mode) { if (s.name == scene) loaded = true; }
        SceneManager.sceneLoaded += OnLoaded;
        SceneManager.LoadScene(scene);
        StoryPlayer p = null;
        try
        {
            await Until(() => loaded && (p = UnityEngine.Object.FindAnyObjectByType<StoryPlayer>()) != null && p.section == section,
                "scene " + scene, 20);
        }
        finally { SceneManager.sceneLoaded -= OnLoaded; }
        s_Player = p;
        p.SetSpeed(Speed);
        await Task.Delay(300);
        return p;
    }

    static bool Logged(StoryPlayer p, string text) => p.Log.Any(line => line.Contains(text));

    static async Task Press(StoryPlayer p, int[] buttons, double start, double[] times)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            await At(start + times[i]);
            p.PressLogical(buttons[i]);
        }
    }

    public static async Task<string> RunScene2()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "";
        var p = await Load("C1_02_Scena2", "s02");
        p.Log.Clear();

        p.JumpTo("s02_int_01");   // the index plays the piano: the prompt was the story line before
        await Until(() => p.WaitingInteractionInput, "index prompt", 5);
        p.PressLogical(1);
        await Until(() => Logged(p, "s02_int_01 passed"), "index press passes", 5);

        p.JumpTo("s02_int_02");   // piano and forte: same notes, the right answer is Yes
        await Until(() => p.WaitingInteractionInput, "compare prompt", 5);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => Logged(p, "s02_int_02 not passed"), "No is not the answer here", 5);
        await Until(() => p.WaitingInteractionInput, "compare listens again", 20);
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => Logged(p, "s02_int_02 passed"), "Yes passes", 5);

        p.JumpTo("s02_int_03");   // the register choice with the note buttons
        await Until(() => p.WaitingInteractionInput, "register choice", 5);
        p.PressLogical(StoryInteractionRules.Yes);   // the thumb does not choose here
        await Task.Delay(200);
        Check(StoryPlayer.ChosenRegister == "", "Yes is not a register");
        p.PressLogical(2);
        await Until(() => p.Current != null && p.Current.Id == "s02_sfx_13", "the middle finger leads to the high branch", 8);
        Check(StoryPlayer.ChosenRegister == "acuto" && p.BranchChoice == "B", "middle = high register, branch B");

        p.JumpTo("s02_sfx_14");   // the Maestro's rhythm on the chosen note
        await Until(() => p.CurrentClip == "s02_sfx_14_acuto", "the high variant plays", 5);

        p.JumpTo("s02_int_05");
        await Until(() => p.WaitingInteractionInput, "rhythm capture", 5);
        double t0 = Clock + 0.2;
        await Press(p, new[] { 1, 1, 1 }, t0, new[] { 0, 0.32, 0.95 });
        await Until(() => Logged(p, "s02_int_05 passed"), "TAC TAC... TOC on the piano", 6);
        return $"Scene 2: {checks} checks passed; register {StoryPlayer.ChosenRegister}";
    }

    public static async Task<string> RunScene3()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "grave";
        var p = await Load("C1_03_Scena3", "s03");
        p.Log.Clear();

        p.JumpTo("s03_int_01");   // the middle button: index does nothing, middle passes
        await Until(() => p.WaitingInteractionInput, "middle prompt", 5);
        p.PressLogical(1);
        await Task.Delay(200);
        Check(!Logged(p, "s03_int_01 passed"), "the index does not confirm the middle-finger test");
        p.PressLogical(2);
        await Until(() => Logged(p, "s03_int_01 passed"), "middle press passes", 5);

        p.JumpTo("s03_sfx_04");   // the count-in starts the loop on the beat
        await Until(() => p.BattleRunning, "the battle loop is scheduled", 6);
        double bar = p.BattleBarSeconds, start = p.BattleStart;
        Check(Math.Abs(bar - 2.4) < 1e-3, "100 BPM: a bar is 2.4 s");
        Check(Math.Abs(start - Clock - 4.8) < 0.25, "the loop starts two bars (the count-in) later");

        // Round 1: D... D-D, echoed on the beat of the echo bar (bar 3 of the exchange).
        await Until(() => p.Current?.Id == "s03_int_02" && p.WaitingInteractionInput, "round 1", 8);
        double ex = p.ExchangeStart;
        Check(Math.Abs(ex - start) < 1e-6, "the first exchange starts on the first bar of the loop");
        await Press(p, new[] { 1, 1, 1 }, ex + 2 * bar, new[] { 0, 1.2, 1.5 });
        await Until(() => Logged(p, "s03_int_02 passed"), "round 1 passes", 6);
        await Until(() => p.BattleLayers == 1, "success adds the pad at the bar line", 8);

        // Round 2: a wrong middle note first (Sbuffo laughs, the top layer goes), then the right echo.
        await Until(() => p.Current?.Id == "s03_int_03" && p.WaitingInteractionInput, "round 2", 25);
        ex = p.ExchangeStart;
        double k = (ex - start) / bar;
        Check(Math.Abs(k - 4 * Math.Round(k / 4)) < 1e-6, "rounds start on an exchange boundary (chord change)");
        await Press(p, new[] { 1, 1, 1 }, ex + 2 * bar, new[] { 0, 1.2, 1.5 });
        await Until(() => Logged(p, "s03_int_03 not passed"), "the wrong echo is not accepted", 6);
        await Until(() => p.BattleLayers == 0, "a failure removes only the top layer", 6);
        await Until(() => p.WaitingInteractionInput && p.ExchangeStart > ex, "the same call comes back", 25);
        ex = p.ExchangeStart;
        await Press(p, new[] { 1, 2, 1 }, ex + 2 * bar, new[] { 0, 1.2, 1.5 });
        await Until(() => Logged(p, "s03_int_03 passed"), "round 2 passes", 6);

        // Round 3: La... Fa-Re, the ring finger joins: the same rhythm, an echo like the others.
        await Until(() => p.Current?.Id == "s03_int_04" && p.WaitingInteractionInput, "round 3", 30);
        ex = p.ExchangeStart;
        await Press(p, new[] { 3, 2, 1 }, ex + 2 * bar, new[] { 0, 1.2, 1.5 });
        await Until(() => Logged(p, "s03_int_04 passed"), "round 3 passes", 6);

        // Victory: the loop stops on a bar line and the fanfare starts exactly there.
        await Until(() => p.Current?.Id == "s03_sfx_05", "the fanfare", 15);
        Check(!p.BattleRunning && Logged(p, "battle ends on the bar line"), "the loop ends with the fanfare");
        Check(p.CurrentClip == "s03_sfx_05_grave", "the reprise is in the chosen register");
        return $"Scene 3: {checks} checks passed";
    }

    public static async Task<string> RunScene4()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "acuto";
        var p = await Load("C1_04_Scena4", "s04");
        p.Log.Clear();

        p.JumpTo("s04_sfx_01");   // the recovered song, in the chosen register
        await Until(() => p.CurrentClip == "s04_sfx_01_acuto", "the high variant of the song", 5);

        p.JumpTo("s04_int_01");   // free play: any buttons, the piece ends by itself
        await Until(() => p.WaitingInteractionInput, "free play listens", 5);
        foreach (int b in new[] { 1, 3, 2, 4, 4, 1 }) { p.PressLogical(b); await Task.Delay(400); }
        Check(p.LastAttempt == "6 notes played", "every press is a note");
        await Until(() => Logged(p, "s04_int_01 passed (6 notes"), "the story goes on when the music ends", 20);

        p.JumpTo("s04_int_02");   // Sbuffo question: No leads to branch B, there is no wrong answer
        await Until(() => p.WaitingInteractionInput, "Sbuffo question", 5);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => p.Current != null && p.Current.Id == "s04_maes_09", "No leads to branch B", 8);
        return $"Scene 4: {checks} checks passed";
    }

    public static async Task<string> RunScene5()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "grave";
        var p = await Load("C1_05_Scena5", "s05");
        p.Log.Clear();

        p.JumpTo("s05_sfx_01");   // the piano goes on softly, in the register chosen in Scene 2
        await Until(() => p.CurrentClip == "s05_sfx_01_grave", "the low variant of the piano", 5);

        p.JumpTo("s05_int_01");   // index = drum (A), middle = bell (B); the thumbs do not choose
        await Until(() => p.WaitingInteractionInput, "drum or bell", 5);
        p.PressLogical(StoryInteractionRules.Yes);
        await Task.Delay(300);
        Check(p.Current != null && p.Current.Id == "s05_int_01" && p.WaitingInteractionInput, "Yes does not choose an instrument");
        p.PressLogical(1);
        await Until(() => p.Current != null && p.Current.Id == "s05_maes_03", "the index leads to the drum (branch A)", 8);
        Check(p.BranchChoice == "A", "index = branch A");
        Check(StoryPlayer.ChosenRegister == "grave", "this choice does not change the piano register");

        p.JumpTo("s05_int_01");
        await Until(() => p.WaitingInteractionInput, "drum or bell again", 5);
        p.PressLogical(2);
        await Until(() => p.Current != null && p.Current.Id == "s05_maes_04", "the middle finger leads to the bell (branch B)", 8);
        Check(p.BranchChoice == "B", "middle = branch B");
        return $"Scene 5: {checks} checks passed";
    }

    public static async Task<string> RunEpilogue()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "acuto";
        var p = await Load("C1_99_Epilogo", "e00");
        p.Log.Clear();

        p.JumpTo("e00_sfx_01");   // the recovered melody, slow, in the chosen register
        await Until(() => p.CurrentClip == "e00_sfx_01_acuto", "the high variant of the slow melody", 5);

        // TA... TA-TA once more: nothing is counted, every press is a note, the piano answers anyway.
        p.JumpTo("e00_int_01");
        await Until(() => p.WaitingInteractionInput, "the last exchange listens", 5);
        Check(p.CurrentClip == "x_e00_eco_acuto", "the exchange is in the chosen register");
        foreach (int b in new[] { 1, 1, 1 }) { p.PressLogical(b); await Task.Delay(300); }
        Check(p.LastAttempt == "3 notes played", "every press is a note");
        await Until(() => Logged(p, "e00_int_01 passed (3 notes"), "the story goes on when the exchange ends", 20);

        // No answer at all: the piano plays the phrase by itself and the story still goes on.
        p.JumpTo("e00_int_01");
        await Until(() => p.WaitingInteractionInput, "the last exchange listens again", 5);
        await Until(() => Logged(p, "e00_int_01 passed (0 notes)"), "silence is fine", 20);
        await Until(() => p.Current != null && p.Current.Id == "e00_maes_02", "the Maestro's goodbye follows", 8);

        p.JumpTo("e00_sfx_05");   // the laugh, then the whole song fading out
        await Until(() => p.CurrentClip == "e00_sfx_05_acuto", "the high variant of the final song", 5);
        return $"Epilogue: {checks} checks passed";
    }

    public static async Task<string> RunContinuity()
    {
        checks = 0;
        StoryPlayer.ChosenRegister = "grave";
        var p = await Load("C1_04_Scena4", "s04");
        p.Log.Clear();
        string scene = SceneManager.GetActiveScene().name;

        // The last line of Scene 4: the chapter goes on into Scene 5 in the same Unity scene.
        p.continueToNextSection = true;
        p.JumpTo("s04_maes_11");
        await Until(() => p.section == "s05" && p.Current != null && p.Current.Id == "s05_sfx_01", "Scene 5 follows Scene 4", 30);
        Check(SceneManager.GetActiveScene().name == scene, "no scene load between two sections");
        Check(p.BranchChoice == "A" && Logged(p, "section s05"), "the new section starts on the main line");
        Check(StoryPlayer.SectionName("s05") == "Scena 5" && StoryPlayer.SectionName("p00") == "Intro" &&
              StoryPlayer.SectionName("e00") == "Epilogo", "the overlay names the scenes");

        // "This scene only": the end of Scene 5 stops there.
        p.continueToNextSection = false;
        p.JumpTo("s05_maes_04");
        await Until(() => p.Current == null, "Scene 5 ends", 30);
        await Task.Delay(1500);
        Check(p.section == "s05" && p.Current == null, "with play-on off the story stops at the end of the scene");
        p.continueToNextSection = true;
        return $"Continuity: {checks} checks passed";
    }
}
