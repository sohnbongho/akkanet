using Library.ECSSystem;

namespace Library.AkkaActors.Socket.Handler;

public class SessionSystemHandler : IECSSystemManager, IDisposable
{
    private ECSSystemManager? _manager = new ECSSystemManager();

    public T AddSystem<T>(T component) where T : class, IECSSystem
    {
        if (_manager == null)
        {
            _manager = new ECSSystemManager();
        }

        return _manager.AddSystem<T>(component);
    }
    public T? GetSystem<T>() where T : class, IECSSystem
    {
        if (_manager == null)
        {
            return null;
        }
        return _manager.GetSystem<T>();
    }
    public void RemoveSystem<T>() where T : class, IECSSystem
    {
        if (_manager == null)
        {
            return;
        }
        _manager.RemoveSystem<T>();
    }

    public void Dispose()
    {
        if (_manager != null)
        {
            _manager.Dispose();
            _manager = null;
        }
    }
    public void Init()
    {
        _manager = new();
    }
}
