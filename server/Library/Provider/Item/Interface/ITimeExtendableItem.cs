using Library.DTO;
using MySqlConnector;

namespace Library.Provider.Item.Interface;

/// <summary>
/// 시간 연장이 가능한 아이템
/// </summary>
public interface ITimeExtendableItem
{
    Task<(ErrorCode, string, DateTime, ulong)> ExtendItemUsagePeriod(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, int itemId, int itemPeriod, int colorId);
}

