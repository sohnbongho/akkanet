using Library.DTO;

namespace Library.Data.Models;

/// <summary>
/// 기획팀에서 만든 상점 기본 정보
/// </summary>
public interface IDesignBaseItemInfo
{
    MainItemType ItemType { get; set; } 
    int ItemSn { get; set; }
    string ItemName { get; set; }  // 아이템 이름
}
public interface IDesignPurchasableItemInfo
{
    CurrencyType CurrencyType { get; set; } // price_type: 가격종류
    uint ItemPrice { get; set; } // 아이템 가격                

    int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    short PurchaseAvail { get; set; } // 판매 여부: 
    int MaxPurchaseCount { get; set; }// 최대 구매 횟수
}
public interface IDesignDyeItemInfo
{
    short ColorAvail { get; set; } // 염색 여부
    
    int ColorId { get; set; }
    short DetailCategory { get; set; }
}
public interface IDesignHasSlotItemInfo
{
    int SlotId { get; set; }
}
public interface IDesignHasIngameType
{
    InGameItemType InGameItemType { get; set; }
}
public class DesignNoneItemInfo : IDesignBaseItemInfo
{
    public static readonly DesignNoneItemInfo Instance = new DesignNoneItemInfo();
    public MainItemType ItemType { get; set; } = MainItemType.None;
    public int ItemSn { get; set; }
    public string ItemName { get; set; } = string.Empty;
}

public class DesignWearableItemInfo: IDesignBaseItemInfo, IDesignPurchasableItemInfo, IDesignHasSlotItemInfo, IDesignDyeItemInfo
{
    public MainItemType ItemType { get; set; } = MainItemType.None;
    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름

    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } = 0;   // 아이템 가격                

    public int ItemPeriodDay { get; set; } = 0;// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } = 0;// 판매 여부: 
    public int MaxPurchaseCount { get; set; } = 0;// 최대 구매 횟수
    public short ColorAvail { get; set; } = 0;// 염색 여부

    public int SlotId { get; set; } = 0;
    public int ColorId { get; set; } = 0;
    public short DetailCategory { get; set; }
}

public class DesignCurrencyItemInfo : IDesignBaseItemInfo
{
    public MainItemType ItemType { get; set; } = MainItemType.Currency;
    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
}

public class DesignBackgroundItemInfo : IDesignBaseItemInfo, IDesignHasSlotItemInfo, IDesignPurchasableItemInfo
{
    public MainItemType ItemType { get; set; } = MainItemType.BackGround;
    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } // 아이템 가격                

    public int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } // 판매 여부: 
    public int MaxPurchaseCount { get; set; }// 최대 구매 횟수
    public int SlotId { get; set; } = 0;
    public int UseAvail { get; set; } = 1;
    public short SpaceCategory { get; set; }
}

public class DesignBackgroundPropItemInfo : IDesignBaseItemInfo, IDesignHasSlotItemInfo, IDesignPurchasableItemInfo
{
    public MainItemType ItemType { get; set; } = MainItemType.BackGroundProp;
    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } // 아이템 가격                

    public int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } // 판매 여부: 
    public int MaxPurchaseCount { get; set; }// 최대 구매 횟수
    public int SlotId { get; set; } = 0;
    public int UseAvail { get; set; } = 1;
    public short SpaceCategory { get; set; }

}

// 쇼룸
public class DesignShowRoomItemInfo : IDesignBaseItemInfo, IDesignHasSlotItemInfo, IDesignPurchasableItemInfo
{
    public MainItemType ItemType { get; set; } = MainItemType.ShowRoom;
    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } // 아이템 가격                

    public int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } // 판매 여부: 
    public int MaxPurchaseCount { get; set; }// 최대 구매 횟수
    public int SlotId { get; set; } = 0;
    public int UseAvail { get; set; } = 1;    
}

public class DesignSoftCatItemInfo : IDesignBaseItemInfo, IDesignHasSlotItemInfo, IDesignPurchasableItemInfo, IDesignHasIngameType
{
    public MainItemType ItemType { get; set; } = MainItemType.SoftCat;
    public InGameItemType InGameItemType { get; set; } = InGameItemType.None;

    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } // 아이템 가격                

    public int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } // 판매 여부: 
    public int MaxPurchaseCount { get; set; }// 최대 구매 횟수
    public int SlotId { get; set; } = 0;
    public bool StoreDb { get; set; } = true; // DB에 저장 여부

}

// 삽투사
public class DesignDiggingWarrior : IDesignBaseItemInfo, IDesignHasSlotItemInfo, IDesignPurchasableItemInfo, IDesignHasIngameType
{
    public MainItemType ItemType { get; set; } = MainItemType.DiggingWarrior;
    public InGameItemType InGameItemType { get; set; } = InGameItemType.None;

    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } // 아이템 가격                

    public int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } // 판매 여부: 
    public int MaxPurchaseCount { get; set; }// 최대 구매 횟수
    public int SlotId { get; set; } = 0;    

}


public class PackageItem : IDesignBaseItemInfo, IDesignPurchasableItemInfo
{
    public MainItemType ItemType { get; set; } = MainItemType.Package;    

    public int GroupId { get; set; } = 0;
    public int ItemSn { get; set; } = 0;
    public string ItemName { get; set; } = string.Empty;        // 아이템 이름
    public CurrencyType CurrencyType { get; set; } = CurrencyType.None; // price_type: 가격종류
    public uint ItemPrice { get; set; } // 아이템 가격                

    public int ItemPeriodDay { get; set; }// 아이템 기간, 일을 기준으로 정한다.        
    public short PurchaseAvail { get; set; } // 판매 여부: 
    public int MaxPurchaseCount { get; set; }// 최대 구매 횟수    
    public PackageCategory PackageCategory { get; set; }// 최대 구매 횟수    

}

/// <summary>
/// 아이템 해제 정보
/// </summary>
public class DesignTblUnequippedSetItem
{
    public int SlotId { get; set; }
    public int UnequipSlotId { get; set; }
}

public class DesignSlotToItemType
{
    public int SlotId { get; set; }
    public MainItemType ItemType { get; set; }
}


public class CurrencyItem
{
    public CurrencyType CurrencyType { get; set; }
    public long Amount { get; set; }
}
