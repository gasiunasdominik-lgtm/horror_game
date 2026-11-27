using System.Collections.Generic;
using UnityEngine;

namespace Vchod.World
{
    /// <summary>
    /// A loud sound source (the argument behind a door) that covers other noises near it:
    /// your steps carry much less there. Static registry, like HidingSpot.All.
    /// </summary>
    [ExecuteAlways]
    public sealed class NoiseMask : MonoBehaviour
    {
        public static readonly List<NoiseMask> All = new();

        [SerializeField] private float radius = 5f;
        [Range(0f, 1f)] [SerializeField] private float factor = 0.35f;

        public float Radius { get => radius; set => radius = value; }
        public float Factor { get => factor; set => factor = value; }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        /// <summary>The multiplier for a noise made at this position (1 = not masked).</summary>
        public static float FactorAt(Vector3 position)
        {
            var f = 1f;
            foreach (var m in All)
                if ((m.transform.position - position).sqrMagnitude <= m.radius * m.radius) f = Mathf.Min(f, m.factor);
            return f;
        }
    }
}
