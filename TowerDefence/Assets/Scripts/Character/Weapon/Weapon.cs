using System;
using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;


[System.Serializable]
public struct WeaponData
{
    public string ID;
    public float Damage;
    public float FireDelay;
    public string ProjectileID;
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected Vector3 _projectileSpawnOffset = new Vector3(0f, 1f, 0f);

    [SerializeField, ReadOnly] protected WeaponData _data;

    [SerializeField, ReadOnly] protected float _coolDown;
    [SerializeField, ReadOnly] protected bool _fireable;

    protected Tower _tower;

    public event Action<Weapon> OnFireReady;

    public WeaponData Data => _data;

    private void Update()
    {
        UpdateCooldown();
    }

    protected virtual void UpdateCooldown()
    {
        if (_tower == null) return;
        if (_fireable) return;

        _coolDown -= Time.deltaTime;
        if (_coolDown <= 0f)
        {
            _coolDown = 0f;
            _fireable = true;
            OnFireReady?.Invoke(this);
        }
    }

    #region Weapon
    public virtual void Initialization(Tower tower, WeaponTableData tableData)
    {
        _tower = tower;

        _data = new WeaponData
        {
            ID = tableData.ID,
            Damage = tableData.Damage,
            FireDelay = tableData.FireDelay,
            ProjectileID = tableData.DefaultProjectileID,
        };

        ResetCooldown();
    }
    public void ResetCooldown()
    {
        _fireable = false;
        _coolDown = _data.FireDelay;
    }
    public virtual void Fire(Vector3 direction) { }
    #endregion

    #region Projectile
    public void ChangeProjectile(string projectileID)
    {
        _data.ProjectileID = projectileID;
    }
    public Projectile CreateProjectile(string projectileID)
    {
        var projectileData = TableManager.Instance.ProjectileTable.GetData(projectileID);
        if (projectileData == null) return null;

        var projectile = Instantiate(projectileData.Prefab);
        projectile.Initialization(this, projectileData);

        return projectile;
    }
    public Projectile CreateProjectile(string projectileID, Vector3 direction)
    {
        var projectile = CreateProjectile(projectileID);
        if (projectile == null) return null;

        var position = _tower.Position + _projectileSpawnOffset + direction;
        var rotation = Quaternion.LookRotation(direction);
        projectile.SetPositionAndRotation(position, rotation);
        return projectile;
    }
    public Projectile CreateProjectile(string projectileID, Vector3 position, Quaternion rotation)
    {
        var projectile = CreateProjectile(projectileID);
        if (projectile == null) return null;

        projectile.SetPositionAndRotation(position, rotation);
        return projectile;
    }
    public Projectile[] CreateProjectileSpread(string projectileID, Vector3 direction, int spreadCount, float spreadAngle)
    {
        var projectiles = new List<Projectile>();
        var angleStep = spreadAngle / spreadCount;

        var position = _tower.Position + _projectileSpawnOffset + direction;
        for (int i = 0; i < spreadCount; i++)
        {
            var angle = -(spreadAngle * 0.5f) + (angleStep * 0.5f) + angleStep * i;

            var rotation = Quaternion.LookRotation(direction) * Quaternion.AngleAxis(angle, Vector3.up);

            var projectile = CreateProjectile(projectileID, position, rotation);
            if (projectile != null)
            {
                projectiles.Add(projectile);
            }
        }
        return projectiles.ToArray();
    }
    #endregion
}
