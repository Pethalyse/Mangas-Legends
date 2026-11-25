using System;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    public enum RigMove
    {
        None,
        StretchPunch,
        Barrage,
        GrabDash,
        Orbit
    }

    // Bone-driven animation for models that ship without clips: walk cycle, Luffy's rubber moves, flying creatures.
    // Runs in LateUpdate on top of whatever the bind pose or an Animator produced; display only.
    public sealed class ProceduralRig : MonoBehaviour
    {
        [Serializable]
        private sealed class Limb
        {
            public Transform Upper;
            public Transform Lower;
            public Transform End;
            [NonSerialized] public Quaternion UpperRest;
            [NonSerialized] public Quaternion LowerRest;
            [NonSerialized] public Vector3 UpperScaleRest;
            [NonSerialized] public Vector3 LowerScaleRest;
            [NonSerialized] public float Length;
            public bool Valid => Upper != null && Lower != null;
        }

        [SerializeField] private Unit _unit;
        [SerializeField] private Transform _root;
        [SerializeField] private bool _creature;
        [SerializeField] private Transform _hips;
        [SerializeField] private Limb _leftArm = new Limb();
        [SerializeField] private Limb _rightArm = new Limb();
        [SerializeField] private Limb _leftLeg = new Limb();
        [SerializeField] private Limb _rightLeg = new Limb();
        [SerializeField] private Transform[] _tail = new Transform[0];
        [SerializeField] private Transform _neck;
        [SerializeField] private float _walkSwing = 28f;
        [SerializeField] private float _stepsPerMeter = 0.9f;
        [SerializeField] private float _maxStretch = 6f;
        [SerializeField, Tooltip("A, Z, E, R")] private RigMove[] _spellMoves = new RigMove[4];
        [SerializeField, Tooltip("Basic attacks stretch the arm only while this status is on the unit (empty = always).")] private string _stretchAttackStatus;
        [SerializeField] private string _barrageStatus;
        [SerializeField] private float _hoverHeight = 0.6f;
        [SerializeField] private float _flapSpeed = 3f;
        [SerializeField] private float _flapAngle = 35f;
        [SerializeField, Tooltip("Moving faster than this (m/s, speed buffs) turns the walk into a run.")] private float _runSpeed = 4.2f;
        [SerializeField, Tooltip("Every Nth chained basic attack uses the left hand (Kirito's passive); 0 = never.")] private int _leftHandEvery;
        [SerializeField, Tooltip("Bone straightened by Posture Angle (a model whose rest pose is bent).")] private Transform _postureBone;
        [SerializeField] private float _postureAngle;
        [SerializeField, Tooltip("Model root: leans into the movement and whirls during an Orbit move.")] private Transform _model;

        private Vector3 _lastPosition;
        private float _phase;
        private float _speed;
        private int _lastAttackTick;
        private int _lastCastTick;
        private float _punchTime = -1f;
        private bool _punchRight = true;
        private Vector3 _punchTarget;
        private float _punchStretch;
        private Vector3 _hipsRest;
        private Quaternion[] _tailRest = new Quaternion[0];
        private Quaternion _neckRest;
        private bool _initialised;
        private const float OrbitSeconds = 1f;
        private Quaternion _postureRest;
        private Quaternion _modelRest;
        private Vector3 _modelPosition;
        private int _attackCount;
        private float _orbitTime = -1f;
        private Vector3 _orbitCenter;

        private void Start()
        {
            if (!_creature) { Relax(_leftArm); Relax(_rightArm); }
            foreach (Limb limb in new[] { _leftArm, _rightArm, _leftLeg, _rightLeg })
            {
                if (!limb.Valid) continue;
                limb.UpperRest = limb.Upper.localRotation;
                limb.LowerRest = limb.Lower.localRotation;
                limb.UpperScaleRest = limb.Upper.localScale;
                limb.LowerScaleRest = limb.Lower.localScale;
                Transform end = limb.End != null ? limb.End : limb.Lower;
                limb.Length = Vector3.Distance(limb.Upper.position, limb.Lower.position) + Vector3.Distance(limb.Lower.position, end.position);
            }
            if (_hips != null) _hipsRest = _hips.localPosition;
            if (_postureBone != null) _postureRest = _postureBone.localRotation;
            if (_model != null) { _modelRest = _model.localRotation; _modelPosition = _model.localPosition; }
            _tailRest = new Quaternion[_tail.Length];
            for (int i = 0; i < _tail.Length; i++) if (_tail[i] != null) _tailRest[i] = _tail[i].localRotation;
            if (_neck != null) _neckRest = _neck.localRotation;
            _lastPosition = transform.position;
            _initialised = true;
        }

        // Models come in T-pose: bring the arms down along the body before taking the rest pose.
        private static void Relax(Limb arm)
        {
            if (!arm.Valid) return;
            Transform end = arm.End != null ? arm.End : arm.Lower;
            Vector3 current = end.position - arm.Upper.position;
            if (current.sqrMagnitude < 0.0001f) return;
            Vector3 wanted = Vector3.Slerp(current.normalized, Vector3.down, 0.82f);
            arm.Upper.rotation = Quaternion.FromToRotation(current, wanted) * arm.Upper.rotation;
        }

        private void LateUpdate()
        {
            if (!_initialised || _unit == null || _unit.Object == null) return;
            Vector3 delta = transform.position - _lastPosition;
            delta.y = 0f;
            _lastPosition = transform.position;
            float measured = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            _speed = Mathf.Lerp(_speed, measured, 1f - Mathf.Exp(-10f * Time.deltaTime));
            _phase += _speed * Time.deltaTime * _stepsPerMeter * Mathf.PI;

            if (_creature) AnimateCreature();
            else AnimateBody();
        }

        // Walk cycle with a bounce, a lean and bent elbows; past the run threshold (speed buffs) the stride becomes a run.
        private void AnimateBody()
        {
            Reset(_leftArm); Reset(_rightArm); Reset(_leftLeg); Reset(_rightLeg);
            float walk = Mathf.Clamp01(_speed / 3f);
            float run = Mathf.Clamp01((_speed - _runSpeed) / 1.5f);
            float stride = _walkSwing * Mathf.Lerp(1f, 1.6f, run);
            float swing = Mathf.Sin(_phase) * stride * walk;
            Swing(_leftLeg, swing);
            Swing(_rightLeg, -swing);
            Swing(_leftArm, -swing * Mathf.Lerp(0.7f, 1.1f, run));
            Swing(_rightArm, swing * Mathf.Lerp(0.7f, 1.1f, run));
            float knee = Mathf.Lerp(35f, 70f, run) * walk;
            Bend(_leftLeg, Mathf.Max(0f, -Mathf.Sin(_phase)) * knee);
            Bend(_rightLeg, Mathf.Max(0f, Mathf.Sin(_phase)) * knee);
            Bend(_leftArm, -Mathf.Lerp(12f, 55f, run) * walk);
            Bend(_rightArm, -Mathf.Lerp(12f, 55f, run) * walk);
            if (_hips != null) _hips.localPosition = _hipsRest + _hips.parent.InverseTransformVector(Vector3.up * Mathf.Abs(Mathf.Sin(_phase)) * Mathf.Lerp(0.04f, 0.09f, run) * walk);
            if (_postureBone != null) _postureBone.localRotation = Quaternion.AngleAxis(_postureAngle, _postureBone.parent != null ? _postureBone.parent.InverseTransformDirection(_root.right) : _root.right) * _postureRest;
            if (_model != null) _model.localRotation = Quaternion.AngleAxis(Mathf.Lerp(3f, 12f, run) * walk, Vector3.right) * _modelRest;

            if (!_unit.IsAlive) return;
            WatchAttacks();
            WatchCasts();
            Barrage();
            GrabDash();
            Punch();
            Orbit();
        }

        private void WatchAttacks()
        {
            if (_unit.Attack == null || _unit.Attack.LastAttackTick == _lastAttackTick) return;
            bool chained = _unit.Attack.LastAttackTick - _lastAttackTick <= _unit.Runner.TickRate * 4;
            _lastAttackTick = _unit.Attack.LastAttackTick;
            _attackCount = chained ? _attackCount + 1 : 1;
            if (!_unit.TryResolve(_unit.Attack.Target, out Unit target)) return;
            bool stretch = string.IsNullOrEmpty(_stretchAttackStatus) || HasStatus(_stretchAttackStatus);
            bool left = _leftHandEvery > 0 && _attackCount % _leftHandEvery == 0;
            StartPunch(target.Position + Vector3.up, !left, stretch ? _maxStretch : Mathf.Min(1f, _maxStretch));
        }

        private void WatchCasts()
        {
            AbilityCaster caster = _unit.Caster;
            if (caster == null || caster.LastCastTick == _lastCastTick) return;
            _lastCastTick = caster.LastCastTick;
            int slot = caster.LastCastSlot - (int)CasterSlot.A;
            if (slot < 0 || slot >= _spellMoves.Length) return;
            if (_spellMoves[slot] == RigMove.Orbit) StartOrbit();
            if (_spellMoves[slot] != RigMove.StretchPunch) return;
            Vector3 aim = transform.position + transform.forward * 6f + Vector3.up;
            StartPunch(aim, true, _maxStretch);
        }

        // Starburst Stream: the model whirls around the closest enemy for a moment (display only; the unit stays put).
        private void StartOrbit()
        {
            Unit target = _unit.Services?.Units.Closest(_unit.Position, 8f, u => u.IsAlive && _unit.IsEnemyOf(u));
            if (target == null || _model == null) return;
            _orbitCenter = target.Position;
            _orbitTime = 0f;
        }

        private void Orbit()
        {
            if (_orbitTime < 0f || _model == null) return;
            _orbitTime += Time.deltaTime;
            if (_orbitTime >= OrbitSeconds)
            {
                _orbitTime = -1f;
                _model.localPosition = _modelPosition;
                return;
            }
            float angle = _orbitTime / OrbitSeconds * 720f;
            Vector3 around = _orbitCenter + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * 1.8f;
            _model.position = new Vector3(around.x, _model.position.y, around.z);
            _model.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_orbitCenter - around, Vector3.up).normalized, Vector3.up);
        }

        private void StartPunch(Vector3 target, bool right, float stretch)
        {
            _punchTime = 0f;
            _punchTarget = target;
            _punchRight = right;
            _punchStretch = stretch;
        }

        // The arm aims at the target and stretches out then back over a short punch.
        private void Punch()
        {
            if (_punchTime < 0f) return;
            _punchTime += Time.deltaTime;
            const float duration = 0.35f;
            float t = _punchTime / duration;
            if (t >= 1f)
            {
                _punchTime = -1f;
                return;
            }
            float reach = Mathf.Sin(t * Mathf.PI);
            Limb arm = _punchRight ? _rightArm : _leftArm;
            float distance = Vector3.Distance(arm.Upper != null ? arm.Upper.position : transform.position, _punchTarget);
            float stretch = Mathf.Min(_punchStretch, Mathf.Max(1f, distance / Mathf.Max(0.1f, arm.Length)));
            Reach(arm, _punchTarget, Mathf.Lerp(1f, stretch, reach), reach);
        }

        // Gatling: both arms take turns while the barrage status lasts.
        private void Barrage()
        {
            if (string.IsNullOrEmpty(_barrageStatus) || !HasStatus(_barrageStatus) || _unit.Attack == null) return;
            Vector3 target = _unit.TryResolve(_unit.Attack.Target, out Unit unit) ? unit.Position + Vector3.up : transform.position + transform.forward * 2f + Vector3.up;
            float cycle = Time.time * 7f;
            bool right = Mathf.FloorToInt(cycle) % 2 == 0;
            float reach = Mathf.Sin((cycle % 1f) * Mathf.PI);
            Reach(right ? _rightArm : _leftArm, target, Mathf.Lerp(1f, 2.5f, reach), reach);
        }

        // Rocket: the hand stays on the target while the body is pulled to it.
        private void GrabDash()
        {
            int slot = Array.IndexOf(_spellMoves, RigMove.GrabDash);
            if (slot < 0 || _unit.Motor == null || !_unit.Motor.IsDashing) return;
            Vector3 hand = _unit.Motor.DashEnd + Vector3.up;
            float distance = Vector3.Distance(_rightArm.Upper != null ? _rightArm.Upper.position : transform.position, hand);
            Reach(_rightArm, hand, Mathf.Clamp(distance / Mathf.Max(0.1f, _rightArm.Length), 1f, _maxStretch * 2f), 1f);
        }

        private void Reach(Limb arm, Vector3 target, float stretch, float weight)
        {
            if (!arm.Valid) return;
            Transform end = arm.End != null ? arm.End : arm.Lower;
            Vector3 current = end.position - arm.Upper.position;
            Vector3 wanted = target - arm.Upper.position;
            if (current.sqrMagnitude < 0.0001f || wanted.sqrMagnitude < 0.0001f) return;
            Quaternion aim = Quaternion.FromToRotation(current, wanted);
            arm.Upper.rotation = Quaternion.Slerp(arm.Upper.rotation, aim * arm.Upper.rotation, weight);
            arm.Lower.localRotation = Quaternion.Slerp(arm.Lower.localRotation, arm.LowerRest, weight);
            Stretch(arm.Upper, arm.Lower, arm.UpperScaleRest, stretch);
            Stretch(arm.Lower, end, arm.LowerScaleRest, stretch);
        }

        // Scales a bone along the axis that points to its child, which stretches the skinned mesh like rubber.
        private static void Stretch(Transform bone, Transform child, Vector3 rest, float factor)
        {
            if (child == null || child == bone) return;
            Vector3 axis = bone.InverseTransformDirection(child.position - bone.position).normalized;
            Vector3 abs = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
            Vector3 scale = rest;
            if (abs.x >= abs.y && abs.x >= abs.z) scale.x *= factor;
            else if (abs.y >= abs.z) scale.y *= factor;
            else scale.z *= factor;
            bone.localScale = scale;
        }

        private void Swing(Limb limb, float angle)
        {
            if (!limb.Valid) return;
            Vector3 axis = limb.Upper.parent != null ? limb.Upper.parent.InverseTransformDirection(_root.right) : _root.right;
            limb.Upper.localRotation = Quaternion.AngleAxis(angle, axis) * limb.UpperRest;
        }

        private void Bend(Limb limb, float angle)
        {
            if (!limb.Valid) return;
            Vector3 axis = limb.Lower.parent != null ? limb.Lower.parent.InverseTransformDirection(_root.right) : _root.right;
            limb.Lower.localRotation = Quaternion.AngleAxis(-angle, axis) * limb.LowerRest;
        }

        private static void Reset(Limb limb)
        {
            if (!limb.Valid) return;
            limb.Upper.localRotation = limb.UpperRest;
            limb.Lower.localRotation = limb.LowerRest;
            limb.Upper.localScale = limb.UpperScaleRest;
            limb.Lower.localScale = limb.LowerScaleRest;
        }

        // Birds: hover with a bob, flap the wings (arm chains), sway the tail, lunge the neck on attacks.
        private void AnimateCreature()
        {
            float flap = Mathf.Sin(Time.time * _flapSpeed * Mathf.PI * 2f) * _flapAngle;
            Vector3 forward = _root.forward;
            foreach (Limb wing in new[] { _leftArm, _rightArm })
            {
                if (!wing.Valid) continue;
                float side = wing == _leftArm ? 1f : -1f;
                Vector3 axis = wing.Upper.parent != null ? wing.Upper.parent.InverseTransformDirection(forward) : forward;
                wing.Upper.localRotation = Quaternion.AngleAxis(flap * side, axis) * wing.UpperRest;
                Vector3 lowerAxis = wing.Lower.parent != null ? wing.Lower.parent.InverseTransformDirection(forward) : forward;
                wing.Lower.localRotation = Quaternion.AngleAxis(flap * 0.6f * side, lowerAxis) * wing.LowerRest;
            }
            if (_hips != null) _hips.localPosition = _hipsRest + _hips.parent.InverseTransformVector(Vector3.up * (Mathf.Sin(Time.time * _flapSpeed * Mathf.PI * 2f + 1f) * _hoverHeight * 0.3f));
            for (int i = 0; i < _tail.Length; i++)
            {
                if (_tail[i] == null) continue;
                Vector3 axis = _tail[i].parent != null ? _tail[i].parent.InverseTransformDirection(Vector3.up) : Vector3.up;
                _tail[i].localRotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * 1.6f + i * 0.5f) * 8f, axis) * _tailRest[i];
            }
            if (_neck == null) return;
            if (_unit.Attack != null && _unit.Attack.LastAttackTick != _lastAttackTick)
            {
                _lastAttackTick = _unit.Attack.LastAttackTick;
                _punchTime = 0f;
            }
            float lunge = 0f;
            if (_punchTime >= 0f)
            {
                _punchTime += Time.deltaTime;
                lunge = Mathf.Sin(Mathf.Clamp01(_punchTime / 0.5f) * Mathf.PI) * 30f;
                if (_punchTime > 0.5f) _punchTime = -1f;
            }
            Vector3 neckAxis = _neck.parent != null ? _neck.parent.InverseTransformDirection(_root.right) : _root.right;
            _neck.localRotation = Quaternion.AngleAxis(lunge, neckAxis) * _neckRest;
        }

        private bool HasStatus(string id)
        {
            var statuses = _unit.Services?.Content.StatusEffects;
            if (statuses == null) return false;
            int index = statuses.IndexOf(id);
            return index >= 0 && _unit.Status.StacksOf(statuses[index]) > 0;
        }
    }
}
