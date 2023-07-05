using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WeaponTableData
{
    public string ID;
    public Weapon Prefab;
    public float Damage;
    public float FireDelay;
    public string DefaultProjectileID;
}

[CreateAssetMenu(fileName = "WeaponTable", menuName = "Game/Table/WeaponTable")]
public class WeaponTable : ScriptableObject
{
    [SerializeField] private WeaponTableData[] _datas;
    private Dictionary<string, WeaponTableData> _dicDatas;

    public IReadOnlyDictionary<string, WeaponTableData> Datas => _dicDatas;

    private void OnEnable()
    {
        _dicDatas = _datas.ToDictionary(x => x.ID, x => x);
    }
    private void OnDisable()
    {
        _dicDatas = null;
    }

    public bool HasData(string id) => _dicDatas.ContainsKey(id);
    public WeaponTableData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!_dicDatas.TryGetValue(id, out var data))
            Debug.LogError($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}
