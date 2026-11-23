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
    const string Scenes = "Assets/EchoQuest/Story/Scenes/Tamburo/";
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

        var p = await Load("Tamburo_Prologo", "t00");
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
        var p = await Load("Tamburo_Scena2", "t02");

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
}
