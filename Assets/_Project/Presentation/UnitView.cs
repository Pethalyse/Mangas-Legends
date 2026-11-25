using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    [RequireComponent(typeof(Unit))]
    public sealed class UnitView : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int AttackId = Animator.StringToHash("Attack");
        private static readonly int AttackSpeedId = Animator.StringToHash("AttackSpeed");

        [SerializeField] private Renderer[] _body;
        [SerializeField] private Renderer _teamRing;
        [SerializeField] private Transform _model;
        [SerializeField] private Color _blue = new Color(0.2f, 0.45f, 1f);
        [SerializeField] private Color _red = new Color(1f, 0.25f, 0.2f);
        [SerializeField] private Color _neutral = new Color(0.45f, 0.8f, 0.3f);
        [SerializeField] private float _attackPunch = 0.15f;
        [SerializeField] private Animator _animator;
        [SerializeField, Min(0.01f), Tooltip("Length of the attack clip, so that one clip plays per basic attack.")] private float _attackAnimationSeconds = 1f;
        [SerializeField, Min(0.1f), Tooltip("Ground speed (m/s) at which the run animation fully plays.")] private float _runBlendSpeed = 3.5f;
        [SerializeField, Tooltip("Off for textured models: only primitive placeholders take the unit tint.")] private bool _tintBody = true;

        private Unit _unit;
        private MaterialPropertyBlock _block;
        private int _lastAttackTick;
        private float _punch;
        private Vector3 _baseScale = Vector3.one;
        private bool _shown = true;
        private Vector3 _lastPosition;
        private float _speed;

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _block = new MaterialPropertyBlock();
            if (_model != null) _baseScale = _model.localScale;
        }

        private void Update()
        {
            if (_unit.Object == null || _unit.Definition == null) return;
            bool visible = _unit.Services == null || _unit.Services.View.Vision.IsVisible(_unit);
            SetVisible(visible);
            if (!visible) return;
            Color team = _unit.Team == Team.Blue ? _blue : _unit.Team == Team.Red ? _red : _neutral;
            Color body = _unit.Definition.Tint;
            if (!_unit.IsAlive) body *= 0.25f;
            if (_tintBody) Paint(_body, body);
            if (_teamRing != null)
            {
                _teamRing.enabled = _unit.IsAlive;
                Paint(_teamRing, team);
            }
            if (_animator != null) Animate();
            else AnimateAttack();
        }

        private void Animate()
        {
            Vector3 position = transform.position;
            Vector3 delta = position - _lastPosition;
            delta.y = 0f;
            _lastPosition = position;
            float measured = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            _speed = Mathf.Lerp(_speed, measured, 1f - Mathf.Exp(-12f * Time.deltaTime));
            _animator.SetFloat(SpeedId, _unit.IsAlive ? Mathf.Clamp01(_speed / _runBlendSpeed) : 0f);
            if (_unit.Attack == null || _unit.Attack.LastAttackTick == _lastAttackTick) return;
            _lastAttackTick = _unit.Attack.LastAttackTick;
            float attacksPerSecond = Mathf.Max(0.1f, _unit.Stats.Get(Core.Stats.StatType.AttackSpeed));
            _animator.SetFloat(AttackSpeedId, Mathf.Max(0.5f, _attackAnimationSeconds * attacksPerSecond));
            _animator.SetTrigger(AttackId);
        }

        private void SetVisible(bool visible)
        {
            if (visible == _shown) return;
            _shown = visible;
            if (_body != null)
                foreach (Renderer renderer in _body)
                    if (renderer != null) renderer.enabled = visible;
            if (_teamRing != null) _teamRing.enabled = visible;
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
