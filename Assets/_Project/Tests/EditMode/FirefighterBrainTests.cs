using NUnit.Framework;
using UnityEngine;
using Vchod.Enemy;

namespace Vchod.Tests
{
    public sealed class FirefighterBrainTests
    {
        private const float Dt = 0.1f;
        private static readonly FirefighterSettings S = new();
        private static readonly Vector3 Somewhere = new(5f, 0f, 5f);

        private static FirefighterPerception Nothing => default;

        private static FirefighterPerception Sees(float distance = 5f) => new()
        {
            SeesPlayer = true,
            PlayerPosition = Somewhere,
            DistanceToPlayer = distance,
        };

        private static FirefighterPerception Hears(Vector3 at) => new() { HasStimulus = true, StimulusPosition = at };

        private static void Run(FirefighterBrain b, float seconds, FirefighterPerception p)
        {
            for (var t = 0f; t < seconds; t += Dt) b.Tick(Dt, p);
        }

        [Test]
        public void StartsPatrollingAndWandering()
        {
            var b = new FirefighterBrain(S);
            Assert.AreEqual(FirefighterState.Patrol, b.State);
            Assert.AreEqual(FirefighterGoal.Wander, b.Goal);
        }

        [Test]
        public void NoiseOrLight_LeadsToInvestigate_TowardsIt()
        {
            var b = new FirefighterBrain(S);
            Assert.IsTrue(b.Tick(Dt, Hears(Somewhere)));
            Assert.AreEqual(FirefighterState.Investigate, b.State);
            Assert.AreEqual(FirefighterGoal.GoTo, b.Goal);
            Assert.AreEqual(Somewhere, b.Destination);
        }

        [Test]
        public void Investigate_NothingThere_LooksAroundThenPatrols()
        {
            var b = new FirefighterBrain(S);
            b.Tick(Dt, Hears(Somewhere));
            b.Tick(Dt, new FirefighterPerception { ArrivedAtDestination = true });
            Assert.AreEqual(FirefighterGoal.Hold, b.Goal);
            Run(b, S.investigateLookTime + 0.5f, Nothing);
            Assert.AreEqual(FirefighterState.Patrol, b.State);
        }

        [Test]
        public void Sight_StartsChase_FromAnyState()
        {
            foreach (var state in new[] { FirefighterState.Patrol, FirefighterState.Investigate, FirefighterState.Call, FirefighterState.Search })
            {
                var b = new FirefighterBrain(S);
                b.Force(state, Vector3.zero);
                b.Tick(Dt, Sees());
                Assert.AreEqual(FirefighterState.Chase, b.State, $"from {state}");
                Assert.AreEqual(Somewhere, b.Destination);
            }
        }

        [Test]
        public void LosingSight_CallsAfterGrace_ThenSearches_ThenPatrols()
        {
            var b = new FirefighterBrain(S);
            b.Tick(Dt, Sees());
            Run(b, S.loseSightTime - 0.3f, Nothing);
            Assert.AreEqual(FirefighterState.Chase, b.State, "keeps chasing briefly after losing sight");
            Run(b, 0.5f, Nothing);
            Assert.AreEqual(FirefighterState.Call, b.State);
            Assert.AreEqual(0f, b.Speed);
            Run(b, S.callDuration + 0.2f, Nothing);
            Assert.AreEqual(FirefighterState.Search, b.State);
            Assert.AreEqual(FirefighterGoal.SearchAround, b.Goal);
            Assert.AreEqual(Somewhere, b.Destination, "searches around where he last saw the player");
            Run(b, S.searchDuration + 0.2f, Nothing);
            Assert.AreEqual(FirefighterState.Patrol, b.State);
        }

        [Test]
        public void Search_NoiseRedirectsToInvestigate()
        {
            var b = new FirefighterBrain(S);
            b.Force(FirefighterState.Search, Vector3.zero);
            b.Tick(Dt, Hears(Somewhere));
            Assert.AreEqual(FirefighterState.Investigate, b.State);
        }

        [Test]
        public void CloseDuringChase_CatchesPlayer_AndStops()
        {
            var b = new FirefighterBrain(S);
            b.Tick(Dt, Sees(5f));
            b.Tick(Dt, Sees(S.catchRadius - 0.1f));
            Assert.IsTrue(b.HasCaughtPlayer);
            Assert.AreEqual(FirefighterGoal.Hold, b.Goal);
            Assert.IsFalse(b.Tick(Dt, Hears(Vector3.zero)), "nothing happens after the catch");
        }

        [Test]
        public void GodMode_NeverCatches()
        {
            var b = new FirefighterBrain(S) { CanCatch = false };
            b.Tick(Dt, Sees(5f));
            Run(b, 2f, Sees(0.2f));
            Assert.IsFalse(b.HasCaughtPlayer);
            Assert.AreEqual(FirefighterState.Chase, b.State);
        }

        [Test]
        public void ChaseIsSlowerThanSprint_SoEscapeIsPossible()
        {
            var player = ScriptableObject.CreateInstance<Player.PlayerConfig>();
            Assert.Less(S.chaseSpeed, player.sprintSpeed);
            Assert.Greater(S.chaseSpeed, player.walkSpeed, "walking away is not enough");
            Object.DestroyImmediate(player);
        }
    }
}
