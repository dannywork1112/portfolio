using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{ 
using System.Collections;
using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;

    public struct EnemySpawnTableData
    {

    }

[System.Serializable]
public struct SpawnData
{
    public float SpawnInterval;
    public EnemySpawnTableData[] SpawnDatas;
}

public class Spawner : MonoBehaviour
{
    [SerializeField, ReadOnly] private SpawnData _data;
    [SerializeField, ReadOnly] private float _spawnCooldown;

    private Stage _stage;
    private IEnumerator _spawnCoroutine;
    private bool _spawnable;

    public void Initialization(Stage stage, Tower tower, float spawnInterval, EnemySpawnTableData[] spawnData)
    {
        _stage = stage;

        _data = new SpawnData
        {
            SpawnInterval = spawnInterval,
            SpawnDatas = spawnData,
        };

        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        _spawnCoroutine = SpawnCo(tower);
        StartCoroutine(_spawnCoroutine);
    }

    private IEnumerator SpawnCo(Tower tower)
    {
        var delay = 0.02f;
        var enemyTable = TableManager.Instance.EnemyTable;
        var wfs = new WaitForSeconds(delay);
        var notSpawnableWfs = new WaitForSeconds(1f);
        var spawnDistance = GameSceneManager.Instance.SpawnDistance;
        while (true)
        {
            if (!_spawnable)
            {
                yield return notSpawnableWfs;
                continue;
            }

            _spawnCooldown -= delay;

            if (_spawnCooldown > 0f)
            {
                yield return wfs;
                continue;
            }

            for (int i = 0; i < _data.SpawnDatas.Length; i++)
            {
                //var spawnData = _data.SpawnDatas[i];
                //var chance = Random.value * 100f;
                //if (spawnData.Chance < chance) continue;

                //var enemyTableData = enemyTable.GetData(spawnData.EnemyID);
                //if (enemyTableData == null) continue;

                //var amount = Random.Range(spawnData.AmountRange.x, spawnData.AmountRange.y + 1);
                //for (int j = 0; j < amount; j++)
                //{
                //    var position = GetRandomSpawnPos(spawnDistance.x, spawnDistance.y);
                //    var enemy = Instantiate(enemyTableData.Prefab);
                //    enemy.Initialization(enemyTableData, tower.transform, position);
                //}
            }

            _spawnCooldown = _data.SpawnInterval;
            yield return wfs;
        }
    }
    public void SetSpawnable(bool spawnable)
    {
        _spawnable = spawnable;
    }
    private Vector3 GetRandomSpawnPos(float minDistance, float maxDistance)
    {
        var value = Random.Range(minDistance, maxDistance);
        var spawnPos = Random.insideUnitCircle.normalized * value;
        return new Vector3(spawnPos.x, 0f, spawnPos.y);
    }
}
}

public class Spawner
{
    public void Spawn(WaveSpawnData spawnData, Transform target, Action onDead)
    {
        var chance = Random.value * 100f;
        if (spawnData.Chance < chance) return;

        var enemyTableData = TableManager.Instance.EnemyTable.GetData(spawnData.EnemyID);
        if (enemyTableData == null) return;

        var spawnDistance = GameSceneManager.Instance.SpawnDistance;

        for (int i = 0; i < spawnData.Amount; i++)
        {
            var position = GetRandomSpawnPos(spawnDistance.x, spawnDistance.y);
            var enemy = GameObject.Instantiate(enemyTableData.Prefab);
            enemy.Initialization(enemyTableData, target, position);
            enemy.OnDeadEvent += onDead;
        }
    }
    private Vector3 GetRandomSpawnPos(float minDistance, float maxDistance)
    {
        var value = Random.Range(minDistance, maxDistance);
        var spawnPos = Random.insideUnitCircle.normalized * value;
        return new Vector3(spawnPos.x, 0f, spawnPos.y);
    }
}