using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WeaponTableData
{
    public int ID;
    public float Delay;
    public WeaponFireInfo FireInfo;
    public GameObject Prefab;
    public GameObject ProjectilePrefab;
}

[CreateAssetMenu(fileName = "WeaponTable", menuName = "Game/Table/WeaponTable")]
public class WeaponTable : ScriptableObject
{
    [SerializeField] private WeaponTableData[] _datas;

    public IReadOnlyList<WeaponTableData> Datas => _datas;

    public WeaponTableData GetData(int id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        if (data == null) Debug.Log($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}