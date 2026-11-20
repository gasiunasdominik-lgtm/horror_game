using System;
using UnityEngine;

namespace Vchod.Core
{
    /// <summary>
    /// The single source of randomness for game logic. Every consumer derives its own
    /// named stream from the round seed, so adding a new consumer never shifts the
    /// numbers another one gets (keeps old seeds reproducible).
    /// </summary>
    public static class SeedService
    {
        public const string SeedArgument = "-seed";
        public const int MaxRandomSeed = 999_999;

        public static int Seed { get; private set; }
        public static bool IsInitialized { get; private set; }

        public static void Initialize(int seed)
        {
            Seed = seed;
            IsInitialized = true;
        }

        /// <summary>Uses <c>-seed N</c> from the command line when present, otherwise a fresh random seed.</summary>
        public static void InitializeFromCommandLine()
        {
            Initialize(TryParseSeedArgument(Environment.GetCommandLineArgs(), out var seed) ? seed : NewRandomSeed());
        }

        public static bool TryParseSeedArgument(string[] args, out int seed)
        {
            seed = 0;
            if (args == null) return false;
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], SeedArgument, StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(args[i + 1], out seed))
                    return true;
            }
            return false;
        }

        /// <summary>Small positive seeds are easy to read aloud and type into the overlay.</summary>
        public static int NewRandomSeed()
        {
            return 1 + (int)((uint)Guid.NewGuid().GetHashCode() % MaxRandomSeed);
        }

        public static System.Random CreateStream(string stream, int index = 0)
        {
            return new System.Random(DeriveSeed(Seed, stream, index));
        }

        /// <summary>Stable across platforms and runtimes (FNV-1a), unlike string.GetHashCode.</summary>
        public static int DeriveSeed(int seed, string stream, int index)
        {
            unchecked
            {
                var hash = 2166136261u;
                hash = Mix(hash, (uint)seed);
                if (stream != null)
                    foreach (var c in stream) hash = Mix(hash, c);
                hash = Mix(hash, (uint)index);
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        private static uint Mix(uint hash, uint value)
        {
            unchecked
            {
                for (var i = 0; i < 4; i++)
                {
                    hash ^= (value >> (i * 8)) & 0xFF;
                    hash *= 16777619u;
                }
                return hash;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Seed = 0;
            IsInitialized = false;
        }
    }
}
