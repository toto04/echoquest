using EchoQuest;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoQuest.Editor
{
    public static class TestSceneBuilder
    {
        public const string ScenePath = "Assets/EchoQuest/Scenes/ControllerTest.unity";
        private const string MaterialsDir = "Assets/EchoQuest/Materials";

        [MenuItem("EchoQuest/Rebuild Controller Test Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[EchoQuest] exit Play mode before rebuilding the scene");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var body = GetMaterial("CritterBody", new Color(1.00f, 0.55f, 0.15f), 0.55f);
            var white = GetMaterial("EyeWhite", Color.white, 0.8f);
            var black = GetMaterial("EyePupil", new Color(0.05f, 0.05f, 0.08f), 0.9f);
            var floor = GetMaterial("Floor", new Color(0.16f, 0.18f, 0.24f), 0.2f);
            var marker = GetMaterial("Marker", new Color(0.35f, 0.40f, 0.55f), 0.3f);
            var north = GetMaterial("NorthMarker", new Color(1.00f, 0.85f, 0.25f), 0.3f);

            // Lighting and environment.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.60f, 0.75f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.35f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.14f, 0.16f);

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Camera behind and above the critter, looking forward (+Z), so the critter
            // faces the same way as the controller in your hands.
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.12f);
            cam.fieldOfView = 50f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 2.6f, -5.2f);
            camGo.transform.LookAt(new Vector3(0f, 0.8f, 0.6f));

            // Floor with reference markers: a yellow one straight ahead marks "yaw 0".
            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorGo.name = "Floor";
            floorGo.transform.localScale = new Vector3(3f, 1f, 3f);
            floorGo.GetComponent<Renderer>().sharedMaterial = floor;
            Object.DestroyImmediate(floorGo.GetComponent<Collider>());

            var markers = new GameObject("Markers").transform;
            for (var i = 0; i < 8; i++)
            {
                var angle = i * 45f;
                var m = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m.name = i == 0 ? "Marker Forward" : $"Marker {angle:0}";
                Object.DestroyImmediate(m.GetComponent<Collider>());
                m.transform.SetParent(markers, false);
                m.transform.localPosition = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0.25f, 4f);
                m.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                m.transform.localScale = i == 0 ? new Vector3(0.5f, 0.5f, 0.5f) : new Vector3(0.3f, 0.3f, 0.3f);
                m.GetComponent<Renderer>().sharedMaterial = i == 0 ? north : marker;
            }

            // The critter: Root (hops) > Pivot (rotates) > body parts.
            var root = new GameObject("Critter");
            root.transform.position = new Vector3(0f, 0.75f, 0f);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root.transform, false);

            var bodyR = Part("Body", PrimitiveType.Sphere, pivot, Vector3.zero, new Vector3(1.5f, 0.85f, 1.25f), body);
            var tail = Part("Tail", PrimitiveType.Sphere, pivot, new Vector3(0f, 0.15f, -0.7f), new Vector3(0.3f, 0.3f, 0.3f), body);
            var footL = Part("Foot L", PrimitiveType.Sphere, pivot, new Vector3(-0.42f, -0.42f, 0.15f), new Vector3(0.4f, 0.16f, 0.5f), body);
            var footR = Part("Foot R", PrimitiveType.Sphere, pivot, new Vector3(0.42f, -0.42f, 0.15f), new Vector3(0.4f, 0.16f, 0.5f), body);
            var stalk = Part("Antenna", PrimitiveType.Cylinder, pivot, new Vector3(0f, 0.55f, 0.1f), new Vector3(0.05f, 0.18f, 0.05f), black);
            var bobble = Part("Antenna Tip", PrimitiveType.Sphere, pivot, new Vector3(0f, 0.75f, 0.1f), new Vector3(0.16f, 0.16f, 0.16f), body);
            stalk.gameObject.name = "Antenna";
            // Eyes on the front (+Z) so "forward" is obvious.
            for (var side = -1; side <= 1; side += 2)
            {
                var eye = Part(side < 0 ? "Eye L" : "Eye R", PrimitiveType.Sphere, pivot,
                    new Vector3(0.28f * side, 0.18f, 0.5f), new Vector3(0.32f, 0.32f, 0.32f), white);
                Part("Pupil", PrimitiveType.Sphere, eye.transform,
                    new Vector3(0f, 0.05f, 0.38f), new Vector3(0.5f, 0.5f, 0.4f), black);
            }

            var critter = root.AddComponent<CritterController>();
            critter.pivot = pivot;
            critter.bodyRenderers = new[] { bodyR, tail, footL, footR, bobble };

            var hudGo = new GameObject("Debug HUD");
            var hud = hudGo.AddComponent<ControllerDebugHud>();
            hud.critter = critter;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Debug.Log($"[EchoQuest] built {ScenePath}");
        }

        private static Renderer Part(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            return r;
        }

        private static Material GetMaterial(string name, Color color, float smoothness)
        {
            var path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssetIfDirty(mat);
            return mat;
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
