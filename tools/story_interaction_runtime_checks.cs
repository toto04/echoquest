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
        public Harness(string mode, string times = "", int attempts = 0, bool twoExercises = false,
            AudioClip noteExample = null, string declineVoices = null)
        {
            Root = new GameObject("Interaction test"); Root.SetActive(false);
            Player = Root.AddComponent<StoryPlayer>();
            var listener = new GameObject("Test listener"); listener.transform.SetParent(Root.transform);
            listener.AddComponent<AudioListener>(); Player.listener = listener.transform;
            Player.startDelay = Player.pauseAfterLine = Player.pauseAfterSound = 0;
            string sequence = "ordine,sezione,ramo,tipo,id,personaggio,testo,dentro_battuta\n";
            sequence += "1,s01,,interazione,s01_int_01,,,\n" + (twoExercises ?
                "2,s01,,interazione,s01_int_02,,,\n3,s01,,convergenza,s01_int_03,,,\n" :
                "2,s01,,convergenza,s01_int_02,,,\n");
            Player.section = "s01";
            Player.sequence = new TextAsset(sequence); Assets.Add(Player.sequence);
            string example = mode == "conferma" || mode == "scelta" ? "" : mode == "confronto" ? "eq_test_example;eq_test_example2" : "eq_test_example";
            if (noteExample != null) example = noteExample.name;
            string row = "s01_int_01," + mode + ",eq_test_prompt,eq_test_success,eq_test_retry," + example + "," + times + "," + (mode == "scelta" ? "5=B;6=A" : "") + "," + (mode == "conferma" ? declineVoices ?? "eq_test_retry" : "") + "," +
                (attempts > 0 ? attempts + ",eq_test_question,eq_test_continue;eq_test_continue_alt" : ",,") + "\n";
            Player.staging = new TextAsset("id,modalita,consegna,successo,riprova,esempi,tempi,scelte,attesa_no,tentativi,domanda_riprova,prosegui\n" +
                row + (twoExercises ? row.Replace("s01_int_01,", "s01_int_02,") : ""));
            Assets.Add(Player.staging);
            var clips = new List<AudioClip>();
            foreach (string name in new[] { "eq_test_prompt", "eq_test_success", "eq_test_retry", "eq_test_example", "eq_test_example2", "eq_test_question", "eq_test_continue", "eq_test_continue_alt", "eq_test_decline_a", "eq_test_decline_b" })
            {
                if (name == "eq_test_example" && noteExample != null) continue;
                var clip = AudioClip.Create(name, mode == "nota" && name == "eq_test_example" ? 16000 : 800, 1, 16000, false);
                clips.Add(clip); Assets.Add(clip);
            }
            if (noteExample != null) clips.Add(noteExample);
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
        public async Task BadRhythm()
        {
            double start = AudioSettings.dspTime;
            await Until(() => AudioSettings.dspTime >= start + .05, "rhythm input timestamps");
            double origin = InputState.currentTime - .02;
            foreach (double offset in new[] { 0d, .01, .02 })
            {
                State(origin + offset, Key.Digit1); State(origin + offset + .000001);
            }
            InputSystem.Update();
        }
        public async Task Finished() => await Until(() => Player.Current == null && Player.Status.StartsWith("Fine"), "interaction finish");
        public async Task Dispose()
        {
            UnityEngine.Object.Destroy(Root);
            await Until(() => Root == null, "harness cleanup");
            InputSystem.RemoveDevice(Keys);
            foreach (var asset in Assets) UnityEngine.Object.Destroy(asset);
        }
    }

    public static async Task<string> NoteAfterEditorPause()
    {
        if (!Application.isPlaying) throw new Exception("Run in Play mode in an empty temporary scene.");
        await RealNoteTiming(false, Key.Digit1);
        await RealNoteTiming(false, Key.Numpad1);
        await RealNoteTiming(true, Key.Digit1);
        return "PASS: real missing note accepts top-row/keypad 1 and survives Editor pause";
    }

    static async Task RealNoteTiming(bool pauseEditor, Key key)
    {
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/EchoQuest/Story/Audio/Sfx/x_s01_nota_mancante.wav");
        var h = new Harness("nota", "0;1.2;1.5", noteExample: clip);
        UnityEditor.EditorApplication.CallbackFunction resume = null;
        try
        {
            await h.Ready();
            double ready = AudioSettings.dspTime;
            if (pauseEditor)
            {
                var pause = System.Diagnostics.Stopwatch.StartNew();
                resume = () =>
                {
                    if (pause.Elapsed.TotalSeconds < 1) return;
                    UnityEditor.EditorApplication.isPaused = false;
                    UnityEditor.EditorApplication.update -= resume;
                };
                UnityEditor.EditorApplication.isPaused = true;
                UnityEditor.EditorApplication.update += resume;
                await Until(() => !UnityEditor.EditorApplication.isPaused, "Editor resumes during note exercise");
            }
            await Until(() => AudioSettings.dspTime >= ready + .15 + 1.5, "real missing-note moment");
            h.Press(key);
            await Until(() => h.Player.CurrentClip != clip.name, "real note response");
            Check(h.Player.CurrentClip == "eq_test_success", $"real note timing: {key}, Editor pause {pauseEditor}");
            await h.Finished();
        }
        finally
        {
            if (resume != null) UnityEditor.EditorApplication.update -= resume;
            UnityEditor.EditorApplication.isPaused = false;
            await h.Dispose();
        }
    }

    public static async Task<string> NoteAfterSilence()
    {
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/EchoQuest/Story/Audio/Sfx/x_s01_nota_mancante.wav");
        var h = new Harness("nota", "0;1.2;1.5", attempts: 1, noteExample: clip);
        try
        {
            await h.Ready();
            double ready = AudioSettings.dspTime;
            await Until(() => AudioSettings.dspTime >= ready + .15 + clip.length + .5, "press after the piano finishes");
            Check(h.Player.WaitingInteractionInput && !h.Player.WaitingRetryChoice,
                "the exercise must still accept input after the piano finishes");
            h.Press(Key.Digit1);
            await Until(() => h.Player.CurrentClip == "eq_test_success", "recognize the missing note in silence");
            await h.Finished();
            Check(!h.Player.WaitingRetryChoice, "a late response must not trigger another attempt");
            return "PASS: 1 is accepted in silence after the complete piano example";
        }
        finally { await h.Dispose(); }
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

            h = new Harness("conferma", declineVoices: "eq_test_decline_a;eq_test_decline_b");
            await h.Ready();
            foreach (string voice in new[] { "eq_test_decline_a", "eq_test_decline_b", "eq_test_decline_a" })
            {
                h.Press(Key.N);
                await Until(() => h.Player.CurrentClip == voice, "next invitation after No");
                await h.Ready(); await Frames();
                Check(h.Player.CurrentClip == voice, "silence does not advance the invitation loop");
            }
            h.Press(Key.S); await h.Finished();
            Check(!h.Player.WaitingInteractionInput, "Italian S confirms help after repeated refusals");
            await h.Dispose(); h = null;

            foreach (var choice in new[] { Key.Y, Key.N, Key.S })
            {
                h = new Harness("scelta"); await h.Ready();
                h.Press(choice); await h.Finished();
                Check(h.Player.BranchChoice == (choice == Key.N ? "A" : "B"), "thumb choice selects its narrative branch");
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

            h = new Harness("nota", "0;0.12;0.3"); await h.Ready();
            ready = AudioSettings.dspTime;
            await Until(() => AudioSettings.dspTime >= ready + .22, "early press during music");
            h.Press(Key.Digit1);
            await Until(() => AudioSettings.dspTime >= ready + .43, "corrected note timing");
            h.Press(Key.Digit1);
            await Until(() => h.Player.CurrentClip != "eq_test_example", "note response after early press");
            Check(h.Player.CurrentClip == "eq_test_success", "early press must not invalidate a later correct press");
            await h.Finished();
            await h.Dispose(); h = null;

            h = new Harness("conferma"); await h.Ready();
            h.Press(Key.Numpad1); await Frames();
            Check(h.Player.WaitingInteractionInput, "keypad notes cannot answer a thumb question");
            h.Press(Key.Y); await h.Finished();
            await h.Dispose(); h = null;

            h = new Harness("ritmo", "0;0.25;0.5"); await h.Ready();
            double entered = AudioSettings.dspTime;
            await Until(() => AudioSettings.dspTime >= entered + .6, "keypad rhythm timestamps");
            double keypadOrigin = InputState.currentTime - .5;
            foreach (double offset in new[] { 0d, .25, .5 })
            {
                h.State(keypadOrigin + offset, Key.Numpad1); h.State(keypadOrigin + offset + .000001);
            }
            InputSystem.Update(); await h.Finished();
            Check(!h.Player.WaitingInteractionInput, "keypad 1 can play the rhythm");
            await h.Dispose(); h = null;

            h = new Harness("nota", "0;0.12;0.3", attempts: 2); await h.Ready();
            typeof(StoryPlayer).GetProperty("BranchChoice").SetValue(h.Player, "B");
            await Until(() => h.Player.WaitingRetryChoice, "two missed note windows offer a choice", seconds: 12);
            Check(h.Player.CurrentClip == "eq_test_question", "question replaces automatic retry after two attempts");
            h.Press(Key.Digit1); await Frames();
            Check(h.Player.WaitingRetryChoice, "a musical button cannot answer the retry question");
            h.Player.TogglePause(); h.Press(Key.Y); await Frames();
            Check(h.Player.WaitingRetryChoice, "pause prevents a retry answer");
            h.Player.TogglePause(); await Frames();
            Check(h.Player.WaitingRetryChoice, "silence waits without repeating the retry question");
            h.Press(Key.Y);
            await Until(() => h.Player.WaitingInteractionInput && !h.Player.WaitingRetryChoice, "Yes starts another round");
            await Until(() => h.Player.CurrentClip == "eq_test_retry", "first miss in new round");
            Check(!h.Player.WaitingRetryChoice, "Yes resets the two-attempt allowance");
            await Until(() => h.Player.WaitingRetryChoice, "second round also stops after two misses");
            h.Press(Key.N);
            await Until(() => h.Player.CurrentClip == "eq_test_continue", "No receives encouragement");
            await h.Finished();
            Check(h.Player.BranchChoice == "B", "declining practice preserves the story branch");
            await h.Dispose(); h = null;

            h = new Harness("nota", "0;0.12;0.3", attempts: 2); await h.Ready();
            await Until(() => h.Player.WaitingRetryChoice, "retry question for Italian keyboard", seconds: 12);
            h.Press(Key.S);
            await Until(() => h.Player.WaitingInteractionInput && !h.Player.WaitingRetryChoice, "S starts another practice round");
            Check(!h.Player.WaitingRetryChoice, "Italian S answers the retry question");
            await h.Dispose(); h = null;

            h = new Harness("confronto", attempts: 2); await h.Ready();
            h.Press(Key.Y);
            await Until(() => h.Player.CurrentClip == "eq_test_retry", "first comparison miss");
            await h.Ready();
            Check(!h.Player.WaitingRetryChoice, "first miss still permits a second comparison");
            h.Press(Key.Y);
            await Until(() => h.Player.WaitingRetryChoice, "second comparison miss offers a choice");
            h.Press(Key.N);
            await Until(() => h.Player.CurrentClip == "eq_test_continue", "No skips instead of solving the comparison");
            await h.Finished();
            Check(!h.Player.WaitingRetryChoice, "comparison exits cleanly on No");
            await h.Dispose(); h = null;

            h = new Harness("ritmo", "0;0.25;0.5", attempts: 2); await h.Ready();
            await h.BadRhythm();
            await Until(() => h.Player.CurrentClip == "eq_test_retry", "first rhythm miss");
            await h.Ready(); h.Player.Repeat();
            await Until(() => h.Player.CurrentClip == "eq_test_prompt", "repeat restarts exercise");
            await h.Ready(); await h.BadRhythm();
            await Until(() => h.Player.CurrentClip == "eq_test_retry", "first miss after repeating");
            await h.Ready();
            Check(!h.Player.WaitingRetryChoice, "repeat resets the attempt counter");
            await h.BadRhythm();
            await Until(() => h.Player.WaitingRetryChoice, "second rhythm miss offers a choice");
            h.Player.Restart();
            await Until(() => h.Player.CurrentClip == "eq_test_prompt", "restart during retry question");
            await h.Ready();
            Check(!h.Player.WaitingRetryChoice, "restart clears retry-choice state");
            await h.Dispose(); h = null;

            h = new Harness("confronto", attempts: 2, twoExercises: true);
            foreach (string response in new[] { "eq_test_continue", "eq_test_continue_alt" })
            {
                await h.Ready(); h.Press(Key.Y);
                await Until(() => h.Player.CurrentClip == "eq_test_retry", "first failed comparison before encouragement");
                await h.Ready(); h.Press(Key.Y);
                await Until(() => h.Player.WaitingRetryChoice, "retry choice before encouragement");
                h.Press(Key.N);
                await Until(() => h.Player.CurrentClip == response, "alternating encouragement " + response);
                Check(!h.Player.WaitingRetryChoice, "encouragement closes the retry question");
            }
            await h.Finished();
            await h.Dispose(); h = null;
            await NoteAfterEditorPause();
            await NoteAfterSilence();
            return checks;
        }
        catch (Exception e) { throw new Exception(e.Message + " | " + h?.Player.Status + " | item=" + h?.Player.Current?.Id + " | branch=" + h?.Player.BranchChoice + " | keyboard=" + Keyboard.current?.deviceId + " expected=" + h?.Keys.deviceId, e); }
        finally { if (h != null) await h.Dispose(); }
    }
}
