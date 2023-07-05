using UnityEngine;

public class GameSceneManager : Singleton<GameSceneManager>
{
    [Header("GameConfig")]
    [SerializeField] private GameConfiguration _gameConfiguration;

    [Header("Camera")]
    [SerializeField] private CameraController _cameraController;

    private Tower _tower;

    public CameraController CameraController => _cameraController;
    public Vector2 SpawnDistance => _gameConfiguration.SpawnDistance;

    private void Start()
    {
        var stageTableData = TableManager.Instance.StageTable.GetData(_gameConfiguration.StageID);
        if (stageTableData == null) return;

        _tower = CreateTower(_gameConfiguration.TowerID);
        if (_tower == null) return;

        // Weapon Test
        _tower.AddWeapon("Weapon 1");
        _tower.AddWeapon("Weapon 2");
        _tower.AddWeapon("Weapon 3");
        _tower.AddWeapon("Weapon 4");
        _tower.AddWeapon("Weapon 5");

        var stageObject = new GameObject("Stage");
        var stage = stageObject.AddComponent<Stage>();
        stage.Initalization(stageTableData, _tower.Transform);
        stage.StartStage();
    }

    #region Tower
    public Tower CreateTower(string towerID)
    {
        if (_tower != null) return _tower;

        var towerTableData = TableManager.Instance.TowerTable.GetData(towerID);
        if (towerTableData == null) return null;

        var tower = Instantiate(towerTableData.Prefab);
        if (tower == null) return null;

        tower.Initialization(towerTableData);

        return tower;
    }
    public void CreateWeapon(string weaponID)
    {
        if (_tower == null) return;

        _tower.AddWeapon(weaponID);
    }
    public void Fire(Vector2 value)
    {
        _tower.Fire(value);
    }
    #endregion

    public Transform GetCloseTarget(Transform from, float radius, LayerMask layerMask)
    {
        var closeDistance = float.MaxValue;
        Transform closeTarget = null;
        var overlapTargets = Physics.OverlapSphere(from.position, radius, layerMask);
        if (overlapTargets.Length < 1) return null;

        for (int i = 0; i < overlapTargets.Length; i++)
        {
            var target = overlapTargets[i];
            if (target.transform == this.transform) continue;
            var targetTransform = target.transform;
            var distance = Vector3.Distance(from.position, targetTransform.position);
            if (distance > radius) continue;
            if (distance < closeDistance)
            {
                // 가장 가까운 적의 거리 갱신
                closeDistance = distance;
                closeTarget = targetTransform;
            }
        }
        return closeTarget;
    }

}
