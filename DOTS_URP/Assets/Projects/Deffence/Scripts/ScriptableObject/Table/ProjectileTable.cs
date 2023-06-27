using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ProjectileTableData
{
    public int ID;
    public float Speed;
    public GameObject Prefab;
}

[CreateAssetMenu(fileName = "ProjectileTable", menuName = "Game/Table/ProjectileTable")]
public class ProjectileTable : ScriptableObject
{
    [SerializeField] private ProjectileTableData[] _datas;

    public IReadOnlyList<ProjectileTableData> Datas => _datas;

    public ProjectileTableData GetData(int id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        if (data == null) Debug.Log($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}