using UnityEngine;

public class Pistol : Weapon
{
    public override void Fire(Vector3 direction)
    {
        CreateProjectile(_data.ProjectileID, direction);

        ResetCooldown();
    }
}
