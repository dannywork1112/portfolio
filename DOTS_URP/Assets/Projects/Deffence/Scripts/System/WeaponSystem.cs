using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

#region System
//[UpdateAfter(typeof(GameInitializeSystem))]
//// ���⸦ �����ϴ� �ý���
//public partial struct WeaponCreateSystem : ISystem
//{
//    [BurstCompile]
//    private void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<CreateReadyWeaponElementData>();
//    }

//    [BurstCompile]
//    private void OnUpdate(ref SystemState state)
//    {
//        var createReadyWeapons = SystemAPI.GetSingletonBuffer<CreateReadyWeaponElementData>();
//        while (createReadyWeapons.Length > 0)
//        {
//            var createWeapon = createReadyWeapons[0];
//            foreach ((var weaponComponent, var weaponEntity) in SystemAPI.Query<WeaponComponent>().WithAll<Prefab>().WithEntityAccess())
//            {
//                if (createWeapon.WeaponID != weaponComponent.ID) continue;

//                var entity = state.EntityManager.Instantiate(weaponEntity);
//                state.EntityManager.SetComponentData(entity, new WeaponComponent
//                {
//                    ID = weaponComponent.ID,
//                    Delay = weaponComponent.Delay,
//                    FireInfo = weaponComponent.FireInfo,
//                    ProjectileEntity = weaponComponent.ProjectileEntity,
//                });
//            }
//            createReadyWeapons.RemoveAt(0);
//        }
//    }
//}

//[UpdateAfter(typeof(WeaponCreateSystem))]
[UpdateAfter(typeof(GameInitializeSystem))]
// Ÿ�̸� �ý���
public partial struct WeaponTimerSystem : ISystem
{
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var fireReadyWeaponBuffer = SystemAPI.GetSingletonBuffer<AttackReadyWeaponElementData>();
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        foreach (var weaponAspect in SystemAPI.Query<WeaponAspect>())
        {
            var weaponEntity = weaponAspect.Entity;

            var isReady = state.EntityManager.HasComponent<WeaponFireReadyTag>(weaponEntity);
            var isFire = state.EntityManager.HasComponent<WeaponFireTag>(weaponEntity);

            var projectileSpawnBuffer = state.EntityManager.GetBuffer<CreateReadyProjectileElementData>(weaponEntity);

            // ���� ������ x
            if (!isReady && !isFire)
            {
                if (weaponAspect.AttackCooldown > 0f)
                {
                    // ���� ���� ���ð�
                    weaponAspect.AttackCooldown -= deltaTime;
                }
                else
                {
                    // ���� ��� ����
                    weaponAspect.AttackCooldown = 0f;

                    var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

                    // �߻����� ���
                    var spawnInfo = weaponAspect.FireInfo;
                    for (int i = 0; i < spawnInfo.Count; i++)
                    {
                        projectileSpawnBuffer.Add(new CreateReadyProjectileElementData
                        {
                            Amount = spawnInfo.Amount,
                            Timer = i == 0 ? 0f : spawnInfo.Interval,
                            Angle = spawnInfo.Angle,
                            Random = spawnInfo.Random,
                            Entity = weaponAspect.DefaultProjectile,
                        });
                    }

                    // �߻� �غ�Ϸ�
                    ecb.AddComponent(weaponEntity, new WeaponFireReadyTag());

                    fireReadyWeaponBuffer.Add(new AttackReadyWeaponElementData
                    {
                        WeaponID = weaponAspect.WeaponID,
                    });
                }
            }

            // �߻���
            if (isFire)
            {
                // ���� ��� ���϶���
                // �߻� ���� ���
                if (projectileSpawnBuffer.Length > 0)
                {
                    var element = projectileSpawnBuffer[0];
                    element.Timer -= deltaTime;
                    projectileSpawnBuffer[0] = element;
                }
            }
        }
    }
}
[UpdateAfter(typeof(WeaponTimerSystem))]
// ���� �ý���
public partial struct WeaponFireSystem : ISystem
{
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var gameConfig = SystemAPI.GetSingletonRW<GameConfigComponent>();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // ���� ������ ���·�
        foreach (var towerAspect in SystemAPI.Query<TowerAspect>().WithAll<TowerAttackReadyTag>())
        {
            var fireReadyWeaponBuffer = state.EntityManager.GetBuffer<AttackReadyWeaponElementData>(towerAspect.Entity);
            
            // ���� ������ ���Ⱑ ����
            if (fireReadyWeaponBuffer.Length <= 0) continue;

            var fireReadyWeaponID = fireReadyWeaponBuffer[0].WeaponID;

            foreach ((var weaponComponent, var weaponEntity) in SystemAPI.Query<WeaponComponent>().WithAll<WeaponFireReadyTag>().WithEntityAccess())
            {
                if (weaponComponent.ID != fireReadyWeaponID) continue;
                
                ecb.RemoveComponent<WeaponFireReadyTag>(weaponEntity);
                ecb.AddComponent<WeaponFireTag>(weaponEntity);
                ecb.SetComponent(weaponEntity, new WeaponFireDirectionComponent
                {
                    Direction = gameConfig.ValueRO.AttackDirection,
                });

                fireReadyWeaponBuffer.RemoveAt(0);
                ecb.RemoveComponent<TowerAttackReadyTag>(towerAspect.Entity);
                towerAspect.Fire();
                break;
            }
        }

