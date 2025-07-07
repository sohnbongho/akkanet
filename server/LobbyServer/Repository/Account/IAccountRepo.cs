using Library.DBTables;
using Library.DBTables.MySql;
using Library.DTO;
using MySqlConnector;

namespace LobbyServer.Repository.Account;

public interface IAccountRepo
{
    Task<TblMember> FetchMemberAsync(ulong userSeq);
    Task<TblMember> FetchMemberAsync(MySqlConnection db, MySqlTransaction transaction, ulong userSeq);
    Task<ErrorCode> Deactivate(ulong userSeq, ulong charSeq, List<int> reasonIds);    
    Task<RedisCommonQuery.UserSessionInfo> FetchUserSessionAsync(string sessionGuid);
}
