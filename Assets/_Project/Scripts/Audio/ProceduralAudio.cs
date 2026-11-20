using UnityEngine;

namespace Vchod.Audio
{
    /// <summary>
    /// Placeholder sounds synthesised at runtime: no third-party assets, no licences to track.
    /// The M2 audio pass replaces them with recorded clips.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        /// <summary>Short square-ish beep like a cheap CO alarm.</summary>
        public static AudioClip CreateBeep(float frequency, float length)
        {
            var samples = Mathf.CeilToInt(SampleRate * length);
            var data = new float[samples];
            var fade = Mathf.Min(samples / 8, SampleRate / 200);
            for (var i = 0; i < samples; i++)
            {
                var s = Mathf.Sin(2f * Mathf.PI * frequency * i / SampleRate);
                s = Mathf.Clamp(s * 3f, -1f, 1f) * 0.5f; // soft-clipped: harsher, more "electronic"
                var env = Mathf.Min(1f, Mathf.Min(i, samples - 1 - i) / (float)fade);
                data[i] = s * env;
            }
            var clip = AudioClip.Create("Beep", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Seamless 2 s loop: high ringing plus a low pressure hum.</summary>
        public static AudioClip CreateTinnitus()
        {
            const int seconds = 2;
            var samples = SampleRate * seconds;
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                // Whole cycles per loop (frequency × 2 s is an integer) so the loop has no click.
                var ring = Mathf.Sin(2f * Mathf.PI * 4000f * t) * (0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.5f * t));
                var low = Mathf.Sin(2f * Mathf.PI * 110f * t);
                data[i] = ring * 0.25f + low * 0.35f;
            }
            var clip = AudioClip.Create("Tinnitus", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
