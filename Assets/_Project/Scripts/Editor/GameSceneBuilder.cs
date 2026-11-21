using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Vchod.Core;
using Vchod.Player;
using Vchod.Systems;
using Vchod.World;
using Vchod.Audio;
using Vchod.UI;
using static Vchod.EditorTools.SceneBuildUtil;

namespace Vchod.EditorTools
{
    /// <summary>
    /// Builds the Game scene wiring from code so it can be re-run after changes
    /// (menu: Vchod/Rebuild Game Scene Wiring). Idempotent: finds before it creates.
    /// </summary>
    public static class GameSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Game.unity";
        public const string DataFolder = "Assets/_Project/Data";

        [MenuItem("Vchod/Rebuild Game Scene Wiring")]
        public static void BuildAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Exit Play Mode before rebuilding the scene wiring");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);

            EnsureFolder(DataFolder);
            var player = SetupPlayer();
            SetupUI(player);
            var systems = FindOrCreateRoot("Systems");
            GetOrAdd<GameBootstrap>(systems);
            var lightTimer = SetupLightTimer(systems);
            var house = SetupHouse(player, lightTimer);
            SetupCo(systems, player, house);
            SetupEnvironment();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static PlayerController SetupPlayer()
        {
            var config = LoadOrCreateAsset<PlayerConfig>($"{DataFolder}/PlayerConfig.asset");

            var root = FindOrCreateRoot("Player");
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            var body = GetOrAdd<CharacterController>(root);
            body.height = 1.75f;
            body.radius = 0.3f;
            body.center = new Vector3(0f, 0.875f, 0f);
            body.stepOffset = 0.3f;
            body.skinWidth = 0.03f;

            var pivot = FindOrCreateChild(root.transform, "CameraPivot");
            pivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            pivot.transform.localRotation = Quaternion.identity;

            var cam = GameObject.Find("Main Camera") ?? Camera.main?.gameObject;
            if (cam != null)
            {
                cam.transform.SetParent(pivot.transform, false);
                cam.transform.localPosition = Vector3.zero;
                cam.transform.localRotation = Quaternion.identity;
                var camera = cam.GetComponent<Camera>();
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 60f;
                camera.fieldOfView = 70f;
            }

            var controller = GetOrAdd<PlayerController>(root);
            SetRef(controller, "config", config);
            SetRef(controller, "cameraPivot", pivot.transform);

            var interactor = GetOrAdd<PlayerInteractor>(root);
            SetRef(interactor, "controller", controller);
            return controller;
        }

        private static LightTimer SetupLightTimer(GameObject systems)
        {
            var config = LoadOrCreateAsset<LightTimerConfig>($"{DataFolder}/LightTimerConfig.asset");
            var timer = GetOrAdd<LightTimer>(systems);
            SetRef(timer, "config", config);
            return timer;
        }

        private static void SetupCo(GameObject systems, PlayerController player, ModuleLayoutBuilder house)
        {
            var config = LoadOrCreateAsset<CoConfig>($"{DataFolder}/CoConfig.asset");
            var co = GetOrAdd<CoSystem>(systems);
            SetRef(co, "config", config);
            SetRef(co, "player", player);
            SetRef(co, "house", house);

            var cam = player.CameraPivot.GetComponentInChildren<Camera>().gameObject;
            var lowPass = GetOrAdd<AudioLowPassFilter>(cam);
            lowPass.cutoffFrequency = SymptomState.OpenCutoff;
            var hum = GetOrAdd<AudioSource>(cam);
            hum.playOnAwake = false;
            hum.spatialBlend = 0f;
            hum.loop = true;

            var symptoms = GetOrAdd<SymptomEffects>(systems);
            SetRef(symptoms, "co", co);
            SetRef(symptoms, "player", player);
            SetRef(symptoms, "lowPass", lowPass);
            SetRef(symptoms, "hum", hum);
            SetRef(symptoms, "view", cam.GetComponent<Camera>());

            var detectorGo = FindOrCreateRoot("CoDetector");
            GetOrAdd<AudioSource>(detectorGo);
            var detector = GetOrAdd<CoDetectorBeeper>(detectorGo);
            SetRef(detector, "co", co);
            SetRef(detector, "house", house);
        }

        private static ModuleLayoutBuilder SetupHouse(PlayerController player, LightTimer lightTimer)
        {
            var houseConfig = GreyboxAssetBuilder.BuildAll();
            var house = FindOrCreateRoot("House");
            house.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var builder = GetOrAdd<ModuleLayoutBuilder>(house);
            SetRef(builder, "config", houseConfig);
            SetRef(builder, "lightTimer", lightTimer);
            SetRef(builder, "player", player);
            return builder;
        }

        /// <summary>Indoor night: no sun, dark ambient, dark fog that limits view (also a perf budget).</summary>
        private static void SetupEnvironment()
        {
            var sun = GameObject.Find("/Directional Light");
            if (sun != null) sun.SetActive(false);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.21f, 0.26f);
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.015f, 0.016f, 0.02f);
            RenderSettings.fogDensity = 0.07f;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
            }
        }

        private static void SetupUI(PlayerController player)
        {
            var canvas = GameObject.Find("/UI");
            if (canvas == null)
            {
                Debug.LogError("UI canvas missing from Game scene");
                return;
            }

            var crosshairGo = FindOrCreateChild(canvas.transform, "Crosshair");
            var crosshairRect = GetOrAdd<RectTransform>(crosshairGo);
            crosshairRect.anchorMin = crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.sizeDelta = new Vector2(6f, 6f);
            crosshairRect.anchoredPosition = Vector2.zero;
            var image = GetOrAdd<Image>(crosshairGo);
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            image.color = new Color(1f, 1f, 1f, 0.8f);
            image.raycastTarget = false;
            var group = GetOrAdd<CanvasGroup>(crosshairGo);
            group.interactable = false;
            group.blocksRaycasts = false;

            var promptGo = FindOrCreateChild(canvas.transform, "InteractionPrompt");
            var promptRect = GetOrAdd<RectTransform>(promptGo);
            promptRect.anchorMin = promptRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptRect.sizeDelta = new Vector2(600f, 60f);
            promptRect.anchoredPosition = new Vector2(0f, -70f);
            var text = GetOrAdd<TextMeshProUGUI>(promptGo);
            text.text = string.Empty;
            text.fontSize = 30f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.92f, 0.9f, 0.85f, 0.95f);
            text.raycastTarget = false;
            text.enabled = false;

            var view = GetOrAdd<InteractionPromptView>(canvas);
            SetRef(view, "interactor", player.GetComponent<PlayerInteractor>());
            SetRef(view, "promptText", text);
            SetRef(view, "crosshair", group);
        }
    }
}
