using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [FormerlySerializedAs("movementRange")]
    [SerializeField]
    private float m_MovementRange = 50;

    private Vector3 m_StartPos;
    private Vector2 m_PointerDownPos;
    private Vector2 _dragDelta;

    #region Events
    public UnityEvent<Vector2> OnDragDelta;
    #endregion

    public float movementRange
    {
        get => m_MovementRange;
        set => m_MovementRange = value;
    }

    private void Start()
    {
        m_StartPos = ((RectTransform)transform).anchoredPosition;
    }

    private void OnEnable()
    {
        //InputManager.Instance.OnTouchPoint += OnMoveJoystick;
    }
    private void OnDisable()
    {
        //if (InputManager.Instance != null)
        //{
        //    InputManager.Instance.OnTouchPoint -= OnMoveJoystick;
        //}
    }
    private void Update()
    {
        if (_dragDelta == Vector2.zero) return;

        OnDragDelta?.Invoke(_dragDelta.normalized);
    }

    private void OnMoveJoystick(Vector2 value)
    {
        m_PointerDownPos = value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerDown");
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out m_PointerDownPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
        var delta = position - m_PointerDownPos;

        delta = Vector2.ClampMagnitude(delta, movementRange);
        ((RectTransform)transform).anchoredPosition = m_StartPos + (Vector3)delta;

        //Debug.Log($"Position : {position} / Delta : {delta}");
        _dragDelta = delta.normalized;

        //var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        //SendValueToControl(newPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ((RectTransform)transform).anchoredPosition = m_StartPos;
        //SendValueToControl(Vector2.zero);

        _dragDelta = Vector2.zero;
        OnDragDelta?.Invoke(Vector2.zero);
    }
}
