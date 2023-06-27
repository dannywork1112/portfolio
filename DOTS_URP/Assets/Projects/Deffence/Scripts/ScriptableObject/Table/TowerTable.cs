using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TowerTableData
{
    public int ID;
    public float MaxHP;
    public float Delay;
    public float Damage;
    public GameObject Prefab;
    public int DefaultWeaponID;
}

/// <summary>
/// ���� ���۽� Ÿ�� ID ����
/// Ÿ�� ���̺��� �ش� ID�� Ÿ�� ������ ������
/// Ÿ�� ����
/// Ÿ�̸� ����
/// ����ü �߻�
/// </summary>

[CreateAssetMenu(fileName = "TowerTable", menuName = "Game/Table/TowerTable")]
public class TowerTable : ScriptableObject
{
    [SerializeField] private TowerTableData[] _datas;

    public IReadOnlyList<TowerTableData> Datas => _datas;

    public TowerTableData GetData(int id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        if (data == null) Debug.Log($"{this.GetType().Name} : {id} is not found.");
        return data;
    }
}