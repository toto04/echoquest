// Play-mode checks of chapter 3 (la campanella) with the real scene data (capitolo_03_regia.csv, clips, timing).
// Open any story scene, set run-in-background, enter Play mode, then evaluate this file followed by
//   return await CampanellaRuntimeChecks.RunScene1();   (one Run method per scene)
// (unity command script.eval --file <combined file>). Presses go through the on-screen buttons
// (StoryPlayer.PressLogical). Parts that only wait for the story run at a higher playback speed.
using System;
using System.Linq;
using System.Threading.Tasks;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampanellaRuntimeChecks
{
    const string Scenes = "Assets/EchoQuest/Story/Scenes/Capitolo3_Campanella/";
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
        await Until(() => s_Player.Now >= time - 0.003, "story time " + time, 30);
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
        p.SetSpeed(3f);
        await Task.Delay(300);
        p.Log.Clear();
        return p;
    }

    static bool Logged(StoryPlayer p, string text) => p.Log.Any(line => line.Contains(text));

    static bool Playing(StoryPlayer p, string label) => p.Active.Any(s => s != null && s.Label == label);

    /// <summary>Waits until the Maestro asks the question of a listening game (its clip is playing).</summary>
    static Task Asked(StoryPlayer p, string clip, string label) => Until(() => p.CurrentClip == clip, label, 20);

    /// <summary>Every line, sound and interaction of chapter 3 has its recording or clip.</summary>
    public static async Task<string> RunAssets()
    {
        checks = 0;
        var p = await Load("C3_00_Prologo", "c00");
        var missing = p.AllItems.Where(i => i.Section.StartsWith("c") && !p.HasAudio(i)).Select(i => i.Id).ToList();
        Check(missing.Count == 0, "missing audio: " + string.Join(", ", missing));
        Check(p.AllItems.Count(i => i.Kind == StoryItemKind.Interaction) >= 27, "every interaction of the script is there");
        return $"Campanella assets: {checks} checks passed, {p.AllItems.Count} items";
    }

    /// <summary>Prologue and Scene 1: the bell's question with no answer, the prologue playing on into
    /// Scene 1, free play on the bells, "is the second shorter?", then long or short with a finger.</summary>
    public static async Task<string> RunScene1()
    {
        checks = 0;
        Check(StoryPlayer.SectionName("c00") == "Prologo" && StoryPlayer.SectionName("c03") == "Scena 3" &&
              StoryPlayer.SectionName("c99") == "Epilogo", "section names for chapter 3");

        var p = await Load("C3_00_Prologo", "c00");
        Check(p.Sections().First() == "c00" && p.Sections().Contains("c99"), "the scene holds chapter 3's script");
        await Until(() => p.Current?.Id == "c00_sfx_01", "the prologue starts", 5);
        Check(p.CurrentClip == "c00_sfx_01_" + p.Register, "the prologue opens on the piano's piece, in the chosen register");

        p.JumpTo("c00_sfx_03");
        p.SetSpeed(3f);
        await Until(() => p.section == "c01" && p.Current?.Id == "c01_maes_01", "the prologue continues into Scene 1", 20);

        // Free play: every button sounds its bell over the soft F chord; nothing is judged.
        p.JumpTo("c01_int_01");
        await Until(() => p.WaitingInteractionInput, "free play listens", 5);
        p.SetSpeed(1f);
        Check(p.CurrentClip == "x_c01_accordo", "free play runs over the F chord");
        p.PressLogical(4);
        await Task.Delay(100);
        Check(Playing(p, "x_c01_campanella_do"), "the little finger plays the Do bell");
        p.PressLogical(2);
        p.SetSpeed(3f);
        await Until(() => Logged(p, "c01_int_01 passed (2 notes"), "free play ends with the chord", 15);

        // "Is the second sound shorter?": No is wrong (the examples come back), Yes is right.
        await Until(() => p.Current?.Id == "c01_int_02" && p.WaitingInteractionInput, "the comparison asks", 40);
        p.PressLogical(StoryInteractionRules.No);
        await Until(() => Logged(p, "c01_int_02 not passed"), "No is not the answer", 5);
        await Until(() => p.WaitingInteractionInput, "the rings come back", 30);
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => Logged(p, "c01_int_02 passed"), "Yes: the hand stopped it", 5);

        // Long or short: the first ring has no question (the Maestro just explained); a wrong finger brings back
        // the ring and then the question; the ring finger is not an answer.
        await Until(() => p.Current?.Id == "c01_int_03" && p.WaitingInteractionInput, "long or short?", 40);
        Check(p.CurrentClip == "x_c01_lungo", "the first listen is the ring alone");
        p.PressLogical(3);
        await Task.Delay(200);
        Check(!Logged(p, "c01_int_03 not passed") && p.WaitingInteractionInput, "the ring finger is not an answer");
        p.PressLogical(2);
        await Until(() => Logged(p, "c01_int_03 not passed"), "short is wrong", 5);
        await Asked(p, "eq_c01_domanda_lungo", "after the ring comes back, the question");
        await Until(() => p.WaitingInteractionInput, "listening again", 20);
        p.PressLogical(1);
        await Until(() => Logged(p, "c01_int_03 passed"), "long, index", 5);
        await Asked(p, "eq_c01_domanda_lungo", "the second ring is asked straight away (chiedi = sempre)");
        await Until(() => p.Current?.Id == "c01_int_04" && p.WaitingInteractionInput, "and this one?", 20);
        p.PressLogical(2);
        await Until(() => Logged(p, "c01_int_04 passed"), "short, middle", 5);
        await Until(() => p.Current?.Id == "c01_maes_05", "the scene ends with the Maestro's carrot", 20);
        p.SetSpeed(1f);
        return $"Campanella Scene 1: {checks} checks passed";
    }

    static Task Ready(StoryPlayer p, string id) => Until(() => p.Current?.Id == id && p.WaitingInteractionInput, id, 60);

    static async Task Echo(StoryPlayer p, int[] buttons, double[] times, int calls = 2, int bars = 1)
    {
        double start = p.ExchangeStart + calls * bars * p.BattleBarSeconds;
        for (int i = 0; i < buttons.Length; i++)
        {
            await At(start + times[i]);
            p.PressLogicalAt(buttons[i], start + times[i]);
        }
    }

    public static async Task<string> RunScene2()
    {
        checks = 0;
        var p = await Load("C3_02_Scena2", "c02");
        p.SetSpeed(3f);
        for (int i = 1; i <= 3; i++)
        {
            string id = "c02_int_0" + i;
            await Ready(p, id);
            p.PressLogical(i == 2 ? 2 : 1);
            await Until(() => Logged(p, id + " passed"), id + " correct side", 10);
            checks++;
        }
        await Ready(p, "c02_int_04");
        p.SetSpeed(1f);
        p.PressLogical(1); // one instead of two
        await Until(() => Logged(p, "c02_int_04 not passed"), "wrong count rejected", 5);
        await Ready(p, "c02_int_04");
        p.PressLogical(1);
        await At(p.Now + 0.25);
        p.PressLogical(1);
        await Until(() => Logged(p, "c02_int_04 passed"), "two rings", 6);
        await Ready(p, "c02_int_05");
        // Uneven spacing still counts: rhythm is irrelevant here.
        p.PressLogical(1);
        await At(p.Now + 0.2); p.PressLogical(1);
        await At(p.Now + 0.7); p.PressLogical(1);
        await Until(() => Logged(p, "c02_int_05 passed"), "three rings, uneven rhythm", 6);
        Check(Logged(p, "count 3 presses, 3 rings: right"), "count judges quantity");
        return $"Campanella Scene 2: {checks} checks passed";
    }

    public static async Task<string> RunAnswers()
    {
        checks = 0;
        var p = await Load("C3_03_Scena3", "c03");
        p.JumpTo("c03_int_02");
        await Ready(p, "c03_int_02");
        await Echo(p, new[] {4,3}, new[] {0d,.6});
        await Until(() => Logged(p, "c03_int_02 passed"), "bell greeting", 20);
        Check(p.BattleRunning, "practice loop continues");
        p.JumpTo("c03_int_05");
        await Ready(p, "c03_int_05");
        await Echo(p, new[] {2,3,2}, new[] {0d,.6,1.2});
        await Until(() => Logged(p, "c03_int_05 passed"), "home refrain", 20);
        await Ready(p, "c04_int_01");
        double ex = p.ExchangeStart;
        await Echo(p, new[] {4,3}, new[] {0d,.6}); // repeating the question is wrong
        await Until(() => Logged(p, "c04_int_01 not passed"), "question is not the answer", 20);
        await Until(() => p.WaitingInteractionInput && p.ExchangeStart > ex, "retry answer", 30);
        await Echo(p, new[] {2,3,2}, new[] {0d,.6,1.2});
        await Until(() => Logged(p, "c04_int_01 passed"), "answer differs from question", 20);
        Check(Logged(p, "c04_int_01 passed"), "fixed answer accepted");
        return $"Campanella answers: {checks} checks passed";
    }

    public static async Task<string> RunFreeAnswers()
    {
        checks = 0;
        var p = await Load("C3_05_Scena5", "c05");
        p.JumpTo("c05_sfx_02");
        for (int i = 1; i <= 3; i++)
        {
            await Ready(p, "c05_int_0" + i);
            p.ForcePass();
        }
        await Ready(p, "c05_int_04");
        double ex = p.ExchangeStart;
        await Echo(p, new[] {2,4,2}, new[] {0d,.4,2.5}, calls: 1); // home after the 2.4-second response bar is too late
        await Until(() => Logged(p, "c05_int_04 not passed"), "must end at home within the response bar", 20);
        await Until(() => p.WaitingInteractionInput && p.ExchangeStart > ex, "free answer retry", 30);
        await Echo(p, new[] {4,1,2}, new[] {0d,.3,.8}, calls: 1);
        await Until(() => Logged(p, "c05_int_04 passed"), "invented answer ending at home", 20);
        Check(p.LastAttempt.Contains("a real answer"), "free phrase accepted");
        p.JumpTo("c05_int_06");
        await Ready(p, "c05_int_06");
        await Echo(p, new[] {4}, new[] {0d}, calls: 1);
        await Until(() => Logged(p, "c05_int_06 passed"), "any answer to Sbuffo", 20);
        Check(!Logged(p, "c05_int_06 not passed"), "no wrong answer to Sbuffo");
        p.JumpTo("c05_int_06");
        await Ready(p, "c05_int_06");
        await Until(() => Logged(p, "no answer after the TOC, then x_c03_altalena"), "companion answers silence", 20);
        Check(Logged(p, "then x_c03_altalena"), "silence has a musical response");
        return $"Campanella free answers: {checks} checks passed";
    }

    public static async Task<string> RunLullaby()
    {
        checks = 0;
        var p = await Load("C3_05_Scena5", "c05");
        p.JumpTo("c05_int_08");
        await Ready(p, "c05_int_08");
        Check(Math.Abs(p.BattleBarSeconds - 1.8) < .001, "three beats at 100 BPM");
        double firstExchange = p.ExchangeStart;
        await Echo(p, new[] {3,2,3,4}, new[] {0d,1.2,1.8,3d}, bars: 2);
        await Until(() => Logged(p, "piece 1 of 2) passed"), "first two bars", 20);
        await Until(() => p.WaitingInteractionInput && p.ExchangeStart > firstExchange, "second piece opens", 20);
        await Echo(p, new[] {3,2,1}, new[] {0d,1.2,1.8}, bars: 2);
        await Until(() => Logged(p, "piece 2 of 2) passed"), "last two bars", 20);
        Check(Logged(p, "piece 2 of 2) passed"), "both pieces played");
        return $"Campanella lullaby: {checks} checks passed";
    }

    public static async Task<string> RunChapterHandoffs()
    {
        checks = 0;
        StoryPlayer.PlayedChapters.Clear();
        var p = await Load("C3_99_Epilogo", "c99");
        p.JumpTo("c99_int_01");
        await Ready(p, "c99_int_01");
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => SceneManager.GetActiveScene().name == "C2_00_Prologo", "bell to drum", 20);
        Check(StoryPlayer.PlayedChapters.Contains("C3"), "bell remembered in session");
        p = await Load("C3_99_Epilogo", "c99");
        StoryPlayer.PlayedChapters.Add("C2");
        p.JumpTo("c99_int_01");
        await Ready(p, "c99_int_01");
        p.PressLogical(StoryInteractionRules.Yes);
        await Until(() => p.Current == null && p.Status.StartsWith("Fine del capitolo"), "no repeated chapter", 20);
        Check(StoryPlayer.NextChapterScene == "", "played drum is skipped");
        return $"Campanella handoffs: {checks} checks passed";
    }

    public static async Task<string> RunFinalRing()
    {
        checks = 0;
        var p = await Load("C3_05_Scena5", "c05");
        p.JumpTo("c05_int_05");
        await Ready(p, "c05_int_05");
        double start = p.BeatStart;
        await At(start + 1.4);
        p.PressLogicalAt(1, start + 1.4);
        await Until(() => Logged(p, "c05_int_05 not passed"), "early bell rejected", 10);
        await Until(() => p.WaitingInteractionInput && p.BeatStart > start, "count again", 20);
        start = p.BeatStart;
        await At(start + 1.85);
        p.PressLogicalAt(4, start + 1.85);
        await Until(() => Logged(p, "c05_int_05 passed"), "bell on four", 10);
        Check(p.LastAttempt.EndsWith("BUM"), "on-time final ring");
        return $"Campanella final ring: {checks} checks passed";
    }
}
