using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vchod.Systems;

namespace Vchod.UI
{
    /// <summary>Volume, look sensitivity, invert Y, fullscreen. Changes apply and save at once.</summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private UnityEngine.UI.Slider volume;
        [SerializeField] private UnityEngine.UI.Slider sensitivity;
        [SerializeField] private UnityEngine.UI.Toggle invertY;
        [SerializeField] private UnityEngine.UI.Toggle fullscreen;
        [SerializeField] private UnityEngine.UI.Button back;
        [SerializeField] private TMP_Text volumeValue;
        [SerializeField] private TMP_Text sensitivityValue;

        private System.Action _onBack;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            volume.minValue = 0f;
            volume.maxValue = 1f;
            sensitivity.minValue = GameSettings.MinSensitivity;
            sensitivity.maxValue = GameSettings.MaxSensitivity;
            volume.onValueChanged.AddListener(v => { GameSettings.MasterVolume = v; Refresh(); });
            sensitivity.onValueChanged.AddListener(v => { GameSettings.LookSensitivity = v; Refresh(); });
            invertY.onValueChanged.AddListener(v => GameSettings.InvertY = v);
            fullscreen.onValueChanged.AddListener(v => GameSettings.Fullscreen = v);
            back.onClick.AddListener(Close);
            root.SetActive(false);
        }

        public void Open(System.Action onBack)
        {
            _onBack = onBack;
            volume.SetValueWithoutNotify(GameSettings.MasterVolume);
            sensitivity.SetValueWithoutNotify(GameSettings.LookSensitivity);
            invertY.SetIsOnWithoutNotify(GameSettings.InvertY);
            fullscreen.SetIsOnWithoutNotify(GameSettings.Fullscreen);
            Refresh();
            root.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(volume.gameObject);
        }

        public void Close()
        {
            if (!IsOpen) return;
            GameSettings.Save();
            root.SetActive(false);
            _onBack?.Invoke();
        }

        private void Refresh()
        {
            if (volumeValue != null) volumeValue.text = $"{Mathf.RoundToInt(GameSettings.MasterVolume * 100f)} %";
            if (sensitivityValue != null) sensitivityValue.text = $"{GameSettings.LookSensitivity:0.00}×";
        }

        private void Update()
        {
            if (!IsOpen) return;
            var k = Keyboard.current;
            var g = Gamepad.current;
            if ((k != null && k.escapeKey.wasPressedThisFrame) || (g != null && g.buttonEast.wasPressedThisFrame)) Close();
        }
    }
}
