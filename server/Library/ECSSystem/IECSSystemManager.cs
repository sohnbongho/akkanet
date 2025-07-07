namespace Library.ECSSystem;

public interface IECSSystemManager
{
    T AddSystem<T>(T component) where T : class, IECSSystem;
    T? GetSystem<T>() where T : class, IECSSystem;
    void RemoveSystem<T>() where T : class, IECSSystem;
}
