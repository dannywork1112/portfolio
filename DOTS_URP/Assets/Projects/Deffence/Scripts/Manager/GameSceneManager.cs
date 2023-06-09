using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameSceneManager : Singleton<GameSceneManager>
{
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private WeaponCooldownUI _cooldownUI;
    [SerializeField] private TMP_Text _projectileCountText;

    private World _world;
    private EntityManager _entityManager;
    private EntityQuery _gameConfigQuery;

    private bool _initialized = false;

    private Vector3 _attackDirection;
    private float _cameraAngleY;

    private List<int> _createdWeaponsID;

    public GameConfig GameConfig => _gameConfig;
    public RefRW<GameConfigComponent> GameConfigComponent => _gameConfigQuery.GetSingletonRW<GameConfigComponent>();
    public Vector3 AttackDirection => CalculateAttackDirection();
    public Quaternion AttackRotation => Quaternion.LookRotation(AttackDirection);

    public float3 CalculateAttackDirection()
    {
        var attackDirection = new Vector3(_attackDirection.x, 0f, _attackDirection.y);
        var rotation = Quaternion.AngleAxis(_cameraAngleY, Vector3.up);
        return rotation * attackDirection;
    }

    public void SetAttackDirection(Vector2 value)
    {
        if (!_initialized) return;
        _attackDirection = value;

        GameConfigComponent.ValueRW.AttackDirection = AttackDirection;
    }

    public void SetCameraAngleY(Vector2 value)
    {
        if (!_initialized) return;
        _cameraAngleY = _cameraController.Transform.eulerAngles.y;
    }

    private void Start()
    {
        _world = World.DefaultGameObjectInjectionWorld;
        _entityManager = _world.EntityManager;
        _gameConfigQuery = _entityManager.CreateEntityQuery(typeof(GameConfigComponent));

        var gameConfig = _gameConfigQuery.GetSingletonRW<GameConfigComponent>();

        gameConfig.ValueRW.TowerID = _gameConfig.TowerID;
        gameConfig.ValueRW.WeaponID = _gameConfig.WeaponID;

        _createdWeaponsID = new List<int>();

        var cooldownSystem = _world.GetOrCreateSystemManaged<WeaponCooldownSystem>();
        cooldownSystem.OnUpdageCooldown += SetCooldown;

        _initialized = true;
    }
    private void OnDisable()
    {
        if (World.DefaultGameObjectInjectionWorld == null) return;
        var cooldownSystem = _world.GetExistingSystemManaged<WeaponCooldownSystem>();
        cooldownSystem.OnUpdageCooldown -= SetCooldown;
    }
    private void Update()
    {
        var projectileQuery = _entityManager.CreateEntityQuery(typeof(ProjectileComponent));
        var count = projectileQuery.CalculateEntityCount();

        _projectileCountText.SetText($"Projectile Entity Count : {count}");
    }

    public void AddWeapon(int weaponID)
    {
        var MAX_WEAPON_COUNT = 8;
        if (_createdWeaponsID.Count == MAX_WEAPON_COUNT) return;
        if (_createdWeaponsID.Contains(weaponID)) return;

        var weaponQuery = _entityManager.CreateEntityQuery(typeof(WeaponComponent), typeof(Prefab));
        var weapons = weaponQuery.ToComponentDataArray<WeaponComponent>(Allocator.Temp);
        var weaponEntities = weaponQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < weaponEntities.Length; i++)
        {
            var weaponComponent = weapons[i];
            if (weaponComponent.ID != weaponID) continue;

            var weaponEntity = _entityManager.Instantiate(weaponEntities[i]);
            _entityManager.SetComponentData(weaponEntity, new WeaponComponent
            {
                ID = weaponComponent.ID,
                Delay = weaponComponent.Delay,
                FireInfo = weaponComponent.FireInfo,
                ProjectileEntity = weaponComponent.ProjectileEntity,
            });
            _createdWeaponsID.Add(weaponID);

            _cooldownUI.AddWeapon(_createdWeaponsID.Count - 1, $"{weaponID}");
        }
    }
    private void SetCooldown(int id, float ratio, bool isFire = false)
    {
        var index = _createdWeaponsID.FindIndex(x => x == id);
        if (index == -1) AddWeapon(id);

        var color = isFire ? Color.red : Color.black;
        color.a = 0.7f;

        _cooldownUI.SetCooldownColor(index, ratio, color);
    }
}

#region System
public partial class GameInitializeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (GameSceneManager.Instance == null) return;
        if (GameSceneManager.Instance.GameConfig == null) return;

        var towerID = GameSceneManager.Instance.GameConfig.TowerID;

        if (TableContainer.Instance == null) return;
        var towerData = TableContainer.Instance.TowerTable.GetData(towerID);
        if (towerData == null) return;

        var towerEntity = SystemAPI.GetSingletonEntity<TowerTag>();
        if (towerEntity == Entity.Null) return;

        Enabled = false;

        // Init Scene
        var gameConfig = SystemAPI.GetSingletonRW<GameConfigComponent>();
        gameConfig.ValueRW.RandomData.InitState((uint)DateTime.Today.Ticks);

        // 타워 초기화
        var towerComponent = SystemAPI.GetComponentRW<TowerComponent>(towerEntity, false);
        towerComponent.ValueRW.ID = towerID;
        towerComponent.ValueRW.MaxHP = towerData.MaxHP;
        towerComponent.ValueRW.Delay = towerData.Delay;
        towerComponent.ValueRW.Damage = towerData.Damage;

        // 타이머 초기화
        var towerTimerComponent = SystemAPI.GetComponentRW<TowerAttackTimerComponent>(towerEntity, false);
        towerTimerComponent.ValueRW.Timer = towerData.Delay;

        //// 기본 무기 버퍼에 추가
        //var weaponID = GameSceneManager.Instance.GameConfig.WeaponID;
        //var createReadyBuffer = SystemAPI.GetBuffer<CreateReadyWeaponElementData>(towerEntity);
        //createReadyBuffer.Add(new CreateReadyWeaponElementData { WeaponID = weaponID });
    }
}
public partial class WeaponCooldownSystem : SystemBase
{
    public Action<int, float, bool> OnUpdageCooldown;

    protected override void OnUpdate()
    {
        Entities.WithoutBurst()
            .WithAll<WeaponComponent, WeaponFireTag>()
            .ForEach((ref WeaponComponent weaponComponent) =>
            {
                OnUpdageCooldown?.Invoke(weaponComponent.ID, 1f, true);
            }).Run();

        Entities.WithoutBurst()
            .WithAll<WeaponComponent, WeaponAttackTimerComponent>()
            .WithNone<WeaponFireReadyTag>()
            .WithNone<WeaponFireTag>()
            .ForEach((ref WeaponComponent weaponComponent, ref WeaponAttackTimerComponent timer) =>
            {
                var ratio = timer.Value / weaponComponent.Delay;
                OnUpdageCooldown?.Invoke(weaponComponent.ID, ratio, false);
            }).Run();
    }
}
#endregion

#region SystemGroup

#endregion