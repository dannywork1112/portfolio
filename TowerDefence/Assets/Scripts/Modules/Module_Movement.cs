using UnityEngine;

public class Module_Movement : Module
{
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _rotationSpeed;

    private Transform _movementTargetTransform;
    private Vector3 _direction;

    public float MovementSpeed
    {
        get => _movementSpeed;
        set => _movementSpeed = value;
    }
    public float RotationSpeed
    {
        get => _rotationSpeed;
        set => _rotationSpeed = value;
    }

    public override void OnInitialization()
    {
        base.OnInitialization();

        _direction = Vector3.zero;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (_movementTargetTransform == null) return;

        // 회전
        if (_direction != Vector3.zero)
        {
            var eulerAngle = Quaternion.LookRotation(_direction).eulerAngles;
            var slerpEulerAngle = Vector3.zero;

            if (_rotationSpeed > 0)
                slerpEulerAngle = Vector3.Slerp(_movementTargetTransform.eulerAngles, eulerAngle, Time.deltaTime * _rotationSpeed);
            else
                slerpEulerAngle = eulerAngle;

            _movementTargetTransform.Rotate(slerpEulerAngle);

            var temp = _direction - _movementTargetTransform.forward;
            if (temp.sqrMagnitude < 0.1f)
                _direction = Vector3.zero;
        }

        // 이동
        _movementTargetTransform.Translate(_movementTargetTransform.forward * _movementSpeed * Time.deltaTime);
    }

    public void SetMovementTarget(Transform target) => _movementTargetTransform = target;
    public virtual void SetDirection(Vector3 direction)
    {
        if (_movementTargetTransform == null) return;

        _direction = direction;
        _direction.y = 0f;
        _direction.Normalize();
    }
    public void Initialization(Transform target, float moveSpeed, float rotateSpeed, Vector3 direction)
    {
        MovementSpeed = moveSpeed;
        RotationSpeed = rotateSpeed;
        SetMovementTarget(target);
        SetDirection(direction);
    }
}
