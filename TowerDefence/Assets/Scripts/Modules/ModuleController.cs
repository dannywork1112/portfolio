using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;

public class ModuleController : MonoBehaviour
{
    [SerializeField] private Transform _modulesParent;

    [SerializeField, ReadOnly] private List<Module> _modules;

    public T Create<T>() where T : Module
    {
        var moduleObject = new GameObject(nameof(T));
        moduleObject.transform.SetParent(_modulesParent?.transform);
        var module = moduleObject.AddComponent<T>();
        return module;
    }
    public void Destroy(Module module)
    {
        if (module != null) Destroy(module.gameObject);
    }
    public void Regist(Module module)
    {
        if (!_modules.Contains(module)) return;
        _modules.Add(module);
        module.OnInitialization();
    }
    public void Unregist(Module module)
    {
        if (_modules.Contains(module)) return;
        _modules.Remove(module);
        module.OnExit();
    }
    public Module GetModule<T>() where T : Module
    {
        var ability = _modules.Find(x => x.GetType().Equals(typeof(T)));
        return ability;
    }

    public void Update()
    {
        for (int i = 0; i < _modules.Count; i++)
        {
            var ability = _modules[i];
            if (!ability.Enabled) continue;
            ability.OnUpdate();
        }
    }
    public void FixedUpdate()
    {
        for (int i = 0; i < _modules.Count; i++)
        {
            var ability = _modules[i];
            if (!ability.Enabled) continue;
            ability.OnFixedUpdate();
        }
    }
    private void LateUpdate()
    {
        for (int i = 0; i < _modules.Count; i++)
        {
            var ability = _modules[i];
            if (!ability.Enabled) continue;
            ability.OnLateUpdate();
        }
    }
}