using NUnit.Framework;
using UnityEngine;
using Vchod.Systems;

namespace Vchod.Tests
{
    public sealed class SettingsTests
    {
        [Test]
        public void Look_ScalesBySensitivity_AndInvertsOnlyY()
        {
            var v = GameSettings.ApplyLook(new Vector2(2f, 3f), 0.5f, invertY: false);
            Assert.AreEqual(new Vector2(1f, 1.5f), v);
            var inv = GameSettings.ApplyLook(new Vector2(2f, 3f), 1f, invertY: true);
            Assert.AreEqual(new Vector2(2f, -3f), inv);
        }

        [Test]
        public void Sensitivity_IsClampedToTheSliderRange()
        {
            var before = GameSettings.LookSensitivity;
            try
            {
                GameSettings.LookSensitivity = 99f;
                Assert.AreEqual(GameSettings.MaxSensitivity, GameSettings.LookSensitivity);
                GameSettings.LookSensitivity = 0f;
                Assert.AreEqual(GameSettings.MinSensitivity, GameSettings.LookSensitivity);
            }
            finally { GameSettings.LookSensitivity = before; }
        }
    }
}
