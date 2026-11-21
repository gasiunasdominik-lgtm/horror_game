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

        /// <summary>
        /// Breathing through a full-face mask, one 4 s cycle (loop): slow filtered inhale,
        /// demand-valve click, harsher exhale. Fixed noise seed: same sound every run.
        /// </summary>
        public static AudioClip CreateMaskBreath()
        {
            const float length = 4f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(11);
            float low = 0f, band = 0f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)(rng.NextDouble() * 2 - 1);
                low += 0.08f * (noise - low);           // dull hiss
                band += 0.35f * (noise - band);         // brighter hiss
                float v = 0f;
                if (t < 1.5f) v = low * Mathf.Sin(Mathf.PI * t / 1.5f) * 2.2f;                      // inhale
                else if (t < 1.56f) v = Mathf.Sin(2f * Mathf.PI * 900f * t) * (1.56f - t) * 12f;    // valve click
                else if (t < 3.1f) v = band * Mathf.Sin(Mathf.PI * (t - 1.6f) / 1.5f) * 1.1f;       // exhale
                data[i] = Mathf.Clamp(v, -1f, 1f) * 0.8f;
            }
            return Make("MaskBreath", data);
        }

        /// <summary>Heavy boot plus a clink of gear (carabiner, cylinder strap).</summary>
        public static AudioClip CreateGearStep(int variant)
        {
            const float length = 0.35f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(100 + variant);
            var detune = 1f + variant * 0.07f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var thud = Mathf.Sin(2f * Mathf.PI * 65f * t) * Mathf.Exp(-t * 35f)
                           + (float)(rng.NextDouble() * 2 - 1) * Mathf.Exp(-t * 60f) * 0.4f;
                var ct = t - 0.04f;
                var clink = ct < 0f ? 0f : (Mathf.Sin(2f * Mathf.PI * 2100f * detune * ct)
                                            + 0.7f * Mathf.Sin(2f * Mathf.PI * 3170f * detune * ct)
                                            + 0.5f * Mathf.Sin(2f * Mathf.PI * 4890f * detune * ct)) * Mathf.Exp(-ct * 18f) * 0.18f;
                data[i] = Mathf.Clamp(thud * 0.8f + clink, -1f, 1f);
            }
            return Make("GearStep", data);
        }

        /// <summary>
        /// A call through the mask's speech diaphragm: two buzzing syllables ("ha-lo"),
        /// muffled and clipped. Unintelligible on purpose: the player hears a threat, not words.
        /// </summary>
        public static AudioClip CreateMaskedCall()
        {
            const float length = 1.3f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            float phase = 0f, lp = 0f, hp = 0f, prev = 0f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var syllable = t < 0.35f ? Mathf.Sin(Mathf.PI * t / 0.35f)
                    : t > 0.45f && t < 1.15f ? Mathf.Sin(Mathf.PI * (t - 0.45f) / 0.7f) : 0f;
                var f0 = t < 0.4f ? 135f : Mathf.Lerp(150f, 105f, (t - 0.45f) / 0.7f);
                phase += f0 / SampleRate;
                var saw = 2f * (phase - Mathf.Floor(phase)) - 1f;
                lp += 0.12f * (saw - lp);           // muffled by the mask
                hp = 0.9f * (hp + lp - prev);       // thin, radio-like
                prev = lp;
                data[i] = Mathf.Clamp(hp * syllable * 4f, -0.9f, 0.9f);
            }
            return Make("MaskedCall", data);
        }

        private static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
