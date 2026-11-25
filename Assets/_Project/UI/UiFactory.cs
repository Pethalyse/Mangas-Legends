using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Builds UGUI elements from code so that screens stay plain scripts without hand-made prefabs.
    public static class UiFactory
    {
        public static readonly Color Panel = new Color(0.06f, 0.07f, 0.1f, 0.88f);
        public static readonly Color Accent = new Color(0.95f, 0.75f, 0.3f);
        public static readonly Color ButtonColor = new Color(0.18f, 0.22f, 0.3f, 1f);
        public static readonly Color Health = new Color(0.25f, 0.8f, 0.3f);
        public static readonly Color Mana = new Color(0.25f, 0.5f, 1f);

        private static Font _font;

        public static Font Font => _font != null ? _font : _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Canvas Canvas(Transform parent, string name, int order)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static RectTransform Anchored(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            RectTransform rect = Rect(parent, name, anchor, anchor, Vector2.zero, Vector2.zero);
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static Image Image(Transform parent, string name, Color color)
        {
            RectTransform rect = Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Image Box(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = Anchored(parent, name, anchor, pivot, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Text(Transform parent, string name, string value, int size, TextAnchor alignment, Color color)
        {
            RectTransform rect = Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button Button(Transform parent, string name, string label, Action onClick, int fontSize = 22)
        {
            Image image = Image(parent, name, ButtonColor);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());
            Text(image.transform, "Label", label, fontSize, TextAnchor.MiddleCenter, Color.white);
            return button;
        }

        public static InputField Input(Transform parent, string name, string value, string placeholder)
        {
            Image image = Image(parent, name, new Color(0.12f, 0.14f, 0.2f));
            InputField field = image.gameObject.AddComponent<InputField>();
            Text text = Text(image.transform, "Text", value, 24, TextAnchor.MiddleLeft, Color.white);
            text.rectTransform.offsetMin = new Vector2(12f, 0f);
            text.supportRichText = false;
            Text hint = Text(image.transform, "Placeholder", placeholder, 24, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.35f));
            hint.rectTransform.offsetMin = new Vector2(12f, 0f);
            field.textComponent = text;
            field.placeholder = hint;
            field.text = value;
            return field;
        }

        public static void Place(Component component, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            RectTransform rect = (RectTransform)component.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Image Bar(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color, out Image fill)
        {
            Image background = Box(parent, name, anchor, pivot, position, size, new Color(0f, 0f, 0f, 0.7f));
            fill = Image(background.transform, "Fill", color);
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            return background;
        }

        public static void SetFill(Image fill, float ratio)
        {
            RectTransform rect = fill.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
