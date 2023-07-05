public interface IDamagable
{
    void OnDamage(DamageInfo damageInfo);
}
public struct DamageInfo
{
    public float Damage;
    public float StiffTime;
    public float KnockbackDistance;
}