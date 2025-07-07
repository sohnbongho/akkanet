using Library.DTO;
using System.Reflection;

namespace BenchMark.Test.Attributes;

public static class AttributeFactory
{
    private static readonly Dictionary<MainItemType, IItemPolicy> _strategies = new();
    static AttributeFactory()
    {
        // 현재 어셈블리에서 모든 타입을 검색
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            // IPaymentStrategy 인터페이스를 구현하고, PaymentStrategy 속성이 있는 클래스만 필터링
            if (typeof(IItemPolicy).IsAssignableFrom(type) && type.GetCustomAttribute<MainItemTypeHandlerAttribute>() != null)
            {
                var attribute = type.GetCustomAttribute<MainItemTypeHandlerAttribute>();
                if (attribute != null)
                {
                    // 인스턴스 생성 후 등록
                    var instance = Activator.CreateInstance(type) as IItemPolicy;
                    if (instance != null)
                    {
                        _strategies[attribute.MessageType] = instance;
                    }

                }
            }
        }
    }
    // 전략 선택 및 실행
    public static IItemPolicy GetStrategy(MainItemType itemType)
    {
        if (_strategies.TryGetValue(itemType, out var strategy))
        {
            return strategy;
        }
        throw new Exception($"전략 '{itemType.ToString()}'을(를) 찾을 수 없습니다.");
    }
}
