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

        [Header("Floors")]
        [Min(1)] public int floorCount = 3;
        [Tooltip("0 = ground floor. The player's flat and the CO source are on this floor.")]
        public int playerFloor = 2;
        [Tooltip("Storey height; the half-landing between floors sits at half of it (8 steps × 0.17).")]
        public float floorHeight = 2.72f;
        [Tooltip("Round start on the ground floor, relative to the stairwell cell centre.")]
        public Vector3 startOffset = new(1.0f, 0f, 0.9f);

        [Header("Generation")]
        public LayoutRules rules = new();

        [Header("Shell prefabs (pivot: bottom centre; walls/features: inner face, +Z into the cell)")]
        public GameObject floorPrefab;
        public GameObject ceilingPrefab;
        public GameObject wallPrefab;
        public GameObject lampPrefab;

        [Header("Feature prefabs")]
        [Tooltip("Up flight, half-landing and the next floor's flight above (not on the top floor).")]
        public GameObject stairwellPrefab;
        [Tooltip("Down flight, half-landing below and the previous flight (not on the ground floor).")]
        public GameObject stairsDownPrefab;
        public GameObject stairwellFloorPrefab;
        [Tooltip("Ground floor only: slab where upper floors have the down flight.")]
        public GameObject stairwellGroundPrefab;
        [Tooltip("Top floor only: rail where lower floors have the up flight.")]
        public GameObject stairwellTopRailPrefab;
        public GameObject stairwellCeilingPrefab;
        public GameObject lightButtonPrefab;
        public GameObject apartmentDoorPrefab;
        public GameObject playerDoorPrefab;
        public GameObject windowPrefab;

        [Header("Hiding spot prefabs (pivot: wall face, +Z into the cell)")]
        public GameObject wardrobePrefab;
        public GameObject tablePrefab;

        [Tooltip("Corridor light switches sit this far from the wall centre (m).")]
        public float switchOffset = 1.2f;

        [Header("Spawn")]
        [Tooltip("How far inside the cell the player can be placed after a reshuffle, from the centre.")]
        public float safeHalfExtent = 1.3f;
        [Tooltip("CO source sits this far behind its apartment door wall.")]
        public float coSourceDepth = 1.5f;
    }
}
