using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TowerSkillData
{
    public int ID;
    public float Damage;
    public float Interval;
    public GameObject Skill;
}

[CreateAssetMenu(fileName = "TowerSkillTable", menuName = "Game/Table/TowerSkillTable")]
public class TowerSkillTable : ScriptableObject
{
    [SerializeField] private TowerSkillData[] _datas;

    public IReadOnlyList<TowerSkillData> Datas => _datas;

    public TowerSkillData GetData(int id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        return data;
    }
}
