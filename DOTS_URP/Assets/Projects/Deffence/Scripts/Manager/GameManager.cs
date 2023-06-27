using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    class Baker : Baker<GameManager>
    {
        public override void Bake(GameManager authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new TowerAttackDirection
            {
                Direction = float2.zero,
            });
        }
    }
}
