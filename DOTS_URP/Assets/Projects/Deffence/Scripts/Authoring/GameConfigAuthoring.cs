using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

// 게임 설정 데이터
public struct GameConfigComponent : IComponentData
{
    public int TowerID;
    public int WeaponID;
    public Random RandomData;
    public float3 AttackDirection;
}

public class GameConfigAuthoring : MonoBehaviour
{
    class Baker : Baker<GameConfigAuthoring>
    {
        public override void Bake(GameConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new GameConfigComponent
            {
                TowerID = -1,
                WeaponID = -1,
                AttackDirection = float3.zero,
                RandomData = new Random(),
            });
        }
    }
}