using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public int ID;
    public float MaxHP;
    public float Damage;
    public float MoveSpeed;
    public float AttackDelay;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddSharedComponent(entity, new EnemyComponent
            {
                ID = authoring.ID,
            });
            AddComponent(entity, new MoveComponent
            {
                Value = authoring.MoveSpeed,
            });
            AddComponent(entity, new HitPointComponent
            {
                Value = authoring.MaxHP,
            });
            AddComponent(entity, new AttackPowerComponent
            {
                Value = authoring.Damage,
            });
        }
    }
}

//public readonly partial struct EnemyAspect : IAspect
//{
//    public readonly Entity Entity;

//    private readonly RefRW<EnemyComponent> _enemyComponent;
//}