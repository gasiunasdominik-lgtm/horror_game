using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vchod.Core;
using Vchod.Player;
using Vchod.Systems;
using Vchod.World;

namespace Vchod.Enemy
{
    /// <summary>
    /// The firefighter's body: senses (sight, hearing, light), NavMesh movement, and
    /// carrying out what FirefighterBrain decides. Placed again after every reshuffle.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class FirefighterController : MonoBehaviour
    {
        private const float ArriveSlack = 0.25f;
        private const float RepathDistance = 0.5f;
        private const float SampleRadius = 1.5f;
        private const float TurnSpeed = 240f;

        [SerializeField] private FirefighterConfig config;
        [SerializeField] private PlayerController player;
        [SerializeField] private ModuleLayoutBuilder house;
        [SerializeField] private LightTimer lightTimer;
        [SerializeField] private Transform eyes;

        private NavMeshAgent _agent;
        private System.Random _rng;
        private bool _placed;
        private bool _hasStimulus;
        private Vector3 _stimulus;
        private bool _caughtReported;

        // Hiding spots: the one being checked, those already checked in this search,
        // and the one he saw the player climb into.
        private HidingSpot _checking;
        private float _checkTimer;
        private readonly HashSet<HidingSpot> _checked = new();
        private HidingSpot _sawHide;
        private bool _playerWasHidden;
        private Vector3? _noiseChecked;
        private readonly List<Vector3> _spotPoints = new();
        private readonly List<int> _checkedIdx = new();

        public FirefighterBrain Brain { get; private set; }
        public FirefighterState State => Brain.State;
        public bool SeesPlayer { get; private set; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.enabled = false; // no NavMesh until the first floor is built
            Brain = new FirefighterBrain(config.settings);
            _rng = SeedService.CreateStream("firefighter");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LayoutRebuilt>(OnLayoutRebuilt);
            EventBus.Subscribe<NoiseMade>(OnNoise);
            EventBus.Subscribe<LightTurnedOn>(OnLightOn);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LayoutRebuilt>(OnLayoutRebuilt);
            EventBus.Unsubscribe<NoiseMade>(OnNoise);
            EventBus.Unsubscribe<LightTurnedOn>(OnLightOn);
        }

        private void OnNoise(NoiseMade e)
        {
            if ((e.Position - transform.position).sqrMagnitude <= e.Radius * e.Radius) Notice(e.Position);
        }

        // The whole floor lights up: he knows where the switch was pressed.
        private void OnLightOn(LightTurnedOn e) => Notice(e.Position);

        private void Notice(Vector3 position)
        {
            _hasStimulus = true;
            _stimulus = position;
        }

        private void OnLayoutRebuilt(LayoutRebuilt e)
        {
            var target = _placed && OnCurrentFloor(transform.position, out var kept) ? kept : SpawnPoint();
            // Move first: an agent enabled off the new NavMesh logs an error.
            transform.position = target;
            _agent.enabled = true;
            _agent.Warp(target);
            if (_placed && Brain.State != FirefighterState.Patrol)
                Brain.Force(FirefighterState.Search, target); // old positions mean nothing in the new layout
            _placed = true;
        }

        private bool OnCurrentFloor(Vector3 position, out Vector3 onMesh)
        {
            onMesh = position;
            if (house.CellIndexAt(position) < 0) return false;
            if (!NavMesh.SamplePosition(position, out var hit, SampleRadius, NavMesh.AllAreas)) return false;
            onMesh = hit.position;
            return true;
        }

        /// <summary>Deepest cell that is not the player's door: as far from the stairs as possible.</summary>
        private Vector3 SpawnPoint()
        {
            var plan = house.Plan;
            var best = LayoutPlan.StartIndex;
            for (var i = 0; i < plan.Count; i++)
            {
                var c = plan.Cells[i];
                if (c.Feature == CellFeature.PlayerDoor) continue;
                if (c.Depth > plan.Cells[best].Depth) best = i;
            }
            var p = house.GetCellCenter(best);
            return NavMesh.SamplePosition(p, out var hit, SampleRadius, NavMesh.AllAreas) ? hit.position : p;
        }

        private void Update()
        {
            if (!_placed || Brain.HasCaughtPlayer) return;

            var hiddenNow = player.IsHidden;
            if (hiddenNow && !_playerWasHidden && SeesPlayer) _sawHide = player.HiddenIn as HidingSpot;
            _playerWasHidden = hiddenNow;

            SeesPlayer = CanSeePlayer(out var distance);
            var perception = new FirefighterPerception
            {
                SeesPlayer = SeesPlayer,
                PlayerPosition = player.transform.position,
                DistanceToPlayer = distance,
                HasStimulus = _hasStimulus,
                StimulusPosition = _stimulus,
                ArrivedAtDestination = Arrived(),
            };
            _hasStimulus = false;

            if (Brain.State != _executedState) OnStateChanged(_executedState); // forced (debug, reshuffle)
            var previous = Brain.State;
            if (Brain.Tick(Time.deltaTime, perception)) OnStateChanged(previous);

            if (Brain.HasCaughtPlayer)
            {
                ReportCaught();
                return;
            }
            Execute(perception.ArrivedAtDestination);
        }

        private FirefighterState _executedState;

        private void OnStateChanged(FirefighterState previous)
        {
            _executedState = Brain.State;
            _checking = null;
            _noiseChecked = null;
            if (Brain.State is FirefighterState.Search or FirefighterState.Investigate) _checked.Clear();
            if (Brain.State == FirefighterState.Patrol) _sawHide = null;
            // Drop the old route: a new state picks its own first waypoint.
            if (_agent.isOnNavMesh) _agent.ResetPath();
            EventBus.Publish(new FirefighterStateChanged { Previous = previous, State = Brain.State });
        }

        private void ReportCaught()
        {
            if (_caughtReported) return;
            _caughtReported = true;
            _agent.isStopped = true;
            player.SetInputLock(this, true);
            EventBus.Publish(new PlayerCaught { Position = player.transform.position });
        }

        private bool CheckingAllowed => Brain.State is FirefighterState.Search or FirefighterState.Investigate;

        /// <summary>Walk to the spot, wait, open it. Returns false when there is nothing to check.</summary>
        private bool UpdateCheck()
        {
            if (_checking == null || !CheckingAllowed || !_checking.isActiveAndEnabled)
            {
                _checking = null;
                return false;
            }
            var target = _checking.CheckPoint;
            var flat = target - transform.position;
            flat.y = 0f;
            if (flat.magnitude > _agent.stoppingDistance + 0.5f)
            {
                _checkTimer = 0f;
                MoveTo(target, force: false);
                return true;
            }

            _agent.isStopped = true;
            FaceTowards(_checking.transform.position);
            _checkTimer += Time.deltaTime;
            if (_checkTimer < config.settings.checkDuration) return true;

            var spot = _checking;
            _checked.Add(spot);
            _checking = null;
            if (spot == _sawHide) _sawHide = null;
            if (spot.IsOccupied && Brain.CatchHidden())
            {
                spot.Expose();
                ReportCaught();
            }
            return true;
        }

        private HidingSpot PickSpot(Vector3 center, float radius, bool alwaysCheck)
        {
            var spots = HidingSpot.All;
            _spotPoints.Clear();
            _checkedIdx.Clear();
            var saw = -1;
            for (var i = 0; i < spots.Count; i++)
            {
                _spotPoints.Add(spots[i].CheckPoint);
                if (_checked.Contains(spots[i])) _checkedIdx.Add(i);
                if (spots[i] == _sawHide) saw = i;
            }
            var chance = alwaysCheck ? 1f : config.settings.hidingCheckChance;
            var pick = HidingSearch.Pick(_spotPoints, center, radius, _checkedIdx, saw, _rng.NextDouble(), chance, out var considered);
            if (pick < 0 && considered >= 0) _checked.Add(spots[considered]); // walked past it this search
            return pick < 0 ? null : spots[pick];
        }

        private void Execute(bool arrived)
        {
            _agent.speed = Brain.Speed;
            if (UpdateCheck()) return;
            switch (Brain.Goal)
            {
                case FirefighterGoal.Hold when Brain.State == FirefighterState.Call:
                    _agent.isStopped = true;
                    FaceTowards(Brain.LastKnownPlayerPosition);
                    break;
                case FirefighterGoal.Hold:
                    // A creaking wardrobe right where the noise was gets opened, once per noise.
                    if (Brain.State == FirefighterState.Investigate && _noiseChecked != Brain.Destination)
                    {
                        _noiseChecked = Brain.Destination;
                        _checking = PickSpot(Brain.Destination, config.settings.checkNearNoise, alwaysCheck: true);
                        if (_checking != null) break;
                    }
                    // Looking around where the noise or light was: a slow turn, torch sweeping.
                    _agent.isStopped = true;
                    transform.Rotate(0f, config.settings.lookAroundSpeed * Time.deltaTime, 0f);
                    break;
                case FirefighterGoal.GoTo:
                    MoveTo(Brain.Destination, force: false);
                    break;
                case FirefighterGoal.Wander:
                    if (arrived || !_agent.hasPath) MoveTo(RandomCellPoint(), force: true);
                    break;
                case FirefighterGoal.SearchAround:
                    if (!arrived && _agent.hasPath) break;
                    _checking = PickSpot(Brain.Destination, config.settings.searchRadius, alwaysCheck: false);
                    if (_checking == null) MoveTo(RandomPointAround(Brain.Destination), force: true);
                    break;
            }
        }

        private void MoveTo(Vector3 target, bool force)
        {
            _agent.isStopped = false;
            if (!force && _agent.hasPath && (_agent.destination - target).sqrMagnitude < RepathDistance * RepathDistance)
                return;
            _agent.SetDestination(target);
        }

        private bool Arrived() =>
            _agent.enabled && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + ArriveSlack;

        private Vector3 RandomCellPoint()
        {
            var plan = house.Plan;
            return house.GetCellCenter(_rng.Next(plan.Count));
        }

        private Vector3 RandomPointAround(Vector3 center)
        {
            var r = config.settings.searchRadius;
            for (var i = 0; i < 8; i++)
            {
                var offset = new Vector3((float)(_rng.NextDouble() * 2 - 1) * r, 0f, (float)(_rng.NextDouble() * 2 - 1) * r);
                if (NavMesh.SamplePosition(center + offset, out var hit, SampleRadius, NavMesh.AllAreas)) return hit.position;
            }
            return RandomCellPoint();
        }

        private void FaceTowards(Vector3 point)
        {
            var dir = point - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), TurnSpeed * Time.deltaTime);
        }

        private static readonly float[] SightHeights = { 1.2f, 1.45f }; // chest, head (inside the player's capsule)

        /// <summary>A wardrobe hides completely. Under a table the light or a close torch gives you away.</summary>
        private bool CanSeeHidden(HidingSpot spot, float distance)
        {
            if (spot.Kind != HidingKind.Table) return false;
            var s = config.settings;
            var lightOn = lightTimer != null && lightTimer.IsOn;
            if (distance > (lightOn ? s.tableRevealLit : s.tableRevealDark)) return false;
            s.SightFor(lightOn, out _, out var angle);
            var to = player.CameraPivot.position - eyes.position;
            if (Vector3.Angle(eyes.forward, to) > angle * 0.5f) return false;
            if (!Physics.Raycast(eyes.position, to.normalized, out var hit, to.magnitude + 0.5f, ~0, QueryTriggerInteraction.Ignore))
                return true;
            return hit.collider.transform.IsChildOf(spot.transform);
        }

        private bool CanSeePlayer(out float distance)
        {
            var s = config.settings;
            var origin = eyes.position;
            var playerPos = player.transform.position;
            distance = Vector3.Distance(transform.position, playerPos);
            if (player.HiddenIn is HidingSpot spot) return CanSeeHidden(spot, distance);

            s.SightFor(lightTimer != null && lightTimer.IsOn, out var range, out var angle);
            var toPlayer = playerPos + Vector3.up * SightHeights[0] - origin;
            if (toPlayer.magnitude > range) return false;
            var near = distance <= s.nearSenseRadius;
            if (!near && Vector3.Angle(eyes.forward, toPlayer) > angle * 0.5f) return false;

            foreach (var h in SightHeights)
            {
                var to = playerPos + Vector3.up * h - origin;
                if (Physics.Raycast(origin, to.normalized, out var hit, to.magnitude + 0.5f, ~0, QueryTriggerInteraction.Ignore)
                    && (hit.collider.transform == player.transform || hit.collider.transform.IsChildOf(player.transform)))
                    return true;
            }
            return false;
        }
    }
}
