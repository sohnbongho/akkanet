namespace Library.Component;

public interface IComponentManager
{
    T AddComponent<T>(T component) where T : class, IECSComponent;
    T? GetComponent<T>() where T : class, IECSComponent;
    void RemoveComponent<T>() where T : class, IECSComponent;
}
