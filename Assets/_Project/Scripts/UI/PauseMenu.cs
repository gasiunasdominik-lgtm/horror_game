using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Vchod.Player;

namespace Vchod.UI
{
    /// <summary>Esc / gamepad Start: pause with Continue, Settings, Main menu. Not while reading or at an ending.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        public const string MenuScene = "Menu";

        [SerializeField] private GameObject root;
        [SerializeField] private UnityEngine.UI.Button resume;
        [SerializeField] private UnityEngine.UI.Button settings;
        [SerializeField] private UnityEngine.UI.Button mainMenu;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private StoryView story;
        [SerializeField] private EndingView ending;
        [SerializeField] private PlayerController player;

        private float _timeScale = 1f;
        private bool _openedThisFrame;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            resume.onClick.AddListener(Resume);
            settings.onClick.AddListener(OpenSettings);
            mainMenu.onClick.AddListener(ToMainMenu);
            root.SetActive(false);
        }

        private void Update()
        {
            _openedThisFrame = false;
            var k = Keyboard.current;
            var g = Gamepad.current;
            var toggle = (k != null && k.escapeKey.wasPressedThisFrame) || (g != null && g.startButton.wasPressedThisFrame);
            if (!toggle) return;
            if (!IsPaused)
            {
                if ((story != null && story.IsOpen) || (ending != null && ending.IsShowing)) return;
                Pause();
            }
            else if (settingsPanel == null || !settingsPanel.IsOpen) Resume();
        }

        public void Pause()
        {
            IsPaused = true;
            _openedThisFrame = true;
            _timeScale = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (player != null) player.SetInputLock(this, true);
            root.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(resume.gameObject);
        }

        public void Resume()
        {
            if (!IsPaused || _openedThisFrame) return;
            IsPaused = false;
            Time.timeScale = _timeScale <= 0f ? 1f : _timeScale;
            AudioListener.pause = false;
            root.SetActive(false);
            if (player != null) player.SetInputLock(this, false);
        }

        private void OpenSettings()
        {
            root.SetActive(false);
            settingsPanel.Open(() =>
            {
                root.SetActive(true);
                EventSystem.current?.SetSelectedGameObject(settings.gameObject);
            });
        }

        private void ToMainMenu()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(MenuScene);
        }
    }
}
