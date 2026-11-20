// Run in Play mode in an empty temporary scene.
// unity command 'script run' --file ../../tools/story_audio_checks.cs --entry StoryAudioChecks.Run
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EchoQuest.Story;
using UnityEditor;
using UnityEngine;

public static class StoryAudioChecks
{
    static async Task Until(Func<bool> condition, string label, double seconds = 8)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            if (watch.Elapsed.TotalSeconds > seconds) throw new Exception("Timeout: " + label);
            await Task.Delay(10);
        }
    }

    static async Task CheckPlayback(bool interaction)
    {
        var root = new GameObject("Audio sequencing regression"); root.SetActive(false);
        var player = root.AddComponent<StoryPlayer>();
        var listener = new GameObject("Test listener"); listener.transform.SetParent(root.transform);
        listener.AddComponent<AudioListener>(); player.listener = listener.transform;
        player.startDelay = player.pauseAfterLine = 0;
        player.section = "s01";
        player.continueToNextSection = false;
        var sequence = new TextAsset("ordine,sezione,ramo,tipo,id,personaggio,testo,dentro_battuta\n" +
            (interaction ? "1,s01,,interazione,s01_int_01,,,\n" :
            "1,s01,,voce,s01_maes_01,MAESTRO,Prima battuta,\n" +
            "2,s01,,voce,s01_maes_02,MAESTRO,Seconda battuta,\n"));
        player.sequence = sequence;
        var staging = new TextAsset("id,modalita,consegna\ns01_int_01,conferma,s01_maes_01\n");
        if (interaction) player.staging = staging;
        var first = AudioClip.Create(interaction ? "s01_maes_01_p1_a" : "s01_maes_01_a", 32000, 1, 16000, false);
        var second = AudioClip.Create(interaction ? "s01_maes_01_p2_a" : "s01_maes_02_a", 32000, 1, 16000, false);
        player.clips = new[] { first, second };
        bool background = Application.runInBackground;
        Application.runInBackground = true;
        EditorApplication.CallbackFunction resume = null;
        try
        {
            root.SetActive(true);
            await Until(() => player.Active.Any(s => s != null && s.Source.time > .1f), "first voice starts");
            var source = player.Active.Single(s => s != null).Source;
            // Reproduce a real Editor pause: audio freezes while wall time continues.
            // Editor updates resume it automatically, independently of game coroutines.
            var pause = Stopwatch.StartNew();
            resume = () =>
            {
                if (pause.Elapsed.TotalSeconds < 2.5) return;
                EditorApplication.isPaused = false;
                EditorApplication.update -= resume;
            };
            EditorApplication.isPaused = true;
            EditorApplication.update += resume;
            await Until(() => !EditorApplication.isPaused, "Editor resumes");
            float resumedAt = source.time;
            await Until(() => player.CurrentClip == second.name, "second voice starts");
            int voices = player.Active.Count(s => s != null && s.IsVoice && s.Source.isPlaying);
            if (voices > 1 || source != null && source.isPlaying)
                throw new Exception($"Voice overlap after Editor pause: {voices} active voices; first at {source?.time:F3}s / {first.length:F3}s, resumed at {resumedAt:F3}s");
            await Until(() => interaction ? player.WaitingInteractionInput : player.Current == null, "last voice finishes");
            if (player.Active.Any(s => s != null && s.IsVoice && s.Source.isPlaying))
                throw new Exception("Section finished before the final voice finished");
        }
        finally
        {
            if (resume != null) EditorApplication.update -= resume;
            EditorApplication.isPaused = false;
            AudioListener.pause = false;
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(sequence);
            UnityEngine.Object.Destroy(staging);
            UnityEngine.Object.Destroy(first);
            UnityEngine.Object.Destroy(second);
            Application.runInBackground = background;
        }
    }

    public static async Task<string> Run()
    {
        if (!Application.isPlaying) throw new Exception("Run in Play mode in an empty temporary scene.");
        await CheckPlayback(false);
        await Until(() => UnityEngine.Object.FindObjectsByType<StoryPlayer>().Length == 0, "narration cleanup");
        await CheckPlayback(true);
        return "PASS: narration and interaction voices remain sequential after Editor pause; final speech completes before advancing";
    }
}
