using UnityEngine;

namespace MangaLegends.Presentation
{
    // Renders the static map once from above; units are drawn live on top by the UI.
    public static class MinimapCapture
    {
        public static RenderTexture Render(Vector2 mapSize, int resolution, LayerMask excluded)
        {
            GameObject holder = new GameObject("MinimapCapture", typeof(Camera));
            Camera camera = holder.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = mapSize.y * 0.5f;
            camera.aspect = mapSize.x / mapSize.y;
            camera.transform.SetPositionAndRotation(new Vector3(0f, 150f, 0f), Quaternion.Euler(90f, 0f, 0f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.06f);
            camera.cullingMask = ~excluded;
            camera.farClipPlane = 300f;
            RenderTexture texture = new RenderTexture(resolution, resolution, 16) { name = "Minimap" };
            camera.targetTexture = texture;
            camera.Render();
            camera.targetTexture = null;
            Object.Destroy(holder);
            return texture;
        }
    }
}
