using System.Collections;
using UnityEngine;
/*
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float _spawnInterval;

    public void Initialization(Tower tower)
    {
        StartCoroutine(SpawnCO(tower));
    }

    public IEnumerator SpawnCO(Tower tower)
    {
        int spawnCount = 0;
        var stageData = TableManager.Instance.StageTable.GetData("Enemy 1");
        var spawnDataAmount = stageData.SpawnDatas.Length;
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);
            var towerTransform = tower.transform;
            var spawnPos = Vector3.zero;
            do
            {
                var randomPos = Random.insideUnitCircle * 15f;
                spawnPos = new Vector3(randomPos.x, 0f, randomPos.y);
                yield return null;
            } while (Vector3.Distance(towerTransform.position, spawnPos) < 10f);

            // spawn
            var index = Random.Range(0, spawnDataAmount);
            var enemyID = stageData.SpawnDatas[index].EnemyID;
            var enemyData = TableManager.Instance.EnemyTable.GetData(enemyID);
            var enemy = Instantiate(enemyData.Prefab);
            enemy.Initialization(enemyData, towerTransform, spawnPos);
            enemy.name = $"Enemy {spawnCount++}";
        }
    }

    //public Enemy Spawn(EnemyTableData tableData, Vector3 position, Quaternion rotation)
    //{
    //    var tableData = TableManager.Instance.EnemyTable.GetData(id);
    //    var targetDirection = _target.position - spawnPos;
    //    targetDirection.y = 0f;
    //    var targetRotation = Quaternion.LookRotation(targetDirection);
    //    _transform.SetPositionAndRotation(spawnPos, targetRotation);
    //}
}
*/