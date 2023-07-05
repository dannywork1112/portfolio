using System;
using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;

[System.Serializable]
public struct TowerData
{
    public string ID;
    public float MaxHP;
    public float FireDelay;
}

public class Tower : MonoBehaviour, IDamagable
{
    [Header("ObjectParents")]
    [SerializeField] private Transform _weaponObjectParent;

    [Header("Datas")]
    [SerializeField, ReadOnly] private TowerData _data;
    [SerializeField] private LayerMask _targetLayer;

    [Header("Weapons")]
    [SerializeField, ReadOnly] private List<Weapon> _weapons;
    [SerializeField, ReadOnly] private Queue<Weapon> _fireReadyWeapons;

    private Transform _transform;
    [SerializeField, ReadOnly] private float _curHP;
    [SerializeField, ReadOnly] private bool _isDead;

    [SerializeField, ReadOnly] private float _fireDelay;

    public Transform Transform => _transform;
    public LayerMask TargetLayerMask => _targetLayer;
    public Vector3 Position => _transform.position;
    public bool FireReady => _fireDelay <= 0f;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _transform = transform;
    }
    private void Update()
    {
        UpdateCooldown();
    }

    private void UpdateCooldown()
    {
        if (FireReady) return;

        _fireDelay -= Time.deltaTime;
        if (FireReady) _fireDelay = 0f;
    }

    #region IDamagable
    public float CurHP => _curHP;
    public Action<float> OnDamageEvent;
    public Action OnDeadEvent;
    public void OnDamage(DamageInfo damageInfo)
    {
        _curHP -= damageInfo.Damage;
        if (_curHP > 0f)
        {
            OnDamageEvent?.Invoke(damageInfo.Damage);
        }
        else
        {
            _curHP = 0f;
            _isDead = true;
            OnDeadEvent?.Invoke();
        }
    }
    #endregion

    #region Tower
    public void Initialization(TowerTableData tableData)
    {
        _data = new TowerData
        {
            ID = tableData.ID,
            MaxHP = tableData.MaxHP,
            FireDelay = tableData.FireDelay,
        };

        _weapons ??= new();
        for (int i = 0; i < _weapons.Count; i++)
            Destroy(_weapons[i].gameObject);
        _weapons.Clear();

        _fireReadyWeapons ??= new();
        _fireReadyWeapons.Clear();

        _isDead = false;
        _curHP = _data.MaxHP;
        _fireDelay = _data.FireDelay;

        _transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
    public void Fire(Vector2 direction)
    {
        if (!FireReady) return;
        if (_fireReadyWeapons.Count < 1) return;

        var camEulerY = GameSceneManager.Instance.CameraController.Transform.eulerAngles.y;
        var inputDir = new Vector3(direction.x, 0f, direction.y);
        var rotation = Quaternion.AngleAxis(camEulerY, Vector3.up);
        var fireDir = rotation * inputDir;

        var weapon = _fireReadyWeapons.Dequeue();
        weapon.Fire(fireDir);

        ResetCooldown(_data.FireDelay);
    }
    public void ResetCooldown(float value)
    {
        _fireDelay = value;
    }
    #endregion

    #region Weapon
    public bool HasWeapon(string id) => _weapons.Exists(x => x.Data.ID == id);
    public Weapon GetWeapon(string id)
    {
        var weapon = _weapons.Find(x => x.Data.ID == id);
        return weapon;
    }
    public void AddWeapon(string id)
    {
        if (_weapons.Exists(x => x.Data.ID == id)) return;

        var weaponData = TableManager.Instance.WeaponTable.GetData(id);
        if (weaponData == null) return;

        var weapon = Instantiate(weaponData.Prefab, _weaponObjectParent);
        weapon.Initialization(this, weaponData);
        weapon.OnFireReady += AddFireReadyWeapon;

        _weapons.Add(weapon);
    }
    public void RemoveWeapon(string id)
    {
        var index = _weapons.FindIndex(x => x.Data.ID == id);
        if (index < 0 || index > _weapons.Count - 1) return;
        _weapons.RemoveAt(index);
        Destroy(_weapons[index].gameObject);
    }
    public void RemoveWeapon(Weapon weapon)
    {
        _weapons.Remove(weapon);
        Destroy(weapon.gameObject);
    }
    private void AddFireReadyWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        if (_fireReadyWeapons.Contains(weapon)) return;

        _fireReadyWeapons.Enqueue(weapon);
        Debug.Log($"{this.name} is Ready!");
    }
    #endregion
}
