using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

#region System
public partial struct GameManagementSystem : ISystem
{
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        //var entity = state.EntityManager.CreateEntity();
        //var buffer = state.EntityManager.AddBuffer<WeaponCreateElementData>(entity);
        //buffer.Add(new WeaponCreateElementData
        //{
        //    WeaponID = 123123,
        //});

        //// Weapon Spawn
        //var towerEntity = SystemAPI.GetSingletonEntity<TowerTag>();
        //var weaponEntityBuffer = state.EntityManager.GetBuffer<WeaponEntityElementData>(towerEntity);
        //var weaponCreateBuffer = state.EntityManager.GetBuffer<CreateReadyWeaponElementData>(towerEntity);

        //foreach (var readyWeaponElement in SystemAPI.Query<DynamicBuffer<CreateReadyWeaponElementData>>())
        //{
        //    var weaponEntityElement = weaponEntityBuffer.Where(x => x.ID == 0).FirstOrDefault();

        //    var weaponEntity = state.EntityManager.Instantiate(weaponEntityElement.Entity);
        //    if (weaponEntity != Entity.Null)
        //    {

        //    }
        //}
    }
}
#endregion

public partial struct WeaponCreateJob : IJob
{
    public void Execute()
    {

    }
}