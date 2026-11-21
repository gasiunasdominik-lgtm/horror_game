using UnityEngine;

namespace Vchod.Core
{
    // Event payloads shared through EventBus. Plain structs: no allocations when published.

    public struct RoundStarted
    {
        public int Seed;
    }

    public struct LightTurnedOn
    {
        public float Duration;
        /// <summary>Where the switch was pressed: switching the light on gives the player away (GDD).</summary>
        public Vector3 Position;
    }

    /// <summary>A sound others can hear within Radius metres (footsteps, doorbell, ...).</summary>
    public struct NoiseMade
    {
        public Vector3 Position;
        public float Radius;
    }

    public struct LightTurnedOff
    {
    }

    public struct LayoutRebuilt
    {
        public int Seed;
        public int ReshuffleIndex;
        public uint LayoutHash;
    }

    public struct CoStageChanged
    {
        public int PreviousStage;
        public int Stage;
        public float Level;
    }

    public struct CoCollapsed
    {
        public float Level;
    }
}
