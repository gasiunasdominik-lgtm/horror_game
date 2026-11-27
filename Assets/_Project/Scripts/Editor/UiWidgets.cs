using TMPro;
using UnityEditor;
using UnityEngine;
using Vchod.UI;
using static Vchod.EditorTools.SceneBuildUtil;

namespace Vchod.EditorTools
{
    /// <summary>
    /// Minimal uGUI widgets for menus, built from code (no sprites: flat panels in the game's
    /// dim palette). Keyboard and gamepad navigation work through the Selectable colours.
    /// </summary>
    public static class UiWidgets
    {
        public static readonly Color Text = new(0.92f, 0.9f, 0.85f, 1f);
        public static readonly Color TextDim = new(0.7f, 0.68f, 0.64f, 1f);
        private static readonly Color Normal = new(0.16f, 0.16f, 0.15f, 0.95f);
        private static readonly Color Selected = new(0.62f, 0.38f, 0.12f, 1f);
        private static readonly Color Pressed = new(0.8f, 0.5f, 0.15f, 1f);

        public static RectTransform Rect(GameObject go, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var r = GetOrAdd<RectTransform>(go);
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
            return r;
        }

        public static void Stretch(GameObject go)
        {
            var r = GetOrAdd<RectTransform>(go);
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        public static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Vector2 pos, Vector2 box,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = FindOrCreateChild(parent, name);
            Rect(go, new Vector2(0.5f, 0.5f), box, pos);
            var t = GetOrAdd<TextMeshProUGUI>(go);
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = Text;
            t.raycastTarget = false;
            return t;
        }

        private static void Colors(UnityEngine.UI.Selectable s)
        {
            var c = s.colors;
            c.normalColor = Normal;
            c.highlightedColor = Selected;
            c.selectedColor = Selected;
            c.pressedColor = Pressed;
            c.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            c.colorMultiplier = 1f;
            s.colors = c;
            var nav = s.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Automatic;
            s.navigation = nav;
        }

        public static UnityEngine.UI.Button Button(Transform parent, string name, string label, Vector2 pos, float width = 420f)
        {
            var go = FindOrCreateChild(parent, name);
            Rect(go, new Vector2(0.5f, 0.5f), new Vector2(width, 64f), pos);
            var img = GetOrAdd<UnityEngine.UI.Image>(go);
            img.color = Color.white;
            img.raycastTarget = true;
            var b = GetOrAdd<UnityEngine.UI.Button>(go);
            b.targetGraphic = img;
            Colors(b);
            var t = Label(go.transform, "Label", label, 30f, Vector2.zero, new Vector2(width, 64f));
            Stretch(t.gameObject);
            return b;
        }

        /// <summary>Label on the left, slider on the right, value text after it.</summary>
        public static UnityEngine.UI.Slider Slider(Transform parent, string name, string label, Vector2 pos, out TextMeshProUGUI value)
        {
            var row = FindOrCreateChild(parent, name);
            Rect(row, new Vector2(0.5f, 0.5f), new Vector2(900f, 60f), pos);
            Label(row.transform, "Label", label, 28f, new Vector2(-260f, 0f), new Vector2(360f, 60f), TextAlignmentOptions.Left);
            value = Label(row.transform, "Value", "", 26f, new Vector2(390f, 0f), new Vector2(120f, 60f), TextAlignmentOptions.Right);
            value.color = TextDim;

            var sGo = FindOrCreateChild(row.transform, "Slider");
            Rect(sGo, new Vector2(0.5f, 0.5f), new Vector2(380f, 28f), new Vector2(130f, 0f));
            var bg = FindOrCreateChild(sGo.transform, "Background");
            Stretch(bg);
            var bgImg = GetOrAdd<UnityEngine.UI.Image>(bg);
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            bgImg.raycastTarget = true;
            var fillArea = FindOrCreateChild(sGo.transform, "FillArea");
            Stretch(fillArea);
            var fill = FindOrCreateChild(fillArea.transform, "Fill");
            Stretch(fill);
            var fillImg = GetOrAdd<UnityEngine.UI.Image>(fill);
            fillImg.color = new Color(0.45f, 0.42f, 0.36f, 1f);
            fillImg.raycastTarget = false;
            var handleArea = FindOrCreateChild(sGo.transform, "HandleArea");
            Stretch(handleArea);
            var handle = FindOrCreateChild(handleArea.transform, "Handle");
            var hr = GetOrAdd<RectTransform>(handle);
            hr.sizeDelta = new Vector2(22f, 0f);
            hr.anchorMin = new Vector2(0f, 0f);
            hr.anchorMax = new Vector2(0f, 1f);
            var handleImg = GetOrAdd<UnityEngine.UI.Image>(handle);
            handleImg.color = Color.white;

            var s = GetOrAdd<UnityEngine.UI.Slider>(sGo);
            s.fillRect = fill.GetComponent<RectTransform>();
            s.handleRect = hr;
            s.targetGraphic = handleImg;
            s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            Colors(s);
            return s;
        }

