using Unity.AI.Navigation;
using UnityEngine;
using Vchod.Core;
using Vchod.Player;
using Vchod.Systems;

namespace Vchod.World
{
    /// <summary>
    /// Turns a LayoutPlan into greybox geometry from module prefabs, and rebuilds the
    /// floor in the dark: every time the staircase light goes off, the next layout of
    /// the same seed replaces the current one (GDD: the house reshuffles at light-off).
    /// </summary>
    public sealed class ModuleLayoutBuilder : MonoBehaviour
    {
        public const string LayoutStream = "layout";

        [SerializeField] private HouseConfig config;
        [SerializeField] private LightTimer lightTimer;
        [SerializeField] private PlayerController player;
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private bool reshuffleOnLightOff = true;

        private Transform _floorRoot;
        private bool _reshufflePending;

        public HouseConfig Config => config;
        public LayoutPlan Plan { get; private set; }
        public int ReshuffleIndex { get; private set; }
        public uint LayoutHash { get; private set; }
        public Vector3 CoSourcePosition { get; private set; }

        private void OnEnable() => EventBus.Subscribe<LightTurnedOff>(OnLightOff);
        private void OnDisable() => EventBus.Unsubscribe<LightTurnedOff>(OnLightOff);

        private void Start()
        {
            Build(0);
            var start = GetCellCenter(LayoutPlan.StartIndex);
            player.Teleport(start, Direction.North.Yaw());
        }

        private void OnLightOff(LightTurnedOff e)
        {
            if (!reshuffleOnLightOff) return;
            // Never rebuild around a hidden player: the wardrobe would vanish with them inside.
            if (player.IsHidden) _reshufflePending = true;
            else Reshuffle();
        }

        private void Update()
        {
            if (!_reshufflePending || player.IsHidden) return;
            _reshufflePending = false;
            if (lightTimer == null || !lightTimer.IsOn) Reshuffle();
        }

        /// <summary>Next layout of the same seed. The player keeps their depth along the corridor.</summary>
        public void Reshuffle()
        {
            var anchor = CapturePlayerAnchor();
            Build(ReshuffleIndex + 1);
            RestorePlayerAnchor(anchor);
        }

        public void Build(int reshuffleIndex)
        {
            ReshuffleIndex = reshuffleIndex;
            var layoutSeed = SeedService.DeriveSeed(SeedService.Seed, LayoutStream, reshuffleIndex);
            Plan = LayoutGenerator.Generate(layoutSeed, config.rules);
            LayoutHash = Plan.ComputeHash();

            if (_floorRoot != null)
            {
                // Detach and deactivate first: old colliders vanish this frame and the NavMesh
                // bake below only sees the new floor. Destroy happens at frame end.
                _floorRoot.SetParent(null);
                _floorRoot.gameObject.SetActive(false);
                Destroy(_floorRoot.gameObject);
            }
            _floorRoot = new GameObject($"Floor_{reshuffleIndex}").transform;
            _floorRoot.SetParent(transform, false);

            for (var i = 0; i < Plan.Count; i++) BuildCell(i, Plan.Cells[i]);

            var lightOn = lightTimer != null && lightTimer.IsOn;
            foreach (var lamp in _floorRoot.GetComponentsInChildren<StairLamp>()) lamp.SetOn(lightOn);
            foreach (var button in _floorRoot.GetComponentsInChildren<LightButton>()) button.Bind(lightTimer);
            if (navMeshSurface != null) navMeshSurface.BuildNavMesh();

            EventBus.Publish(new LayoutRebuilt
            {
                Seed = SeedService.Seed,
                ReshuffleIndex = reshuffleIndex,
                LayoutHash = LayoutHash,
            });
        }

