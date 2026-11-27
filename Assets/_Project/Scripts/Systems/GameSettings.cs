using UnityEngine;

namespace Vchod.Systems
{
    /// <summary>Player settings, saved in PlayerPrefs and applied at once. Read by the player and audio code.</summary>
    public static class GameSettings
    {
        private const string Prefix = "vchod.settings.";

        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 2.5f;

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(Prefix + "volume", 0.8f);
            set { PlayerPrefs.SetFloat(Prefix + "volume", Mathf.Clamp01(value)); AudioListener.volume = MasterVolume; }
        }

        public static float LookSensitivity
        {
            get => PlayerPrefs.GetFloat(Prefix + "sensitivity", 1f);
            set => PlayerPrefs.SetFloat(Prefix + "sensitivity", Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        public static bool InvertY
        {
            get => PlayerPrefs.GetInt(Prefix + "invertY", 0) == 1;
            set => PlayerPrefs.SetInt(Prefix + "invertY", value ? 1 : 0);
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(Prefix + "fullscreen", 1) == 1;
            set
            {
                PlayerPrefs.SetInt(Prefix + "fullscreen", value ? 1 : 0);
                Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            }
        }

        public static void Save() => PlayerPrefs.Save();

        /// <summary>Applies saved settings at startup (volume; screen mode only in a player build).</summary>
        public static void ApplyAll()
        {
            AudioListener.volume = MasterVolume;
            if (!Application.isEditor)
                Screen.fullScreenMode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }

        /// <summary>Mouse/stick look after the player's sensitivity and invert-Y.</summary>
        public static Vector2 ApplyLook(Vector2 look, float sensitivity, bool invertY) =>
            new(look.x * sensitivity, look.y * sensitivity * (invertY ? -1f : 1f));
    }
}
