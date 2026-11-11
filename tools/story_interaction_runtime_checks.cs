// Run in Play mode in an empty temporary scene; restore the original scene afterwards.
// unity command 'script run' --file ../../tools/story_interaction_runtime_checks.cs --entry StoryInteractionRuntimeChecks.Run
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class StoryInteractionRuntimeChecks
{
    static int checks;
    static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
        checks++;
    }
    static async Task Until(Func<bool> condition, string label, double seconds = 6)
    {
        double end = Time.realtimeSinceStartupAsDouble + seconds;
        while (!condition())
        {
            if (Time.realtimeSinceStartupAsDouble > end) throw new Exception("Timeout: " + label);
            await Task.Delay(10);
        }
    }
    static Task Frames()
    {
        int frame = Time.frameCount;
        return Until(() => Time.frameCount > frame + 1, "two updates");
    }
    sealed class Harness
    {
        public GameObject Root;
        public StoryPlayer Player;
        public Keyboard Keys;
        public readonly List<UnityEngine.Object> Assets = new();
        public Harness(string mode, string times = "")
        {
            Root = new GameObject("Interaction test"); Root.SetActive(false);
            Player = Root.AddComponent<StoryPlayer>();
            var listener = new GameObject("Test listener"); listener.transform.SetParent(Root.transform);
            listener.AddComponent<AudioListener>(); Player.listener = listener.transform;
            Player.startDelay = Player.pauseAfterLine = Player.pauseAfterSound = 0;
            string sequence = "ordine,sezione,ramo,tipo,id,personaggio,testo,dentro_battuta\n";
            sequence += "1,s01,,interazione,s01_int_01,,,\n2,s01,,convergenza,s01_int_02,,,\n";
            Player.section = "s01";
            Player.sequence = new TextAsset(sequence); Assets.Add(Player.sequence);
            string example = mode == "conferma" || mode == "scelta" ? "" : mode == "confronto" ? "eq_test_example;eq_test_example2" : "eq_test_example";
            Player.staging = new TextAsset("id,modalita,consegna,successo,riprova,esempi,tempi,scelte,attesa_no\n" +
                "s01_int_01," + mode + ",eq_test_prompt,eq_test_success,eq_test_retry," + example + "," + times + "," + (mode == "scelta" ? "5=B;6=A" : "") + "," + (mode == "conferma" ? "eq_test_retry" : "") + "\n");
            Assets.Add(Player.staging);
            var clips = new List<AudioClip>();
            foreach (string name in new[] { "eq_test_prompt", "eq_test_success", "eq_test_retry", "eq_test_example", "eq_test_example2" })
            {
                var clip = AudioClip.Create(name, mode == "nota" && name == "eq_test_example" ? 16000 : 800, 1, 16000, false);
                clips.Add(clip); Assets.Add(clip);
            }
            Player.clips = clips.ToArray(); Keys = InputSystem.AddDevice<Keyboard>();
            Root.SetActive(true);
        }
        public void State(double time, params Key[] keys) => InputSystem.QueueStateEvent(Keys, new KeyboardState(keys), time);
        public void Press(Key key)
        {
            double t = InputState.currentTime;
            State(t, key); State(t + .000001); InputSystem.Update();
        }
        public async Task Ready() => await Until(() => Player.WaitingInteractionInput, "input ready");
        public async Task Finished() => await Until(() => Player.Current == null && Player.Status.StartsWith("Fine"), "interaction finish");
        public async Task Dispose()
        {
            UnityEngine.Object.Destroy(Root);
            await Until(() => Root == null, "harness cleanup");
            InputSystem.RemoveDevice(Keys);
            foreach (var asset in Assets) UnityEngine.Object.Destroy(asset);
        }
    }
    public static async Task<int> Run()
    {
        if (!Application.isPlaying) throw new Exception("Run in Play mode in an empty temporary scene.");
        checks = 0;
        Harness h = null;
        try
        {
            h = new Harness("conferma"); await h.Ready();
            h.Press(Key.Digit2); await Frames();
            Check(h.Player.WaitingInteractionInput, "wrong finger must not confirm");
            h.Player.TogglePause(); h.Press(Key.Y); await Frames();
            Check(h.Player.WaitingInteractionInput, "paused input must not confirm");
            h.Player.TogglePause(); await Frames();
            Check(h.Player.WaitingInteractionInput, "paused input must not leak on resume");
            h.Press(Key.Y); await h.Finished(); Check(!h.Player.WaitingInteractionInput, "valid confirmation closes input");
            await h.Dispose(); h = null;

            h = new Harness("conferma"); await h.Ready();
            h.Press(Key.N);
            await Until(() => h.Player.CurrentClip == "eq_test_retry", "No is acknowledged");
            await h.Ready(); await Frames();
            Check(h.Player.WaitingInteractionInput && h.Player.CurrentClip == "eq_test_retry", "No waits without repeating the question");
            h.Press(Key.Y); await h.Finished();
            Check(!h.Player.WaitingInteractionInput, "Yes resumes after No");
            await h.Dispose(); h = null;

            foreach (var choice in new[] { Key.Y, Key.N })
            {
                h = new Harness("scelta"); await h.Ready();
                h.Press(choice); await h.Finished();
                Check(h.Player.BranchChoice == (choice == Key.Y ? "B" : "A"), "thumb choice selects its narrative branch");
                await h.Dispose(); h = null;
            }

            h = new Harness("confronto");
            // Seed existing story state; old frame-polled keyboard input is outside these event-input checks.
            await h.Ready();
            typeof(StoryPlayer).GetProperty("BranchChoice").SetValue(h.Player, "B");
            Check(h.Player.BranchChoice == "B", "existing narrative branch B");
            h.Press(Key.Digit1); await Frames();
            Check(h.Player.WaitingInteractionInput, "a note cannot answer a question");
            h.Press(Key.Y);
            await Until(() => h.Player.CurrentClip == "eq_test_retry", "wrong comparison replays");
            await h.Ready(); Check(h.Player.BranchChoice == "B", "retry preserves narrative branch");
            h.Press(Key.N); await h.Finished();
            Check(h.Player.BranchChoice == "B", "comparison success preserves narrative branch");
            h.Player.Restart(); await h.Ready();
            Check(h.Player.BranchChoice == "A", "restart resets narrative branch");
            h.Press(Key.N); await h.Finished();
            Check(!h.Player.WaitingInteractionInput, "restart clears previous response");
            await h.Dispose(); h = null;

            h = new Harness("ritmo", "0;0.25;0.5"); await h.Ready();
            double started = AudioSettings.dspTime;
            await Until(() => AudioSettings.dspTime >= started + .65, "recorded input timestamps");
            double origin = InputState.currentTime - .5;
            foreach (double offset in new[] { 0d, .25, .5 })
            {
                h.State(origin + offset, Key.Digit1); h.State(origin + offset + .000001);
            }
            InputSystem.Update(); await h.Finished();
            Check(!h.Player.WaitingInteractionInput, "buffered rhythm completes and closes input");
            await h.Dispose(); h = null;

            h = new Harness("ritmo", "0;0.25;0.5"); await h.Ready();
            h.State(InputState.currentTime, Key.Digit1); InputSystem.Update(); await Frames();
            h.State(InputState.currentTime, Key.Digit1); InputSystem.Update(); await Frames();
            Check(h.Player.WaitingInteractionInput, "held button must not complete rhythm");
            h.State(InputState.currentTime); InputSystem.Update();
            h.Player.Repeat();
            await Until(() => h.Player.CurrentClip == "eq_test_prompt", "repeat restarts prompt");
            await h.Ready();
            h.Press(Key.Digit1); await Frames(); h.Press(Key.Digit1); await Frames();
            Check(h.Player.WaitingInteractionInput, "repeat clears earlier presses");
            await h.Dispose(); h = null;

            h = new Harness("nota", "0;0.12;0.3"); await h.Ready();
            double ready = AudioSettings.dspTime;
            h.Press(Key.Digit1); await Frames();
            Check(h.Player.WaitingInteractionInput, "press before scheduled music is ignored");
            await Until(() => AudioSettings.dspTime >= ready + .43, "third-note moment");
            h.Press(Key.Digit1); await h.Finished();
            Check(!h.Player.WaitingInteractionInput, "scheduled note completes and closes input");
            await h.Dispose(); h = null;
            return checks;
        }
        catch (Exception e) { throw new Exception(e.Message + " | " + h?.Player.Status + " | item=" + h?.Player.Current?.Id + " | branch=" + h?.Player.BranchChoice + " | keyboard=" + Keyboard.current?.deviceId + " expected=" + h?.Keys.deviceId, e); }
        finally { if (h != null) await h.Dispose(); }
    }
}
