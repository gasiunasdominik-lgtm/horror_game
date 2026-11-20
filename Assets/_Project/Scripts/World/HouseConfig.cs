using UnityEngine;

namespace Vchod.World
{
    /// <summary>Module prefabs and floor dimensions. Art pass swaps prefabs here, code stays the same.</summary>
    [CreateAssetMenu(menuName = "Vchod/House Config", fileName = "HouseConfig")]
    public sealed class HouseConfig : ScriptableObject
    {
        [Header("Dimensions (m)")]
        public float cellSize = 4f;
        public float wallHeight = 2.8f;
        public float wallThickness = 0.2f;

        [Header("Generation")]
        public LayoutRules rules = new();

        [Header("Shell prefabs (pivot: bottom centre; walls/features: inner face, +Z into the cell)")]
        public GameObject floorPrefab;
        public GameObject ceilingPrefab;
        public GameObject wallPrefab;
        public GameObject lampPrefab;

        [Header("Feature prefabs")]
        public GameObject stairwellPrefab;
        public GameObject lightButtonPrefab;
        public GameObject apartmentDoorPrefab;
        public GameObject playerDoorPrefab;
        public GameObject windowPrefab;

        [Header("Spawn")]
        [Tooltip("How far inside the cell the player can be placed after a reshuffle, from the centre.")]
        public float safeHalfExtent = 1.3f;
        [Tooltip("CO source sits this far behind its apartment door wall.")]
        public float coSourceDepth = 1.5f;
    }
}