        // ����
        foreach (var weaponAspect in SystemAPI.Query<WeaponAspect>().WithAll<WeaponFireTag>())
        {
            var projectileSpawnBuffer = state.EntityManager.GetBuffer<CreateReadyProjectileElementData>(weaponAspect.Entity);
            
            // �߻����� Ȯ��
            if (projectileSpawnBuffer.Length > 0)
            {
                var spawnData = projectileSpawnBuffer[0];

                // Ÿ�̹� ���
                if (spawnData.Timer > 0f) continue;

                var projectileEntities = state.EntityManager.Instantiate(spawnData.Entity, spawnData.Amount, Allocator.TempJob);
                
                var jobHandle = new WeaponAttackJob
                {
                    ECB = ecb.AsParallelWriter(),
                    Entities = projectileEntities,
                    Amount = spawnData.Amount,
                    Angle = spawnData.Angle,
                    RandomSeed = gameConfig.ValueRW.RandomData.NextUInt(),
                    RandomSpread = spawnData.Random,
                    Position = weaponAspect.MuzzleEffectPosition,
                    Rotation = quaternion.LookRotation(weaponAspect.FireDirection, math.up()),
                }.Schedule(projectileEntities.Length, 10, state.Dependency);

                jobHandle.Complete();
                state.Dependency = jobHandle;

                projectileSpawnBuffer.RemoveAt(0);

                if (projectileSpawnBuffer.Length == 0)
                {
                    weaponAspect.Fire();
                    ecb.RemoveComponent<WeaponFireTag>(weaponAspect.Entity);
                    ecb.SetComponent(weaponAspect.Entity, new WeaponFireDirectionComponent
                    {
                        Direction = float3.zero,
                    });
                }
            }
        }
    }
}

#region NOT_USE
//[UpdateAfter(typeof(WeaponCreateSystem))]
//[UpdateAfter(typeof(TowerAttackTimerSystem))]
//public partial struct WeaponAttackSystem : ISystem
//{
//    private BufferLookup<CreateReadyProjectileElementData> _projectileReadyBufferLookup;

//    [BurstCompile]
//    private void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<GameConfigComponent>();
//        _projectileReadyBufferLookup = state.GetBufferLookup<CreateReadyProjectileElementData>();
//    }
//    [BurstCompile]
//    private void OnUpdate(ref SystemState state)
//    {
//        var deltaTime = SystemAPI.Time.DeltaTime;
//        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
//        var gameConfig = SystemAPI.GetSingleton<GameConfigComponent>();
//        //var weaponQuery = new EntityQueryBuilder().WithAll<WeaponTag>().WithAll<WeaponAspect>().Build(state.EntityManager);

//        //var towerEntity1 = SystemAPI.GetSingletonEntity<TowerComponent>();
//        //var towerAspect1 = state.EntityManager.GetAspect<TowerAspect>(towerEntity1);

//        _projectileReadyBufferLookup.Update(ref state);

//        // Ÿ���� ���� ��� ���� ����Ʈ
//        // ���� ������ �����
//        foreach ((var towerAspect, var towerEntity) in SystemAPI.Query<TowerAspect>().WithEntityAccess())
//        {
//            // Ÿ�̸�
//            var buffer = state.EntityManager.GetBuffer<AttackReadyWeaponElementData>(towerEntity);

//            var jobHandle = new WeaponAttackTimerJob
//            {
//                DeltaTime = deltaTime,
//                AttackReadyBuffer = buffer,
//                ProjectileReadyBufferLookup = _projectileReadyBufferLookup,
//            }.Schedule(state.Dependency);
//            jobHandle.Complete();

//            // �߻� ���
//            if (!towerAspect.AttackReady || buffer.Length <= 0) return;

//            var weaponBufferElement = buffer[0];
//            //var rotation = gameConfig.AttackRotation;

//            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//            sw.Start();

//            // �߻����
//            foreach (var weaponAspect in SystemAPI.Query<WeaponAspect>().WithNone<Prefab>())
//            {
//                if (weaponBufferElement.WeaponID != weaponAspect.WeaponID) continue;

//                // �̶� ���⸦ ���ۿ� �ִ°��� �ƴ϶� �������� ��ü�� ���ۿ� �ִ´ٸ�..?
//                // Ÿ�̸Ӵ� ���� ���ɻ���, ���� �ð����� ����һ�
//                var attackInfo = weaponAspect.FireInfo;
//                for (int i = 0; i < attackInfo.Count; i++)
//                {
//                    _projectileReadyBufferLookup[weaponAspect.Entity].Add(new CreateReadyProjectileElementData
//                    {
//                        FireType = attackInfo.FireType,
//                        Amount = attackInfo.Amount,
//                        Timer = attackInfo.Interval,
//                        Angle = attackInfo.Angle,
//                        Random = attackInfo.Random,
//                        Entity = weaponAspect.DefaultProjectile,
//                    });
//                }
//            }

