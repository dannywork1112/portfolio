using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    private static object _lock = new object();

    internal static bool ApplicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (ApplicationIsQuitting) return null;

            lock (_lock)
            {
                if (_instance == null) CreateInstance();
                return _instance;
            }
        }
    }
    private void Awake()
    {
        if (!Application.isPlaying) return;

        if (_instance == null)
        {
            CreateInstance();

            _OnAwake();
        }
        else
        {
            if (!IsSameObject())
            {
                Destroy(gameObject);
            }
        }
    }
    private void OnDestroy()
    {
        ApplicationIsQuitting = true;
        _OnDestroy();
    }
    protected virtual void OnApplicationQuit()
    {
        ApplicationIsQuitting = true;
        _instance = null;
    }

    private bool IsSameObject()
    {
        if (gameObject == null) return true;
        return _instance.gameObject.GetHashCode() == gameObject.GetHashCode();
    }

    private static void CreateInstance()
    {
        ApplicationIsQuitting = false;

        _instance = FindObjectOfType<T>();

        if (_instance == null)
        {
            var singletonObject = new GameObject();
            _instance = singletonObject.AddComponent<T>();
            singletonObject.name = $"{typeof(T).Name}Singleton";
            DontDestroyOnLoad(_instance);
        }
    }
    public static bool HasInstance
    {
        get
        {
            return _instance != null && !ApplicationIsQuitting;
        }
    }

    protected virtual void _OnAwake() { }
    protected virtual void _OnDestroy() { }
}