using Library.DTO;
using MySqlConnector;

namespace Library.Provider.Item.Interface;

/// <summary>
/// 만료된 아이템에 대한 처리
/// </summary>
public interface IExpiredItem
{
    Task<ErrorCode> HandleExpiredItem(MySqlConnection db, MySqlTransaction transaction, ulong charSeq, DateTime now);
}

