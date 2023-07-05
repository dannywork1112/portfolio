using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StageTableData
{
    public string ID;
    public float LimitTime;
    public WaveData[] WaveDatas;
}

[System.Serializable]
public struct WaveData
{
    public WaveSpawnData[] SpawnDatas;
}
[System.Serializable]
public struct WaveSpawnData
{
    public string EnemyID;
    public float Chance;
    public int Amount;
}

[CreateAssetMenu(fileName = "StageTable", menuName = "Game/Table/StageTable")]
public class StageTable : ScriptableObject
{
    [SerializeField] private StageTableData[] _datas;
    private Dictionary<string, StageTableData> _dicDatas;

    public IReadOnlyDictionary<string, StageTableData> Datas => _dicDatas;

    private void OnEnable()
    {
        _dicDatas = _datas.ToDictionary(x => x.ID, x => x);
    }
    private void OnDisable()
    {
        _dicDatas = null;
    }
    //private void OnValidate()
    //{
    //    for (int i = 0; i < _datas.Length; i++)
    //    {
    //        var stageData = _datas[i];
    //        var limitTime = stageData.LimitTime;
    //        for (int j = 0; j < stageData.WaveDatas.Length; j++)
    //        {
    //            var waveData = stageData.WaveDatas[j];
    //            waveData.SpawnTime = Mathf.Clamp(waveData.SpawnTime, 0f, limitTime);
    //            stageData.WaveDatas[j] = waveData;
    //        }
    //    }
    //}

    public bool HasData(string id) => _dicDatas.ContainsKey(id);
    public StageTableData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!_dicDatas.TryGetValue(id, out var data))
            Debug.LogError($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}
