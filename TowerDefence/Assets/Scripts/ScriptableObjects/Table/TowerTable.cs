using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TowerTableData
{
    public string ID;
    public float MaxHP;
    public float FireDelay;
    public Tower Prefab;
}

[CreateAssetMenu(fileName = "TowerTable", menuName = "Game/Table/TowerTable")]
public class TowerTable : ScriptableObject
{
    [SerializeField] private TowerTableData[] _datas;
    private Dictionary<string, TowerTableData> _dicDatas;

    public IReadOnlyDictionary<string, TowerTableData> Datas => _dicDatas;

    private void OnEnable()
    {
        _dicDatas = _datas.ToDictionary(x => x.ID, x => x);
    }
    private void OnDisable()
    {
        _dicDatas = null;
    }

    public bool HasData(string id) => _dicDatas.ContainsKey(id);
    public TowerTableData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!_dicDatas.TryGetValue(id, out var data))
            Debug.LogError($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
    //public TowerTableData GetData(string id)
    //{
    //    var data = _datas.Where(x => x.ID == id).FirstOrDefault();
    //    if (data == null) Debug.Log($"{this.GetType().Name} : {id} is not found.");
    //    return data;
    //}
}
