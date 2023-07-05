using DG.Tweening;
using System;
using UnityAtoms;
using UnityEngine;

[System.Serializable]
public struct EnemyData
{
    public string ID;
    public float MaxHP;
    public float Damage;
    public float MovementSpeed;
    public float AttackSpeed;
    public float AttackRange;
}

public class Enemy : MonoBehaviour, IDamagable
{
    [SerializeField, ReadOnly] private EnemyData _data;
    [SerializeField] private Collider _hitCollider;
    [SerializeField] private Transform _animationTarget;

    private Transform _transform;
    private Transform _target;
    [SerializeField, ReadOnly] private float _curHP;
    [SerializeField, ReadOnly] private bool _isDead;

    [SerializeField, ReadOnly] private bool _attackable;
    [SerializeField, ReadOnly] private float _attackDelay;
    [SerializeField, ReadOnly] private float _stiffTime;

    private SimpleFSM _fsm;
    private bool _inAction;

    public bool IsDead => _isDead;

    private void Awake()
    {
        _transform = transform;
    }
    private void Update()
    {
        if (_target == null) return;

        UpdateAttackDelay();
        UpdateStiffTime();

        _fsm?.OnUpdate();
    }

    #region IDamagable
    public float CurHP => _curHP;
    public Action<float> OnDamageEvent;
    public Action OnDeadEvent;
    public void OnDamage(DamageInfo damageInfo)
    {
        _curHP -= damageInfo.Damage;
        if (_curHP > 0f)
        {
            OnDamageEvent?.Invoke(damageInfo.Damage);
            _stiffTime = damageInfo.StiffTime;
            _transform.DOKill();
            var knockbackVec = -_transform.forward * damageInfo.KnockbackDistance;
            _transform.DOMove(_transform.position + knockbackVec, 0.1f);
            _fsm.ChangeState(TypeFSM_State.HIT);
        }
        else
        {
            _curHP = 0f;
            _isDead = true;
            OnDeadEvent?.Invoke();
            _fsm.ChangeState(TypeFSM_State.DIE);
        }
    }
    #endregion

