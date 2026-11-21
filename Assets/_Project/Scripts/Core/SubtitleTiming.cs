namespace Vchod.Core
{
    /// <summary>How long a subtitle stays up: long enough to read it calmly (~13 characters per second).</summary>
    public static class SubtitleTiming
    {
        public const float Base = 1.2f;
        public const float PerCharacter = 0.075f;
        public const float Minimum = 3f;

        public static float ReadTime(string text) =>
            System.Math.Max(Minimum, Base + PerCharacter * (text?.Length ?? 0));

        public static float Duration(string text, float requested) => System.Math.Max(requested, ReadTime(text));
    }
}
