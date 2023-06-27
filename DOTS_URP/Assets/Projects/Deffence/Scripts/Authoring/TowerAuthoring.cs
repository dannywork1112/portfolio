using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#region Authoring
public class TowerAuthoring : MonoBehaviour
{
    public int ID;
    public float MaxHP;
    public float Delay;
    public float Damage;
    public int DefaultWeaponID;

    public class Baker : Baker<TowerAuthoring>
    {
        public override void Bake(TowerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<TowerTag>(entity);
            AddComponent(entity, new TowerComponent
            {
                ID = authoring.ID,
                MaxHP = authoring.MaxHP,
                Delay = authoring.Delay,
                Damage = authoring.Damage,
            });

            //// 생성대기 Weapon Buffer
            //AddBuffer<CreateReadyWeaponElementData>(entity);
            // 공격대기 Weapon Buffer
            AddBuffer<AttackReadyWeaponElementData>(entity);
            // 공격 타이머
            AddComponent(entity, new TowerAttackTimerComponent
            {
                Timer = -1f
            });
        }
    }
}
#endregion

#region Aspect
public readonly partial struct TowerAspect : IAspect
{
    public readonly Entity Entity;

    private readonly RefRO<TowerComponent> _towerComponent;
    private readonly RefRW<TowerAttackTimerComponent> _attackTimer;
    private readonly RefRW<LocalTransform> _localTransform;

    public float AttackDelay => _towerComponent.ValueRO.Delay;
    public float AttackCooldown
    {
        get => _attackTimer.ValueRO.Timer;
        set => _attackTimer.ValueRW.Timer = value;
    }

    public float3 Position
    {
        get => _localTransform.ValueRO.Position;
        set => _localTransform.ValueRW.Position = value;
    }

    public void Fire()
    {
        _attackTimer.ValueRW.Timer = _towerComponent.ValueRO.Delay;
    }
}
#endregion