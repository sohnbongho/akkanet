using Library.DTO;
using Messages;
using MySqlConnector;

namespace Library.Provider.Item.Interface;

/// <summary>
/// 장착 가능한 아이템
/// </summary>
public interface IEquipableItem
{
    Task<InventoryItem> FetchPrevEquippedItemAsync(MySqlConnection db, ulong charSeq, ulong itemSeq, int slotId);
    Task<ErrorCode> UpdateInventoryEquipedAsync(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, ulong itemSeq, int slotId, short equipped);    
}

/// <summary>
/// InGame 에서 장착하는 아이템
/// </summary>
public interface IInGameEquipableItem
{    
    ErrorCode UpdateInventoryEquiped(MySqlConnection db, MySqlTransaction transaction, GameModeType gameModeType,
        ulong charSeq, int itemId, ulong itemSeq, int slotId, short equipped, string inGameParts);
}