        private void BuildCell(int index, LayoutCell cell)
        {
            var root = new GameObject($"Cell{index}_{cell.Feature}").transform;
            root.SetParent(_floorRoot, false);
            var center = GetCellCenter(index);
            var up = Vector3.up * config.wallHeight;

            Spawn(config.floorPrefab, center, Quaternion.identity, root);
            Spawn(config.ceilingPrefab, center + up, Quaternion.identity, root);
            Spawn(config.lampPrefab, center + up, Quaternion.identity, root);

            foreach (var d in DirectionUtil.All)
                if (!cell.IsOpen(d)) Spawn(config.wallPrefab, WallPoint(center, d), Inward(d), root);

            var feature = cell.Feature switch
            {
                CellFeature.Stairwell => config.stairwellPrefab,
                CellFeature.ApartmentDoor => config.apartmentDoorPrefab,
                CellFeature.PlayerDoor => config.playerDoorPrefab,
                CellFeature.Window => config.windowPrefab,
                _ => null,
            };
            if (feature != null) Spawn(feature, WallPoint(center, cell.FeatureSide), Inward(cell.FeatureSide), root);

            var hiding = cell.Hiding switch
            {
                HidingKind.Wardrobe => config.wardrobePrefab,
                HidingKind.Table => config.tablePrefab,
                _ => null,
            };
            if (hiding != null) Spawn(hiding, WallPoint(center, cell.HidingSide), Inward(cell.HidingSide), root);

            if (cell.HasSwitch)
            {
                // Off-centre, so it never sits behind a door, window or wardrobe on the next wall.
                var rot = Inward(cell.SwitchSide);
                Spawn(config.lightButtonPrefab, WallPoint(center, cell.SwitchSide) + rot * Vector3.right * config.switchOffset, rot, root);
            }

            if (index == LayoutPlan.StartIndex)
            {
                // The switch is on the stairwell's east wall, beside the corridor opening.
                var pos = WallPoint(center, Direction.East) + ToVector(Direction.North) * 1.0f;
                Spawn(config.lightButtonPrefab, pos, Inward(Direction.East), root);
            }

            if (cell.IsCoSource)
            {
                CoSourcePosition = center + ToVector(cell.FeatureSide) * (config.cellSize * 0.5f + config.coSourceDepth)
                                   + Vector3.up * 1.2f;
            }
        }

        private static void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab != null) Instantiate(prefab, position, rotation, parent);
        }

        private Vector3 WallPoint(Vector3 center, Direction side) =>
            center + ToVector(side) * (config.cellSize * 0.5f - config.wallThickness);

        /// <summary>Wall-mounted prefabs face +Z into the cell.</summary>
        private static Quaternion Inward(Direction side) => Quaternion.Euler(0f, side.Yaw() + 180f, 0f);

        private static Vector3 ToVector(Direction d)
        {
            var o = d.Offset();
            return new Vector3(o.X, 0f, o.Z);
        }

        public Vector3 GetCellCenter(int index)
        {
            var p = Plan.Cells[index].Pos;
            return transform.position + new Vector3(p.X * config.cellSize, 0f, p.Z * config.cellSize);
        }

        public int CellIndexAt(Vector3 world)
        {
            if (Plan == null) return -1;
            var local = world - transform.position;
            return Plan.IndexOf(new GridPos(Mathf.RoundToInt(local.x / config.cellSize),
                Mathf.RoundToInt(local.z / config.cellSize)));
        }

        public bool IsInStairwell(Vector3 world) => CellIndexAt(world) == LayoutPlan.StartIndex;

        private struct PlayerAnchor
        {
            public bool Valid;
            public int Depth;
            public Vector3 Local;
            public float RelativeYaw;
        }

        private PlayerAnchor CapturePlayerAnchor()
        {
            var index = CellIndexAt(player.transform.position);
            if (index < 0) return default;
            var cell = Plan.Cells[index];
            var frameYaw = cell.InDirection.Yaw();
            return new PlayerAnchor
            {
                Valid = true,
                Depth = cell.Depth,
                Local = Quaternion.Euler(0f, -frameYaw, 0f) * (player.transform.position - GetCellCenter(index)),
                RelativeYaw = player.transform.eulerAngles.y - frameYaw,
            };
        }

        /// <summary>
        /// Puts the player in the cell at the same corridor depth, same relative spot and heading.
        /// In the dark this reads as disorientation, never as being stuck in a wall.
        /// </summary>
        private void RestorePlayerAnchor(PlayerAnchor anchor)
        {
            if (!anchor.Valid || anchor.Depth == 0) return; // the stairwell never changes

            var target = Plan.MainPath[Mathf.Min(anchor.Depth, Plan.MainPath.Count - 1)];
            var frameYaw = Plan.Cells[target].InDirection.Yaw();
            var local = anchor.Local;
            local.x = Mathf.Clamp(local.x, -config.safeHalfExtent, config.safeHalfExtent);
            local.z = Mathf.Clamp(local.z, -config.safeHalfExtent, config.safeHalfExtent);
            var position = GetCellCenter(target) + Quaternion.Euler(0f, frameYaw, 0f) * local;
            player.Teleport(position, frameYaw + anchor.RelativeYaw);
        }
    }
}
