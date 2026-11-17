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
        private const string ScenePath = Root + "/Scenes/Intro.unity";
        private const string Scene1Path = Root + "/Scenes/Scena1.unity";
        private const string Scene2Path = Root + "/Scenes/Scena2.unity";
        private const string Scene3Path = Root + "/Scenes/Scena3.unity";
        private const string MaterialsPath = Root + "/Materials";
        private const string AudioPath = Root + "/Audio";
        private const string SequencePath = Root + "/Script/capitolo_01_sequenza.csv";
        private const string StagingPath = Root + "/Script/capitolo_01_regia.csv";

        [MenuItem("EchoQuest/Rebuild Story Scenes")]
        public static void Build()
        {
            // The chapter plays on: each scene loads the next one when it ends.
            BuildSection("s03", Scene3Path, "EchoQuest · Scena 3");
            BuildSection("s02", Scene2Path, "EchoQuest · Scena 2", "Scena3");
            BuildSection("s01", Scene1Path, "EchoQuest · Scena 1", "Scena2");
            BuildSection("p00", ScenePath, "EchoQuest · Intro", "Scena1");   // last, so it stays open
        }

        public static void BuildSection(string section, string scenePath, string title, string nextScene = "")
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
            player.sequence = AssetDatabase.LoadAssetAtPath<TextAsset>(SequencePath);
            player.staging = AssetDatabase.LoadAssetAtPath<TextAsset>(StagingPath);
            player.clips = FindClips();
            player.section = section;
            player.nextScene = nextScene;
            player.listener = listenerGo.transform;
            player.characterMarker = GetMaterial("StoryMarkerCharacter", new Color(1.00f, 0.62f, 0.20f));
            player.distantMarker = GetMaterial("StoryMarkerDistant", new Color(0.65f, 0.45f, 0.25f));
            player.windMarker = GetMaterial("StoryMarkerWind", new Color(0.45f, 0.80f, 1.00f));
            player.soundMarker = GetMaterial("StoryMarkerSound", new Color(1.00f, 0.88f, 0.30f));
            var hud = storyGo.AddComponent<StoryHud>();
            hud.player = player;
            hud.recorder = recorder;
            hud.title = title;

            if (player.sequence == null) Debug.LogError("[Story] Missing " + SequencePath + ": run tools/story_extract.py");
            if (player.staging == null) Debug.LogError("[Story] Missing " + StagingPath);

            EnsureFolder(Root + "/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            AddToBuildSettings(scenePath);
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

        private static void AddToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
