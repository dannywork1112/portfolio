using UnityEngine;

[CreateAssetMenu(fileName = "Gameconfig", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    [SerializeField] private int _towerID;
    [SerializeField] private int _weaponID;
    [SerializeField] private int _stageID;

    public int TowerID => _towerID;
    public int WeaponID => _weaponID;
    public int StageID => _stageID;
}
