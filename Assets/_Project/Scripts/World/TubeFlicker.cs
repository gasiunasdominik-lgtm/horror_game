namespace Vchod.World
{
    /// <summary>
    /// An old fluorescent tube: it strikes with a few stutters, then now and then dips for a
    /// moment. Deterministic per lamp (seed) and time, so it is testable and costs nothing.
    /// </summary>
    public static class TubeFlicker
    {
        public const float StrikeTime = 0.7f;
        private const float DipSlot = 0.25f;   // time is cut into slots; some slots dip
        private const float DipChance = 0.035f; // ~once every 7 s per lamp
        private const float DipLength = 0.09f;

        public static bool IsLit(float sinceOn, int seed)
        {
            if (sinceOn < 0f) return false;
            if (sinceOn < StrikeTime)
            {
                // Striking: off-on-off-on with a per-lamp rhythm.
                var phase = Hash01(seed, 0) * 0.15f;
                var t = sinceOn + phase;
                return t > 0.12f && t < 0.2f || t > 0.33f && t < 0.38f || t > 0.5f;
            }
            var slot = (int)(sinceOn / DipSlot);
            if (Hash01(seed, slot) >= DipChance) return true;
            return sinceOn - slot * DipSlot > DipLength;
        }

        public static float Hash01(int seed, int n)
        {
            unchecked
            {
                var h = (uint)(seed * 374761393 + n * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }
    }
}
