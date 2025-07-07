using Library.DTO;
using MySqlConnector;

namespace Library.Provider.Item.Interface;

/// <summary>
/// 염색이 가능한 아이템
/// </summary>
public interface IDyeableItem
{
    Task<(ErrorCode, string, DateTime)> DyeItem(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, ulong itemSeq, string jsonData, int colorId);
}

