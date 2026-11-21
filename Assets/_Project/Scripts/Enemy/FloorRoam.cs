namespace Vchod.Enemy
{
    /// <summary>One step of the firefighter's walk through the house while he is not on the player's floor.</summary>
    public static class FloorRoam
    {
        /// <summary>Always moves exactly one floor; toward the player with the given chance, otherwise away if possible.</summary>
        public static int Step(int current, int playerFloor, int floorCount, double roll, float towardChance)
        {
            var toward = playerFloor > current ? 1 : playerFloor < current ? -1 : (current + 1 < floorCount ? 1 : -1);
            var next = roll < towardChance ? current + toward : current - toward;
            if (next < 0 || next >= floorCount) next = current + toward;
            if (next < 0 || next >= floorCount) next = current;
            return next;
        }
    }
}
