using Library.DTO;
using MySqlConnector;

namespace Library.Provider.Item.Interface;

/// <summary>
/// 인벤에 바로 지급 가능한 아이템
/// </summary>
public interface IDistributableItem
{
    Task<(ErrorCode, string, DateTime)> DistributeItemAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, int slotId, ulong itemSeq, int amount, int itemPeriod, int colorId);
    (ErrorCode, string, DateTime) DistributeItem(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, int slotId, ulong itemSeq, int amount, int itemPeriod, int colorId);
}

public interface IDistributableCurrencyItem
{
    Task<(ErrorCode, string, DateTime)> DistributeItemAsync(MySqlConnection db, MySqlTransaction transaction, 
        ItemGainReason reason, ulong charSeq, int itemId, int slotId, ulong itemSeq, int amount, int itemPeriod, int colorId);
    (ErrorCode, string, DateTime) DistributeItem(MySqlConnection db, MySqlTransaction transaction, 
        ItemGainReason reason, ulong charSeq, int itemId, int slotId, ulong itemSeq, int amount, int itemPeriod, int colorId);
}

