using UnityEngine;

namespace Vchod.Enemy
{
    /// <summary>What the firefighter noticed this frame.</summary>
    public struct FirefighterPerception
    {
        public bool SeesPlayer;
        public Vector3 PlayerPosition;
        public float DistanceToPlayer;
        /// <summary>A noise he heard or a light switched on, this frame.</summary>
        public bool HasStimulus;
        public Vector3 StimulusPosition;
        public bool ArrivedAtDestination;
    }

    /// <summary>What the body should do now.</summary>
    public enum FirefighterGoal
    {
        /// <summary>Walk between random points of the floor.</summary>
        Wander,
        /// <summary>Walk to <see cref="FirefighterBrain.Destination"/>.</summary>
        GoTo,
        /// <summary>Stand still (calling, looking around).</summary>
        Hold,
        /// <summary>Visit random points around <see cref="FirefighterBrain.Destination"/>.</summary>
        SearchAround,
    }

    /// <summary>
    /// Patrol → Investigate → Chase → Call → Search (GDD). Pure logic: the controller feeds
    /// perception and executes the goal, so transitions are covered by EditMode tests.
    /// </summary>
    public sealed class FirefighterBrain
    {
        private readonly FirefighterSettings _s;
        private float _timeSinceSeen = float.PositiveInfinity;
        private float _arrivedFor = -1f;

        public FirefighterBrain(FirefighterSettings settings) => _s = settings;

        public FirefighterState State { get; private set; } = FirefighterState.Patrol;
        public FirefighterGoal Goal { get; private set; } = FirefighterGoal.Wander;
        public Vector3 Destination { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; private set; }
        public float StateTime { get; private set; }
        public bool HasCaughtPlayer { get; private set; }
        /// <summary>Debug god mode: he still chases, but never catches.</summary>
        public bool CanCatch { get; set; } = true;

        public float Speed => State switch
        {
            FirefighterState.Investigate => _s.investigateSpeed,
            FirefighterState.Chase => _s.chaseSpeed,
            FirefighterState.Search => _s.searchSpeed,
            FirefighterState.Call => 0f,
            _ => _s.patrolSpeed,
        };

        /// <summary>Returns true when the state changed this tick.</summary>
        public bool Tick(float dt, in FirefighterPerception p)
        {
            if (HasCaughtPlayer) return false;
            var before = State;
            StateTime += dt;

            if (p.SeesPlayer)
            {
                _timeSinceSeen = 0f;
                LastKnownPlayerPosition = p.PlayerPosition;
                if (State != FirefighterState.Chase) Enter(FirefighterState.Chase);
            }
            else
            {
                _timeSinceSeen += dt;
            }

            switch (State)
            {
                case FirefighterState.Patrol:
                    if (p.HasStimulus) Investigate(p.StimulusPosition);
                    break;

                case FirefighterState.Investigate:
                    if (p.HasStimulus)
                    {
                        Investigate(p.StimulusPosition);
                        break;
                    }
                    if (p.ArrivedAtDestination && _arrivedFor < 0f)
                    {
                        _arrivedFor = 0f;
                        Goal = FirefighterGoal.Hold;
                    }
                    if (_arrivedFor >= 0f)
                    {
                        _arrivedFor += dt;
                        if (_arrivedFor >= _s.investigateLookTime) Enter(FirefighterState.Patrol);
                    }
                    break;

                case FirefighterState.Chase:
                    Destination = LastKnownPlayerPosition;
                    if (p.SeesPlayer && CanCatch && p.DistanceToPlayer <= _s.catchRadius)
                    {
                        HasCaughtPlayer = true;
                        Goal = FirefighterGoal.Hold;
                    }
                    else if (_timeSinceSeen >= _s.loseSightTime)
                    {
                        Enter(FirefighterState.Call);
                    }
                    break;

                case FirefighterState.Call:
                    if (StateTime >= _s.callDuration) Enter(FirefighterState.Search);
                    break;

                case FirefighterState.Search:
                    if (p.HasStimulus) Investigate(p.StimulusPosition);
                    else if (StateTime >= _s.searchDuration) Enter(FirefighterState.Patrol);
                    break;
            }

            return State != before;
        }

        private void Investigate(Vector3 where)
        {
            if (State != FirefighterState.Investigate) Enter(FirefighterState.Investigate);
            Destination = where;
            Goal = FirefighterGoal.GoTo;
            _arrivedFor = -1f;
        }

        /// <summary>Debug (F5) and layout changes: jump to a state directly.</summary>
        public void Force(FirefighterState state, Vector3 around)
        {
            if (HasCaughtPlayer) return;
            LastKnownPlayerPosition = around;
            Enter(state);
            if (state == FirefighterState.Investigate)
            {
                Destination = around;
                Goal = FirefighterGoal.GoTo;
            }
        }

        private void Enter(FirefighterState state)
        {
            State = state;
            StateTime = 0f;
            _arrivedFor = -1f;
            switch (state)
            {
                case FirefighterState.Patrol:
                    Goal = FirefighterGoal.Wander;
                    break;
                case FirefighterState.Chase:
                    Goal = FirefighterGoal.GoTo;
                    Destination = LastKnownPlayerPosition;
                    break;
                case FirefighterState.Call:
                    Goal = FirefighterGoal.Hold;
                    break;
                case FirefighterState.Search:
                    Goal = FirefighterGoal.SearchAround;
                    Destination = LastKnownPlayerPosition;
                    break;
                case FirefighterState.Investigate:
                    Goal = FirefighterGoal.GoTo;
                    break;
            }
        }
    }
}
