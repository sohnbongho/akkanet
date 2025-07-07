namespace Library.ECSSystem;

public class ECSSystemManager : IECSSystemManager, IDisposable
{
    private Dictionary<Type, object>? _systems = new Dictionary<Type, object>();

    public void Dispose()
    {
        if (_systems != null)
        {
            foreach (var system in _systems.Values)
            {
                if (system is IDisposable disposable)
                {
                    disposable.Dispose(); // 리소스는 해제되지만, GC와는 별개                        
                }
            }
            _systems.Clear();
            _systems = null;
        }
    }

    public T AddSystem<T>(T system) where T : class, IECSSystem
    {
        if (_systems == null)
        {
            _systems = new Dictionary<Type, object>();
        }
        _systems[typeof(T)] = system;
        return system;
    }

    public T? GetSystem<T>() where T : class, IECSSystem
    {
        if (_systems == null)
        {
            _systems = new Dictionary<Type, object>();
        }
        if (false == _systems.TryGetValue(typeof(T), out object? system))
        {
            return null;
        }
        if (system == null)
        {
            return null;
        }
        return system as T;
    }

    public void RemoveSystem<T>() where T : class, IECSSystem
    {
        T? system = GetSystem<T>();
        if (system is IDisposable disposable)
        {
            disposable.Dispose(); // 리소스는 해제되지만, GC와는 별개                        
        }
        if (_systems != null)
        {
            _systems.Remove(typeof(T));
        }
    }
}
