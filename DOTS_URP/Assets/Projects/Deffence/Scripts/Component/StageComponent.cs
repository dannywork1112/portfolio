using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


#region Tag
#endregion

public struct EnemySpawnData
{
    public Entity Entity;
    public float Chance;
    public float2 AmountRange;
}

#region ComponentData
public struct StageComponent : IComponentData
{
    public float TimeLimit;
    // ½ºÆù µô·¹ÀÌ
    public float SpawnDelay;
    public NativeList<EnemySpawnData> SpawnDatas;
}
public struct StageLimitTimerComponent : IComponentData
{
    public float Timer;
}
#endregion

#region BufferElement
public struct EnemySpawnElement : IBufferElementData
{
    public float Timer;
    public int Amount;
    public Entity Entity;
}
#endregion