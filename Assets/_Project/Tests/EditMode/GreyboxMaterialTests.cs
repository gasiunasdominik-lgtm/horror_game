using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Vchod.Tests
{
    /// <summary>Regression for B-001: emissive greybox materials must actually glow (light switch visible in the dark).</summary>
    public sealed class GreyboxMaterialTests
    {
        [TestCase("M_LightButton")]
        [TestCase("M_LampOn")]
        [TestCase("M_WindowNight")]
        public void SelfLitMaterial_IsUnlitAndBright(string name)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Materials/{name}.mat");
            Assert.IsNotNull(mat, $"{name} missing");
            Assert.AreEqual("Universal Render Pipeline/Unlit", mat.shader.name, $"{name} must not depend on scene lighting");
            Assert.Greater(mat.GetColor("_BaseColor").maxColorComponent, 0.3f, $"{name} too dark to read in the dark");
        }
    }
}
