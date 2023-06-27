using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class StageTableData
{
    public int ID;
    public float TimeLimit;
    public float SpawnDelay;
    public EnemySpawnTableData[] SpawnDatas;
}
[System.Serializable]
public struct EnemySpawnTableData
{
    public GameObject Prefab;
    public float Chance;
    public float2 AmountRange;
}

[CreateAssetMenu(fileName = "StageTable", menuName = "Game/Table/StageTable")]
public class StageTable : ScriptableObject
{
    [SerializeField] private StageTableData[] _datas;

    public IReadOnlyList<StageTableData> Datas => _datas;

    public StageTableData GetData(int id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        if (data == null) Debug.Log($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}
