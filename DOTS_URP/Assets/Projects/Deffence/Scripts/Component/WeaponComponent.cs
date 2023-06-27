using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

#region Tag
public struct WeaponFireReadyTag : IComponentData { }
public struct WeaponFireTag : IComponentData { }
#endregion

[System.Serializable]
public struct WeaponFireInfo
{
    // 발사량
    public int Amount;
    // 발사 횟수
    public int Count;
    // 발사 간격
    public float Interval;
    // 발사각
    public float Angle;
    // 랜덤여부
    public bool Random;
}

#region ComponentData
public struct WeaponComponent : IComponentData
{
    public int ID;
    public float Delay;
    public WeaponFireInfo FireInfo;
    public Entity ProjectileEntity;
}
public struct WeaponAttackTimerComponent : IComponentData
{
    public float Value;
}
public struct WeaponFireDirectionComponent : IComponentData
{
    public float3 Direction;
}
#endregion

#region BufferElement
#endregion