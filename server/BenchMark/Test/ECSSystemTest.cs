using Library.ECSSystem;

namespace BenchMark.Test;

public class ECSSystemTest
{
    private interface IAA : IECSSystem
    {
        void Test();

    }
    private class BB : IAA
    {
        public void Test()
        {
            Console.WriteLine("BB Test");
        }
    }
    private class CC : IAA
    {
        public void Test()
        {
            Console.WriteLine("CC Test");
        }
    }

    private ECSSystemManager? _systemManager = new ECSSystemManager();
    public T AddSystem<T>(T component) where T : class, IECSSystem
    {
        if (_systemManager == null)
        {
            _systemManager = new();
        }

        return _systemManager.AddSystem<T>(component);
    }

    public T? GetSystem<T>() where T : class, IECSSystem
    {
        return _systemManager?.GetSystem<T>() ?? null;
    }

    public void RemoveSystem<T>() where T : class, IECSSystem
    {
        _systemManager?.RemoveSystem<T>();
    }
    public void Test()
    {
        AddSystem<IAA>(new BB());

        var system = GetSystem<IAA>();

        system?.Test();        

        var bb1 = system as BB;
        if (bb1 == null)
        {
            Console.WriteLine("system is null");
        }
        else
        {
            bb1.Test();
        }

    }

}
