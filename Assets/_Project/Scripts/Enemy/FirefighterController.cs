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

        private void Execute(bool arrived)
        {
            _agent.speed = Brain.Speed;
            switch (Brain.Goal)
            {
                case FirefighterGoal.Hold when Brain.State == FirefighterState.Call:
                    _agent.isStopped = true;
                    FaceTowards(Brain.LastKnownPlayerPosition);
                    break;
                case FirefighterGoal.Hold:
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
                    if (arrived || !_agent.hasPath) MoveTo(RandomPointAround(Brain.Destination), force: true);
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

        private bool CanSeePlayer(out float distance)
        {
            var s = config.settings;
            var origin = eyes.position;
            var playerPos = player.transform.position;
            distance = Vector3.Distance(transform.position, playerPos);

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
