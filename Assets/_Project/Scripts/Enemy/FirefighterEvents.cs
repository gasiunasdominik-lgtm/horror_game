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
}
