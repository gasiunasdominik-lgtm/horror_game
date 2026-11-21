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
            var flat = Mat("M_FlatInterior", new Color(0.25f, 0.17f, 0.10f), emission: new Color(0.55f, 0.36f, 0.16f));
            config.neighbourDoorPrefab = Save("NeighbourDoor", root =>
            {
                // Like a flat door, but the leaf hangs on a hinge and there is a lit hall behind it.
                const float w = 0.9f, dh = 2.0f;
                Box(root, "Hall", new Vector3(0, dh * 0.5f, 0.003f), new Vector3(w, dh, 0.002f), flat, collider: false);
                var hinge = new GameObject("Hinge").transform;
                hinge.SetParent(root.transform, false);
                hinge.localPosition = new Vector3(-w * 0.5f, 0f, 0.02f);
                Box(hinge.gameObject, "Leaf", new Vector3(w * 0.5f, dh * 0.5f, 0f), new Vector3(w, dh, 0.04f), door, collider: false);
                Box(hinge.gameObject, "Handle", new Vector3(w - 0.1f, 1.0f, 0.04f), new Vector3(0.12f, 0.03f, 0.04f), metal, collider: false);
                Box(hinge.gameObject, "Peephole", new Vector3(w * 0.5f, 1.55f, 0.025f), new Vector3(0.03f, 0.03f, 0.01f), metal, collider: false);
                Box(root, "FrameTop", new Vector3(0, dh + 0.04f, 0.03f), new Vector3(w + 0.16f, 0.08f, 0.06f), frame, collider: false);
                Box(root, "FrameLeft", new Vector3(-w * 0.5f - 0.04f, dh * 0.5f, 0.03f), new Vector3(0.08f, dh, 0.06f), frame, collider: false);
                Box(root, "FrameRight", new Vector3(w * 0.5f + 0.04f, dh * 0.5f, 0.03f), new Vector3(0.08f, dh, 0.06f), frame, collider: false);
                var col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0, dh * 0.5f, 0.04f);
                col.size = new Vector3(w, dh, 0.08f);

                var spillGo = new GameObject("Spill");
                spillGo.transform.SetParent(root.transform, false);
                spillGo.transform.localPosition = new Vector3(0.15f, 1.1f, 0.35f);
                var spill = spillGo.AddComponent<Light>();
                spill.type = LightType.Point;
                spill.color = new Color(1f, 0.75f, 0.45f);
                spill.range = 3.5f;
                spill.intensity = 2.2f;
                spill.shadows = LightShadows.None;
                spill.enabled = false;

                AudioSource Source(string name, float max, float volume, bool loop)
                {
                    var go = new GameObject(name);
                    go.transform.SetParent(root.transform, false);
                    go.transform.localPosition = new Vector3(0, 1.2f, 0.1f);
                    var src = go.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    src.loop = loop;
                    src.spatialBlend = 1f;
                    src.minDistance = 1f;
                    src.maxDistance = max;
                    src.volume = volume;
                    return src;
                }

                var n = root.AddComponent<Neighbour>();
                SetRef(n, "leaf", hinge);
                SetRef(n, "spill", spill);
                SetRef(n, "knock", Source("Knock", 15f, 1f, false));
                SetRef(n, "draft", Source("Draft", 8f, 0.5f, true));
            });

            config.playerDoorPrefab = Save("PlayerDoor", root =>
            {
                DoorPiece(root, playerDoor, frame, metal);
                root.AddComponent<HomeDoor>();
                // Doormat: the one thing that says "home" in the dark.
                Box(root, "Doormat", new Vector3(0, 0.01f, 0.45f), new Vector3(0.8f, 0.02f, 0.5f), door, collider: false);
            });

            var night = Mat("M_NightOutside", new Color(0.01f, 0.012f, 0.03f), emission: new Color(0.03f, 0.05f, 0.12f));
            var farLight = Mat("M_FarWindowLight", new Color(1f, 0.8f, 0.5f), emission: new Color(1.2f, 0.9f, 0.5f));
            config.windowPrefab = Save("Window", root =>
            {
                const float sill = 0.9f, w = 1.2f, wh = 1.2f;
                // Behind the glass: the night and a few lit windows of the panelák across the street.
                Box(root, "Outside", new Vector3(0, sill + wh * 0.5f, 0.002f), new Vector3(w, wh, 0.002f), night, collider: false);
                foreach (var (x, y) in new[] { (-0.42f, 0.35f), (-0.18f, 0.35f), (0.3f, 0.6f), (-0.3f, 0.85f), (0.12f, 0.85f) })
                    Box(root, "FarWindow", new Vector3(x, sill + y, 0.0035f), new Vector3(0.07f, 0.05f, 0.001f), farLight, collider: false);

                // Fixed right pane, left sash on a hinge at the left frame.
                Box(root, "GlassRight", new Vector3(w * 0.25f, sill + wh * 0.5f, 0.005f), new Vector3(w * 0.5f, wh, 0.01f), glass, collider: false);
                var sash = new GameObject("Sash");
                sash.transform.SetParent(root.transform, false);
                sash.transform.localPosition = new Vector3(-w * 0.5f, sill, 0.01f);
                var sashGlass = Box(sash, "Glass", new Vector3(w * 0.25f, wh * 0.5f, 0f), new Vector3(w * 0.5f, wh, 0.01f), glass, collider: false);
                Box(sash, "Frame", new Vector3(w * 0.25f, wh * 0.5f, 0.02f), new Vector3(w * 0.5f, 0.04f, 0.03f), windowFrame, collider: false);
                Box(sash, "Handle", new Vector3(w * 0.5f - 0.05f, wh * 0.5f, 0.04f), new Vector3(0.03f, 0.12f, 0.03f), metal, collider: false);

                var col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0, sill + wh * 0.5f, 0.05f);
                col.size = new Vector3(w, wh, 0.1f);

                var windGo = new GameObject("Wind");
                windGo.transform.SetParent(root.transform, false);
                windGo.transform.localPosition = new Vector3(0, sill + wh * 0.5f, 0.1f);
                var windSrc = windGo.AddComponent<AudioSource>();
                windSrc.playOnAwake = false;
                windSrc.loop = true;
                windSrc.spatialBlend = 1f;
                windSrc.minDistance = 1.5f;
                windSrc.maxDistance = 14f;
                windSrc.volume = 0.8f;

                var vent = root.AddComponent<VentWindow>();
                SetRef(vent, "sash", sash.transform);
                SetRef(vent, "wind", windSrc);
                Box(root, "FrameTop", new Vector3(0, sill + wh, 0.03f), new Vector3(w + 0.1f, 0.06f, 0.06f), windowFrame, collider: false);
                Box(root, "FrameBottom", new Vector3(0, sill, 0.06f), new Vector3(w + 0.2f, 0.05f, 0.12f), windowFrame, collider: false);
                Box(root, "FrameLeft", new Vector3(-w * 0.5f, sill + wh * 0.5f, 0.03f), new Vector3(0.06f, wh, 0.06f), windowFrame, collider: false);
                Box(root, "FrameRight", new Vector3(w * 0.5f, sill + wh * 0.5f, 0.03f), new Vector3(0.06f, wh, 0.06f), windowFrame, collider: false);
                Box(root, "FrameMid", new Vector3(0, sill + wh * 0.5f, 0.03f), new Vector3(0.05f, wh, 0.05f), windowFrame, collider: false);
            });

            var wood = Mat("M_WardrobeWood", new Color(0.42f, 0.30f, 0.19f));
            var tableTop = Mat("M_TableTop", new Color(0.36f, 0.27f, 0.20f));
            var cloth = Mat("M_TableCloth", new Color(0.45f, 0.18f, 0.16f));

            config.wardrobePrefab = Save("Wardrobe", root =>
            {
                // Old hallway wardrobe; the upper door halves are slatted so you can watch the corridor.
                const float w = 1.0f, d = 0.6f, h = 2.0f;
                Box(root, "Back", new Vector3(0, h * 0.5f, 0.02f), new Vector3(w, h, 0.04f), wood, collider: false);
                Box(root, "SideL", new Vector3(-w * 0.5f + 0.02f, h * 0.5f, d * 0.5f), new Vector3(0.04f, h, d), wood, collider: false);
                Box(root, "SideR", new Vector3(w * 0.5f - 0.02f, h * 0.5f, d * 0.5f), new Vector3(0.04f, h, d), wood, collider: false);
                Box(root, "Top", new Vector3(0, h - 0.02f, d * 0.5f), new Vector3(w, 0.04f, d), wood, collider: false);
                Box(root, "Plinth", new Vector3(0, 0.05f, d * 0.5f), new Vector3(w, 0.1f, d), wood, collider: false);
                foreach (var side in new[] { -1f, 1f })
                {
                    var x = side * 0.245f;
                    var name = side < 0 ? "L" : "R";
                    Box(root, $"Door{name}Panel", new Vector3(x, 0.7f, d - 0.02f), new Vector3(0.47f, 1.2f, 0.03f), wood, collider: false);
                    for (var i = 0; i < 5; i++)
                        Box(root, $"Door{name}Slat{i}", new Vector3(x, 1.33f + i * 0.1f, d - 0.02f), new Vector3(0.47f, 0.06f, 0.03f), wood, collider: false);
                    Box(root, $"Door{name}Rail", new Vector3(x, 1.88f, d - 0.02f), new Vector3(0.47f, 0.16f, 0.03f), wood, collider: false);
                    Box(root, $"Door{name}Knob", new Vector3(side * 0.05f, 1.05f, d + 0.01f), new Vector3(0.03f, 0.08f, 0.03f), metal, collider: false);
                }
                var col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0, h * 0.5f, d * 0.5f);
                col.size = new Vector3(w, h, d);
                Hiding(root, HidingKind.Wardrobe, view: new Vector3(0, 1.55f, 0.32f), exit: 1.1f, check: 1.0f, noise: 6f, yawLimit: 35f);
            });

            config.tablePrefab = Save("Table", root =>
            {
                // A table someone pushed into the corridor, with a cloth hanging over the front.
                const float w = 1.2f, d = 0.7f, h = 0.75f;
                Box(root, "Top", new Vector3(0, h - 0.02f, d * 0.5f), new Vector3(w, 0.04f, d), tableTop, collider: false);
                foreach (var x in new[] { -w * 0.5f + 0.05f, w * 0.5f - 0.05f })
                foreach (var z in new[] { 0.05f, d - 0.05f })
                    Box(root, "Leg", new Vector3(x, (h - 0.04f) * 0.5f, z), new Vector3(0.05f, h - 0.04f, 0.05f), tableTop, collider: false);
                Box(root, "Cloth", new Vector3(0, h - 0.15f, d + 0.01f), new Vector3(w + 0.02f, 0.28f, 0.01f), cloth, collider: false);
                Box(root, "ClothTop", new Vector3(0, h + 0.005f, d * 0.5f), new Vector3(w + 0.02f, 0.01f, d + 0.02f), cloth, collider: false);
                var col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0, h * 0.5f, d * 0.5f);
                col.size = new Vector3(w, h, d);
                Hiding(root, HidingKind.Table, view: new Vector3(0, 0.36f, 0.3f), exit: 1.2f, check: 1.05f, noise: 0f, yawLimit: 60f);
            });

            var liftMetal = Mat("M_LiftDoor", new Color(0.45f, 0.46f, 0.45f), smoothness: 0.45f);
            var liftLightOff = Mat("M_LiftLightOff", new Color(0.25f, 0.06f, 0.05f));
            var liftLightOn = Mat("M_LiftLightOn", new Color(1f, 0.2f, 0.1f), emission: new Color(4f, 0.6f, 0.3f));
            var paper = Mat("M_Paper", new Color(0.85f, 0.84f, 0.78f));
            config.elevatorPrefab = Save("Elevator", root =>
            {
                // Old two-leaf lift door with a call button and a handwritten "out of order" note.
                const float w = 1.0f, dh = 2.0f;
                Box(root, "LeafL", new Vector3(-w * 0.25f, dh * 0.5f, 0.03f), new Vector3(w * 0.5f - 0.01f, dh, 0.04f), liftMetal, collider: false);
                Box(root, "LeafR", new Vector3(w * 0.25f, dh * 0.5f, 0.03f), new Vector3(w * 0.5f - 0.01f, dh, 0.04f), liftMetal, collider: false);
                Box(root, "FrameTop", new Vector3(0, dh + 0.06f, 0.03f), new Vector3(w + 0.2f, 0.12f, 0.06f), frame, collider: false);
                Box(root, "FrameL", new Vector3(-w * 0.5f - 0.05f, dh * 0.5f, 0.03f), new Vector3(0.1f, dh, 0.06f), frame, collider: false);
                Box(root, "FrameR", new Vector3(w * 0.5f + 0.05f, dh * 0.5f, 0.03f), new Vector3(0.1f, dh, 0.06f), frame, collider: false);
                Box(root, "Panel", new Vector3(w * 0.5f + 0.3f, 1.2f, 0.01f), new Vector3(0.12f, 0.22f, 0.02f), frame, collider: false);
                var light = Box(root, "CallLight", new Vector3(w * 0.5f + 0.3f, 1.2f, 0.025f), new Vector3(0.05f, 0.05f, 0.02f), liftLightOff, collider: false);
                Box(root, "Note", new Vector3(-0.1f, 1.45f, 0.055f), new Vector3(0.3f, 0.21f, 0.005f), paper, collider: false);
                var col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0.15f, dh * 0.5f, 0.05f);
                col.size = new Vector3(w + 0.5f, dh, 0.1f);
                var door = root.AddComponent<ElevatorDoor>();
                SetRef(door, "callLight", light.GetComponent<Renderer>());
                SetRef(door, "lightOff", liftLightOff);
                SetRef(door, "lightOn", liftLightOn);
            });

            var street = Mat("M_StreetGlass", new Color(0.03f, 0.04f, 0.06f), emission: new Color(0.12f, 0.12f, 0.16f));
            config.entrancePrefab = Save("Entrance", root =>
            {
                // The house door you just came in through: metal frame, wired glass, street light behind.
                const float w = 0.95f, dh = 2.05f;
                Box(root, "Glass", new Vector3(0, 0.3f + (dh - 0.3f) * 0.5f, 0.02f), new Vector3(w, dh - 0.3f, 0.02f), street, collider: true);
                Box(root, "KickPlate", new Vector3(0, 0.15f, 0.03f), new Vector3(w, 0.3f, 0.04f), metal, collider: true);
                Box(root, "FrameTop", new Vector3(0, dh + 0.04f, 0.04f), new Vector3(w + 0.16f, 0.08f, 0.08f), frame, collider: false);
                Box(root, "FrameL", new Vector3(-w * 0.5f - 0.04f, dh * 0.5f, 0.04f), new Vector3(0.08f, dh, 0.08f), frame, collider: false);
                Box(root, "FrameR", new Vector3(w * 0.5f + 0.04f, dh * 0.5f, 0.04f), new Vector3(0.08f, dh, 0.08f), frame, collider: false);
                Box(root, "Bar", new Vector3(0, 1.0f, 0.07f), new Vector3(w - 0.2f, 0.04f, 0.04f), metal, collider: false);
            });

            // Two-flight stairwell, authored from the south wall's inner face (+Z north).
            // Up: west flight to the half-landing. Down: east flight to the half-landing below.
            const int steps = 8;
            const float rise = 0.17f, run = 0.28f, landing = 1.0f, flightW = 1.7f, fx = 0.95f;
            var half = rise * steps;
            var storey = half * 2f;
            var inner = size * 0.5f - t;
            var flightEnd = landing + run * steps;

            config.stairwellPrefab = Save("Stairwell", root =>
            {
                for (var i = 0; i < steps; i++)
                {
                    var top = rise * (i + 1);
                    Box(root, $"Up{i}", new Vector3(-fx, top * 0.5f, flightEnd - run * (i + 0.5f)), new Vector3(flightW, top, run), stairs, collider: true);
                }
                Box(root, "Landing", new Vector3(0, half - 0.05f, landing * 0.5f), new Vector3(inner * 2f, 0.1f, landing), stairs, collider: true);
                // The next floor's flight, seen from the landing (visual only: the floor swaps first).
                for (var k = 0; k < steps; k++)
                {
                    var top = half + rise * (k + 1);
                    Box(root, $"Above{k}", new Vector3(fx, top - 0.1f, landing + run * (k + 0.5f)), new Vector3(flightW, 0.2f, run), stairs, collider: false);
                }
                Box(root, "Rail", new Vector3(-0.12f, half + 0.9f, flightEnd * 0.5f + landing * 0.5f), new Vector3(0.04f, 0.04f, run * steps), metal, collider: false);
            });

            config.stairsDownPrefab = Save("StairsDown", root =>
            {
                for (var k = 0; k < steps - 1; k++)
                {
                    var top = -rise * (k + 1);
                    var height = top + half;
                    Box(root, $"Down{k}", new Vector3(fx, -half + height * 0.5f, flightEnd - run * (k + 0.5f)), new Vector3(flightW, height, run), stairs, collider: true);
                }
                Box(root, "Landing", new Vector3(0, -half - 0.05f, landing * 0.5f), new Vector3(inner * 2f, 0.1f, landing), stairs, collider: true);
                // The previous floor's flight below (visual only).
                for (var i = 0; i < steps; i++)
                {
                    var top = -storey + rise * (i + 1);
                    Box(root, $"Below{i}", new Vector3(-fx, top - 0.1f, flightEnd - run * (i + 0.5f)), new Vector3(flightW, 0.2f, run), stairs, collider: false);
                }
                Box(root, "BelowFloor", new Vector3(-fx, -storey - 0.05f, (flightEnd + inner * 2f) * 0.5f), new Vector3(flightW, 0.1f, inner * 2f - flightEnd), floor, collider: false);
            });

            config.stairwellFloorPrefab = Save("StairwellFloor", root =>
            {
                Box(root, "West", new Vector3(-fx, -0.05f, (landing + inner * 2f) * 0.5f), new Vector3(flightW, 0.1f, inner * 2f - landing), floor, collider: true);
                Box(root, "North", new Vector3(0.85f, -0.05f, (flightEnd + inner * 2f) * 0.5f), new Vector3(1.9f, 0.1f, inner * 2f - flightEnd), floor, collider: true);
                // Wall between the two flights, from the landing below to rail height above the one up.
                Box(root, "Spine", new Vector3(0, (-half + 2.4f) * 0.5f, (landing + flightEnd) * 0.5f), new Vector3(0.2f, half + 2.4f, run * steps), wallHigh, collider: true);
            });

            config.stairwellGroundPrefab = Save("StairwellGround", root =>
            {
                Box(root, "East", new Vector3(fx, -0.05f, (landing + flightEnd) * 0.5f), new Vector3(flightW, 0.1f, run * steps), floor, collider: true);
                Box(root, "South", new Vector3(0, -0.05f, landing * 0.5f), new Vector3(inner * 2f, 0.1f, landing), floor, collider: true);
            });

            config.stairwellTopRailPrefab = Save("StairwellTopRail", root =>
            {
                Box(root, "Rail", new Vector3(-fx, 0.5f, landing), new Vector3(flightW, 1.0f, 0.06f), wallLow, collider: true);
            });

            config.stairwellCeilingPrefab = Save("StairwellCeiling", root =>
            {
                const float split = 2.2f;
                Box(root, "Low", new Vector3(0, h + 0.05f, (split + inner * 2f) * 0.5f), new Vector3(inner * 2f, 0.1f, inner * 2f - split), ceiling, collider: true);
                Box(root, "High", new Vector3(0, half + h + 0.05f, split * 0.5f), new Vector3(inner * 2f, 0.1f, split), ceiling, collider: true);
            });

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static void Hiding(GameObject root, HidingKind kind, Vector3 view, float exit, float check, float noise, float yawLimit)
        {
            Transform Point(string name, Vector3 pos)
            {
                var t = new GameObject(name).transform;
                t.SetParent(root.transform, false);
                t.localPosition = pos;
                return t; // local +Z faces out of the spot, into the corridor
            }

            var spot = root.AddComponent<HidingSpot>();
            var so = new SerializedObject(spot);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("viewPoint").objectReferenceValue = Point("View", view);
            so.FindProperty("exitPoint").objectReferenceValue = Point("Exit", new Vector3(0, 0, exit));
            so.FindProperty("checkPoint").objectReferenceValue = Point("Check", new Vector3(0, 0, check));
            so.FindProperty("enterNoiseRadius").floatValue = noise;
            so.FindProperty("yawLimit").floatValue = yawLimit;
            so.ApplyModifiedPropertiesWithoutUndo();
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

        /// <summary>Shared material factory for other greybox builders.</summary>
        public static Material Material(string name, Color color, float smoothness = 0.15f, Color? emission = null)
        {
            EnsureFolder(MaterialFolder);
            return Mat(name, color, smoothness, emission);
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
