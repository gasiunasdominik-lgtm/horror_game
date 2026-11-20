using UnityEngine;

namespace Vchod.Player
{
    [CreateAssetMenu(menuName = "Vchod/Player Config", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movement (m/s)")]
        public float walkSpeed = 2.0f;
        public float sprintSpeed = 3.6f;
        [Tooltip("How fast speed changes toward the target; higher = snappier.")]
        public float acceleration = 10f;
        public float gravity = -20f;

        [Header("Look")]
        [Tooltip("Degrees per mouse pixel.")]
        public float mouseSensitivity = 0.08f;
        [Tooltip("Degrees per second at full stick deflection.")]
        public float gamepadLookSpeed = 140f;
        public float maxPitch = 85f;

        [Header("Interaction")]
        public float interactDistance = 2.2f;
        public LayerMask interactMask = ~0;
    }
}
