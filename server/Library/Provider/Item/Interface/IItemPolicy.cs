using Library.Data.Models;
using Library.DTO;

namespace Library.Provider.Item.Interface;

/***********************************
 * SOLID 원칙
S - 단일 책임 원칙 (Single Responsibility Principle, SRP)
클래스는 하나의 책임만 가져야 합니다. 즉, 클래스는 변경이 필요한 이유가 하나뿐이어야 합니다.
한 클래스가 너무 많은 기능을 가지면, 클래스가 변경될 가능성이 높아지고, 이를 관리하기 어렵게 됩니다

O - 개방-폐쇄 원칙 (Open/Closed Principle, OCP)
소프트웨어 개체(클래스, 모듈, 함수 등)는 확장에는 열려 있어야 하고, 수정에는 닫혀 있어야 합니다.
기존 코드를 변경하지 않고도 새로운 기능을 추가할 수 있어야 합니다

L - 리스코프 치환 원칙 (Liskov Substitution Principle, LSP)
서브타입은 언제나 기반 타입으로 교체할 수 있어야 합니다.
자식 클래스는 부모 클래스의 기능을 모두 수행할 수 있어야 하며, 부모 클래스 대신 자식 클래스를 사용해도 프로그램이 정상적으로 동작해야 합니다.

I - 인터페이스 분리 원칙 (Interface Segregation Principle, ISP)
클라이언트는 사용하지 않는 인터페이스에 의존하지 않아야 합니다.
큰 인터페이스를 작은 인터페이스로 분리하여, 클라이언트가 자신이 사용하지 않는 메서드에 의존하지 않도록 해야 합니다.

D - 의존성 역전 원칙 (Dependency Inversion Principle, DIP)
고수준 모듈은 저수준 모듈에 의존해서는 안 됩니다. 둘 다 추상화에 의존해야 합니다.
구체적인 구현보다 추상화된 인터페이스나 추상 클래스에 의존해야 합니다. 이를 통해 모듈 간의 결합도를 낮출 수 있습니다.
***********************************/

/// <summary>
/// 아이템 생성 정책
/// </summary>
public interface IItemPolicy
{
    MainItemType ItemType { get; }
    string TblName { get; }
    IDesignBaseItemInfo GetShopItemInfo(int itemId);

}


