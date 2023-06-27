using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private Vector2 _limitHeight = new Vector2(35, 85);

    private Transform _transform;
    private Vector2 _rotateDelta;

    public Transform Transform => _transform;

    private void Awake()
    {
        _transform = transform;
    }
    private void Start()
    {
        //InputManager.Instance.OnDragDelta += RotateCamera;
    }
    private void OnDestroy()
    {
        //if (InputManager.Instance != null)
        //{
        //    InputManager.Instance.OnDragDelta -= RotateCamera;
        //}
    }
    private void Update()
    {
        UpdateRotate();
    }

    public void SetRotateDelta(Vector2 delta)
    {
        _rotateDelta = delta;
    }

    public void UpdateRotate()
    {
        if (_rotateDelta == Vector2.zero) return;

        var eulerAngles = _transform.rotation.eulerAngles;

        var x = -_rotateDelta.y * _rotationSpeed;
        var y = _rotateDelta.x * _rotationSpeed;

        if (eulerAngles.x + x < _limitHeight.x) eulerAngles.x = _limitHeight.x;
        else if (eulerAngles.x + x > _limitHeight.y) eulerAngles.x = _limitHeight.y;
        else eulerAngles.x += x;
        eulerAngles.y += y;

        _transform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0f);
    }
}
