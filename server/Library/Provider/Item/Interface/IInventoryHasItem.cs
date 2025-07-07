using Messages;
using MySqlConnector;

namespace Library.Provider.Item.Interface;

/// <summary>
/// 인벤토리를 가지고 있는 아이템
/// </summary>
public interface IInventoryHasItem
{
    Task<InventoryItem> FetchItemByIdAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId);
    Task<InventoryItem> FetchInventoryItemAsync(MySqlConnection db, ulong charSeq, ulong itemSeq);
    Task<List<InventoryItem>> FetchInventoryAllItemsAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq);
    List<InventoryItem> FetchInventoryAllItems(MySqlConnection db, ulong charSeq);

    Task<List<InventoryItem>> FetchInventoryEquippedItemsAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, short equipped);
    Task<int> FetchHasCountAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId);
}

