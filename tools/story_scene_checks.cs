// Run in Play mode in an empty temporary scene. Uses the real script and staging,
// with silent, shortened narration so both story paths can be checked without a full listen.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EchoQuest.Story;
using AssetDatabase = UnityEditor.AssetDatabase;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class StorySceneChecks
{
    public static string Progress = "Not started";
    public static string LastResult;
    static async Task Until(Func<bool> condition, string name, double limit = 12)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (watch.Elapsed.TotalSeconds > limit) throw new Exception("Timeout: " + name);
            await Task.Delay(10);
        }
    }
    static async Task Frames()
    {
        int first = Time.frameCount;
        await Until(() => Time.frameCount > first + 1, "two frames");
    }
    static string FastStaging(string text)
    {
        var rows = Csv.Parse(text); var columns = Csv.Columns(rows[0]);
        var output = new StringBuilder();
        foreach (var row in rows)
        {
            if (row[0].StartsWith("#")) continue;
            if (row != rows[0])
                foreach (var key in new[] { "attesa", "pausa_dopo" })
                    if (columns.TryGetValue(key, out int index) && index < row.Length && row[index].Length > 0)
                        row[index] = key == "attesa" ? "0.015" : "0";
            for (int i = 0; i < row.Length; i++)
            {
                if (i > 0) output.Append(',');
                output.Append('"').Append(row[i].Replace("\"", "\"\"")).Append('"');
            }
            output.Append('\n');
        }
        return output.ToString();
    }
    static async Task<int> Route(bool replay)
    {
        const string rootPath = "Assets/EchoQuest/Story";
        var root = new GameObject("Actual story check"); root.SetActive(false);
        var player = root.AddComponent<StoryPlayer>();
        var head = new GameObject("Test listener"); head.transform.SetParent(root.transform); head.AddComponent<AudioListener>();
        player.listener = head.transform; player.section = "s01";
        player.sequence = new TextAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(rootPath + "/Script/capitolo_01_sequenza.csv").text);
        player.staging = new TextAsset(FastStaging(AssetDatabase.LoadAssetAtPath<TextAsset>(rootPath + "/Script/capitolo_01_regia.csv").text));
        player.startDelay = player.pauseAfterLine = player.pauseAfterSound = player.pauseAroundInlineSound = 0;
        var config = StoryData.ParseStaging(player.staging.text);
        var examples = new HashSet<string>();
        foreach (var entry in config.Values)
            if (entry.Interaction != null) foreach (var example in entry.Interaction.Examples) examples.Add(example);
        var clips = new List<AudioClip>();
        foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { rootPath + "/Audio" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/_")) continue;
            var original = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            clips.Add(AudioClip.Create(original.name, examples.Contains(original.name) ? original.samples : 240, 1, original.frequency, false));
        }
        player.clips = clips.ToArray();
        var keyboard = InputSystem.AddDevice<Keyboard>();
        void State(double time, params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys), time);
        void Press(Key key)
        {
            double now = InputState.currentTime;
            State(now, key); State(now + .000001); InputSystem.Update();
        }
        var visited = new HashSet<string>();
        try
        {
            root.SetActive(true);
            await Until(() => player.Current != null, "story start");
            while (player.Current != null)
            {
                await Until(() => player.Current == null || player.WaitingInteractionInput, player.Current?.Id ?? "next interaction");
                if (player.Current == null) break;
                string id = player.Current.Id;
                Progress = (replay ? "Replay: " : "Direct: ") + id;
                if (!visited.Add(id)) throw new Exception("Unexpected interaction repeat: " + id);
                var interaction = player.StagingFor(player.Current).Interaction;
                if (interaction == null) throw new Exception("Unexpected legacy interaction: " + id);
                if (interaction.Mode == InteractionMode.Rhythm)
                {
                    double capture = AudioSettings.dspTime;
                    double duration = interaction.Times[2];
                    await Until(() => AudioSettings.dspTime >= capture + duration + .08, "rhythm input timestamps");
                    double origin = InputState.currentTime - duration;
                    foreach (float t in interaction.Times)
                    {
                        State(origin + t, Key.Digit1); State(origin + t + .000001);
                    }
                    InputSystem.Update();
                }
                else if (interaction.Mode == InteractionMode.Note)
                {
                    double capture = AudioSettings.dspTime;
                    await Until(() => AudioSettings.dspTime >= capture + .15 + interaction.Times[2], "missing note");
                    Press(Key.Digit1);
                }
                else if (interaction.Mode == InteractionMode.Compare) Press(Key.N);
                else if (interaction.Mode == InteractionMode.Decision) Press(replay ? Key.N : Key.Y);
                else
                {
                    if (!string.IsNullOrEmpty(interaction.Decline))
                    {
                        Press(Key.N); await Frames();
                        await Until(() => player.WaitingInteractionInput, "wait after refusal");
                    }
                    Press(interaction.ConfirmButton == 1 ? Key.Digit1 : Key.Y);
                }
                await Until(() => player.Current == null || player.Current.Id != id, "response to " + id);
            }
            int expected = replay ? 8 : 7;
            if (visited.Count != expected || player.BranchChoice != "B" || !player.Status.StartsWith("Fine"))
                throw new Exception("Story route incomplete: " + string.Join(",", visited));
            return visited.Count;
        }
        catch (Exception e)
        {
            LastResult = e.Message + " | " + player.Current?.Id + " | " + player.Status + " | " + player.CurrentClip;
            throw new Exception(LastResult, e);
        }
        finally
        {
            var ownedSequence = player.sequence; var ownedStaging = player.staging;
            UnityEngine.Object.Destroy(root); await Until(() => root == null, "cleanup");
            InputSystem.RemoveDevice(keyboard);
            UnityEngine.Object.Destroy(ownedSequence); UnityEngine.Object.Destroy(ownedStaging);
            foreach (var clip in clips) UnityEngine.Object.Destroy(clip);
        }
    }
    public static async Task<string> Run()
    {
        if (!Application.isPlaying) throw new Exception("Run in Play mode in an empty temporary scene.");
        int direct = await Route(false);
        int replay = await Route(true);
        LastResult = "Direct path: " + direct + " interactions; replay path: " + replay + " interactions.";
        Progress = "Complete"; return LastResult;
    }
}
