//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Entities.Graphics;
//using Unity.Mathematics;
//using Unity.Rendering;
//using Unity.Transforms;
//using UnityEngine;
//using UnityEngine.Rendering;
//using static UnityEngine.EventSystems.EventTrigger;

//#region System
//[UpdateAfter(typeof(GameInitializeSystem))]
//public partial class StageInitalizeSystem : SystemBase
//{
//    protected override void OnUpdate()
//    {
//        var stageID = GameSceneManager.Instance.GameConfig.StageID;
//        var stageData = TableContainer.Instance.StageTable.GetData(stageID);
//        if (stageData == null) return;
        
//        Enabled = false;

//        var enemyArcheType = EntityManager.CreateArchetype(
//                typeof(LocalTransform),
//                typeof(LocalToWorld),
//                typeof(RenderFilterSettings),
//                typeof(RenderMeshArray),
//                typeof(MaterialMeshInfo),
//                typeof(EnemyComponent),
//                typeof(MoveComponent),
//                typeof(HitPointComponent),
//                typeof(AttackPowerComponent));

//        var desc = new RenderMeshDescription(ShadowCastingMode.Off);
//        var meshArray = new RenderMeshArray();
//        var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);

//        var ecb = new EntityCommandBuffer(Allocator.Temp);

//        // 데이터 초기화
//        foreach ((var stageComponent, var stageLimitTimer, var stageEntity)
//            in SystemAPI.Query<RefRW<StageComponent>, RefRW<StageLimitTimerComponent>>().WithEntityAccess())
//        {
//            // 타이머
//            var enemySpawnBuffer = SystemAPI.GetBuffer<EnemySpawnElement>(stageEntity);
//            stageLimitTimer.ValueRW.Timer = stageData.TimeLimit;

//            // 스폰 데이터
//            for (int i = 0; i < stageData.SpawnDatas.Length; i++)
//            {
//                var spawnData = stageData.SpawnDatas[i];
//                //var enemyData = TableContainer.Instance.EnemyTable.GetData(spawnData.ID);
//                //if (enemyData == null) continue;

//                //var enemyEntity = EntityManager.CreateEntity(enemyArcheType);
//                //meshArray.Materials = new[] { enemyData.Material };
//                //meshArray.Meshes = new[] { enemyData.Mesh };

//                //RenderMeshUtility

//                //ecb.SetSharedComponentManaged(enemyEntity, desc.FilterSettings);
//                //ecb.SetSharedComponentManaged(enemyEntity, meshArray);
//                //ecb.SetComponent(enemyEntity, materialMeshInfo);
//                //ecb.SetComponent(enemyEntity, new LocalTransform
//                //{
//                //    Scale = 1f,
//                //});
//                //EntityManager.SetSharedComponentManaged(enemyEntity, new RenderMesh
//                //{
//                //    mesh = enemyData.Mesh,
//                //    material = enemyData.Material,
//                //});
//                //EntityManager.SetSharedComponentManaged(enemyEntity, new EnemyComponent
//                //{
//                //    ID = enemyData.ID,
//                //});
//                //EntityManager.SetComponentData(enemyEntity, new MoveComponent
//                //{
//                //    Value = enemyData.MoveSpeed,
//                //});
//                //EntityManager.SetComponentData(enemyEntity, new HitPointComponent
//                //{
//                //    Value = enemyData.MaxHP,
//                //});
//                //EntityManager.SetComponentData(enemyEntity, new AttackPowerComponent
//                //{
//                //    Value = enemyData.Damage,
//                //});

//            }
//        }

//        ecb.Playback(EntityManager);
//        ecb.Dispose();
//    }
//}
////[UpdateAfter(typeof(StageInitalizeSystem))]
////public partial class StageManagementSystem : SystemBase
////{
////    [BurstCompile]
////    protected override void OnUpdate()
////    {
////        var deltaTime = SystemAPI.Time.DeltaTime;

////        foreach (var limitTimer in SystemAPI.Query<RefRW<StageLimitTimerComponent>>())
////        {
////            limitTimer.ValueRW.Timer -= deltaTime;
////        }
////    }
////}
//[UpdateAfter(typeof(StageInitalizeSystem))]
//public partial struct StageWaveSystem : ISystem
//{
//    private EntityQuery _enemyPrefabQuery;

//    private void OnCreate(ref SystemState state)
//    {
//        _enemyPrefabQuery = new EntityQueryBuilder(Allocator.Persistent)
//            .WithAll<EnemyComponent>().WithAll<Prefab>().Build(state.EntityManager);

//        SpawnEnemy(0);
//    }

//    private void OnUpdate(ref SystemState state)
//    {
//        var deltaTime = SystemAPI.Time.DeltaTime;

//        foreach ((var limitTimer, var stageEntity) in SystemAPI.Query<RefRW<StageLimitTimerComponent>>().WithEntityAccess())
//        {
//            if (limitTimer.ValueRW.Timer > 0f)
//            {
//                limitTimer.ValueRW.Timer -= deltaTime;
//            }
//            else
//            {
//                limitTimer.ValueRW.Timer = 0f;

//                // 몬스터 스폰
//            }
//        }
//    }

//    private void SpawnEnemy(int id)
//    {
//        //var enemies = _enemyPrefabQuery.ToComponentDataArray<EnemyComponent>(Allocator.Temp);
//        //var archeType = EntityManager.CreateArchetype(
//        //    typeof(MoveComponent),
//        //    typeof(HitPointComponent),
//        //    typeof(AttackPowerComponent),
//        //    typeof(DamageComponent),
//        //    typeof(LocalTransform),
//        //    typeof(LocalToWorld));
//        //var entity = EntityManager.CreateEntity(archeType);
//    }
//}
//public partial struct MovementForwardSystem : ISystem
//{
//    [BurstCompile]
//    private void OnUpdate(ref SystemState state)
//    {
//        var deltaTime = SystemAPI.Time.DeltaTime;

//        foreach ((var moveComponent, var localTransform) in SystemAPI.Query<MoveComponent, RefRW<LocalTransform>>())
//        {
//            localTransform.ValueRW.Position += localTransform.ValueRO.Forward() * moveComponent.Value * deltaTime;
//        }
//    }
//}
//#endregion

//#region Job
//#endregion
