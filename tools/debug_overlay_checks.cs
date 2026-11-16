// Checks for the debug overlay and the story player's control surface.
// Run in Play mode in an empty temporary scene; restore the original scene afterwards.
// unity command 'script run' --file ../../tools/debug_overlay_checks.cs --entry DebugOverlayChecks.Run --timeout 60
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoQuest.DebugUi;
using EchoQuest.Story;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class DebugOverlayChecks
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
    static Task Frames(int count = 2)
    {
        int frame = Time.frameCount;
        return Until(() => Time.frameCount > frame + count, "frames");
    }

    static async Task Click(Mouse mouse, Vector2 guiPoint)
    {
        float s = Mathf.Clamp(Screen.height / 820f, 0.7f, 2.5f);
        var position = new Vector2(guiPoint.x * s, Screen.height - guiPoint.y * s);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        await Frames();
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position }.WithButton(MouseButton.Left));
        await Frames();
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        await Frames(3);
    }

    public static async Task<int> Run()
    {
        if (!Application.isPlaying) throw new Exception("Run in Play mode in an empty temporary scene.");
        checks = 0;
        var assets = new List<UnityEngine.Object>();
        var root = new GameObject("Overlay test"); root.SetActive(false);
        // The real mouse and trackpad would overwrite the simulated pointer.
        var realPointers = new List<InputDevice>();
        foreach (var device in InputSystem.devices)
            if (device is Pointer && device.enabled) realPointers.Add(device);
        foreach (var device in realPointers) InputSystem.DisableDevice(device);
        var mouse = InputSystem.AddDevice<Mouse>();
        var overlay = DebugOverlay.Instance;
        bool wasVisible = overlay != null && overlay.visible;
        try
        {
            var player = root.AddComponent<StoryPlayer>();
            var listener = new GameObject("Test listener"); listener.transform.SetParent(root.transform);
            listener.AddComponent<AudioListener>(); player.listener = listener.transform;
            player.startDelay = player.pauseAfterLine = player.pauseAfterSound = player.pauseOnSpeakerChange = 0;
            player.section = "s01";
            player.sequence = new TextAsset(
                "ordine,sezione,ramo,tipo,id,personaggio,testo,dentro_battuta\n" +
                "1,s01,,voce,s01_maes_01,MAESTRO,Uno,\n" +
                "2,s01,,voce,s01_maes_02,MAESTRO,Due,\n" +
                "3,s01,,interazione,s01_int_01,,,\n" +
                "4,s01,A,voce,s01_maes_03,MAESTRO,Ramo A,\n" +
                "5,s01,B,voce,s01_maes_04,MAESTRO,Ramo B,\n" +
                "6,s01,,convergenza,s01_conv_01,,,\n" +
                "7,s02,,voce,s02_maes_01,MAESTRO,Senza audio,\n");
            player.staging = new TextAsset(
                "id,modalita,consegna,successo,riprova,esempi,tempi\n" +
                "s01_int_01,ritmo,eq_test_prompt,eq_test_success,eq_test_retry,eq_test_example,0;0.3;0.6\n");
            assets.Add(player.sequence); assets.Add(player.staging);
            var clips = new List<AudioClip>();
            foreach (string name in new[] { "s01_maes_01_a", "s01_maes_02_a", "s01_maes_03_a", "s01_maes_04_a" })
                clips.Add(AudioClip.Create(name, 16000 * 4, 1, 16000, false));
            foreach (string name in new[] { "eq_test_prompt", "eq_test_success", "eq_test_retry", "eq_test_example" })
                clips.Add(AudioClip.Create(name, 800, 1, 16000, false));
            assets.AddRange(clips);
            player.clips = clips.ToArray();
            root.SetActive(true);

            // Control surface.
            await Until(() => player.Current != null && player.Current.Id == "s01_maes_01", "story starts");
            Check(player.Sections().Count == 2 && player.Sections()[1] == "s02", "sections are listed in order");
            Check(player.SectionItems.Count == 6, "section items");
            player.JumpTo("s01_maes_02"); await Frames();
            Check(player.Current.Id == "s01_maes_02", "jump to a line");
            player.Previous(); await Frames();
            Check(player.Current.Id == "s01_maes_01", "previous goes one playable item back");
            player.Skip();
            await Until(() => player.Current.Id == "s01_maes_02", "next line after skip");
            player.JumpTo("s01_maes_04"); await Frames();
            Check(player.Current.Id == "s01_maes_04" && player.BranchChoice == "B", "jumping into a branch selects it");
            player.Previous();
            await Until(() => player.WaitingInteractionInput, "previous from branch B reaches the interaction");
            Check(player.Current.Id == "s01_int_01" && player.CurrentInteraction != null, "current interaction is exposed");

            // On-screen buttons go through the same queue as the keyboard.
            player.PressLogical(2); await Frames();
            Check(player.WaitingInteractionInput, "the wrong on-screen button does not count as a tap");
            player.ForcePass();
            await Until(() => player.Current != null && player.Current.Id == "s01_maes_04", "forced pass continues on the chosen branch");
            Check(player.LastAttempt.Contains("overlay"), "a forced pass is labelled");

            player.JumpTo("s01_int_01");
            await Until(() => player.WaitingInteractionInput, "interaction ready again");
            foreach (int wait in new[] { 0, 300, 300 })
            {
                await Task.Delay(wait);
                player.PressLogical(1);
            }
            await Until(() => player.LastAttempt.Length > 0, "three on-screen taps end the attempt");
            Check(player.LastAttempt.Contains("taps at"), "the attempt is described: " + player.LastAttempt);

            // A section with no recordings plays as text.
            Check(player.HasAudio(player.SectionItems[0]), "recorded line has audio");
            player.SetSection("s02");
            await Until(() => player.Current != null && player.Current.Id == "s02_maes_01", "section switch");
            Check(!player.HasAudio(player.Current), "unrecorded line is reported");
            Check(player.Log.Exists(l => l.Contains("missing voice s02_maes_01")), "missing audio is logged");
            Check(player.Log.Exists(l => l.Contains("jump to s01_maes_02")), "jumps are logged");

            // Debug toggles.
            player.ducking = false; player.distanceLowpass = false; await Frames();
            player.listener.rotation = Quaternion.Euler(0, 30, 0);
            player.RecenterHead();
            Check(player.listener.rotation == Quaternion.identity, "recentre faces forward");

            // The overlay exists, finds the player, and takes clicks from the Input System.
            Check(overlay != null, "the overlay boots in every scene");
            overlay.visible = true;
            await Task.Delay(700); await Frames();
            float s = Mathf.Clamp(Screen.height / 820f, 0.7f, 2.5f);
            float dockRight = 8f + Mathf.Min(520f, Screen.width / s - 16f);
            await Click(mouse, new Vector2(dockRight - 29f, 25f));
            Check(!overlay.visible, "clicking Hide hides the overlay");
            await Click(mouse, new Vector2(40f, 20f));
            Check(overlay.visible, "clicking the corner button shows it again");
            return checks;
        }
        finally
        {
            if (overlay != null) overlay.visible = wasVisible;
            UnityEngine.Object.Destroy(root);
            InputSystem.RemoveDevice(mouse);
            foreach (var device in realPointers) InputSystem.EnableDevice(device);
            await Task.Delay(50);
            foreach (var asset in assets) UnityEngine.Object.Destroy(asset);
        }
    }
}
