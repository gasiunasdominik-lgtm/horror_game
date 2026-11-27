using UnityEngine;

namespace Vchod.Enemy
{
    public struct FirefighterStateChanged
    {
        public FirefighterState Previous;
        public FirefighterState State;
    }

    /// <summary>He reached the player: evacuation (ending 2, handled by EndingsManager in T-026).</summary>
    public struct PlayerCaught
    {
        public Vector3 Position;
    }

    /// <summary>He knocks on a flat's door while patrolling (evacuating the house). Call = he also calls out.</summary>
    public struct FirefighterKnocked
    {
        public UnityEngine.Vector3 Position;
        public bool Call;
    }

    /// <summary>Heard on the stairs below/above without coming (false alarm). Audio plays steps there.</summary>
    public struct FirefighterOnStairs
    {
        public UnityEngine.Vector3 Position;
        public bool GoingAway;
    }

    /// <summary>He left the player's floor for another one.</summary>
    public struct FirefighterLeftFloor
    {
        public int From;
        public int To;
    }

}
