using Library.Component;

namespace Library.AkkaActors.Socket.Handler;

public class SessionComponentHandler : IComponentManager, IDisposable
{
    // Component 패턴
    private ComponentManager? _componentManager = new ComponentManager();
    public T AddComponent<T>(T component) where T : class, IECSComponent
    {
        if (_componentManager == null)
        {
            _componentManager = new();
        }
        _componentManager.AddComponent<T>(component);
        return component;
    }
    public T? GetComponent<T>() where T : class, IECSComponent
    {
        if (_componentManager == null)
        {
            return null;
        }

        return _componentManager.GetComponent<T>();
    }

    public void RemoveComponent<T>() where T : class, IECSComponent
    {
        if (_componentManager == null)
        {
            return;
        }

        _componentManager.RemoveComponent<T>();
    }

    public void Init()
    {
        _componentManager = new ComponentManager();

    }
    public void Dispose()
    {
        if (_componentManager != null)
        {
            _componentManager.Dispose();
            _componentManager = null;
        }
    }

}
