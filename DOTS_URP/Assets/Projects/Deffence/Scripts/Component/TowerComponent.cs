using Unity.Entities;

#region Tag
public struct TowerTag : IComponentData { }
public struct TowerAttackReadyTag : IComponentData { }
#endregion

#region ComponentData
public struct TowerComponent : IComponentData
{
    public int ID;
    public float MaxHP;
    public float Delay;
    public float Damage;
}
public struct TowerAttackTimerComponent : IComponentData
{
    public float Timer;
}
#endregion

#region BufferElement
//public struct CreateReadyWeaponElementData : IBufferElementData
//{
//    public int WeaponID;
//}
public struct AttackReadyWeaponElementData : IBufferElementData
{
    public int WeaponID;
}
#endregion