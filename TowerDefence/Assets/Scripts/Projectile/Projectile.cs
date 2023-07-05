using Unity.VisualScripting;
using UnityAtoms;
using UnityEngine;

[System.Serializable]
public struct ProjectileData
{
    public string ID;
    public float Speed;
    public float Damage;
    public float LifeTime;
    public float StiffTime;
    public float KnockbackDistance;
}

[System.Serializable]
public struct ProjectileProperty
{

}

public class Projectile : MonoBehaviour
{
    [SerializeField, ReadOnly] private ProjectileData _data;

    private Weapon _weapon;

    private Transform _transform;
    private bool _collision;

    [SerializeField, ReadOnly] private float _lifeTime;

    public float Damage => _data.Damage + _weapon.Data.Damage;

    private void Awake()
    {
        _transform = transform;
        _collision = false;
    }
    private void Update()
    {
        UpdateCooldown();
        UpdateMovement();
    }
    //public void FixedUpdate()
    //{
    //    UpdateMovement();

    //    //var scalar = _property.Speed * Time.fixedDeltaTime;

    //    //// ray로 충돌체크
    //    //var ray = new Ray(_transform.position, _transform.forward);
    //    //if (Physics.SphereCast(ray, 0.2f, out var hit, scalar, _data.LayerMask))
    //    //{
    //    //    Debug.Log("Hit");
    //    //    var enemy = hit.transform.GetComponent<Enemy>();
    //    //    if (enemy == null) return;
    //    //    enemy.OnHit(_data.Damage);
    //    //    Destroy(gameObject);
    //    //}
    //    //else
    //    //{
    //    //    // position 업데이트
    //    //    var movement = _transform.forward * scalar;
    //    //    _transform.Translate(movement, Space.World);
    //    //}

    //    //// position 업데이트
    //    //var movement = _transform.forward * scalar;
    //    //_transform.Translate(movement, Space.World);

    //    //_transform.SetPositionAndRotation(_transform.position + movement, _transform.rotation);

    //    //// 충돌 체크
    //    //var colliders = Physics.OverlapSphere(_transform.position, 0.2f);
    //    //if (colliders.Length > 0 )
    //    //{
    //    //    Debug.Log("Collision!!");
    //    //    Destroy(gameObject);
    //    //}
    //}
    //private void OnDrawGizmos()
    //{
    //    var direction = _transform.forward * _data.Speed * Time.deltaTime;
    //    Gizmos.DrawRay(_transform.position, direction);
    //    Gizmos.DrawWireSphere(_transform.position + direction, 0.2f);
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (_collision) return;
        var enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || enemy.IsDead) return;

        if (enemy is IDamagable damagable)
        {
            damagable.OnDamage(new DamageInfo 
            {
                Damage = _data.Damage,
                StiffTime = _data.StiffTime,
                KnockbackDistance = _data.KnockbackDistance,
            });
        }

        _collision = true;
        Destroy(this.gameObject);
    }

    protected virtual void UpdateCooldown()
    {
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0f)
        {
            Destroy(this.gameObject);
        }
    }

    #region Projectile
    public void Initialization(Weapon weapon, ProjectileTableData tableData)
    {
        _weapon = weapon;

        _data = new ProjectileData
        {
            ID = tableData.ID,
            LifeTime = tableData.LifeTime,
            Speed = tableData.Speed,
            Damage = _weapon.Data.Damage + tableData.Damage,
            StiffTime = tableData.StiffTime,
            KnockbackDistance = tableData.KnockbackDistance,
        };

        _lifeTime = _data.LifeTime;
    }
    //public virtual void Initialization(float speed, float damage)
    //{
    //    _property.Speed = speed;
    //    _property.Damage = damage;
    //}
    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        _transform.SetPositionAndRotation(position, rotation);
    }
    public virtual void UpdateMovement()
    {
        var movement = _transform.forward * _data.Speed * Time.deltaTime;
        _transform.Translate(movement, Space.World);
    }
    #endregion
}