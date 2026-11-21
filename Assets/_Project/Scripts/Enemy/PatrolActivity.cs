namespace Vchod.Enemy
{
    public enum PatrolAction { None, LookAround, Knock }

    /// <summary>What he does at a patrol point: usually walks on, sometimes stops to look, or knocks on a nearby flat.</summary>
    public static class PatrolActivity
    {
        public static PatrolAction Pick(double roll, bool doorNearby, float idleChance, float knockShare)
        {
            if (roll >= idleChance) return PatrolAction.None;
            return doorNearby && roll < idleChance * knockShare ? PatrolAction.Knock : PatrolAction.LookAround;
        }
    }
}
