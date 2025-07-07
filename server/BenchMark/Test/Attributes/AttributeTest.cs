using Library.DTO;

namespace BenchMark.Test.Attributes;

public class AttributeTest
{
    public void Test()
    {
        var characterPolicy = AttributeFactory.GetStrategy(MainItemType.Character);
        Console.WriteLine($"MainItemType: {characterPolicy.MainItemType} test:{characterPolicy.GetTest()}");
        
        var clothPolicy = AttributeFactory.GetStrategy(MainItemType.Clothing);
        Console.WriteLine($"MainItemType: {clothPolicy.MainItemType} test:{clothPolicy.GetTest()}");
    }
}
