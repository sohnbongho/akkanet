using Akka.Actor;
using Library.Component;
using Library.ECSSystem;

namespace Library.AkkaActors;

public abstract class ECSReceiveActor : ReceiveActor, IComponentManager, IECSSystemManager
{
    private ComponentManager? _componentManager = null;
    private ECSSystemManager? _systemManager = null;
    public ECSReceiveActor()
    {

    }

    /// <summary>
    /// Component 관리
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    public virtual T AddComponent<T>(T component) where T : class, IECSComponent
    {
        if (_componentManager == null)
        {
            _componentManager = new();
        }

        return _componentManager.AddComponent<T>(component);
    }

    public virtual T? GetComponent<T>() where T : class, IECSComponent
    {
        if (_componentManager == null)
            return null;

        return _componentManager.GetComponent<T>();
    }

    public virtual void RemoveComponent<T>() where T : class, IECSComponent
    {
        if (_componentManager == null)
            return;

        _componentManager.RemoveComponent<T>();
    }
    public virtual void DisposeComponent()
    {
        if (_componentManager != null)
        {
            _componentManager.Dispose();
            _componentManager = null;
        }
    }

    /// <summary>
    /// ss
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    public virtual T AddSystem<T>(T component) where T : class, IECSSystem
    {
        if (_systemManager == null)
        {
            _systemManager = new();
        }
        return _systemManager.AddSystem<T>(component);
    }

    public virtual T? GetSystem<T>() where T : class, IECSSystem
    {
        if (_systemManager == null)
            return null;

        return _systemManager.GetSystem<T>();
    }

    public virtual void RemoveSystem<T>() where T : class, IECSSystem
    {
        if (_systemManager == null)
            return;

        _systemManager.RemoveSystem<T>();
    }
    public virtual void DisposeSystem()
    {
        if (_systemManager != null)
        {
            _systemManager.Dispose();
            _systemManager = null;
        }
    }


    protected override void PreStart()
    {
        base.PreStart();
    }

    protected override void PostStop()
    {
        try
        {
            DisposeSystem();
            DisposeComponent();
        }
        finally
        {
            base.PostStop();
        }
    }

}
