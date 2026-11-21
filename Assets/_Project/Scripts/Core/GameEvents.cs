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
        public int Floor;
    }

    /// <summary>The player walked onto the half-landing and the house swapped to another floor.</summary>
    public struct FloorChanged
    {
        public int From;
        public int To;
        /// <summary>False for a shortcut through a flat: nobody saw which way the player went.</summary>
        public bool ViaStairs;
    }

    /// <summary>A line of speech shown at the bottom of the screen.</summary>
    public struct SubtitleRequested
    {
        public string Text;
        public float Duration;
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

    public enum EndingId
    {
        None = 0,
        Collapse = 1,
        Evacuation = 2,
        FreshAir = 3,
    }

    public enum CollapseCause
    {
        Corridor = 0,
        Neighbour = 1,
        Home = 2,
    }

    /// <summary>The player walked into a flat: their own (home) or the neighbour's at the source.</summary>
    public struct EnteredFlat
    {
        public bool Home;
    }

    public struct RoundEnded
    {
        public EndingId Ending;
        public CollapseCause Cause;
    }
}
