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

        private int _neighbourFloor = -1;
        private int _neighbourCell = -1;
        private bool _neighbourTrap;
        private bool _neighbourDone;

        private Transform _floorRoot;
        private bool _reshufflePending;
        // After a floor swap the player stands on the other floor's half-landing: no swap back
        // until they have stepped off it.
        private bool _landingArmed = true;
        private const float LandingDepth = 1.0f;
        private const float LandingTrigger = 1.0f;

        public HouseConfig Config => config;
        public LayoutPlan Plan { get; private set; }
        public int ReshuffleIndex { get; private set; }
        public uint LayoutHash { get; private set; }
        public Vector3 CoSourcePosition { get; private set; }
        public bool HasCoSource { get; private set; }
        public int Floor { get; private set; }
        public bool IsPlayerFloor => Floor == config.playerFloor;
        public bool IsTopFloor => Floor == config.floorCount - 1;
        public int FloorCount => config.floorCount;
        /// <summary>Where the player stands after coming out of this floor's fire door, and their yaw.</summary>
        public Vector3 FireDoorPoint { get; private set; }
        public float FireDoorYaw { get; private set; }
        public int HomeFloor => config.playerFloor;
        /// <summary>
        /// Safe arrival point: on the ground floor beside the stairs (round start); upper floors have
        /// the down flight there, so the first corridor cell instead.
        /// </summary>
        public Vector3 StartPoint => Floor == 0
            ? GetCellCenter(LayoutPlan.StartIndex) + config.startOffset
            : GetCellCenter(Plan.MainPath[1]);

        private void OnEnable() => EventBus.Subscribe<LightTurnedOff>(OnLightOff);
        private void OnDisable() => EventBus.Unsubscribe<LightTurnedOff>(OnLightOff);

        private void Start()
        {
            Floor = 0;
            _neighbourFloor = NeighbourPlacement.FloorFor(SeedService.Seed, config.playerFloor, config.neighbourTrapChance);
            Build(0);
            player.Teleport(StartPoint, Direction.North.Yaw());
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
            UpdateStairs();
            if (!_reshufflePending || player.IsHidden) return;
            _reshufflePending = false;
            if (lightTimer == null || !lightTimer.IsOn) Reshuffle();
        }

        /// <summary>
        /// The half-landing between floors exists on both: as the up landing (+half) of the lower
        /// floor and the down landing (−half) of the upper one. Stepping onto it swaps the floor
        /// and shifts the player by one storey, so nothing visibly jumps.
        /// </summary>
        private void UpdateStairs()
        {
            if (Plan == null || player.IsHidden) return;
            var p = player.transform.position;
            var onLanding = IsInStairwell(p) && WallDistanceSouth(p) < LandingDepth;
            if (!onLanding)
            {
                _landingArmed = true;
                return;
            }
            if (!_landingArmed) return;
            if (p.y > LandingTrigger && !IsTopFloor) GoToFloor(Floor + 1);
            else if (p.y < -LandingTrigger && Floor > 0) GoToFloor(Floor - 1);
        }

        private float WallDistanceSouth(Vector3 world) =>
            world.z - (GetCellCenter(LayoutPlan.StartIndex).z - config.cellSize * 0.5f + config.wallThickness);

        /// <summary>Swaps to another floor, keeping the player's spot on the stairs.</summary>
        public void GoToFloor(int floor) => GoToFloor(floor, viaStairs: true);

        public void GoToFloor(int floor, bool viaStairs)
        {
            floor = Mathf.Clamp(floor, 0, config.floorCount - 1);
            if (floor == Floor) return;
            var from = Floor;
            var shift = (from - floor) * config.floorHeight;
            var t = player.transform;
            var pos = t.position + Vector3.up * shift;
            var yaw = t.eulerAngles.y;

            Floor = floor;
            _landingArmed = false;
            EventBus.Publish(new FloorChanged { From = from, To = floor, ViaStairs = viaStairs });
            Build(ReshuffleIndex);
            player.Teleport(pos, yaw);
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
            var layoutSeed = SeedService.DeriveSeed(SeedService.Seed, $"{LayoutStream}/f{Floor}", reshuffleIndex);
            Plan = LayoutGenerator.Generate(layoutSeed, config.rules, IsPlayerFloor);
            _neighbourCell = Floor == _neighbourFloor ? NeighbourPlacement.DoorCell(Plan, SeedService.Seed, Floor, reshuffleIndex) : -1;
            _neighbourTrap = Floor == config.playerFloor;
            HasCoSource = false;
            CoSourcePosition = new Vector3(0f, -1000f, 0f); // other floors: far away, detector silent
            LayoutHash = Plan.ComputeHash();

            if (_floorRoot != null)
            {
                // Detach and deactivate first: old colliders vanish this frame and the NavMesh
                // bake below only sees the new floor. Destroy happens at frame end.
                _floorRoot.SetParent(null);
                _floorRoot.gameObject.SetActive(false);
                Destroy(_floorRoot.gameObject);
            }
            _floorRoot = new GameObject($"Floor{Floor}_{reshuffleIndex}").transform;
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
                Floor = Floor,
            });
        }

        private void BuildCell(int index, LayoutCell cell)
        {
            var root = new GameObject($"Cell{index}_{cell.Feature}").transform;
            root.SetParent(_floorRoot, false);
            var center = GetCellCenter(index);
            var up = Vector3.up * config.wallHeight;

            if (index == LayoutPlan.StartIndex)
            {
                BuildStairwell(center, root);
            }
            else
            {
                Spawn(config.floorPrefab, center, Quaternion.identity, root);
                Spawn(config.ceilingPrefab, center + up, Quaternion.identity, root);
                Spawn(config.lampPrefab, center + up, Quaternion.identity, root);
            }

            foreach (var d in DirectionUtil.All)
                if (!cell.IsOpen(d)) Spawn(config.wallPrefab, WallPoint(center, d), Inward(d), root);

            var feature = cell.Feature switch
            {
                CellFeature.ApartmentDoor => config.apartmentDoorPrefab,
                CellFeature.PlayerDoor => config.playerDoorPrefab,
                CellFeature.Window => config.windowPrefab,
                CellFeature.Elevator => config.elevatorPrefab,
                _ => null,
            };
            if (index == _neighbourCell && config.neighbourDoorPrefab != null)
            {
                var go = Instantiate(config.neighbourDoorPrefab, WallPoint(center, cell.FeatureSide), Inward(cell.FeatureSide), root);
                go.GetComponent<Neighbour>().Setup(this, player, _neighbourTrap, _neighbourDone);
            }
            else if (feature != null)
            {
                Spawn(feature, WallPoint(center, cell.FeatureSide), Inward(cell.FeatureSide), root);
            }

            var hiding = cell.Hiding switch
            {
                HidingKind.Wardrobe => config.wardrobePrefab,
                HidingKind.Table => config.tablePrefab,
                _ => null,
            };
            if (hiding != null) Spawn(hiding, WallPoint(center, cell.HidingSide), Inward(cell.HidingSide), root);

            if (cell.HasFireDoor && config.fireDoorPrefab != null)
            {
                var wall = WallPoint(center, cell.FireDoorSide);
                var go = Instantiate(config.fireDoorPrefab, wall, Inward(cell.FireDoorSide), root);
                go.GetComponent<FireDoor>()?.Setup(this, player);
                FireDoorPoint = wall + Inward(cell.FireDoorSide) * Vector3.forward * 0.9f;
                FireDoorYaw = cell.FireDoorSide.Yaw() + 180f;
            }

            if (cell.HasSwitch)
            {
                // Off-centre, so it never sits behind a door, window or wardrobe on the next wall.
                var rot = Inward(cell.SwitchSide);
                Spawn(config.lightButtonPrefab, WallPoint(center, cell.SwitchSide) + rot * Vector3.right * config.switchOffset, rot, root);
            }

            if (index == LayoutPlan.StartIndex)
            {
                // The switch is on the stairwell's east wall by the corridor opening, clear of the down flight.
                var pos = WallPoint(center, Direction.East) + ToVector(Direction.North) * 1.65f;
                Spawn(config.lightButtonPrefab, pos, Inward(Direction.East), root);
            }

            if (cell.IsCoSource)
            {
                HasCoSource = true;
                CoSourcePosition = center + ToVector(cell.FeatureSide) * (config.cellSize * 0.5f + config.coSourceDepth)
                                   + Vector3.up * 1.2f;
            }
        }

        /// <summary>
        /// The neighbour's fire door: straight to the floor above, arriving in its first corridor
        /// cell. Not via the stairs, so the firefighter does not follow.
        /// </summary>
        public void TakeShortcut()
        {
            _neighbourDone = true;
            if (IsTopFloor) return;
            GoToFloor(Floor + 1, viaStairs: false);
            var cell = Plan.Cells[Plan.MainPath[1]];
            player.Teleport(StartPoint, cell.InDirection.Yaw());
        }

        /// <summary>
        /// FEEL #2 escape route: the fire door's emergency stairs, one floor up or down, coming out
        /// of the other floor's fire door. Not the main stairs, so a hunting firefighter loses you.
        /// </summary>
        public void TakeFireStairs(bool up)
        {
            var target = Floor + (up ? 1 : -1);
            if (target < 0 || target >= config.floorCount) return;
            GoToFloor(target, viaStairs: false);
            player.Teleport(FireDoorPoint, FireDoorYaw);
            EventBus.Publish(new NoiseMade { Position = FireDoorPoint, Radius = 5f }); // the heavy door slams
        }

        /// <summary>
        /// Two-flight panelák stairwell. Pieces are authored in the stairwell frame: origin on the
        /// south wall's inner face, +Z north. Walls repeat one storey up and down, so looking up
        /// or down the stairs never shows the void.
        /// </summary>
        private void BuildStairwell(Vector3 center, Transform root)
        {
            var origin = WallPoint(center, Direction.South);
            var frame = Inward(Direction.South);
            Spawn(config.stairwellFloorPrefab, origin, frame, root);
            Spawn(config.stairwellCeilingPrefab, origin, frame, root);
            Spawn(config.lampPrefab, center + ToVector(Direction.North) * 0.9f + Vector3.up * config.wallHeight, Quaternion.identity, root);
            if (IsTopFloor) Spawn(config.stairwellTopRailPrefab, origin, frame, root);
            else Spawn(config.stairwellPrefab, origin, frame, root);
            if (Floor == 0)
            {
                Spawn(config.stairwellGroundPrefab, origin, frame, root);
                Spawn(config.entrancePrefab, WallPoint(center, Direction.East) + ToVector(Direction.North) * 0.95f, Inward(Direction.East), root);
            }
            else Spawn(config.stairsDownPrefab, origin, frame, root);

            var storey = Vector3.up * config.floorHeight;
            foreach (var d in DirectionUtil.All)
            {
                Spawn(config.wallPrefab, WallPoint(center, d) + storey, Inward(d), root);
                Spawn(config.wallPrefab, WallPoint(center, d) - storey, Inward(d), root);
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
