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
