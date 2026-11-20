using UnityEngine;

namespace Vchod.Systems
{
    [CreateAssetMenu(menuName = "Vchod/Light Timer Config", fileName = "LightTimerConfig")]
    public sealed class LightTimerConfig : ScriptableObject
    {
        [Tooltip("Seconds the staircase light stays on after a press (GDD: ~20 s, tune).")]
        [Min(1f)] public float duration = 20f;
    }
}
