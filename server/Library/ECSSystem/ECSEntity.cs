using Library.Component;

namespace Library.ECSSystem;

public class ECSEntity : IDisposable, IComponentManager, IECSSystemManager
{
    private static long _totalId = 0;
    public long Id { get; }
    private Dictionary<Type, object>? _components = new Dictionary<Type, object>();
    private Dictionary<Type, object>? _systems = new Dictionary<Type, object>();

    public static ECSEntity Of()
    {
        var id = Interlocked.Increment(ref _totalId);
        var entity = new ECSEntity(id);
        return entity;
    }

    private ECSEntity(long id)
    {
        Id = id;
    }
    public void Dispose()
    {
        if (_components != null)
        {
            foreach (var component in _components.Values)
            {
                if (component is IDisposable disposable)
                {
                    disposable.Dispose(); // 리소스는 해제되지만, GC와는 별개                        
                }
            }
            _components.Clear();
            _components = null;
        }
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

    /// <summary>
    /// Component
    /// </summary>    
    public T AddComponent<T>(T component) where T : class, IECSComponent
    {
        if (_components == null)
        {
            _components = new Dictionary<Type, object>();
        }

        _components[typeof(T)] = component;
        return component;
    }


    public void RemoveComponent<T>() where T : class, IECSComponent
    {
        if (_components == null)
            return;

        T? system = GetComponent<T>();
        if (system is IDisposable disposable)
        {
            disposable.Dispose(); // 리소스는 해제되지만, GC와는 별개                        
        }
        _components.Remove(typeof(T));
    }

    public bool HasComponent<T>() where T : class, IECSComponent
    {
        if (_components == null)
            return false;

        return _components.ContainsKey(typeof(T));
    }

    public T? GetComponent<T>() where T : class, IECSComponent
    {
        if (_components == null)
            return null;

        if (_components.TryGetValue(typeof(T), out var component))
        {
            return component as T;
        }

        return null;
    }

    /// <summary>
    /// system
    /// </summary>    
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
            return null;

        if (_systems.TryGetValue(typeof(T), out object? system))
        {
            return system as T;
        }
        return null;

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

