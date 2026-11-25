using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // LoL-style tooltip above the HUD: title and key/cost/cooldown line, then the description; Shift shows the details.
    public sealed class TooltipPanel
    {
        private static readonly Color Border = new Color(0.78f, 0.67f, 0.43f, 0.9f);

        private readonly GameObject _root;
        private readonly RectTransform _rect;
        private readonly Text _title;
        private readonly Text _header;
        private readonly Text _body;
        private readonly Text _hint;

        public TooltipPanel(Transform canvas)
        {
            Image border = UiFactory.Box(canvas, "Tooltip", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-90f, 190f), new Vector2(640f, 200f), Border);
            _root = border.gameObject;
            _rect = border.rectTransform;
            Image inner = UiFactory.Image(border.transform, "Inner", new Color(0.02f, 0.05f, 0.07f, 1f));
            inner.rectTransform.offsetMin = new Vector2(2f, 2f);
            inner.rectTransform.offsetMax = new Vector2(-2f, -2f);
            _title = UiFactory.Text(inner.transform, "Title", "", 24, TextAnchor.UpperLeft, new Color(0.94f, 0.9f, 0.8f));
            UiFactory.Place(_title, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(420f, 30f));
            _header = UiFactory.Text(inner.transform, "Header", "", 16, TextAnchor.UpperRight, new Color(0.78f, 0.67f, 0.43f));
            UiFactory.Place(_header, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -14f), new Vector2(360f, 24f));
            _body = UiFactory.Text(inner.transform, "Body", "", 17, TextAnchor.UpperLeft, new Color(0.85f, 0.87f, 0.9f));
            _body.supportRichText = true;
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            UiFactory.Place(_body, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -48f), new Vector2(610f, 140f));
            _hint = UiFactory.Text(inner.transform, "Hint", "Maj : plus de détails", 13, TextAnchor.LowerRight, new Color(1f, 1f, 1f, 0.45f));
            UiFactory.Place(_hint, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-12f, 6f), new Vector2(240f, 18f));
            _root.SetActive(false);
        }

        public void Show(in TooltipText.Content content, bool detailed)
        {
            _root.SetActive(true);
            _title.text = content.Title;
            _header.text = content.Header;
            _body.text = content.Body;
            _hint.gameObject.SetActive(!detailed);
            float height = 64f + _body.preferredHeight + 20f;
            _rect.sizeDelta = new Vector2(640f, Mathf.Max(120f, height));
        }

        public void Hide()
        {
            if (_root.activeSelf) _root.SetActive(false);
        }
    }
}
