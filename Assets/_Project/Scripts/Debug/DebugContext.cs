using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vchod.Core;
using Vchod.Player;
using Vchod.Systems;
using Vchod.World;

namespace Vchod.DebugTools
{
    /// <summary>
    /// Shared state for the debug tools: cached scene references (refreshed per scene
    /// load, never searched per frame), FPS, god mode and gameplay input blocking.
    /// </summary>
    public sealed class DebugContext : MonoBehaviour
    {
        public static DebugContext Instance { get; private set; }

        private readonly HashSet<object> _inputBlockers = new();
        private float _fpsAccumTime;
        private int _fpsAccumFrames;

        public float Fps { get; private set; }
        public float WorstFrameMs { get; private set; }
        public bool GodMode { get; private set; }
        public bool InputBlocked => _inputBlockers.Count > 0;
        public float RoundTime => Time.timeSinceLevelLoad;

        public event Action<string> Notified;

        private void Awake() => Instance = this;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Refresh();
        }

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Refresh();
            ApplyGodMode();
            ApplyInputBlock();
        }

        public PlayerController Player { get; private set; }
        public ModuleLayoutBuilder House { get; private set; }
        public LightTimer LightTimer { get; private set; }

        private void Refresh()
        {
            Player = FindFirstObjectByType<PlayerController>();
            House = FindFirstObjectByType<ModuleLayoutBuilder>();
            LightTimer = FindFirstObjectByType<LightTimer>();
        }

        private float _worstThisWindow;

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            _fpsAccumTime += dt;
            _fpsAccumFrames++;
            _worstThisWindow = Mathf.Max(_worstThisWindow, dt * 1000f);
            if (_fpsAccumTime >= 0.5f)
            {
                Fps = _fpsAccumFrames / _fpsAccumTime;
                WorstFrameMs = _worstThisWindow;
                _fpsAccumTime = 0f;
                _fpsAccumFrames = 0;
                _worstThisWindow = 0f;
            }
        }

        public void Notify(string message)
        {
            UnityEngine.Debug.Log($"[Debug] {message}");
            Notified?.Invoke(message);
        }

        public void SetGodMode(bool enabled)
        {
            GodMode = enabled;
            ApplyGodMode();
        }

        private void ApplyGodMode()
        {
        }

        public void SetInputBlocked(object owner, bool blocked)
        {
            if (blocked) _inputBlockers.Add(owner);
            else _inputBlockers.Remove(owner);
            ApplyInputBlock();
        }

        private void ApplyInputBlock()
        {
            if (Player != null)
            {
                Player.SetInputLock(this, InputBlocked);
                return;
            }
            Cursor.lockState = InputBlocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = InputBlocked;
        }

        public Transform PlayerTransform
        {
            get
            {
                if (Player != null) return Player.transform;
                var cam = Camera.main;
                return cam != null ? cam.transform : null;
            }
        }

        public GameSnapshot Capture()
        {
            var snapshot = new GameSnapshot
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                buildVersion = Application.version,
                unityVersion = Application.unityVersion,
                gitHash = BuildInfo.GitHash,
                isEditor = Application.isEditor,
                isDevelopmentBuild = UnityEngine.Debug.isDebugBuild,
                scene = SceneManager.GetActiveScene().name,
                seed = SeedService.Seed,
                roundTime = RoundTime,
                fps = Fps,
                timeScale = Time.timeScale,
                godMode = GodMode,
            };

            if (House != null && House.Plan != null)
            {
                snapshot.layoutHash = House.LayoutHash.ToString("X8");
                snapshot.reshuffleIndex = House.ReshuffleIndex;
            }
            if (LightTimer != null) snapshot.lightOn = LightTimer.IsOn;

            var player = PlayerTransform;
            if (player != null)
            {
                snapshot.playerPosition = player.position;
                snapshot.playerYaw = player.eulerAngles.y;
            }
            return snapshot;
        }
    }
}
