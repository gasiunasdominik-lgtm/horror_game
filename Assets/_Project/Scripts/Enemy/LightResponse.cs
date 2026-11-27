namespace Vchod.Enemy
{
    /// <summary>How he reacts to a light one floor away (IDEA 21. 11.: not always, for suspense).</summary>
    public enum LightReaction { Come, FalseAlarm, Late }

    public static class LightResponse
    {
        /// <summary>Come with comeChance; a false alarm (steps on the stairs that go away) with fakeChance; otherwise later.</summary>
        public static LightReaction Pick(double roll, float comeChance, float fakeChance)
        {
            if (roll < comeChance) return LightReaction.Come;
            return roll < comeChance + fakeChance ? LightReaction.FalseAlarm : LightReaction.Late;
        }
    }
}
