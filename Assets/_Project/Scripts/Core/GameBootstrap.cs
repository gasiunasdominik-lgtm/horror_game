using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vchod.Core
{
    /// <summary>
    /// Starts a round: makes sure a seed exists before any system builds from it.
    /// Runs first (execution order) so every Awake/Start after it sees the seed.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (!SeedService.IsInitialized)
                SeedService.InitializeFromCommandLine();
        }

        private void Start()
        {
            EventBus.Publish(new RoundStarted { Seed = SeedService.Seed });
        }

        /// <summary>Reloads the game scene. Pass a seed to switch to it, otherwise the round repeats.</summary>
        public static void RestartRound(int? newSeed = null)
        {
            if (newSeed.HasValue) SeedService.Initialize(newSeed.Value);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
