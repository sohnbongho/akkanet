using Library.DTO;
using Messages;
using MySqlConnector;

namespace Library.Provider.Item.Interface;


/// <summary>
/// 메일 보상이 가능한 아이템
/// </summary>
public interface IRewardableItem
{
    Task<(ErrorCode, InventoryItem)> RewardItemAsync(MySqlConnection db, MySqlTransaction transaction, ulong userSeq, ulong charSeq, int itemId, int amount);
    (ErrorCode, InventoryItem) RewardItem(MySqlConnection db, MySqlTransaction transaction, ulong userSeq, ulong charSeq, int itemId, int amount);
}

public interface IRewardableCurrencyItem
{
    Task<(ErrorCode, InventoryItem)> RewardItemAsync(MySqlConnection db, MySqlTransaction transaction, ItemGainReason reason, ulong userSeq, ulong charSeq, int itemId, int amount);
    (ErrorCode, InventoryItem) RewardItem(MySqlConnection db, MySqlTransaction transaction, ItemGainReason reason, ulong userSeq, ulong charSeq, int itemId, int amount);
}