using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemyTableData
{
    public string ID;
    public float Damage;
    public float MaxHP;
    public float MovementSpeed;
    public float AttackSpeed;
    public float AttackRange;
    public Enemy Prefab;
}

[CreateAssetMenu(fileName = "EnemyTable", menuName = "Game/Table/EnemyTable")]
public class EnemyTable : ScriptableObject
{
    [SerializeField] private EnemyTableData[] _datas;
    private Dictionary<string, EnemyTableData> _dicDatas;

    public IReadOnlyDictionary<string, EnemyTableData> Datas => _dicDatas;

    private void OnEnable()
    {
        _dicDatas = _datas.ToDictionary(x => x.ID, x => x);
    }
    private void OnDisable()
    {
        _dicDatas = null;
    }

    public bool HasData(string id) => _dicDatas.ContainsKey(id);
    public EnemyTableData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!_dicDatas.TryGetValue(id, out var data))
            Debug.LogError($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}
