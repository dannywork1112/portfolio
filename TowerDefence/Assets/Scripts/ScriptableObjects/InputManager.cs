using System;
using UnityEngine;
using static InputManager;

public class InputManager : Singleton<InputManager>, PlayerControls.IInputActionActions, PlayerControls.ICameraActions
{
    private PlayerControls _playerControls;
    private bool _isTouch;
    private bool _enabled;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (_enabled)
            {
                _playerControls.InputAction.Enable();
                _playerControls.Camera.Enable();
                _playerControls.UI.Enable();
            }
            else
            {
                _playerControls.InputAction.Disable();
                _playerControls.Camera.Disable();
                _playerControls.UI.Disable();
            }
        }
    }

    #region Events
    public delegate void MoveJoystick(Vector2 delta);
    public event MoveJoystick OnMoveJoystick;

    #region InputAction
    public delegate void TouchContact(bool value);
    public event TouchContact OnTouchContact;
    public delegate void TouchPoint(Vector2 value);
    public event TouchPoint OnTouchPoint;
    #endregion

    #region Camera
    public delegate void Rotate(Vector2 value);
    public event Rotate OnDragDelta;
    #endregion
    #endregion

    protected override void _OnAwake()
    {
        base._OnAwake();

        _playerControls = new PlayerControls();
        _playerControls.InputAction.SetCallbacks(this);
        _playerControls.Camera.SetCallbacks(this);
        // UI, 클릭 구성

        _isTouch = false;
    }

    public void OnClick(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log($"OnClick : {context.phase}");
        //if (context.phase == UnityEngine.InputSystem.InputActionPhase.Started)
        //    OnTouchPoint?.Invoke(context.ReadValue<Vector2>());

        //if (context.phase == UnityEngine.InputSystem.InputActionPhase.Started)
        //{
        //    _isTouch = true;
        //    //OnTouchPoint?.Invoke(context.ReadValue<Vector2>());
        //}
        //else if (context.phase == UnityEngine.InputSystem.InputActionPhase.Canceled)
        //    _isTouch = false;
    }

    public void OnPoint(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //if (_isTouch && context.phase == UnityEngine.InputSystem.InputActionPhase.Performed)
        //    OnTouchPoint?.Invoke(context.ReadValue<Vector2>());
        //Debug.Log($"OnPoint : {context.phase}");
    }

    #region Actions

    #region InputAction
    public void OnTouchPress(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log($"OnTouchPress : {context.phase}");
        if (context.phase == UnityEngine.InputSystem.InputActionPhase.Started)
        {
            //OnTouch?.Invoke(true);
            _isTouch = true;
        }
        else if (context.phase == UnityEngine.InputSystem.InputActionPhase.Canceled)
        {
            //OnTouch?.Invoke(false);
            _isTouch = false;
        }
    }
    public void OnTouchPosition(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log($"OnTouchPosition : {context.phase}");
        if (context.phase == UnityEngine.InputSystem.InputActionPhase.Started) return;

        var value = context.ReadValue<Vector2>();
        if (value == Vector2.zero) return;

        //var touchPosition = new Vector2(value.x - (Screen.width * 0.5f), value.y - (Screen.height * 0.5f));
        OnTouchPoint?.Invoke(value);


        //var worldPosition = Camera.main.ScreenToWorldPoint(value);

        //Debug.Log($"OnTouchPosition : {context.phase} - {value} - {worldPosition}");//- {touchPosition} 
        //var obj = new GameObject("Sphere");
        //obj.transform.position = worldPosition;
    }
    #endregion
    #region Camera
    public void OnRotate(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //if (!_isTouch) return;
        //if (context.phase != UnityEngine.InputSystem.InputActionPhase.Performed) return;

        //var value = context.ReadValue<Vector2>();
        //Debug.Log($"OnRotate : {context.phase} - {value}");
        //OnDragDelta?.Invoke(value);
    }
    #endregion

    #endregion

    public void OnPress(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log($"OnPress : {context.phase}");
    }

    public void OnTest(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //OnMoveJoystick?.Invoke(context.ReadValue<Vector2>());
        Debug.Log($"OnTest : {context.phase}");
    }
}
