namespace Vchod.Core
{
    /// <summary>Small shared facts about the house that several systems read. Reset each round.</summary>
    public static class HouseState
    {
        /// <summary>The firefighter's floor, or -1 before he has started walking the house.</summary>
        public static int FirefighterFloor = -1;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => FirefighterFloor = -1;
    }
}
