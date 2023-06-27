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
/// 게임 시작시 타워 ID 정함
/// 타워 테이블에서 해당 ID의 타워 데이터 가져옴
/// 타워 생성
/// 타이머 시작
/// 투사체 발사
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