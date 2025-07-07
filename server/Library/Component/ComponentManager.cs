namespace Library.Component;

public class ComponentManager : IComponentManager, IDisposable
{
    private Dictionary<Type, object>? _components = new Dictionary<Type, object>();

    public T AddComponent<T>(T component) where T : class, IECSComponent
    {
        if (_components == null)
        {
            _components = new Dictionary<Type, object>();
        }
        _components[typeof(T)] = component;
        return component;
    }


    public T? GetComponent<T>() where T : class, IECSComponent
    {
        if (_components == null)
        {
            _components = new Dictionary<Type, object>();
        }
        if (false == _components.TryGetValue(typeof(T), out object? component))
        {
            return null;
        }
        if (component == null)
        {
            return null;
        }
        return component as T;
    }

    public void RemoveComponent<T>() where T : class, IECSComponent
    {
        T? component = GetComponent<T>();
        if (component is IDisposable disposable)
        {
            disposable.Dispose(); // 리소스는 해제되지만, GC와는 별개                        
        }
        if (_components != null)
        {
            _components.Remove(typeof(T));
        }
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
    }
}
