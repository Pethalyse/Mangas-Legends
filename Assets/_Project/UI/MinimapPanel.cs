using System;
using System.Collections.Generic;
using MangaLegends.Presentation;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // LoL minimap: static map picture, live dots for what the team sees, the camera frame;
    // left click moves the camera there, right click sends the champion there.
    public sealed class MinimapPanel
    {
        private const float Size = 300f;

        private readonly MatchView _view;
        private readonly GameObject _root;
        private readonly RawImage _picture;
        private readonly RectTransform _dots;
        private readonly List<Image> _pool = new List<Image>();
        private readonly Image[] _frame = new Image[4];
        private MatchServices _services;

        public MinimapPanel(Transform canvas, MatchView view)
        {
            _view = view;
            Image border = UiFactory.Box(canvas, "Minimap", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-8f, 8f), new Vector2(Size + 8f, Size + 8f), new Color(0.03f, 0.04f, 0.06f, 0.95f));
            _root = border.gameObject;
            RectTransform inner = UiFactory.Rect(border.transform, "Map", Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            _picture = inner.gameObject.AddComponent<RawImage>();
            _dots = UiFactory.Rect(inner, "Dots", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            for (int i = 0; i < _frame.Length; i++)
            {
                _frame[i] = UiFactory.Box(_dots, "Frame", Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one, new Color(1f, 1f, 1f, 0.85f));
                _frame[i].raycastTarget = false;
            }
            MinimapInput input = inner.gameObject.AddComponent<MinimapInput>();
            input.Clicked = OnClicked;
        }

        public void Refresh(MatchServices services, bool visible)
        {
            _services = services;
            _root.SetActive(visible);
            if (!visible) return;
            if (_picture.texture == null) _picture.texture = _view.Minimap;

            LocalView local = services.View;
            int used = 0;
            foreach (Unit unit in services.Units.All)
            {
                if (unit == null || unit.Object == null || unit.Definition == null || !unit.IsAlive || !local.Vision.IsVisible(unit)) continue;
                Image dot = Dot(used++);
                float size = unit.IsChampion ? 16f : unit.IsStructure ? 10f : unit.IsMonster ? 8f : 5f;
                dot.rectTransform.sizeDelta = new Vector2(size, size);
                dot.rectTransform.anchoredPosition = ToMinimap(unit.Position);
                dot.color = unit == local.LocalChampion ? new Color(1f, 0.9f, 0.3f)
                    : unit.Team == Team.Blue ? new Color(0.3f, 0.6f, 1f)
                    : unit.Team == Team.Red ? new Color(1f, 0.3f, 0.3f)
                    : new Color(0.9f, 0.8f, 0.35f);
                if (unit.IsChampion) dot.transform.SetAsLastSibling();
            }
            for (int i = used; i < _pool.Count; i++) _pool[i].gameObject.SetActive(false);
            DrawCameraFrame(local.Camera);
        }

        private Image Dot(int index)
        {
            if (index == _pool.Count)
            {
                Image created = UiFactory.Box(_dots, "Dot", Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one, Color.white);
                created.raycastTarget = false;
                _pool.Add(created);
            }
            _pool[index].gameObject.SetActive(true);
            return _pool[index];
        }

        private Vector2 ToMinimap(Vector3 world)
        {
            Vector2 map = _view.MapSize;
            Rect rect = _dots.rect;
            return new Vector2((world.x / map.x + 0.5f) * rect.width, (world.z / map.y + 0.5f) * rect.height);
        }

        private Vector3 ToWorld(Vector2 local01)
        {
            Vector2 map = _view.MapSize;
            return new Vector3((local01.x - 0.5f) * map.x, 0f, (local01.y - 0.5f) * map.y);
        }

        private void DrawCameraFrame(Camera camera)
        {
            if (camera == null) return;
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector2 corner in new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one })
            {
                Ray ray = camera.ViewportPointToRay(corner);
                if (!ground.Raycast(ray, out float distance)) continue;
                Vector2 point = ToMinimap(ray.GetPoint(distance));
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            if (min.x > max.x) return;
            Vector2 centre = (min + max) * 0.5f;
            Vector2 size = max - min;
            Line(_frame[0], new Vector2(centre.x, max.y), new Vector2(size.x, 2f));
            Line(_frame[1], new Vector2(centre.x, min.y), new Vector2(size.x, 2f));
            Line(_frame[2], new Vector2(min.x, centre.y), new Vector2(2f, size.y));
            Line(_frame[3], new Vector2(max.x, centre.y), new Vector2(2f, size.y));
        }

        private static void Line(Image line, Vector2 position, Vector2 size)
        {
            line.rectTransform.anchoredPosition = position;
            line.rectTransform.sizeDelta = size;
            line.transform.SetAsLastSibling();
        }

        private void OnClicked(Vector2 local01, PointerEventData.InputButton button)
        {
            if (_services == null) return;
            Vector3 world = ToWorld(local01);
            if (button == PointerEventData.InputButton.Right) _services.Commands.RequestMove(world);
            else if (button == PointerEventData.InputButton.Left) _services.View.CameraFocusRequest = world;
        }
    }

    // Turns pointer events on the minimap into normalised map coordinates.
    public sealed class MinimapInput : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Action<Vector2, PointerEventData.InputButton> Clicked;

        public void OnPointerDown(PointerEventData eventData) => Raise(eventData);

        public void OnDrag(PointerEventData eventData) => Raise(eventData);

        private void Raise(PointerEventData eventData)
        {
            RectTransform rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 local)) return;
            Vector2 normalised = new Vector2((local.x - rect.rect.xMin) / rect.rect.width, (local.y - rect.rect.yMin) / rect.rect.height);
            if (normalised.x < 0f || normalised.x > 1f || normalised.y < 0f || normalised.y > 1f) return;
            Clicked?.Invoke(normalised, eventData.button);
        }
    }
}
