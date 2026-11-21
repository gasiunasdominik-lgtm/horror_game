using System.Collections.Generic;
using UnityEngine;
using Vchod.Core;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// A corridor window the player can open for fresh air (GDD: CO drops fast at an open window).
    /// Opening it is loud: the firefighter may come, which is also how ending 3 happens.
    /// </summary>
    public sealed class VentWindow : MonoBehaviour, IInteractable
    {
        private static readonly List<VentWindow> Active = new();

        [SerializeField] private Transform sash;
        [SerializeField] private AudioSource wind;
        [SerializeField] private float openAngle = -80f;
        [SerializeField] private float swingSpeed = 160f;
        [Tooltip("Within this distance in front of an open window you breathe fresh air (m).")]
        [SerializeField] private float freshAirRadius = 1.6f;
        [SerializeField] private float noiseRadius = 8f;
        [SerializeField] private string openPrompt = "E  Otevřít okno";
        [SerializeField] private string closePrompt = "E  Zavřít okno";

        public static IReadOnlyList<VentWindow> All => Active;
        public bool IsOpen { get; private set; }
        public string Prompt => IsOpen ? closePrompt : openPrompt;

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        private void Start()
        {
            if (wind != null) wind.clip = Audio.ProceduralAudio.CreateWind();
        }

        public void Interact(GameObject interactor)
        {
            IsOpen = !IsOpen;
            if (wind != null)
            {
                if (IsOpen) wind.Play();
                else wind.Stop();
            }
            EventBus.Publish(new NoiseMade { Position = transform.position, Radius = noiseRadius });
        }

        private void Update()
        {
            if (sash == null) return;
            var target = Quaternion.Euler(0f, IsOpen ? openAngle : 0f, 0f);
            sash.localRotation = Quaternion.RotateTowards(sash.localRotation, target, swingSpeed * Time.deltaTime);
        }

        /// <summary>True when the player stands in front of any open window.</summary>
        public static bool FreshAirAt(Vector3 position)
        {
            foreach (var w in Active)
                if (w.IsOpen && InFreshAir(position, w.transform.position, w.transform.forward, w.freshAirRadius))
                    return true;
            return false;
        }

        /// <summary>Pure geometry: in front of the window (its +Z faces the corridor) and close enough.</summary>
        public static bool InFreshAir(Vector3 position, Vector3 window, Vector3 forward, float radius)
        {
            var d = position - window;
            d.y = 0f;
            return d.sqrMagnitude <= radius * radius && Vector3.Dot(d, forward) > -0.1f;
        }
    }
}
