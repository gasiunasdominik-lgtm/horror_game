using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Vchod.Enemy;
using static Vchod.EditorTools.SceneBuildUtil;

namespace Vchod.EditorTools
{
    /// <summary>
    /// Placeholder firefighter from primitives: dark turnout gear, reflective stripes that
    /// catch the eye in the dark, helmet, full-face mask, air cylinder. Replaced in the art pass.
    /// </summary>
    public static class FirefighterAssetBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Firefighter.prefab";

        public static GameObject Build()
        {
            EnsureFolder("Assets/_Project/Prefabs");
            var gear = GreyboxAssetBuilder.Material("M_Turnout", new Color(0.10f, 0.11f, 0.14f));
            var stripe = GreyboxAssetBuilder.Material("M_ReflectiveStripe", new Color(0.55f, 0.65f, 0.25f), emission: new Color(0.55f, 0.65f, 0.25f));
            var helmet = GreyboxAssetBuilder.Material("M_Helmet", new Color(0.75f, 0.76f, 0.78f), smoothness: 0.6f);
            var mask = GreyboxAssetBuilder.Material("M_Mask", new Color(0.03f, 0.03f, 0.035f), smoothness: 0.8f);
            var tank = GreyboxAssetBuilder.Material("M_AirTank", new Color(0.6f, 0.6f, 0.58f), smoothness: 0.5f);

            var root = new GameObject("Firefighter");
            try
            {
                Part(root, PrimitiveType.Capsule, "Body", new Vector3(0, 0.9f, 0), new Vector3(0.6f, 0.9f, 0.45f), gear);
                Part(root, PrimitiveType.Cube, "StripeChest", new Vector3(0, 1.25f, 0.2f), new Vector3(0.56f, 0.05f, 0.08f), stripe);
                Part(root, PrimitiveType.Cube, "StripeWaist", new Vector3(0, 0.85f, 0.2f), new Vector3(0.58f, 0.05f, 0.08f), stripe);
                Part(root, PrimitiveType.Cube, "StripeLegs", new Vector3(0, 0.35f, 0.17f), new Vector3(0.5f, 0.05f, 0.08f), stripe);
                Part(root, PrimitiveType.Sphere, "Head", new Vector3(0, 1.7f, 0), new Vector3(0.3f, 0.32f, 0.3f), gear);
                Part(root, PrimitiveType.Sphere, "Helmet", new Vector3(0, 1.8f, -0.01f), new Vector3(0.36f, 0.26f, 0.4f), helmet);
                Part(root, PrimitiveType.Cube, "Mask", new Vector3(0, 1.66f, 0.13f), new Vector3(0.24f, 0.22f, 0.08f), mask);
                Part(root, PrimitiveType.Cylinder, "Regulator", new Vector3(0, 1.57f, 0.19f), new Vector3(0.08f, 0.03f, 0.08f), tank, Quaternion.Euler(90, 0, 0));
                Part(root, PrimitiveType.Cylinder, "AirTank", new Vector3(0, 1.2f, -0.3f), new Vector3(0.18f, 0.3f, 0.18f), tank);

                var eyes = new GameObject("Eyes");
                eyes.transform.SetParent(root.transform, false);
                eyes.transform.localPosition = new Vector3(0, 1.68f, 0.2f);

                var col = root.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0, 0.95f, 0);
                col.height = 1.9f;
                col.radius = 0.32f;

                var agent = root.AddComponent<NavMeshAgent>();
                agent.radius = 0.35f;
                agent.height = 1.9f;
                agent.speed = 1.2f;
                agent.angularSpeed = 360f;
                agent.acceleration = 8f;
                agent.stoppingDistance = 0.3f;
                agent.autoBraking = true;
                agent.enabled = false; // enabled by the controller once the floor's NavMesh exists

                root.AddComponent<FirefighterController>();
                return PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void Part(GameObject root, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot ?? Quaternion.identity;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }
    }
}
