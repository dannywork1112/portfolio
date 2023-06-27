using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class TableContainer : Singleton<TableContainer>
{
    // 테이블은 씬으로 로드후에 파괴시키는게 좋을듯?

    //[SerializeField] private TowerAuthoring[] _towers;
    //[SerializeField] private WeaponAuthoring[] _weapons;
    //[SerializeField] private ProjectileAuthoring[] _projectiles;
    
    [SerializeField] private TowerTable _towerTable;
    [SerializeField] private WeaponTable _weaponTable;
    [SerializeField] private ProjectileTable _projectileTable;
    //[SerializeField] private EnemyTable _enemyTable;
    //[SerializeField] private StageTable _stageTable;

    public TowerTable TowerTable => _towerTable;
    public WeaponTable WeaponTable => _weaponTable;
    public ProjectileTable ProjectileTable => _projectileTable;
    //public EnemyTable EnemyTable => _enemyTable;
    //public StageTable StageTable => _stageTable;


    class Baker : Baker<TableContainer>
    {
        public override void Bake(TableContainer authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent<TableTag>(entity);

            //// Tower
            //var towerBuffer = AddBuffer<TowerEntityElementData>(entity);
            //foreach (var towerData in authoring._towerTable.Datas)
            //{
            //    //if (towerData == null) continue;

            //    var towerEntity = GetEntity(towerData.Prefab, TransformUsageFlags.None);
            //    //var towerComponent = new TowerComponent
            //    //{
            //    //    ID = towerData.ID,
            //    //    MaxHP = towerData.MaxHP,
            //    //    Delay = towerData.Delay,
            //    //    Damage = towerData.Damage,
            //    //    DefaultWeaponID = towerData.DefaultWeaponID,
            //    //};
            //    //SetComponent(towerEntity, in towerComponent);

            //    //towerBuffer.Add(new TowerEntityElementData
            //    //{
            //    //    ID = towerData.ID,
            //    //    MaxHP = towerData.MaxHP,
            //    //    Speed = towerData.Speed,
            //    //    Damage = towerData.Damage,
            //    //    Entity = GetEntity(towerData.Prefab, TransformUsageFlags.None),
            //    //    DefaultWeaponID = towerData.DefaultWeaponID,
            //    //});
            //}

            // Weapon
            //var weaponBuffer = AddBuffer<WeaponEntityElementData>(entity);
            foreach (var weaponData in authoring._weaponTable.Datas)
            {
                if (weaponData == null) continue;

                var weaponEntity = GetEntity(weaponData.Prefab, TransformUsageFlags.None);
                //SetComponent(weaponEntity, new WeaponComponent
                //{
                //    ID = weaponData.ID,
                //    Delay = weaponData.Delay,
                //    FireInfo = weaponData.FireInfo,
                //    ProjectileEntity = GetEntity(weaponData.ProjectilePrefab, TransformUsageFlags.Dynamic),
                //});

                //weaponBuffer.Add(new WeaponEntityElementData
                //{
                //    Entity = GetEntity(weaponData.Prefab, TransformUsageFlags.None),
                //    ID = weaponData.ID,
                //    Delay = weaponData.Delay,
                //    FireInfo = weaponData.FireInfo,
                //});
            }

            // Projectile
            //var projectileBuffer = AddBuffer<ProjectileEntityElementData>(entity);
            foreach (var projectileData in authoring._projectileTable.Datas)
            {
                if (projectileData == null) continue;

                var projectileEntity = GetEntity(projectileData.Prefab, TransformUsageFlags.Dynamic);
                //SetComponent(projectileEntity, new ProjectileComponent
                //{
                //    ID = projectileData.ID,
                //    Speed = projectileData.Speed,
                //});

                //projectileBuffer.Add(new ProjectileEntityElementData
                //{
                //    ID = projectileData.ID,
                //    Speed = projectileData.Speed,
                //    Damage = projectileData.Damage,
                //    Entity = GetEntity(projectileData.Prefab, TransformUsageFlags.Dynamic)
                //});
            }

            //// Enemy
            ////var enemyBuffer = AddBuffer<EnemyEntityElementData>(entity);
            //foreach (var enemyData in authoring._enemyTable.Datas)
            //{
            //    if (enemyData == null) continue;

            //    var enemyEntity = GetEntity(enemyData.Prefab, TransformUsageFlags.Dynamic);

            //    //enemyBuffer.Add(new EnemyEntityElementData
            //    //{
            //    //    ID = enemyData.ID,
            //    //    MaxHP = enemyData.MaxHP,
            //    //    Speed = enemyData.Speed,
            //    //    Damage = enemyData.Damage,
            //    //    AttackSpeed = enemyData.AttackSpeed,
            //    //    Entity = GetEntity(enemyData.Prefab, TransformUsageFlags.Dynamic)
            //    //});
            //}
        }
    }
}

public struct TableTag : IComponentData { }

//public struct TowerEntityElementData : IBufferElementData
//{
//    public int ID;
//    public float MaxHP;
//    public float Speed;
//    public float Damage;
//    public Entity Entity;
//    public int DefaultWeaponID;
//}

//public struct WeaponEntityElementData : IBufferElementData
//{
//    public Entity Entity;
//    public int ID;
//    public float Delay;
//    public WeaponFireInfo FireInfo;
//}

//public struct ProjectileEntityElementData : IBufferElementData
//{
//    public int ID;
//    public float Speed;
//    public float Damage;
//    public Entity Entity;
//}

//public struct EnemyEntityElementData : IBufferElementData
//{
//    public int ID;
//    public float MaxHP;
//    public float Speed;
//    public float Damage;
//    public float AttackSpeed;
//    public Entity Entity;
//}