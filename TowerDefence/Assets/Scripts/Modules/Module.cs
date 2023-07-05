using UnityEngine;

public abstract class Module : MonoBehaviour
{
    internal bool Enabled;

    protected ModuleController _controller;

    private void OnEnable() 
    {
        AttachController();
        if (_controller == null) Enabled = false;
        else _controller.Regist(this);
    }
    private void OnDisable()
    {
        if (_controller != null)
            _controller.Unregist(this);
    }

    protected void AttachController() => _controller = GetComponentInParent<ModuleController>();

    public virtual void OnInitialization() { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnExit() { }
}
