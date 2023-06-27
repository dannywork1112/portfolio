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
//// 무기를 생성하는 시스템
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
// 타이머 시스템
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

            // 무기 사용승인 x
            if (!isReady && !isFire)
            {
                if (weaponAspect.AttackCooldown > 0f)
                {
                    // 무기 재사용 대기시간
                    weaponAspect.AttackCooldown -= deltaTime;
                }
                else
                {
                    // 무기 사용 가능
                    weaponAspect.AttackCooldown = 0f;

                    var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

                    // 발사정보 등록
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

                    // 발사 준비완료
                    ecb.AddComponent(weaponEntity, new WeaponFireReadyTag());

                    fireReadyWeaponBuffer.Add(new AttackReadyWeaponElementData
                    {
                        WeaponID = weaponAspect.WeaponID,
                    });
                }
            }

            // 발사중
            if (isFire)
            {
                // 무기 사용 중일때만
                // 발사 간격 계산
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
// 공격 시스템
public partial struct WeaponFireSystem : ISystem
{
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var gameConfig = SystemAPI.GetSingletonRW<GameConfigComponent>();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // 공격 가능한 상태로
        foreach (var towerAspect in SystemAPI.Query<TowerAspect>().WithAll<TowerAttackReadyTag>())
        {
            var fireReadyWeaponBuffer = state.EntityManager.GetBuffer<AttackReadyWeaponElementData>(towerAspect.Entity);
            
            // 공격 가능한 무기가 없음
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

        // 공격
        foreach (var weaponAspect in SystemAPI.Query<WeaponAspect>().WithAll<WeaponFireTag>())
        {
            var projectileSpawnBuffer = state.EntityManager.GetBuffer<CreateReadyProjectileElementData>(weaponAspect.Entity);
            
            // 발사정보 확인
            if (projectileSpawnBuffer.Length > 0)
            {
                var spawnData = projectileSpawnBuffer[0];

                // 타이밍 대기
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

//        // 타워의 공격 대기 무기 리스트
//        // 실제 공격할 무기들
//        foreach ((var towerAspect, var towerEntity) in SystemAPI.Query<TowerAspect>().WithEntityAccess())
//        {
//            // 타이머
//            var buffer = state.EntityManager.GetBuffer<AttackReadyWeaponElementData>(towerEntity);

//            var jobHandle = new WeaponAttackTimerJob
//            {
//                DeltaTime = deltaTime,
//                AttackReadyBuffer = buffer,
//                ProjectileReadyBufferLookup = _projectileReadyBufferLookup,
//            }.Schedule(state.Dependency);
//            jobHandle.Complete();

//            // 발사 대기
//            if (!towerAspect.AttackReady || buffer.Length <= 0) return;

//            var weaponBufferElement = buffer[0];
//            //var rotation = gameConfig.AttackRotation;

//            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//            sw.Start();

//            // 발사시작
//            foreach (var weaponAspect in SystemAPI.Query<WeaponAspect>().WithNone<Prefab>())
//            {
//                if (weaponBufferElement.WeaponID != weaponAspect.WeaponID) continue;

//                // 이때 무기를 버퍼에 넣는것이 아니라 공격정보 자체를 버퍼에 넣는다면..?
//                // 타이머는 공격 가능상태, 남은 시간만을 계산할뿐
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

//            //    // attackinfo가 담긴 buffer제작
//            //    // 공격정보를 buffer에 담고
//            //    // 매 프레임 정렬...?
//            //    // 어차피 공격이 다 나가야 쿨타임 돌꺼니 ㄴ상관...?
//            //    // 1초간격 10발
//            //    // 0 - 10 / 1 - 10 / 2 - 10 / ... / 10 - 10
//            //    // 정렬도 필요없을지도

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

//            //// 되긴 되지만 Weapon을 특정해서 작업 할 수 있도록 수정
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

//                // 버퍼에 준비된 무기 넣기
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
        // 실제 시간은 GameConfig에서
        ECB.AddComponent(index, entity, new ProjectileLifeTimeComponent
        {
            LifeTime = 10f,
        });
    }
}
#endregion