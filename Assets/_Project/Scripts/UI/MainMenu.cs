using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Vchod.Core;
using Vchod.Systems;

namespace Vchod.UI
{
    /// <summary>Main menu: Play (a new house), Settings, Quit. Command-line seeds and benchmarks skip it.</summary>
    public sealed class MainMenu : MonoBehaviour
    {
        public const string GameScene = "Game";

        [SerializeField] private UnityEngine.UI.Button play;
        [SerializeField] private UnityEngine.UI.Button settings;
        [SerializeField] private UnityEngine.UI.Button quit;
        [SerializeField] private GameObject buttons;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private TMP_Text progress;

        private void Awake()
        {
            GameSettings.ApplyAll();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-seed") >= 0 || Array.IndexOf(args, "-perf") >= 0)
            {
                // Benchmarks and bug repro go straight into the game.
                SceneManager.LoadScene(GameScene);
                return;
            }
            play.onClick.AddListener(Play);
            settings.onClick.AddListener(OpenSettings);
            quit.onClick.AddListener(Quit);
        }

        private void Start()
        {
            if (progress != null)
                progress.text = StoryProgress.Collected.Count > 0
                    ? $"Kousky příběhu {StoryProgress.Collected.Count}/{StoryCatalog.All.Count}"
                    : string.Empty;
            EventSystem.current?.SetSelectedGameObject(play.gameObject);
        }

        private void Play()
        {
            SeedService.Initialize(SeedService.NewRandomSeed());
            SceneManager.LoadScene(GameScene);
        }

        private void OpenSettings()
        {
            buttons.SetActive(false);
            settingsPanel.Open(() =>
            {
                buttons.SetActive(true);
                EventSystem.current?.SetSelectedGameObject(settings.gameObject);
            });
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
