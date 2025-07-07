using Library.Data.Models;
using Library.DBTables.MySql;
using Library.DTO;
using Library.Helper;
using Library.Provider.Item.Interface;
using Library.Repository.Item;
using Messages;
using MySqlConnector;

namespace Library.Provider.Item.Provider;

public class AccessoryItemPolicy : IItemPolicy, IRewardableItem, IDistributableItem, IInventoryHasItem, ITimeExtendableItem, IDyeableItem, IEquipableItem, IWearableItem, IExpiredItem
{
    public MainItemType ItemType { get; } = MainItemType.Accessory;
    public string TblName { get; } = "tbl_inventory_accessory";

    private ItemSharedRepo? _itemRepository;

    public AccessoryItemPolicy()
    {
        _itemRepository = new ItemSharedRepo(ItemType, TblName);
    }

    public void Dispose()
    {
        _itemRepository = null;
    }

    public IDesignBaseItemInfo GetShopItemInfo(int itemId)
    {
        if (_itemRepository == null)
            return DesignNoneItemInfo.Instance;

        return _itemRepository.GetShopItemInfo(itemId);
    }

    public async Task<(ErrorCode, string, DateTime)> DistributeItemAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, int slotId, ulong itemSeq, int amount, int itemPeriod, int colorId)
    {
        if (_itemRepository == null)
            return (ErrorCode.DbInsertedError, string.Empty, DateTime.MinValue);

        var result = await _itemRepository.DistributeItemAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId, slotId, itemSeq, amount, itemPeriod, colorId);
        return result;
    }
    public (ErrorCode, string, DateTime) DistributeItem(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, int slotId, ulong itemSeq, int amount, int itemPeriod, int colorId)
    {
        if (_itemRepository == null)
            return (ErrorCode.DbInsertedError, string.Empty, DateTime.MinValue);

        var result = _itemRepository.DistributeItem<TblInventoryAccessory>(db, transaction, charSeq, itemId, slotId, itemSeq, amount, itemPeriod, colorId);
        return result;
    }

    /// <summary>
    /// 아이템 기간만 증가
    /// </summary>    
    /// <returns></returns>
    public async Task<(ErrorCode, string, DateTime, ulong)> ExtendItemUsagePeriod(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, int itemPeriod, int colorId)
    {
        if (_itemRepository == null)
            return (ErrorCode.DbInsertedError, string.Empty, DateTime.MinValue, 0);

        var (errorCode, query, expired, tbl) = await _itemRepository.ExtendItemUsagePeriodAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId, itemPeriod, colorId);
        if (errorCode != ErrorCode.Succeed)
        {
            return (errorCode, query, expired, 0);
        }
        var itemSeq = tbl != null ? tbl.item_seq : 0;
        expired = tbl != null ? tbl.expiration_date : expired;
        return (errorCode, query, expired, itemSeq);
    }
    public async Task<InventoryItem> FetchItemByIdAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId)
    {
        if (_itemRepository == null)
            return new InventoryItem();

        var tblItem = await _itemRepository.FetchItemByIdAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId);
        if (tblItem == null)
            return new InventoryItem();

        return new InventoryItem
        {
            ItemSeq = tblItem.item_seq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = tblItem.item_sn,
            Equip = tblItem.equipped,
            Amount = tblItem.amount,
            SlotId = tblItem.slot_id,
            JsonData = tblItem.json_data,
            ColorId = tblItem.color_id,

            ExpiredDate = tblItem.expiration_date.ToString(ConstInfo.TimeFormat),
        };

    }

    public async Task<InventoryItem> FetchInventoryItemAsync(MySqlConnection db, ulong charSeq, ulong itemSeq)
    {
        if (_itemRepository == null)
            return new InventoryItem();

        var tblItem = await _itemRepository.FetchInventoryItemAsync<TblInventoryAccessory>(db, charSeq, itemSeq);
        if (tblItem == null)
            return new InventoryItem();

        return new InventoryItem
        {
            ItemSeq = tblItem.item_seq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = tblItem.item_sn,
            Equip = tblItem.equipped,
            Amount = tblItem.amount,
            SlotId = tblItem.slot_id,
            JsonData = tblItem.json_data,
            ColorId = tblItem.color_id,

            ExpiredDate = tblItem.expiration_date.ToString(ConstInfo.TimeFormat),
        };
    }
    public async Task<List<InventoryItem>> FetchInventoryAllItemsAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq)
    {
        if (_itemRepository == null)
            return new List<InventoryItem>();

        var tblItems = await _itemRepository.FetchInventoryAllItemsAsync<TblInventoryAccessory>(db, transaction, charSeq);

        return tblItems.Select(tblItem => new InventoryItem
        {
            ItemSeq = tblItem.item_seq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = tblItem.item_sn,
            Equip = tblItem.equipped,
            Amount = tblItem.amount,
            SlotId = tblItem.slot_id,
            JsonData = tblItem.json_data,
            ColorId = tblItem.color_id,

            ExpiredDate = tblItem.expiration_date.ToString(ConstInfo.TimeFormat),
        }).ToList();
    }
    public List<InventoryItem> FetchInventoryAllItems(MySqlConnection db, ulong charSeq)
    {
        if (_itemRepository == null)
            return new List<InventoryItem>();

        var tblItems = _itemRepository.FetchInventoryAllItems<TblInventoryAccessory>(db, charSeq);

        return tblItems.Select(tblItem => new InventoryItem
        {
            ItemSeq = tblItem.item_seq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = tblItem.item_sn,
            Equip = tblItem.equipped,
            Amount = tblItem.amount,
            SlotId = tblItem.slot_id,
            JsonData = tblItem.json_data,
            ColorId = tblItem.color_id,

            ExpiredDate = tblItem.expiration_date.ToString(ConstInfo.TimeFormat),
        }).ToList();
    }
    public async Task<List<InventoryItem>> FetchInventoryEquippedItemsAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, short equipped)
    {
        if (_itemRepository == null)
            return new List<InventoryItem>();

        var tblItems = await _itemRepository.FetchInventoryEquippedItemsAsync<TblInventoryBackground>(db, transaction, charSeq, equipped);

        return tblItems.Select(tblItem => new InventoryItem
        {
            ItemSeq = tblItem.item_seq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = tblItem.item_sn,
            Equip = tblItem.equipped,
            Amount = 1,
            SlotId = tblItem.slot_id,
            JsonData = string.Empty,
            ColorId = 0,

            ExpiredDate = string.Empty,
        }).ToList();
    }
    public async Task<InventoryItem> FetchPrevEquippedItemAsync(MySqlConnection db, ulong charSeq, ulong itemSeq, int slotId)
    {
        if (_itemRepository == null)
            return new InventoryItem();

        var tblItem = await _itemRepository.FetchPrevEquippedItemAsync<TblInventoryAccessory>(db, charSeq, itemSeq, slotId); ;
        if (tblItem == null)
            return new InventoryItem();

        return new InventoryItem
        {
            ItemSeq = tblItem.item_seq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = tblItem.item_sn,
            Equip = tblItem.equipped,
            Amount = tblItem.amount,
            SlotId = tblItem.slot_id,
            JsonData = tblItem.json_data,
            ColorId = tblItem.color_id,

            ExpiredDate = tblItem.expiration_date.ToString(ConstInfo.TimeFormat),
        };
    }
    public async Task<ErrorCode> UpdateInventoryEquipedAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, ulong itemSeq, int slotId, short equipped)
    {
        if (_itemRepository == null)
            return ErrorCode.DbUpdatedError;

        var errorCode = await _itemRepository.UpdateInventoryEquipedAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId, itemSeq, slotId, equipped);
        return errorCode;
    }
    public async Task<int> FetchHasCountAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId)
    {
        if (_itemRepository == null)
            return 0;

        var count = await _itemRepository.FetchHasCountAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId);
        return count;
    }
    public async Task<(ErrorCode, string, DateTime)> DyeItem(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, ulong itemSeq, string jsonData, int colorId)
    {
        if (_itemRepository == null)
            return (ErrorCode.DbInsertedError, string.Empty, DateTime.MinValue);

        var (errorCode, query, expiredTime, tbl) = await _itemRepository.DyeItem<TblInventoryAccessory>(db, transaction, charSeq, itemSeq, jsonData, colorId);
        if (errorCode != ErrorCode.Succeed)
        {
            return (errorCode, query, expiredTime);
        }
        expiredTime = tbl != null ? tbl.expiration_date : expiredTime;

        return (ErrorCode.Succeed, query, expiredTime);
    }

    /// <summary>
    /// 메일을 통한 아이템 지급
    /// </summary>        
    public async Task<(ErrorCode, InventoryItem)> RewardItemAsync(MySqlConnection db, MySqlTransaction transaction, ulong userSeq, ulong charSeq, int itemId, int amount)
    {
        if (_itemRepository == null)
            return (ErrorCode.DbInsertedError, new InventoryItem());

        var itemInfo = _itemRepository.GetShopItemInfo(itemId);
        if (itemInfo.ItemSn == 0)
            return (ErrorCode.NotFoundItem, new InventoryItem());

        ulong itemSeq = 0;
        var itemAmount = amount;
        DateTime expireTime;
        int slotId = 0;
        int colorId = 0;
        int itemPeriodDay = 0;
        if (itemInfo is IDesignHasSlotItemInfo iSlotInfo)
        {
            slotId = iSlotInfo.SlotId;
        }
        if (itemInfo is IDesignPurchasableItemInfo iPurchase)
        {
            itemPeriodDay = iPurchase.ItemPeriodDay;
        }
        if (itemInfo is IDesignDyeItemInfo iDye)
        {
            colorId = iDye.ColorId;
        }

        var hasCount = await FetchHasCountAsync(db, transaction, charSeq, itemId);
        if (hasCount > 0)
        {
            // 기간만 늘린다.
            var (errorCode, query2, expireTime2, tbl) = await _itemRepository.ExtendItemUsagePeriodAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId, itemPeriodDay, colorId);
            if (errorCode != ErrorCode.Succeed)
                return (errorCode, new InventoryItem());

            expireTime = tbl != null ? tbl.expiration_date : expireTime2;
            itemSeq = tbl != null ? tbl.item_seq : 0;
        }
        else
        {
            itemSeq = (ulong)SnowflakeIdGenerator.Instance.NextId();

            // 아이템 지급
            var (errorCode, query, expireTime2) = await _itemRepository.DistributeItemAsync<TblInventoryAccessory>(db, transaction, charSeq, itemId, slotId, itemSeq, itemAmount, itemPeriodDay, colorId);

            if (errorCode != ErrorCode.Succeed)
                return (errorCode, new InventoryItem());

            expireTime = expireTime2;
        }
        return (ErrorCode.Succeed, new InventoryItem
        {
            ItemSeq = itemSeq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = itemInfo.ItemSn,
            Equip = 0,
            Amount = itemAmount,
            SlotId = slotId,
            JsonData = string.Empty,
            ColorId = colorId,

            ExpiredDate = expireTime.ToString(ConstInfo.TimeFormat),
        });
    }
    public (ErrorCode, InventoryItem) RewardItem(MySqlConnection db, MySqlTransaction transaction, ulong userSeq, ulong charSeq, int itemId, int amount)
    {
        if (_itemRepository == null)
            return (ErrorCode.DbInsertedError, new InventoryItem());

        var itemInfo = _itemRepository.GetShopItemInfo(itemId);
        if (itemInfo.ItemSn == 0)
            return (ErrorCode.NotFoundItem, new InventoryItem());

        ulong itemSeq = 0;
        var itemAmount = amount;
        DateTime expireTime;
        int slotId = 0;
        int colorId = 0;
        int itemPeriodDay = 0;
        if (itemInfo is IDesignHasSlotItemInfo iSlotInfo)
        {
            slotId = iSlotInfo.SlotId;
        }
        if (itemInfo is IDesignPurchasableItemInfo iPurchase)
        {
            itemPeriodDay = iPurchase.ItemPeriodDay;
        }
        if (itemInfo is IDesignDyeItemInfo iDye)
        {
            colorId = iDye.ColorId;
        }

        var hasCount = _itemRepository.FetchHasCount(db, transaction, charSeq, itemId);
        if (hasCount > 0)
        {
            // 기간만 늘린다.
            var (errorCode, query2, expireTime2, tbl) =
                _itemRepository.ExtendItemUsagePeriod<TblInventoryAccessory>(db, transaction, charSeq,
                itemId, itemPeriodDay, colorId);

            if (errorCode != ErrorCode.Succeed)
                return (errorCode, new InventoryItem());

            expireTime = tbl != null ? tbl.expiration_date : expireTime2;
            itemSeq = tbl != null ? tbl.item_seq : 0;
        }
        else
        {
            itemSeq = (ulong)SnowflakeIdGenerator.Instance.NextId();

            // 아이템 지급
            var (errorCode, query, expireTime2)
                = DistributeItem(db, transaction, charSeq, itemId, slotId, itemSeq, itemAmount, itemPeriodDay, colorId);

            if (errorCode != ErrorCode.Succeed)
                return (errorCode, new InventoryItem());

            expireTime = expireTime2;
        }
        return (ErrorCode.Succeed, new InventoryItem
        {
            ItemSeq = itemSeq.ToString(),
            ItemType = (int)ItemType,
            ItemSn = itemInfo.ItemSn,
            Equip = 0,
            Amount = itemAmount,
            SlotId = slotId,
            JsonData = string.Empty,
            ColorId = colorId,

            ExpiredDate = expireTime.ToString(ConstInfo.TimeFormat),
        });
    }


    public async Task<ErrorCode> HandleExpiredItem(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, DateTime now)
    {
        if (_itemRepository == null)
            return ErrorCode.DbInsertedError;

        var errorCode = await _itemRepository.HandleExpiredItem<TblInventoryAccessory>(db, transaction, charSeq, now);
        return errorCode;
    }

}