    public void Initialization(EnemyTableData tableData, Transform target, Vector3 spawnPos)
    {
        _target = target;

        _data = new EnemyData
        {
            ID = tableData.ID,
            Damage = tableData.Damage,
            MaxHP = tableData.MaxHP,
            MovementSpeed = tableData.MovementSpeed,
            AttackSpeed = tableData.AttackSpeed,
            AttackRange = tableData.AttackRange,
        };

        _curHP = _data.MaxHP;
        _attackable = true;
        _attackDelay = _data.AttackSpeed;
        _inAction = false;
        _stiffTime = 0f;

        var targetDirection = _target.position - spawnPos;
        targetDirection.y = 0f;
        var targetRotation = Quaternion.LookRotation(targetDirection);
        _transform.SetPositionAndRotation(spawnPos, targetRotation);

        _fsm = new SimpleFSM();
        _fsm.Initialization();

        #region FSM_Setup
        Func<bool> canIdle = () => !_inAction && TargetInAttackRange() && !_attackable;
        Func<bool> canChase = () => !_inAction && !TargetInAttackRange();
        Func<bool> canAttack = () => !_inAction && TargetInAttackRange() && _attackable;

        #region FSM_IDLE
        var idleState = _fsm.AddState(TypeFSM_State.IDLE, null, null, null);
        idleState.AddTransition(TypeFSM_State.CHASE, canChase);
        idleState.AddTransition(TypeFSM_State.ATTACK, canAttack);
        idleState.AddTransition(TypeFSM_State.HIT, () => false);
        idleState.AddTransition(TypeFSM_State.DIE, () => false);
        #endregion

        #region FSM_CHASE
        var chaseState = _fsm.AddState(TypeFSM_State.CHASE, null, UpdateMovement, null);
        chaseState.AddTransition(TypeFSM_State.IDLE, canIdle);
        chaseState.AddTransition(TypeFSM_State.ATTACK, canAttack);
        chaseState.AddTransition(TypeFSM_State.HIT, () => false);
        chaseState.AddTransition(TypeFSM_State.DIE, () => false);
        #endregion

        #region FSM_ATTACK
        var attackState = _fsm.AddState(TypeFSM_State.ATTACK, null, Attack, null);
        attackState.AddTransition(TypeFSM_State.IDLE, canIdle);
        attackState.AddTransition(TypeFSM_State.CHASE, canChase);
        attackState.AddTransition(TypeFSM_State.DIE, () => false);
        #endregion

        #region FSM_HIT
        var hitState = _fsm.AddState(TypeFSM_State.HIT, () =>
        {
            _animationTarget.DOKill();
            _inAction = false;
        }, OnHitEffect, null);
        hitState.AddTransition(TypeFSM_State.IDLE, canIdle);
        hitState.AddTransition(TypeFSM_State.CHASE, canChase);
        hitState.AddTransition(TypeFSM_State.ATTACK, canAttack);
        hitState.AddTransition(TypeFSM_State.DIE, () => false);
        #endregion
        #region FSM_DIE
        var dieState = _fsm.AddState(TypeFSM_State.DIE, OnDeadEffect, null, null);
        #endregion

        _fsm.ChangeState(TypeFSM_State.IDLE);
        #endregion
    }
    public void OnAttackEffect()
    {
        if (_animationTarget == null) return;

        _inAction = true;
        _attackable = false;
        var attackDistance = _data.AttackRange - 1f;
        var tween = _animationTarget.DOLocalMoveZ(attackDistance, 0.1f);
        tween.onComplete = () => 
        {
            var colliders = Physics.OverlapSphere(_animationTarget.position, 1f);
            for (int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (_hitCollider == collider) continue;
                if (!collider.CompareTag("Tower")) continue;

                var damagable = collider.GetComponentInParent<IDamagable>();
                if (damagable != null)
                {
                    damagable.OnDamage(new DamageInfo
                    {
                        Damage = _data.Damage,
                        StiffTime = 0f,
                        KnockbackDistance = 0f,
                    });
                    Debug.Log("공격");
                }
            }
            tween = _animationTarget.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);
            tween.onComplete = () =>
            {
                tween.Kill();
                _inAction = false;
            };
        };
    }
    public void OnHitEffect()
    {
        if (_animationTarget == null) return;
        if (_inAction) return;

        _animationTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _inAction = true;
        var tween = _animationTarget.DOLocalMoveZ(-0.5f, 0.1f);
        tween.onComplete = () =>
        {
            tween = _animationTarget.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);
            tween.onComplete = () =>
            {
                tween.Kill();
                _inAction = false;
            };
        };
    }
    public void OnDeadEffect()
    {
        if (_animationTarget == null) return;

        _animationTarget.DOKill();
        _animationTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _inAction = true;
        var tween = _animationTarget.DOScale(0f, 1f);
        tween.onComplete = () =>
        {
            tween.Kill();
            _inAction = false;
            Destroy(this.gameObject);
        };
    }
    private void UpdateMovement()
    {
        if (_stiffTime > 0f) return;

        //var distance = Vector3.Distance(_target.position, _transform.position);
        //if (distance <= 1.5f) return;

        var direction = _target.position - _transform.position;
        direction.y = 0f;
        direction.Normalize();

        //// 회전
        //var rotation = Quaternion.RotateTowards(_transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * _property.RotationSpeed);

        // 이동
        var position = _transform.position;
        position += _transform.forward * _data.MovementSpeed * Time.deltaTime;

        _transform.position = position;
        //_transform.SetPositionAndRotation(position, _transform.rotation);
    }
    private void UpdateAttackDelay()
    {
        if (_attackable) return;

        _attackDelay -= Time.deltaTime;

        if (_attackDelay <= 0f)
        {
            _attackDelay = 0f;
            _attackable = true;
        }
    }
    private void UpdateStiffTime()
    {
        if (_stiffTime <= 0f) return;

        _stiffTime -= Time.deltaTime;
    }
    private float GetTargetDistance()
    {
        return Vector3.Distance(_target.position, _transform.position);
    }
    private bool TargetInAttackRange()
    {
        return _data.AttackRange >= GetTargetDistance();
    }
    private void Attack()
    {
        if (_inAction) return;
        if (!_attackable) return;

        OnAttackEffect();

        _attackDelay = _data.AttackSpeed;
        Debug.Log($"{this.name} 공격");
    }
    private void Knockback(float distance)
    {
        if (_transform == null) return;
        if (_inAction) return;

        _transform.Translate(-_transform.position * distance);
    }
}
