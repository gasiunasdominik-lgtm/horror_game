using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Vchod.World;
using static Vchod.EditorTools.SceneBuildUtil;

namespace Vchod.EditorTools
{
    /// <summary>
    /// Generates greybox materials and module prefabs from primitives. Re-runnable:
    /// overwrites the generated prefabs, keeps material assets (so hand tweaks to colours survive).
    /// The M2 art pass replaces these prefabs in HouseConfig.
    /// </summary>
    public static class GreyboxAssetBuilder
    {
        public const string MaterialFolder = "Assets/_Project/Art/Materials";
        public const string PrefabFolder = "Assets/_Project/Prefabs/Modules";

        public const string UnlitShaderName = "Universal Render Pipeline/Unlit";

        private static Shader Lit => Shader.Find("Universal Render Pipeline/Lit");
        private static Shader Unlit => Shader.Find(UnlitShaderName);

        [MenuItem("Vchod/Rebuild Greybox Modules")]
        public static HouseConfig BuildAll()
        {
            EnsureFolder(MaterialFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(GameSceneBuilder.DataFolder);

            var floor = Mat("M_Floor", new Color(0.30f, 0.28f, 0.26f));
            var wallLow = Mat("M_WallLower", new Color(0.33f, 0.42f, 0.34f));
            var wallHigh = Mat("M_WallUpper", new Color(0.72f, 0.71f, 0.65f));
            var ceiling = Mat("M_Ceiling", new Color(0.62f, 0.62f, 0.60f));
            var door = Mat("M_Door", new Color(0.34f, 0.23f, 0.15f));
            var playerDoor = Mat("M_PlayerDoor", new Color(0.42f, 0.10f, 0.08f));
            var frame = Mat("M_Frame", new Color(0.18f, 0.18f, 0.18f));
            var metal = Mat("M_Metal", new Color(0.55f, 0.55f, 0.52f), smoothness: 0.5f);
            var windowFrame = Mat("M_WindowFrame", new Color(0.80f, 0.80f, 0.77f));
            var glass = Mat("M_WindowNight", new Color(0.02f, 0.03f, 0.06f), emission: new Color(0.10f, 0.16f, 0.35f));
            var lampOn = Mat("M_LampOn", new Color(1f, 0.9f, 0.7f), emission: new Color(2.0f, 1.7f, 1.2f));
            var lampOff = Mat("M_LampOff", new Color(0.35f, 0.34f, 0.32f));
            var button = Mat("M_LightButton", new Color(0.9f, 0.4f, 0.1f), emission: new Color(5f, 2f, 0.4f));
            var stairs = Mat("M_Stairs", new Color(0.40f, 0.39f, 0.37f));

            var config = LoadOrCreateAsset<HouseConfig>($"{GameSceneBuilder.DataFolder}/HouseConfig.asset");
            var size = config.cellSize;
            var h = config.wallHeight;
            var t = config.wallThickness;

            config.floorPrefab = Save("Floor", root =>
                Box(root, "Slab", new Vector3(0, -0.05f, 0), new Vector3(size, 0.1f, size), floor, collider: true));

            config.ceilingPrefab = Save("Ceiling", root =>
                Box(root, "Slab", new Vector3(0, 0.05f, 0), new Vector3(size, 0.1f, size), ceiling, collider: true));

            const float paintLine = 1.4f;
            config.wallPrefab = Save("Wall", root =>
            {
                Box(root, "Lower", new Vector3(0, paintLine * 0.5f, -t * 0.5f), new Vector3(size, paintLine, t), wallLow, collider: false);
                Box(root, "Upper", new Vector3(0, paintLine + (h - paintLine) * 0.5f, -t * 0.5f), new Vector3(size, h - paintLine, t), wallHigh, collider: false);
                var col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0, h * 0.5f, -t * 0.5f);
                col.size = new Vector3(size, h, t);
            });

            config.lampPrefab = Save("Lamp", root =>
            {
                var bulb = Box(root, "Bulb", new Vector3(0, -0.05f, 0), new Vector3(0.4f, 0.08f, 0.4f), lampOff, collider: false);
                var lightGo = new GameObject("Light");
                lightGo.transform.SetParent(root.transform, false);
                lightGo.transform.localPosition = new Vector3(0, -0.3f, 0);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 7f;
                light.intensity = 2.2f;
                light.color = new Color(1f, 0.86f, 0.65f);
                light.shadows = LightShadows.None;
                light.enabled = false;
                var lamp = root.AddComponent<StairLamp>();
                SetRef(lamp, "lampLight", light);
                SetRef(lamp, "bulb", bulb.GetComponent<Renderer>());
                SetRef(lamp, "onMaterial", lampOn);
                SetRef(lamp, "offMaterial", lampOff);
            });

            config.lightButtonPrefab = Save("LightButton", root =>
            {
                Box(root, "Plate", new Vector3(0, 1.3f, 0.01f), new Vector3(0.12f, 0.16f, 0.02f), frame, collider: false);
                Box(root, "Button", new Vector3(0, 1.3f, 0.025f), new Vector3(0.06f, 0.08f, 0.02f), button, collider: false);
                var col = root.AddComponent<BoxCollider>();
                // Generous target: in the dark the player aims at a glowing dot.
                col.center = new Vector3(0, 1.3f, 0.08f);
                col.size = new Vector3(0.45f, 0.5f, 0.16f);
                root.AddComponent<LightButton>();
            });

            config.apartmentDoorPrefab = Save("ApartmentDoor", root => DoorPiece(root, door, frame, metal));
            config.playerDoorPrefab = Save("PlayerDoor", root =>
            {
                DoorPiece(root, playerDoor, frame, metal);
                // Doormat: the one thing that says "home" in the dark.
                Box(root, "Doormat", new Vector3(0, 0.01f, 0.45f), new Vector3(0.8f, 0.02f, 0.5f), door, collider: false);
            });

            config.windowPrefab = Save("Window", root =>
            {
                const float sill = 0.9f, w = 1.2f, wh = 1.2f;
                Box(root, "Glass", new Vector3(0, sill + wh * 0.5f, 0.005f), new Vector3(w, wh, 0.01f), glass, collider: false);
                Box(root, "FrameTop", new Vector3(0, sill + wh, 0.03f), new Vector3(w + 0.1f, 0.06f, 0.06f), windowFrame, collider: false);
                Box(root, "FrameBottom", new Vector3(0, sill, 0.06f), new Vector3(w + 0.2f, 0.05f, 0.12f), windowFrame, collider: false);
                Box(root, "FrameLeft", new Vector3(-w * 0.5f, sill + wh * 0.5f, 0.03f), new Vector3(0.06f, wh, 0.06f), windowFrame, collider: false);
                Box(root, "FrameRight", new Vector3(w * 0.5f, sill + wh * 0.5f, 0.03f), new Vector3(0.06f, wh, 0.06f), windowFrame, collider: false);
                Box(root, "FrameMid", new Vector3(0, sill + wh * 0.5f, 0.03f), new Vector3(0.05f, wh, 0.05f), windowFrame, collider: false);
            });

            config.stairwellPrefab = Save("Stairwell", root =>
            {
                // A flight going up against the back wall, left side; the landing stays clear.
                const int steps = 8;
                const float stepH = 0.17f, stepD = 0.28f, width = 1.3f, x = -1.05f;
                for (var i = 0; i < steps; i++)
                {
                    var height = stepH * (i + 1);
                    var depthFromWall = (steps - i) * stepD;
                    Box(root, $"Step{i}", new Vector3(x, height * 0.5f, depthFromWall - stepD * 0.5f),
                        new Vector3(width, height, stepD), stairs, collider: true);
                }
                Box(root, "Rail", new Vector3(x + width * 0.5f + 0.03f, 1.6f, steps * stepD * 0.5f),
                    new Vector3(0.04f, 0.04f, steps * stepD), metal, collider: false);
            });

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static void DoorPiece(GameObject root, Material leaf, Material frame, Material metal)
        {
            const float w = 0.9f, dh = 2.0f;
            Box(root, "Leaf", new Vector3(0, dh * 0.5f, 0.02f), new Vector3(w, dh, 0.04f), leaf, collider: true);
            Box(root, "FrameTop", new Vector3(0, dh + 0.04f, 0.03f), new Vector3(w + 0.16f, 0.08f, 0.06f), frame, collider: false);
            Box(root, "FrameLeft", new Vector3(-w * 0.5f - 0.04f, dh * 0.5f, 0.03f), new Vector3(0.08f, dh, 0.06f), frame, collider: false);
            Box(root, "FrameRight", new Vector3(w * 0.5f + 0.04f, dh * 0.5f, 0.03f), new Vector3(0.08f, dh, 0.06f), frame, collider: false);
            Box(root, "Handle", new Vector3(w * 0.5f - 0.1f, 1.0f, 0.06f), new Vector3(0.12f, 0.03f, 0.04f), metal, collider: false);
            Box(root, "Peephole", new Vector3(0, 1.55f, 0.045f), new Vector3(0.03f, 0.03f, 0.01f), metal, collider: false);
        }

        private static GameObject Box(GameObject root, string name, Vector3 center, Vector3 size, Material mat, bool collider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            var r = go.GetComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        private static GameObject Save(string name, System.Action<GameObject> build)
        {
            var root = new GameObject(name);
            try
            {
                build(root);
                return PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{name}.prefab");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Material Mat(string name, Color color, float smoothness = 0.15f, Color? emission = null)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(emission.HasValue ? Unlit : Lit) { name = name };
                AssetDatabase.CreateAsset(mat, path);
                mat.SetColor("_BaseColor", emission ?? color);
                mat.SetFloat("_Smoothness", smoothness);
            }

            // B-001: URP Lit dropped the _EMISSION keyword at runtime, so nothing glowed.
            // Self-lit parts use Unlit with an HDR colour instead: always visible, bloom picks it up.
            if (emission.HasValue && mat.shader != Unlit)
            {
                mat.shader = Unlit;
                mat.SetColor("_BaseColor", emission.Value);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
