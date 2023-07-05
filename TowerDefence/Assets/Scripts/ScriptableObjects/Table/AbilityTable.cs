using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AbilityTableData
{
    public string ID;
}

[CreateAssetMenu(fileName = "AbilityTable", menuName = "Game/Table/AbilityTable")]
public class AbilityTable : ScriptableObject
{
    [SerializeField] private AbilityTableData[] _datas;

    public IReadOnlyList<AbilityTableData> Datas => _datas;

    public AbilityTableData GetData(string id)
    {
        var data = _datas.Where(x => x.ID == id).FirstOrDefault();
        return data;
    }
}
