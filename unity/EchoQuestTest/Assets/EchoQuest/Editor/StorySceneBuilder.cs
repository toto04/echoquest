using System.Collections.Generic;
using EchoQuest.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoQuest.Editor
{
    public static class StorySceneBuilder
    {
        private const string Root = "Assets/EchoQuest/Story";
        private const string MaterialsPath = Root + "/Materials";
        private const string AudioPath = Root + "/Audio";
        private const string ScenesPath = Root + "/Scenes";

        /// <summary>One folder per chapter; the file names carry the chapter and the section number, so the
        /// scenes sort in story order and stay unique across chapters (the overlay lists them by name).</summary>
        private sealed class Chapter
        {
            public string Folder, Sequence, Staging;
            public (string section, string file, string title)[] Scenes;
        }

        private static readonly Chapter[] Chapters =
        {
            new Chapter
            {
                Folder = "Capitolo1_Pianoforte",
                Sequence = Root + "/Script/capitolo_01_sequenza.csv",
                Staging = Root + "/Script/capitolo_01_regia.csv",
                Scenes = new[]
                {
                    ("p00", "C1_00_Intro", "Capitolo 1 · dall'Intro"),
                    ("s01", "C1_01_Scena1", "Capitolo 1 · Scena 1"),
                    ("s02", "C1_02_Scena2", "Capitolo 1 · Scena 2"),
                    ("s03", "C1_03_Scena3", "Capitolo 1 · Scena 3"),
                    ("s04", "C1_04_Scena4", "Capitolo 1 · Scena 4"),
                    ("s05", "C1_05_Scena5", "Capitolo 1 · Scena 5"),
                    ("e00", "C1_99_Epilogo", "Capitolo 1 · Epilogo"),
                },
            },
            // Chapter 2, the tamburo (docs/capitoli-2-3.md): its own script and staging.
            new Chapter
            {
                Folder = "Capitolo2_Tamburo",
                Sequence = Root + "/Script/capitolo_02_sequenza.csv",
                Staging = Root + "/Script/capitolo_02_regia.csv",
                Scenes = new[]
                {
                    ("t00", "C2_00_Prologo", "Capitolo 2, il tamburo · dal Prologo"),
                    ("t01", "C2_01_Scena1", "Capitolo 2, il tamburo · Scena 1"),
                    ("t02", "C2_02_Scena2", "Capitolo 2, il tamburo · Scena 2"),
                    ("t03", "C2_03_Scena3", "Capitolo 2, il tamburo · Scena 3"),
                    ("t04", "C2_04_Scena4", "Capitolo 2, il tamburo · Scena 4"),
                    ("t05", "C2_05_Scena5", "Capitolo 2, il tamburo · Scena 5"),
                    ("t99", "C2_99_Epilogo", "Capitolo 2, il tamburo · Epilogo"),
                },
            },
        };

        private static string ScenePath(Chapter chapter, string file) => $"{ScenesPath}/{chapter.Folder}/{file}.unity";

        [MenuItem("EchoQuest/Rebuild Story Scenes")]
        public static void Build()
        {
            // Every scene holds the whole chapter and plays on into the next section without a scene
            // load (StoryPlayer.continueToNextSection); a scene only sets where playback starts.
            // Chapter 1's Intro is built last, so it stays open.
            for (int c = Chapters.Length - 1; c >= 0; c--) BuildChapter(Chapters[c]);
            SyncBuildSettings();
        }

        [MenuItem("EchoQuest/Rebuild Tamburo Scenes (Chapter 2)")]
        public static void BuildTamburo()
        {
            BuildChapter(Chapters[1]);
            SyncBuildSettings();
        }

        private static void BuildChapter(Chapter chapter)
        {
            for (int i = chapter.Scenes.Length - 1; i >= 0; i--)
            {
                var (section, file, title) = chapter.Scenes[i];
                BuildSection(section, ScenePath(chapter, file), "EchoQuest · " + title, chapter.Sequence, chapter.Staging);
            }
        }

        public static void BuildSection(string section, string scenePath, string title, string sequencePath, string stagingPath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Top-down view: the listener in the middle, sounds appear as coloured dots around them.
            var camGo = new GameObject("Camera dall'alto");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 15f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.11f);
            camGo.transform.SetPositionAndRotation(new Vector3(0f, 30f, 5f), Quaternion.Euler(90f, 0f, 0f));

            var lightGo = new GameObject("Luce");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(60f, -30f, 0f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Pavimento della sala";
            floor.transform.position = new Vector3(0f, -0.5f, 5f);
            floor.transform.localScale = new Vector3(5f, 1f, 4f);
            floor.GetComponent<Renderer>().sharedMaterial = GetMaterial("StoryFloor", new Color(0.22f, 0.23f, 0.28f));

            // The listener: a head with a nose, so you can see which way it faces.
            var listenerGo = new GameObject("Ascoltatore");
            listenerGo.AddComponent<AudioListener>();
            var recorder = listenerGo.AddComponent<ListenerRecorder>();
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Testa";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(listenerGo.transform, false);
            head.GetComponent<Renderer>().sharedMaterial = GetMaterial("StoryListener", new Color(0.95f, 0.95f, 0.95f));
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Naso (davanti)";
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            nose.transform.SetParent(listenerGo.transform, false);
            nose.transform.localPosition = new Vector3(0f, 0f, 0.65f);
            nose.transform.localScale = new Vector3(0.25f, 0.25f, 0.5f);
            nose.GetComponent<Renderer>().sharedMaterial = head.GetComponent<Renderer>().sharedMaterial;

            // The music room: one reverb for every spatial sound (the narrator stays dry).
            var reverbGo = new GameObject("Riverbero della sala");
            var reverb = reverbGo.AddComponent<AudioReverbZone>();
            reverb.reverbPreset = AudioReverbPreset.Concerthall;
            reverb.minDistance = 40f;
            reverb.maxDistance = 80f;

            var storyGo = new GameObject("Storia");
            var player = storyGo.AddComponent<StoryPlayer>();
            player.sequence = AssetDatabase.LoadAssetAtPath<TextAsset>(sequencePath);
            player.staging = AssetDatabase.LoadAssetAtPath<TextAsset>(stagingPath);
            player.clips = FindClips();
            player.section = section;
            player.listener = listenerGo.transform;
            player.characterMarker = GetMaterial("StoryMarkerCharacter", new Color(1.00f, 0.62f, 0.20f));
            player.distantMarker = GetMaterial("StoryMarkerDistant", new Color(0.65f, 0.45f, 0.25f));
            player.windMarker = GetMaterial("StoryMarkerWind", new Color(0.45f, 0.80f, 1.00f));
            player.soundMarker = GetMaterial("StoryMarkerSound", new Color(1.00f, 0.88f, 0.30f));
            var hud = storyGo.AddComponent<StoryHud>();
            hud.player = player;
            hud.recorder = recorder;
            hud.title = title;

            if (player.sequence == null) Debug.LogError("[Story] Missing " + sequencePath + ": run tools/story_extract.py");
            if (player.staging == null) Debug.LogError("[Story] Missing " + stagingPath);

            EnsureFolder(System.IO.Path.GetDirectoryName(scenePath)!.Replace('\\', '/'));
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Story] Built {scenePath} ({section}) with {player.clips.Length} clips");
        }

        private static AudioClip[] FindClips()
        {
            var clips = new List<AudioClip>();
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AudioPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/_")) continue;   // e.g. Voice/_da_dividere: recordings not split yet
                clips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(path));
            }
            return clips.ToArray();
        }

        private static Material GetMaterial(string name, Color color)
        {
            EnsureFolder(MaterialsPath);
            var path = $"{MaterialsPath}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        /// <summary>Build Settings: the other scenes first (the controller test boots the build), then every
        /// story scene in story order, chapter by chapter; entries whose file no longer exists are dropped.</summary>
        private static void SyncBuildSettings()
        {
            var story = new List<string>();
            foreach (var chapter in Chapters)
                foreach (var (_, file, _) in chapter.Scenes) story.Add(ScenePath(chapter, file));
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var s in EditorBuildSettings.scenes)
                if (!s.path.StartsWith(ScenesPath + "/") && System.IO.File.Exists(s.path)) scenes.Add(s);
            foreach (var path in story) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
