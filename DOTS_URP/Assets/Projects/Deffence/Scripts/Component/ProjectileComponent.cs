using Unity.Entities;
using Unity.Mathematics;

#region Tag
public struct ProjectileTag : IComponentData { }
#endregion

#region ComponentData
public struct ProjectileComponent : IComponentData
{
    public int ID;
    public float Speed;
}
public struct ProjectileLifeTimeComponent : IComponentData
{
    public float LifeTime;
}
public struct ProjectileInitalizeComponent : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
    public float Scale;
}
#endregion

#region BufferElement
public struct CreateReadyProjectileElementData : IBufferElementData
{
    public int Amount;
    public float Timer;
    public float Angle;
    public bool Random;
    public Entity Entity;
}
#endregion