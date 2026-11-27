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

        /// <summary>
        /// Firefighter's heavy rubber boot on concrete: a deep, long thud with a gritty scuff.
        /// Clearly heavier than the player's shoes, so you can tell whose steps you hear.
        /// </summary>
        public static AudioClip CreateBootStep(int variant)
        {
            const float length = 0.32f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(300 + variant);
            var f = 52f + variant * 4f;
            float lp = 0f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var thud = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 22f);
                var noise = (float)(rng.NextDouble() * 2 - 1);
                lp += 0.25f * (noise - lp);
                var scuff = lp * Mathf.Exp(-Mathf.Abs(t - 0.05f) * 40f) * 0.9f;
                data[i] = Mathf.Clamp((thud + scuff) * 0.95f, -1f, 1f);
            }
            return Make("BootStep", data);
        }

        /// <summary>A quiet clink of gear (carabiner, cylinder strap), played on some steps only.</summary>
        public static AudioClip CreateGearClink(int variant)
        {
            const float length = 0.3f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var detune = 1f + variant * 0.07f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 2100f * detune * t)
                           + 0.7f * Mathf.Sin(2f * Mathf.PI * 3170f * detune * t)
                           + 0.5f * Mathf.Sin(2f * Mathf.PI * 4890f * detune * t)) * Mathf.Exp(-t * 18f) * 0.3f;
            }
            return Make("GearClink", data);
        }

        /// <summary>
        /// The player's own shoes on worn linoleum: a short soft tap walking, a sharper slap running.
        /// </summary>
        public static AudioClip CreateShoeStep(int variant, bool running)
        {
            var length = running ? 0.16f : 0.12f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(500 + variant + (running ? 50 : 0));
            float lp = 0f, hp = 0f, prev = 0f;
            var cutoff = running ? 0.55f : 0.3f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)(rng.NextDouble() * 2 - 1);
                lp += cutoff * (noise - lp);
                hp = 0.9f * (hp + lp - prev);
                prev = lp;
                var body = Mathf.Sin(2f * Mathf.PI * (running ? 140f : 120f) * t) * Mathf.Exp(-t * 45f) * 0.5f;
                var env = Mathf.Exp(-t * (running ? 38f : 55f));
                data[i] = Mathf.Clamp((hp * 1.4f + body) * env * (running ? 0.9f : 0.55f), -1f, 1f);
            }
            return Make(running ? "ShoeRun" : "ShoeWalk", data);
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

        /// <summary>
        /// Night wind through an open window, 6 s seamless loop: low rumble plus a whistle
        /// that swells. Modulations complete whole cycles, so the loop point is inaudible.
        /// </summary>
        public static AudioClip CreateWind()
        {
            const float length = 6f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(23);
            float low = 0f, band = 0f;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)(rng.NextDouble() * 2 - 1);
                low += 0.02f * (noise - low);
                band += 0.2f * (noise - band);
                var gust = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * t / length);
                var whistle = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 2f * t / length + 1f);
                data[i] = Mathf.Clamp(low * 6f * gust + band * 0.25f * whistle * gust, -1f, 1f) * 0.7f;
            }
            // Cross-fade the last 0.2 s into the start so the noise filters do not click at the loop.
            var fade = (int)(SampleRate * 0.2f);
            for (var i = 0; i < fade; i++)
            {
                var k = (float)i / fade;
                data[samples - fade + i] = Mathf.Lerp(data[samples - fade + i], data[i], k);
            }
            return Make("Wind", data);
        }

        /// <summary>Three knuckle knocks on a wooden flat door.</summary>
        public static AudioClip CreateKnock()
        {
            const float length = 0.9f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(31);
            foreach (var start in new[] { 0f, 0.24f, 0.5f })
            {
                var from = (int)(start * SampleRate);
                for (var i = from; i < samples; i++)
                {
                    var t = (float)(i - from) / SampleRate;
                    if (t > 0.15f) break;
                    var body = Mathf.Sin(2f * Mathf.PI * 180f * t) * Mathf.Exp(-t * 45f);
                    var click = (float)(rng.NextDouble() * 2 - 1) * Mathf.Exp(-t * 300f) * 0.6f;
                    data[i] += body * 0.8f + click;
                }
            }
            for (var i = 0; i < samples; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            return Make("Knock", data);
        }

        /// <summary>
        /// An old lady's voice through a door crack: syllables of a muffled, slightly wavering
        /// voice with natural pauses. No words: the subtitle carries the meaning. Drowsy = slower and lower.
        /// </summary>
        public static AudioClip CreateMumble(int seed, float length, bool drowsy)
        {
            var rng = new System.Random(seed);
            var samples = Mathf.Max(1, (int)(SampleRate * length));
            var data = new float[samples];
            var baseF0 = drowsy ? 175f : 215f;
            var rate = drowsy ? 0.75f : 1f;
            float phase = 0f, lp1 = 0f, lp2 = 0f, hp = 0f, prev = 0f;
            var syllableStart = 0f;
            var syllableLength = 0.18f;
            var pitch = baseF0;
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var s = t - syllableStart;
                if (s > syllableLength)
                {
                    // Next syllable, sometimes after a short breath.
                    syllableStart = t + (rng.NextDouble() < 0.18 ? 0.25f : 0.03f) / rate;
                    syllableLength = (0.11f + (float)rng.NextDouble() * 0.16f) / rate;
                    pitch = baseF0 * (0.88f + (float)rng.NextDouble() * 0.24f);
                    s = t - syllableStart;
                }
                var env = s >= 0f && s <= syllableLength ? Mathf.Sin(Mathf.PI * s / syllableLength) : 0f;
                var tail = Mathf.Clamp01((length - t) / 0.3f); // fade out at the end of the line
                var f0 = pitch * (1f + 0.025f * Mathf.Sin(2f * Mathf.PI * 5.5f * t)); // old-voice wobble
                phase += f0 / SampleRate;
                var saw = 2f * (phase - Mathf.Floor(phase)) - 1f;
                lp1 += 0.09f * (saw - lp1);   // through a door: dull
                lp2 += 0.09f * (lp1 - lp2);
                hp = 0.97f * (hp + lp2 - prev);
                prev = lp2;
                data[i] = Mathf.Clamp(hp * env * tail * 5f, -0.8f, 0.8f);
            }
            return Make("Mumble", data);
        }

        /// <summary>Adds one muffled voice phrase (syllables on a sawtooth, doubly low-passed) into data.</summary>
        private static void AddPhrase(float[] data, System.Random rng, float start, float length, float f0, float rate, float gain, float muffle)
        {
            var from = (int)(start * SampleRate);
            var to = System.Math.Min(data.Length, (int)((start + length) * SampleRate));
            float phase = 0f, lp1 = 0f, lp2 = 0f, hp = 0f, prev = 0f;
            var sylStart = start;
            var sylLength = 0.16f / rate;
            var pitch = f0;
            for (var i = from; i < to; i++)
            {
                var t = (float)i / SampleRate;
                if (t - sylStart > sylLength)
                {
                    sylStart = t + 0.02f / rate;
                    sylLength = (0.09f + (float)rng.NextDouble() * 0.15f) / rate;
                    pitch = f0 * (0.85f + (float)rng.NextDouble() * 0.35f);
                }
                var s = t - sylStart;
                var env = s >= 0f && s <= sylLength ? Mathf.Sin(Mathf.PI * s / sylLength) : 0f;
                var edge = Mathf.Clamp01((t - start) / 0.05f) * Mathf.Clamp01((start + length - t) / 0.15f);
                phase += pitch / SampleRate;
                var saw = 2f * (phase - Mathf.Floor(phase)) - 1f;
                lp1 += muffle * (saw - lp1);
                lp2 += muffle * (lp1 - lp2);
                hp = 0.97f * (hp + lp2 - prev);
                prev = lp2;
                data[i] += hp * env * edge * gain;
            }
        }

        /// <summary>
        /// Two people arguing behind a wall: a man and a woman cutting into each other, getting louder.
        /// A fixed 12 s loop, so the same fight repeats word for word (it is a TV, GDD).
        /// </summary>
        public static AudioClip CreateArgument()
        {
            const float length = 12f;
            var data = new float[(int)(SampleRate * length)];
            var rng = new System.Random(4242);
            var t = 0.2f;
            var man = true;
            while (t < length - 1f)
            {
                var phrase = 0.8f + (float)rng.NextDouble() * 1.6f;
                var anger = 0.6f + t / length * 0.8f;
                AddPhrase(data, rng, t, Mathf.Min(phrase, length - 0.3f - t), man ? 125f : 235f, 1.15f, 5f * anger, 0.07f);
                t += phrase - (rng.NextDouble() < 0.4 ? 0.3f : -0.25f); // sometimes they talk over each other
                man = !man;
            }
            for (var i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -0.9f, 0.9f);
            return Make("Argument", data);
        }

        /// <summary>An old kitchen radio through a door: tinny music, crackle and a news reader. 8 s loop.</summary>
        public static AudioClip CreateRadio()
        {
            const float length = 8f;
            var data = new float[(int)(SampleRate * length)];
            var rng = new System.Random(77);
            float[] chord = { 220f, 277.2f, 329.6f, 246.9f, 311.1f, 370f };
            float bp = 0f, hp = 0f, prev = 0f;
            for (var i = 0; i < data.Length; i++)
            {
                var t = (float)i / SampleRate;
                var bar = (int)(t / 1f) % 2;
                var music = 0f;
                for (var k = 0; k < 3; k++) music += Mathf.Sin(2f * Mathf.PI * chord[bar * 3 + k] * t);
                music *= 0.12f * (0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 4f * t)) * (t < 5f ? 1f : Mathf.Clamp01((5.5f - t) / 0.5f));
                var crackle = rng.NextDouble() < 0.002 ? (float)(rng.NextDouble() * 2 - 1) * 0.6f : (float)(rng.NextDouble() * 2 - 1) * 0.03f;
                bp += 0.35f * (music + crackle - bp);  // tinny speaker: cut the lows and highs
                hp = 0.95f * (hp + bp - prev);
                prev = bp;
                data[i] = hp;
            }
            AddPhrase(data, rng, 5.4f, 2.4f, 140f, 1.3f, 3f, 0.15f); // "...a nyní zprávy..."
            for (var i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -0.9f, 0.9f);
            return Make("Radio", data);
        }

        /// <summary>Fluorescent tube hum: 100 Hz mains buzz with harmonics, 1 s seamless loop.</summary>
        public static AudioClip CreateTubeHum()
        {
            var samples = SampleRate;
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var v = Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.25f
                        + Mathf.Sin(2f * Mathf.PI * 300f * t) * 0.15f
                        + Mathf.Sin(2f * Mathf.PI * 1200f * t) * 0.04f * (0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 3f * t));
                data[i] = v * 0.35f;
            }
            return Make("TubeHum", data);
        }

        /// <summary>Old bakelite staircase switch: a hard click (press = true) or the relay clack when the timer ends.</summary>
        public static AudioClip CreateSwitchClick(bool press)
        {
            var length = press ? 0.08f : 0.14f;
            var samples = (int)(SampleRate * length);
            var data = new float[samples];
            var rng = new System.Random(press ? 7 : 8);
            for (var i = 0; i < samples; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)(rng.NextDouble() * 2 - 1);
                var tone = Mathf.Sin(2f * Mathf.PI * (press ? 1800f : 420f) * t);
                var env = Mathf.Exp(-t * (press ? 90f : 45f));
                data[i] = Mathf.Clamp((noise * 0.7f + tone * 0.5f) * env * (press ? 0.9f : 0.6f), -1f, 1f);
            }
            return Make(press ? "SwitchClick" : "RelayClack", data);
        }

        private static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
