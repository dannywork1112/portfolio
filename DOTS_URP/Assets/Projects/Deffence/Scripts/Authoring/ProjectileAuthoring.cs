using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#region Authoring
public class ProjectileAuthoring : MonoBehaviour
{
    public int ID;
    public float Speed;

    public class Baker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ProjectileComponent
            {
                ID = authoring.ID,
                Speed = authoring.Speed,
            });
        }
    }
}
#endregion

#region Aspect
public readonly partial struct ProjectileAspect : IAspect
{
    public readonly Entity Entity;

    private readonly RefRW<ProjectileComponent> _projectileComponent;
    private readonly RefRW<LocalTransform> _localTransform;
    private readonly RefRW<ProjectileLifeTimeComponent> _lifetimeComponent;

    public float ProjectileSpeed => _projectileComponent.ValueRO.Speed;
    public float3 Position
    {
        get => _localTransform.ValueRO.Position;
        set => _localTransform.ValueRW.Position = value;
    }
    public quaternion Rotation
    {
        get => _localTransform.ValueRO.Rotation;
        set => _localTransform.ValueRW.Rotation = value;
    }
    public float Scale
    {
        get => _localTransform.ValueRO.Scale;
        set => _localTransform.ValueRW.Scale = value;
    }
    public float3 Forward => _localTransform.ValueRO.Forward();
    public float LifeTime
    {
        get => _lifetimeComponent.ValueRO.LifeTime;
        set => _lifetimeComponent.ValueRW.LifeTime = value;
    }
}
#endregion