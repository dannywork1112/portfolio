using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ProjectileTableData
{
    public string ID;
    public float Speed;
    public float Damage;
    public float LifeTime;
    public float StiffTime;
    public float KnockbackDistance;
    public Projectile Prefab;
}

[CreateAssetMenu(fileName = "ProjectileTable", menuName = "Game/Table/ProjectileTable")]
public class ProjectileTable : ScriptableObject
{
    [SerializeField] private ProjectileTableData[] _datas;
    private Dictionary<string, ProjectileTableData> _dicDatas;

    public IReadOnlyDictionary<string, ProjectileTableData> Datas => _dicDatas;

    private void OnEnable()
    {
        _dicDatas = _datas.ToDictionary(x => x.ID, x => x);
    }
    private void OnDisable()
    {
        _dicDatas = null;
    }

    public bool HasData(string id) => _dicDatas.ContainsKey(id);
    public ProjectileTableData GetData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!_dicDatas.TryGetValue(id, out var data))
            Debug.LogError($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}
