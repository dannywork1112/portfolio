using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyTableData
{
    public int ID;
    public float MaxHP;
    public float Damage;
    public float MoveSpeed;
    public float AttackSpeed;
    public GameObject Prefab;
    public Mesh Mesh;
    public Material Material;
}

[CreateAssetMenu(fileName = "EnemyTable", menuName = "Game/Table/EnemyTable")]
public class EnemyTable : ScriptableObject
{
    [SerializeField] private EnemyTableData[] _datas;

    public IReadOnlyList<EnemyTableData> Datas => _datas;

    public EnemyTableData GetData(int id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        if (data == null) Debug.Log($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}