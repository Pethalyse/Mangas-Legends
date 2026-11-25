using UnityEngine;

namespace MangaLegends.Presentation
{
    // Greys the screen while the local champion is dead, as the LoL client does.
    [RequireComponent(typeof(Camera))]
    public sealed class DeathGrayscale : MonoBehaviour
    {
        [SerializeField] private Shader _shader;
        [SerializeField, Range(0f, 1f)] private float _amount = 0.85f;

        private Material _material;

        public void Configure(Shader shader) => _shader = shader;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_shader == null) _shader = Shader.Find("Hidden/MangaLegends/Grayscale");
            if (_material == null && _shader != null) _material = new Material(_shader);
            if (_material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            _material.SetFloat("_Amount", _amount);
            Graphics.Blit(source, destination, _material);
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }
    }
}
