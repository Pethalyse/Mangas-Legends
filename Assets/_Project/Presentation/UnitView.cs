using MangaLegends.Data.Definitions;
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
        [SerializeField, Tooltip("Optional per-team models (structures): the one matching the unit's team is shown.")] private GameObject _blueModel;
        [SerializeField] private GameObject _redModel;
        [SerializeField, Tooltip("Shown instead once the unit is destroyed.")] private GameObject _deadModel;
        [SerializeField, Tooltip("Clips of the legacy Animation found on the active model (structures).")] private string _aliveClip;
        [SerializeField] private string _deathClip;
        [SerializeField] private string _deadClip;
        [SerializeField] private string _respawnClip;
        [SerializeField] private Animator _animator;
        [SerializeField, Tooltip("Optional transformation: while a status with this id is on the unit, the form model replaces the main one.")] private string _formStatusId;
        [SerializeField] private GameObject _mainModel;
        [SerializeField] private GameObject _formModel;
        [SerializeField] private Animator _formAnimator;
        [SerializeField, Tooltip("Optional companion (a stand): shown beside the unit while a status with this id is on it.")] private string _companionStatusId;
        [SerializeField] private GameObject _companionModel;
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
            ShowTeamModel();
            ShowForm();
            ShowCompanion();
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
            if (ActiveAnimator != null) Animate();
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
            ActiveAnimator.SetFloat(SpeedId, _unit.IsAlive ? Mathf.Clamp01(_speed / _runBlendSpeed) : 0f);
            if (_unit.Attack == null || _unit.Attack.LastAttackTick == _lastAttackTick) return;
            _lastAttackTick = _unit.Attack.LastAttackTick;
            float attacksPerSecond = Mathf.Max(0.1f, _unit.Stats.Get(Core.Stats.StatType.AttackSpeed));
            ActiveAnimator.SetFloat(AttackSpeedId, Mathf.Max(0.5f, _attackAnimationSeconds * attacksPerSecond));
            ActiveAnimator.SetTrigger(AttackId);
        }

        private bool _inForm;
        private StatusEffectDefinition _formStatus;

        private Animator ActiveAnimator => _inForm && _formAnimator != null ? _formAnimator : _animator;

        private bool _companionShown;
        private StatusEffectDefinition _companionStatus;

        private void ShowForm()
        {
            if (_formModel == null || !HasStatus(_formStatusId, ref _formStatus, out bool form) || form == _inForm) return;
            _inForm = form;
            if (_mainModel != null) _mainModel.SetActive(!form);
            _formModel.SetActive(form);
            Smoke();
        }

        private void ShowCompanion()
        {
            if (_companionModel == null || !HasStatus(_companionStatusId, ref _companionStatus, out bool shown)) return;
            shown &= _shown;
            if (shown == _companionShown) return;
            _companionShown = shown;
            _companionModel.SetActive(shown);
            if (shown) Smoke();
        }

        private bool HasStatus(string statusId, ref StatusEffectDefinition status, out bool active)
        {
            active = false;
            if (string.IsNullOrEmpty(statusId) || _unit.Services == null) return false;
            if (status == null)
            {
                var statuses = _unit.Services.Content.StatusEffects;
                int index = statuses.IndexOf(statusId);
                if (index < 0) return false;
                status = statuses[index];
            }
            active = _unit.IsAlive && _unit.Status.StacksOf(status) > 0;
            return true;
        }

        private void Smoke()
        {
            var go = new GameObject("FormSmoke");
            go.transform.position = transform.position + Vector3.up;
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.duration = 0.5f; main.loop = false; main.startLifetime = 1.1f; main.startSpeed = 3f; main.startSize = 1.6f;
            main.startColor = new Color(0.85f, 0.85f, 0.85f, 0.7f); main.gravityModifier = -0.15f; main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = particles.emission; emission.rateOverTime = 0f; emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
            var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.8f;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SmokeMaterial;
            particles.Play();
        }

        private Material _smokeMaterial;
        private Material SmokeMaterial => _smokeMaterial != null ? _smokeMaterial : _smokeMaterial = new Material(Shader.Find("Sprites/Default"));

        private void ShowTeamModel()
        {
            PlayStateClips();
            if (_blueModel == null && _redModel == null && _deadModel == null) return;
            bool alive = _unit.IsAlive;
            if (_blueModel != null && _blueModel.activeSelf != (alive && _unit.Team != Team.Red)) _blueModel.SetActive(alive && _unit.Team != Team.Red);
            if (_redModel != null && _redModel.activeSelf != (alive && _unit.Team == Team.Red)) _redModel.SetActive(alive && _unit.Team == Team.Red);
            if (_deadModel != null && _deadModel.activeSelf != !alive) _deadModel.SetActive(!alive);
        }

        private Animation _legacy;
        private bool _legacyAlive;
        private bool _legacyStarted;

        private void PlayStateClips()
        {
            if (string.IsNullOrEmpty(_aliveClip)) return;
            GameObject model = _unit.Team == Team.Red && _redModel != null ? _redModel : _blueModel;
            Animation legacy = model != null ? model.GetComponentInChildren<Animation>(true) : null;
            if (legacy == null) return;
            bool alive = _unit.IsAlive;
            if (legacy == _legacy && _legacyStarted && alive == _legacyAlive) return;
            bool changed = _legacyStarted && legacy == _legacy;
            _legacy = legacy;
            _legacyAlive = alive;
            _legacyStarted = true;
            if (alive)
            {
                Loop(legacy, _aliveClip);
                if (changed && Has(legacy, _respawnClip)) { legacy.Play(_respawnClip); legacy.PlayQueued(_aliveClip); }
                else legacy.Play(_aliveClip);
            }
            else
            {
                if (Has(legacy, _deadClip)) Loop(legacy, _deadClip);
                string death = Has(legacy, _deathClip) ? _deathClip : _deadClip;
                if (changed && Has(legacy, death)) { Once(legacy, death); legacy.Play(death); if (Has(legacy, _deadClip) && death != _deadClip) legacy.PlayQueued(_deadClip); }
                else if (Has(legacy, death)) { Once(legacy, death); legacy.Play(death); legacy[death].normalizedTime = 1f; }
            }
        }

        private static bool Has(Animation legacy, string clip) => !string.IsNullOrEmpty(clip) && legacy[clip] != null;
        private static void Loop(Animation legacy, string clip) { if (Has(legacy, clip)) legacy[clip].wrapMode = WrapMode.Loop; }
        private static void Once(Animation legacy, string clip) { if (Has(legacy, clip)) legacy[clip].wrapMode = WrapMode.ClampForever; }

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
