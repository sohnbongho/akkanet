using Library.DTO;
using Library.Provider.Item.Interface;
using Library.Provider.Item.Provider;

namespace Library.Provider.Item;

/// <summary>
/// 아이템 구매/판매 관련 처리
/// </summary>
/// 
public static class ItemPolicyFactory
{
    private static readonly Dictionary<MainItemType, IItemPolicy> _policies = new();

    static ItemPolicyFactory()
    {
        // Dictionary에 미리 모든 정책을 초기화
        _policies[MainItemType.None] = new NoneItemPolicy();
        _policies[MainItemType.Character] = new CharterItemPolicy();
        _policies[MainItemType.Clothing] = new ClothingItemPolicy();
        _policies[MainItemType.Accessory] = new AccessoryItemPolicy();
        _policies[MainItemType.BackGround] = new BackGroundItemPolicy();
        _policies[MainItemType.BackGroundProp] = new BackGroundPropItemPolicy();
        _policies[MainItemType.Currency] = new CurrencyItemPolicy();
        _policies[MainItemType.SoftCat] = new SoftCatItemPolicy();
        _policies[MainItemType.ShowRoom] = new ShowRoomItemPolicy();
    }

    // 없는 경우 기본 정책 반환
    public static IItemPolicy GetPolicy(MainItemType itemType)
        => _policies.TryGetValue(itemType, out var policy) ? policy : _policies[MainItemType.None];    
}

