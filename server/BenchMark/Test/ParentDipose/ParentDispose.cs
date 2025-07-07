namespace BenchMark.Test.ParentDipose;

public class Component
{
    ~Component()
    {
        Console.WriteLine("~Component()");

    }
    public int _count = 0;
    public void Increase()
    {
        _count += 100;
        Console.WriteLine($"Component Count:{_count}");
    }

    public void Dispose()
    {
        Console.WriteLine("Dipose Component");
    }
}

public class Parent
{
    private Component? _compo;
    private Child? _child1;
    private Child? _child2;
    public Parent()
    {
        _compo = new Component();

        _child1 = new Child(_compo);
        _child2 = new Child(_compo);
    }
    public void Increase()
    {
        _compo?.Increase();
    }

    public void Dispose()
    {
        _child1?.Dispose();
        _child1 = null;

        _child2?.Dispose();
        _child2 = null;

        _compo?.Dispose();
        _compo = null;
    }
}

public class Child
{
    public Component? Compo => _compo;
    private Component? _compo;
    public Child(Component compo)
    {
        _compo = compo;
    }
    public void Dispose()
    {
        _compo = null;
    }
}

public class ParentDiposeTest
{
    public void Test()
    {
        var parent = new Parent();

        parent.Increase();
        parent.Dispose();

        GC.Collect();      // ✅ 강제 GC 호출
        GC.WaitForPendingFinalizers(); // ✅ Finalizer 실행 대기        
    }
}
