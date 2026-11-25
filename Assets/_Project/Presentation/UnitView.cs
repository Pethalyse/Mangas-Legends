using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    [RequireComponent(typeof(Unit))]
    public sealed class UnitView : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] _body;
        [SerializeField] private Renderer _teamRing;
        [SerializeField] private Transform _model;
        [SerializeField] private Color _blue = new Color(0.2f, 0.45f, 1f);
        [SerializeField] private Color _red = new Color(1f, 0.25f, 0.2f);
        [SerializeField] private Color _neutral = new Color(0.45f, 0.8f, 0.3f);
        [SerializeField] private float _attackPunch = 0.15f;

        private Unit _unit;
        private MaterialPropertyBlock _block;
        private int _lastAttackTick;
        private float _punch;
        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _block = new MaterialPropertyBlock();
            if (_model != null) _baseScale = _model.localScale;
        }

        private void Update()
        {
            if (_unit.Object == null || _unit.Definition == null) return;
            Color team = _unit.Team == Team.Blue ? _blue : _unit.Team == Team.Red ? _red : _neutral;
            Color body = _unit.Definition.Tint;
            if (!_unit.IsAlive) body *= 0.25f;
            Paint(_body, body);
            if (_teamRing != null)
            {
                _teamRing.enabled = _unit.IsAlive;
                Paint(_teamRing, team);
            }
            AnimateAttack();
        }

        private void AnimateAttack()
        {
            if (_model == null) return;
            if (_unit.Attack != null && _unit.Attack.LastAttackTick != _lastAttackTick)
            {
                _lastAttackTick = _unit.Attack.LastAttackTick;
                _punch = 1f;
            }
            _punch = Mathf.MoveTowards(_punch, 0f, Time.deltaTime * 6f);
            float alive = _unit.IsAlive ? 1f : 0.35f;
            float squash = _unit.IsStructure ? 1f : alive;
            _model.localScale = new Vector3(_baseScale.x * (1f + _punch * _attackPunch), _baseScale.y * squash, _baseScale.z * (1f + _punch * _attackPunch));
        }

        private void Paint(Renderer[] renderers, Color color)
        {
            if (renderers == null) return;
            foreach (Renderer renderer in renderers) Paint(renderer, color);
        }

        private void Paint(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            renderer.GetPropertyBlock(_block);
            _block.SetColor(ColorId, color);
            _block.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(_block);
        }
    }
}
