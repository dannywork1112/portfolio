using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
    [SerializeField] private TowerTable _towerTable;

    public TowerTable TowerTable => _towerTable;
}