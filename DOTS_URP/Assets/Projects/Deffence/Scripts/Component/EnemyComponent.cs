using Unity.Entities;

#region Tag
#endregion

#region ComponentData
public struct EnemyComponent : ISharedComponentData
{
    public int ID;
}
public struct MoveComponent : IComponentData
{
    public float Value;
}
public struct HitPointComponent : IComponentData
{
    public float Value;
}
public struct AttackPowerComponent : IComponentData
{
    public float Value;
}
#endregion

#region BufferElement
public struct DamageComponent : IBufferElementData
{
    public float Value;
    public Entity Owner;
}
#endregion