//            //jobHandle = new ProjectileCreateJob
//            //{
//            //    DeltaTime = deltaTime,
//            //    ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
//            //    Position = towerAspect.Position + new float3(0f, 1f, 0f),
//            //    Rotation = rotation,
//            //}.Schedule(jobHandle);
//            //jobHandle.Complete();

//            //foreach (var weaponAspect in SystemAPI.Query<WeaponAspect>().WithNone<Prefab>())
//            //{
//            //    if (weaponBufferElement.WeaponID != weaponAspect.WeaponID) continue;

//            //    //var angle = weaponAspect.FireInfo.Angle;
//            //    //var amount = weaponAspect.FireInfo.Amount;
//            //    //for (int i = 0; i < weaponAspect.FireInfo.Count; i++)
//            //    //{

//            //    //}

//            //    // attackinfo�� ��� buffer����
//            //    // ���������� buffer�� ���
//            //    // �� ������ ����...?
//            //    // ������ ������ �� ������ ��Ÿ�� ������ �����...?
//            //    // 1�ʰ��� 10��
//            //    // 0 - 10 / 1 - 10 / 2 - 10 / ... / 10 - 10
//            //    // ���ĵ� �ʿ��������

//            //    var angle = 60f;
//            //    var amount = 100;

//            //    var entities = state.EntityManager.Instantiate(weaponAspect.DefaultProjectile, amount, Allocator.TempJob);
//            //    jobHandle = new WeaponAttackTestJob
//            //    {
//            //        Entities = entities,
//            //        ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
//            //        Position = towerAspect.Position + new float3(0f, 1f, 0f),
//            //        Rotation = rotation,
//            //        Angle = angle,
//            //        Amount = amount,
//            //    }.Schedule(amount, 10, jobHandle);

//            //    jobHandle.Complete();
//            //    weaponAspect.Fire();
//            //    break;
//            //}

//            sw.Stop();
//            Debug.Log(sw.ElapsedTicks);

//            //// �Ǳ� ������ Weapon�� Ư���ؼ� �۾� �� �� �ֵ��� ����
//            //jobHandle = new WeaponAttackJob
//            //{
//            //    ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
//            //    WeaponID = weaponBufferElement.WeaponID,
//            //    Position = towerAspect.Position + new float3(0f, 1f, 0f),
//            //    Rotation = rotation,
//            //    Angle = angle,
//            //    Amount = amount,
//            //}.ScheduleParallel(jobHandle);
//            //jobHandle.Complete();

//            buffer.RemoveAt(0);
//            towerAspect.Fire();

//            state.Dependency = jobHandle;
//        }
//    }
//}

//public partial struct WeaponAttackTimerJob : IJobEntity
//{
//    public float DeltaTime;
//    public DynamicBuffer<AttackReadyWeaponElementData> AttackReadyBuffer;
//    public BufferLookup<CreateReadyProjectileElementData> ProjectileReadyBufferLookup;

//    [BurstCompile]
//    public void Execute(WeaponAspect weaponAspect)
//    {
//        if (!weaponAspect.AttackReady)
//        {
//            weaponAspect.AttackCooldown -= DeltaTime;

//            if (weaponAspect.AttackCooldown <= 0f)
//            {
//                weaponAspect.AttackReady = true;
//                //Debug.Log($"Weapon ID {weaponAspect.WeaponID} is ready");

//                // ���ۿ� �غ�� ���� �ֱ�
//                AttackReadyBuffer.Add(new AttackReadyWeaponElementData
//                {
//                    WeaponID = weaponAspect.WeaponID,
//                });
//            }
//        }

//        var buffer = ProjectileReadyBufferLookup[weaponAspect.Entity];
//        if (buffer.Length > 0)
//        {
//            var element = buffer[0];
//            element.Timer -= DeltaTime;
//            buffer[0] = element;
//        }
//    }
//}
#endregion

#endregion

#region Job

public partial struct WeaponAttackJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public NativeArray<Entity> Entities;
    public float3 Position;
    public quaternion Rotation;
    public float Angle;
    public int Amount;
    public uint RandomSeed;
    public bool RandomSpread;

    [BurstCompile]
    public void Execute(int index)
    {
        var entity = Entities[index];
        var angle = 0f;
        if (RandomSpread)
        {
            var halfAngle = Angle * 0.5f;
            angle = Random.CreateFromIndex(RandomSeed + (uint)index).NextFloat(-halfAngle, halfAngle);
        }
        else
        {
            var step = Angle / Amount;
            angle = -(Angle * 0.5f) + (step * 0.5f) + step * index;
        }
        var position = Position + (math.normalize(math.mul(Rotation, math.forward())) * 1f);
        var rotation = math.mul(Rotation, quaternion.Euler(0f, math.radians(angle), 0f));

        ECB.SetComponent(index, entity, new LocalTransform
        {
            Position = position,
            Rotation = rotation,
            Scale = 1f,
        });
        // ���� �ð��� GameConfig����
        ECB.AddComponent(index, entity, new ProjectileLifeTimeComponent
        {
            LifeTime = 10f,
        });
    }
}
#endregion