using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

#region System
[UpdateAfter(typeof(GameInitializeSystem))]
public partial struct TowerAttackTimerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameConfigComponent>();
    }

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var gameConfig = SystemAPI.GetSingleton<GameConfigComponent>();
        var timer = SystemAPI.GetSingletonRW<TowerAttackTimerComponent>();

        foreach ((var towerTimer, var towerEntity) in SystemAPI.Query<RefRW<TowerAttackTimerComponent>>().WithEntityAccess())
        {
            if (towerTimer.ValueRO.Timer > 0f)
            {
                timer.ValueRW.Timer -= deltaTime;
            }
            else
            {
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                var hasAttackReadyTag = state.EntityManager.HasComponent<TowerAttackReadyTag>(towerEntity);

                // 방향 입력값 없음 (공격x)
                if (gameConfig.AttackDirection.Equals(float3.zero))
                {
                    if (hasAttackReadyTag)
                        ecb.RemoveComponent<TowerAttackReadyTag>(towerEntity);
                }
                else
                {
                    if (!hasAttackReadyTag)
                        ecb.AddComponent<TowerAttackReadyTag>(towerEntity);
                }
            }
        }
    }
}
#endregion

#region Job
#endregion