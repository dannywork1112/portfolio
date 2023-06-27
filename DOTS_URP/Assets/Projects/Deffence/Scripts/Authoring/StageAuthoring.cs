using Unity.Collections;
using Unity.Entities;
using UnityEngine;

#region Authoring
public class StageAuthoring : MonoBehaviour
{
    [SerializeField] private StageTable _stageTable;
    [SerializeField] private GameConfig _gameConfig;

    public StageTable StageTable => _stageTable;
    public GameConfig GameConfig => _gameConfig;

    class Baker : Baker<StageAuthoring>
    {
        public override void Bake(StageAuthoring authoring)
        {
            var stageData = authoring.StageTable.GetData(authoring.GameConfig.StageID);
            var spawnDatas = new NativeList<EnemySpawnData>(Allocator.Persistent);
            for (int i = 0; i < stageData.SpawnDatas.Length; i++)
            {
                var tableSpawnData = stageData.SpawnDatas[i];
                spawnDatas.Add(new EnemySpawnData
                {
                    Entity = GetEntity(tableSpawnData.Prefab, TransformUsageFlags.Dynamic),
                    AmountRange = tableSpawnData.AmountRange,
                    Chance = tableSpawnData.Chance,
                });
            }
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new StageComponent
            {
                TimeLimit = stageData.TimeLimit,
                SpawnDelay = stageData.SpawnDelay,
                SpawnDatas = spawnDatas,
            });
            AddComponent<StageLimitTimerComponent>(entity);

            AddBuffer<EnemySpawnElement>(entity);
        }
    }
}
#endregion

#region Aspect
#endregion