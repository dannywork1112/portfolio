using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
    [SerializeField] private StageTable _stagelTable;
    [SerializeField] private TowerTable _towerTable;
    [SerializeField] private WeaponTable _weaponTable;
    [SerializeField] private ProjectileTable _projectileTable;
    [SerializeField] private EnemyTable _enemyTable;

    public StageTable StageTable => _stagelTable;
    public TowerTable TowerTable => _towerTable;
    public WeaponTable WeaponTable => _weaponTable;
    public ProjectileTable ProjectileTable => _projectileTable;
    public EnemyTable EnemyTable => _enemyTable;
}
