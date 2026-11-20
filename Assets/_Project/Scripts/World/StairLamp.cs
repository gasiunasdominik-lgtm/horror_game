using UnityEngine;
using Vchod.Core;

namespace Vchod.World
{
    /// <summary>A ceiling lamp on the staircase circuit. Swaps shared materials (no per-instance copies).</summary>
    public sealed class StairLamp : MonoBehaviour
    {
        [SerializeField] private Light lampLight;
        [SerializeField] private Renderer bulb;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;

        private void OnEnable()
        {
            EventBus.Subscribe<LightTurnedOn>(OnLightOn);
            EventBus.Subscribe<LightTurnedOff>(OnLightOff);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LightTurnedOn>(OnLightOn);
            EventBus.Unsubscribe<LightTurnedOff>(OnLightOff);
        }

        private void OnLightOn(LightTurnedOn e) => SetOn(true);
        private void OnLightOff(LightTurnedOff e) => SetOn(false);

        public void SetOn(bool on)
        {
            lampLight.enabled = on;
            bulb.sharedMaterial = on ? onMaterial : offMaterial;
        }
    }
}
