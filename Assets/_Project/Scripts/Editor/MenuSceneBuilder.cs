using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Vchod.UI;
using static Vchod.EditorTools.SceneBuildUtil;

namespace Vchod.EditorTools
{
    /// <summary>Builds the Menu scene (first in the build) from code. Idempotent.</summary>
    public static class MenuSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Menu.unity";

        [MenuItem("Vchod/Rebuild Menu Scene")]
        public static void BuildAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Exit Play Mode before rebuilding the menu");
            var active = EditorSceneManager.GetActiveScene().path;
            var scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = FindOrCreateRoot("Camera");
            var cam = GetOrAdd<Camera>(camGo);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.025f);
            GetOrAdd<AudioListener>(camGo);

            var esGo = FindOrCreateRoot("EventSystem");
            GetOrAdd<EventSystem>(esGo);
            GetOrAdd<InputSystemUIInputModule>(esGo);

            var canvasGo = FindOrCreateRoot("UI");
            var canvas = GetOrAdd<Canvas>(canvasGo);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = GetOrAdd<UnityEngine.UI.CanvasScaler>(canvasGo);
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            GetOrAdd<UnityEngine.UI.GraphicRaycaster>(canvasGo);

            var title = UiWidgets.Label(canvasGo.transform, "Title", "VCHOD C", 96f, new Vector2(0f, 270f), new Vector2(1200f, 140f));
            title.characterSpacing = 18f;
            var sub = UiWidgets.Label(canvasGo.transform, "Subtitle", "Po noční směně. Domů, do druhého patra.", 26f,
                new Vector2(0f, 180f), new Vector2(1200f, 50f));
            sub.color = UiWidgets.TextDim;

            var buttons = FindOrCreateChild(canvasGo.transform, "Buttons");
            UiWidgets.Stretch(buttons);
            var play = UiWidgets.Button(buttons.transform, "Play", "Hrát", new Vector2(0f, 30f));
            var settings = UiWidgets.Button(buttons.transform, "SettingsButton", "Nastavení", new Vector2(0f, -50f));
            var quit = UiWidgets.Button(buttons.transform, "Quit", "Konec", new Vector2(0f, -130f));
            var progress = UiWidgets.Label(canvasGo.transform, "Progress", "", 22f, new Vector2(0f, -300f), new Vector2(900f, 40f));
            progress.color = UiWidgets.TextDim;
            var hint = UiWidgets.Label(canvasGo.transform, "Controls", "WASD pohyb · myš rozhlížení · Shift běh · E použít · Esc pauza", 20f,
                new Vector2(0f, -440f), new Vector2(1400f, 40f));
            hint.color = UiWidgets.TextDim;

            var panel = UiWidgets.SettingsPanel(canvasGo);
            canvasGo.transform.Find("Settings").SetAsLastSibling();

            var menu = GetOrAdd<MainMenu>(canvasGo);
            SetRef(menu, "play", play);
            SetRef(menu, "settings", settings);
            SetRef(menu, "quit", quit);
            SetRef(menu, "buttons", buttons);
            SetRef(menu, "settingsPanel", panel);
            SetRef(menu, "progress", progress);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            // Menu first, then the game.
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(GameSceneBuilder.ScenePath, true),
            };
            if (!string.IsNullOrEmpty(active) && active != ScenePath) EditorSceneManager.OpenScene(active);
        }
    }
}
