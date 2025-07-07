using Dapper;
using Library.Component;
using Library.DBTables;
using Library.DBTables.MySql;
using Library.DTO;
using Library.Helper;
using Library.Repository.Mysql;
using Library.Repository.Redis;
using log4net;
using MySqlConnector;
using System.Reflection;

namespace LobbyServer.Repository.Account;

public class AccountRepo : MySqlDbCommonRepo, IAccountRepo
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private UserSharedRepo _userRepository = UserSharedRepo.Of();    
    private UserInfoSessionSharedRepo _userInfoRedisRepo = UserInfoSessionSharedRepo.Of();

    public Task<TblMember> FetchMemberAsync(ulong userSeq)
    {
        return _userRepository.FetchMemberAsync(userSeq);
    }
    public Task<TblMember> FetchMemberAsync(MySqlConnection db, MySqlTransaction transaction, ulong userSeq)
    {
        return _userRepository.FetchMemberAsync(db, transaction, userSeq);
    }

    /// <summary>
    /// 계정 탈퇴(비활성화)
    /// </summary>    
    public async Task<ErrorCode> Deactivate(ulong userSeq, ulong charSeq, List<int> reasonIds)
    {
        var now = DateTimeHelper.Now;
        var query = string.Empty;
        await using (var db = await ConnectionFactoryAsync(DbConnectionType.Game))
        {
            if (db == null)
            {
                return ErrorCode.DbInitializedError;
            }
            await using var transaction = await db.BeginTransactionAsync();
            try
            {
                {
                    query = $"UPDATE tbl_member SET deactive_date = @deactive_date WHERE user_seq = @user_seq;";

                    var affected = await db.ExecuteAsync(query, new TblMember
                    {
                        user_seq = userSeq,
                        deactive_date = now,
                    }, transaction);
                }
                var tblMemeber = await FetchMemberAsync(db, transaction, userSeq);

                var hasDeactive = false;

                {
                    query = $"select * from tbl_member_deactive WHERE user_seq = @user_seq;";
                    var result = await db.QueryAsync(query, new TblMemberDeactive
                    {
                        user_seq = userSeq,
                    }, transaction);
                    var tbl = result?.FirstOrDefault() ?? null;
                    hasDeactive = tbl != null;
                }

                if (hasDeactive == false)
                {
                    query = $"INSERT INTO tbl_member_deactive VALUES(NULL, @user_seq, @char_seq, @login_type, @user_id, @user_handle, @deactive_date, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);";

                    var affected = await db.ExecuteAsync(query, new TblMemberDeactive
                    {
                        user_seq = userSeq,
                        char_seq = charSeq,
                        login_type = tblMemeber.login_type,
                        user_id = tblMemeber.user_id,
                        user_handle = tblMemeber.user_handle,
                        deactive_date = now,
                    }, transaction);
                }
                else
                {
                    query = $"UPDATE tbl_member_deactive SET deactive_date = @deactive_date WHERE user_seq = @user_seq;";

                    var affected = await db.ExecuteAsync(query, new TblMemberDeactive
                    {
                        user_seq = userSeq,
                        deactive_date = now,
                    }, transaction);
                }

                //로그들 추가
                foreach (var reasonId in reasonIds)
                {
                    query = $"INSERT INTO tbl_log_member_deactive VALUES(NULL, @user_seq, @reason_id, CURRENT_TIMESTAMP);";

                    var affected = await db.ExecuteAsync(query, new TblLogMemberDeactive
                    {
                        user_seq = userSeq,
                        reason_id = reasonId,
                    }, transaction);
                }

                await transaction.CommitAsync();

                return ErrorCode.Succeed;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.Error($"failed to Deactivate.", ex);
                return ErrorCode.DbInsertedError;
            }
        }
    }    
    public Task<RedisCommonQuery.UserSessionInfo> FetchUserSessionAsync(string sessionGuid)
    {
        return _userInfoRedisRepo.FetchUserSessionAsync(sessionGuid);
    }

}
