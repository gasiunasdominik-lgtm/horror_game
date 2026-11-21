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
            EventBus.Subscribe<FloorChanged>(OnFloorChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LayoutRebuilt>(OnLayoutRebuilt);
            EventBus.Unsubscribe<NoiseMade>(OnNoise);
            EventBus.Unsubscribe<LightTurnedOn>(OnLightOn);
            EventBus.Unsubscribe<FloorChanged>(OnFloorChanged);
        }

        // Floors: only the player's floor exists, so when he is elsewhere he is "offstage" with
        // an abstract floor number. He walks the house a floor at a time and comes onto the
        // player's floor when he gets there, when a light goes on nearby, or right behind a
        // player who fled up/down the stairs while he was hunting.
        private bool _floorChanged;
        private bool _following;
        private int _leftFloor;
        private bool _offstage;
        private int _myFloor;
        private float _arriveAt = -1f;
        private float _roamFrom;
        private float _nextRoam;
        private bool _pendingLight;
        private Vector3 _lightPosition;
        private static readonly Vector3 Offstage = new(0f, -500f, 0f);

        public bool IsOffstage => _offstage;
        /// <summary>The floor he is on (the player's floor when he is on stage).</summary>
        public int CurrentFloor => _offstage ? _myFloor : house.Floor;

        private void OnFloorChanged(FloorChanged e)
        {
            _floorChanged = true;
            _leftFloor = e.From;
            _following = e.ViaStairs && !(_offstage && !_arrivalHunting) &&
                         Brain.State is FirefighterState.Chase or FirefighterState.Call or FirefighterState.Search;
        }

        // Patrol life: leaving for another floor, stopping to look, knocking on flats.
        private bool _leaving;
        private float _leaveAt;
        private PatrolAction _activity;
        private float _activityUntil;
        private Vector3 _activityTarget;
        private Vector3 _activityFace;
        private int _activityDoor = -1;
        private float _nextKnock;
        private readonly HashSet<int> _knockedDoors = new();
        private bool _activityStarted;

        public bool IsLeaving => _leaving;

        private void StartPatrolClock()
        {
            var s = config.settings;
            _leaveAt = Time.time + s.patrolStayMin + (float)_rng.NextDouble() * Mathf.Max(0f, s.patrolStayMax - s.patrolStayMin);
            _activity = PatrolAction.None;
        }

        // A new floor visit (arrival, or the house reshuffled): every door is new again.
        private void ForgetKnocks() => _knockedDoors.Clear();

        private void StartLeaving()
        {
            _leaving = true;
            _activity = PatrolAction.None;
        }

        private void LeaveFloor()
        {
            _leaving = false;
            var from = house.Floor;
            var up = from + 1 < house.FloorCount;
            var down = from > 0;
            _myFloor = up && down ? (_rng.NextDouble() < 0.5 ? from + 1 : from - 1) : up ? from + 1 : from - 1;
            GoOffstage(-1f, hunting: false);
            ScheduleRoam();
            EventBus.Publish(new FirefighterLeftFloor { From = from, To = _myFloor });
        }

        /// <summary>Calm patrol on top of the brain's wandering. True = it handled movement this frame.</summary>
        private bool UpdatePatrolLife(bool arrived)
        {
            if (Brain.State != FirefighterState.Patrol) return false;
            var s = config.settings;
            if (!_leaving && Time.time >= _leaveAt) StartLeaving();
            if (_leaving)
            {
                var stairs = StairsEntry();
                MoveTo(stairs, force: false);
                if ((transform.position - stairs).sqrMagnitude < 1.5f * 1.5f) LeaveFloor();
                return true;
            }
            if (_activity != PatrolAction.None)
            {
                if (!_activityStarted)
                {
                    if (_activity == PatrolAction.Knock && !(arrived && (transform.position - _activityTarget).sqrMagnitude < 1.2f * 1.2f))
                    {
                        MoveTo(_activityTarget, force: false);
                        return true;
                    }
                    _activityStarted = true;
                    _agent.isStopped = true;
                    _activityUntil = Time.time + (_activity == PatrolAction.Knock ? s.knockTime : s.lookAroundTime);
                    if (_activity == PatrolAction.Knock)
                    {
                        _knockedDoors.Add(_activityDoor);
                        _nextKnock = Time.time + s.knockCooldown;
                    }
                    if (_activity == PatrolAction.Knock)
                        EventBus.Publish(new FirefighterKnocked { Position = _activityTarget, Call = _rng.NextDouble() < 0.5 });
                }
                if (_activity == PatrolAction.LookAround) transform.Rotate(0f, s.lookAroundSpeed * Time.deltaTime, 0f);
                else FaceTowards(_activityFace);
                if (Time.time >= _activityUntil)
                {
                    _activity = PatrolAction.None;
                    _agent.isStopped = false;
                }
                return true;
            }
            if (arrived || !_agent.hasPath)
            {
                Vector3 front = default, wall = default;
                var door = Time.time >= _nextKnock && NearestDoor(s.knockDoorRange, out front, out wall, out _activityDoor);
                _activityFace = door ? wall : default;
                _activity = PatrolActivity.Pick(_rng.NextDouble(), door, s.idleChance, s.knockShare);
                _activityStarted = false;
                _activityTarget = front;
                if (_activity != PatrolAction.None) return true;
            }
            return false;
        }

        private bool NearestDoor(float range, out Vector3 front, out Vector3 wall, out int index)
        {
            front = default;
            wall = default;
            index = -1;
            var best = range * range;
            var found = false;
            for (var i = 0; i < house.DoorFronts.Count; i++)
            {
                if (_knockedDoors.Contains(i)) continue;
                var d = house.DoorFronts[i];
                var dist = (d - transform.position).sqrMagnitude;
                if (dist > best || dist < 0.5f) continue;
                if (!NavMesh.SamplePosition(d, out var hit, 1f, NavMesh.AllAreas)) continue;
                best = dist;
                front = hit.position;
                wall = house.DoorWalls[i];
                index = i;
                found = true;
            }
            return found;
        }

        private void ScheduleRoam()
        {
            var s = config.settings;
            var wait = s.roamIntervalMin + (float)_rng.NextDouble() * Mathf.Max(0f, s.roamIntervalMax - s.roamIntervalMin);
            _nextRoam = Mathf.Max(_roamFrom, Time.time) + wait;
        }

        private bool _arrivalHunting;
        private readonly List<Vector3> _entries = new();

        /// <summary>Leave the player's floor. delay &lt; 0: no fixed arrival, he roams.</summary>
        private void GoOffstage(float delay, bool hunting)
        {
            _agent.enabled = false;
            transform.position = Offstage;
            _offstage = true;
            _arriveAt = delay < 0f ? -1f : Time.time + delay;
            _arrivalHunting = hunting;
            _pendingLight = false;
        }

        /// <summary>Debug: skip the wait and walk onto the player's floor now.</summary>
        public void ArriveNow()
        {
            if (!_offstage) return;
            _myFloor = house.Floor;
            _arriveAt = Time.time;
        }

        private Vector3 StairsEntry()
        {
            var stairs = house.GetCellCenter(LayoutPlan.StartIndex) + Vector3.forward * 1.6f;
            return NavMesh.SamplePosition(stairs, out var hit, 2.5f, NavMesh.AllAreas) ? hit.position : SpawnPoint();
        }

        private void ArriveByStairs()
        {
            Vector3 target;
            if (_arrivalHunting)
            {
                // Following the player: straight up/down the stairs behind them.
                target = StairsEntry();
            }
            else
            {
                // Calm arrival: from the stairs or the far end, whichever is farther from the player.
                _entries.Clear();
                _entries.Add(StairsEntry());
                _entries.Add(SpawnPoint());
                if (!ArrivalPoint.Pick(_entries, player.transform.position, config.settings.minArrivalDistance, out target))
                {
                    _arriveAt = Time.time + config.settings.arrivalRetry;
                    return;
                }
            }
            _arriveAt = -1f;
            _offstage = false;
            transform.position = target;
            _agent.enabled = true;
            _agent.Warp(target);
            _leaving = false;
            ForgetKnocks();
            StartPatrolClock();
            if (_arrivalHunting) Brain.Force(FirefighterState.Search, player.transform.position);
            else if (_pendingLight) Brain.Force(FirefighterState.Investigate, _lightPosition);
            else Brain.Force(FirefighterState.Patrol, target);
            _pendingLight = false;
        }

        private void OnNoise(NoiseMade e)
        {
            if ((e.Position - transform.position).sqrMagnitude <= e.Radius * e.Radius) Notice(e.Position);
        }

        // The whole floor lights up: he knows where the switch was pressed.
        // Offstage, a light one floor away shows through the stairwell: he comes to look.
        private void OnLightOn(LightTurnedOn e)
        {
            if (!_offstage)
            {
                Notice(e.Position);
                return;
            }
            if (_arrivalHunting || Time.time < _roamFrom) return;
            if (Mathf.Abs(_myFloor - house.Floor) > config.settings.lightHeardFloors) return;
            var delay = _myFloor == house.Floor ? 2f : config.settings.lightResponseDelay;
            _myFloor = house.Floor;
            _pendingLight = true;
            _lightPosition = e.Position;
            _arriveAt = _arriveAt >= 0f ? Mathf.Min(_arriveAt, Time.time + delay) : Time.time + delay;
        }

        private void Notice(Vector3 position)
        {
            _hasStimulus = true;
            _stimulus = position;
        }

        private void OnLayoutRebuilt(LayoutRebuilt e)
        {
            if (_floorChanged)
            {
                _floorChanged = false;
                _checking = null;
                _sawHide = null;
                if (_following)
                {
                    _myFloor = house.Floor;
                    GoOffstage(config.settings.followDelay, hunting: true);
                    return;
                }
                if (_offstage)
                {
                    // He is elsewhere in the house. If the player walked onto his floor, he is there.
                    if (!_arrivalHunting && _myFloor == house.Floor && Time.time >= _roamFrom) _arriveAt = Time.time + 2f;
                    else if (!_arrivalHunting) _arriveAt = -1f;
                    return;
                }
                // Calm, on stage: he stays on the floor the player left.
                _myFloor = _leftFloor;
                GoOffstage(-1f, hunting: false);
                ScheduleRoam();
                return;
            }
            if (_offstage) return;
            if (!_placed)
            {
                // Round start: he is up at the CO source (the emergency call came from there) and
                // starts walking the house after a calm minute or so. Never on the player's floor at once.
                _placed = true;
                _myFloor = house.HomeFloor != house.Floor ? house.HomeFloor : house.FloorCount - 1 - house.Floor;
                _roamFrom = Time.time + config.settings.introGrace;
                GoOffstage(-1f, hunting: false);
                ScheduleRoam();
                return;
            }

            ForgetKnocks();
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
            if (_offstage)
            {
                SeesPlayer = false;
                _hasStimulus = false;
                if (_arriveAt >= 0f)
                {
                    if (Time.time >= _arriveAt) ArriveByStairs();
                    return;
                }
                if (Time.time >= _nextRoam)
                {
                    var s = config.settings;
                    _myFloor = FloorRoam.Step(_myFloor, house.Floor, house.FloorCount, _rng.NextDouble(), s.roamTowardPlayer);
                    ScheduleRoam();
                    if (_myFloor == house.Floor) _arriveAt = Time.time;
                }
                return;
            }

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
            if (Brain.State == FirefighterState.Patrol)
            {
                _sawHide = null;
                StartPatrolClock();
                // Nobody found after a search or a light check: he usually goes to look elsewhere.
                if (previous is FirefighterState.Search or FirefighterState.Investigate &&
                    _rng.NextDouble() < config.settings.leaveAfterSearch)
                    StartLeaving();
            }
            else
            {
                _leaving = false;
                _activity = PatrolAction.None;
            }
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
            if (UpdatePatrolLife(arrived)) return;
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