        public static UnityEngine.UI.Toggle Toggle(Transform parent, string name, string label, Vector2 pos)
        {
            var row = FindOrCreateChild(parent, name);
            Rect(row, new Vector2(0.5f, 0.5f), new Vector2(900f, 60f), pos);
            Label(row.transform, "Label", label, 28f, new Vector2(-260f, 0f), new Vector2(360f, 60f), TextAlignmentOptions.Left);
            var box = FindOrCreateChild(row.transform, "Box");
            Rect(box, new Vector2(0.5f, 0.5f), new Vector2(40f, 40f), new Vector2(-40f + 0f, 0f));
            var boxImg = GetOrAdd<UnityEngine.UI.Image>(box);
            boxImg.color = Color.white;
            var check = FindOrCreateChild(box.transform, "Check");
            Rect(check, new Vector2(0.5f, 0.5f), new Vector2(24f, 24f), Vector2.zero);
            var checkImg = GetOrAdd<UnityEngine.UI.Image>(check);
            checkImg.color = Text;
            checkImg.raycastTarget = false;
            var t = GetOrAdd<UnityEngine.UI.Toggle>(box);
            t.targetGraphic = boxImg;
            t.graphic = checkImg;
            Colors(t);
            return t;
        }

        /// <summary>The shared settings panel (main menu and pause), hidden until opened.</summary>
        public static SettingsPanel SettingsPanel(GameObject canvas)
        {
            var root = FindOrCreateChild(canvas.transform, "Settings");
            Stretch(root);
            var dim = GetOrAdd<UnityEngine.UI.Image>(root);
            dim.color = new Color(0f, 0f, 0f, 0.9f);
            dim.raycastTarget = true;
            Label(root.transform, "Title", "Nastavení", 46f, new Vector2(0f, 260f), new Vector2(900f, 80f));
            var volume = Slider(root.transform, "Volume", "Hlasitost", new Vector2(0f, 140f), out var volumeValue);
            var sens = Slider(root.transform, "Sensitivity", "Citlivost pohledu", new Vector2(0f, 60f), out var sensValue);
            var invert = Toggle(root.transform, "InvertY", "Obrátit osu Y", new Vector2(0f, -20f));
            var full = Toggle(root.transform, "Fullscreen", "Celá obrazovka", new Vector2(0f, -100f));
            var back = Button(root.transform, "Back", "Zpět", new Vector2(0f, -220f));
            var hint = Label(root.transform, "Hint", "Esc  Zpět", 20f, new Vector2(0f, -300f), new Vector2(900f, 40f));
            hint.color = TextDim;

            var panel = GetOrAdd<SettingsPanel>(canvas);
            SetRef(panel, "root", root);
            SetRef(panel, "volume", volume);
            SetRef(panel, "sensitivity", sens);
            SetRef(panel, "invertY", invert);
            SetRef(panel, "fullscreen", full);
            SetRef(panel, "back", back);
            SetRef(panel, "volumeValue", volumeValue);
            SetRef(panel, "sensitivityValue", sensValue);
            root.SetActive(false);
            return panel;
        }
    }
}
