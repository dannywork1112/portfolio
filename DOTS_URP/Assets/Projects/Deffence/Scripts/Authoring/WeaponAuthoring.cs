using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#region Authoring
public class WeaponAuthoring : MonoBehaviour
{
    public int ID;
    public float Delay;
    public WeaponFireInfo FireInfo;
    public ProjectileAuthoring DefaultProjectile;

    class Baker : Baker<WeaponAuthoring>
    {
        public override void Bake(WeaponAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new WeaponComponent
            {
                ID = authoring.ID,
                Delay = authoring.Delay,
                FireInfo = authoring.FireInfo,
                ProjectileEntity = GetEntity(authoring.DefaultProjectile, TransformUsageFlags.None),
            });
            AddComponent(entity, new WeaponAttackTimerComponent
            {
                Value = authoring.Delay,
            });
            AddComponent(entity, new WeaponFireDirectionComponent
            {
                Direction = float3.zero,
            });
            AddBuffer<CreateReadyProjectileElementData>(entity);
        }
    }
}
#endregion

#region Aspect
public readonly partial struct WeaponAspect : IAspect
{
    public readonly Entity Entity;

    private readonly RefRO<WeaponComponent> _weaponComponent;
    private readonly RefRW<WeaponAttackTimerComponent> _attackTimer;
    private readonly RefRO<LocalTransform> _localTransform;
    private readonly RefRO<WeaponFireDirectionComponent> _fireDirectionComponent;

    public int WeaponID => _weaponComponent.ValueRO.ID;

    public float AttackDelay => _weaponComponent.ValueRO.Delay;
    public float AttackCooldown
    {
        get => _attackTimer.ValueRO.Value;
        set => _attackTimer.ValueRW.Value = value;
    }
    public float3 FireDirection => _fireDirectionComponent.ValueRO.Direction;

    public WeaponFireInfo FireInfo => _weaponComponent.ValueRO.FireInfo;
    public float3 MuzzleEffectPosition => _localTransform.ValueRO.Position + new float3(0f, 1f, 0f);

    public Entity DefaultProjectile => _weaponComponent.ValueRO.ProjectileEntity;

    public void Fire()
    {
        _attackTimer.ValueRW.Value = _weaponComponent.ValueRO.Delay;
    }
}
#endregion