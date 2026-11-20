namespace Vchod.Systems
{
    /// <summary>
    /// Staircase timer switch: a press lights it for a fixed time (a press while on restarts the time).
    /// Pure logic so it can be tested without a scene.
    /// </summary>
    public sealed class LightTimerLogic
    {
        public LightTimerLogic(float duration) => Duration = duration;

        public float Duration { get; set; }
        public float Remaining { get; private set; }
        public bool IsOn => Remaining > 0f;

        /// <summary>Returns true when the press turned the light on (it was off).</summary>
        public bool Press()
        {
            var wasOn = IsOn;
            Remaining = Duration;
            return !wasOn;
        }

        /// <summary>Returns true on the tick the light goes off.</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsOn) return false;
            Remaining -= deltaTime;
            if (Remaining > 0f) return false;
            Remaining = 0f;
            return true;
        }

        /// <summary>Returns true if the light was on.</summary>
        public bool ForceOff()
        {
            var wasOn = IsOn;
            Remaining = 0f;
            return wasOn;
        }
    }
}
