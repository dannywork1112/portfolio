using Unity.Burst;
using Unity.Entities;

#region System
[UpdateAfter(typeof(WeaponFireSystem))]
public partial struct ProjectileMoveSystem : ISystem
{
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        new ProjectileMoveJob
        {
            DeltaTime = deltaTime,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
        }.ScheduleParallel();
    }
}
#endregion

#region Job
public partial struct ProjectileMoveJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    [BurstCompile]
    public void Execute(ProjectileAspect projectileAspect, [ChunkIndexInQuery]int sortKey)
    {
        projectileAspect.LifeTime -= DeltaTime;

        if (projectileAspect.LifeTime <= 0)
            ECB.DestroyEntity(sortKey, projectileAspect.Entity);
        else
            projectileAspect.Position += projectileAspect.Forward * projectileAspect.ProjectileSpeed * DeltaTime;
    }
}
#endregion