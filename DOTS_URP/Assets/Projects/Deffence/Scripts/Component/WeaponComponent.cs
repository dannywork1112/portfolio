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
    // �߻緮
    public int Amount;
    // �߻� Ƚ��
    public int Count;
    // �߻� ����
    public float Interval;
    // �߻簢
    public float Angle;
    // ��������
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