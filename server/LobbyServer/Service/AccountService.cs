using Library.DBTables;
using Library.DTO;
using LobbyServer.Repository.Account;
using log4net;
using System.Reflection;

namespace LobbyServer.Service;

public interface IAccountService
{
    Task<AccountDeactivateResponse> Deactivate(AccountDeactivateRequest request); // 계정 탈퇴
}

public class AccountService : IAccountService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly IAccountRepo _repo;


    public AccountService(IAccountRepo repo)
    {
        _repo = repo;
    }

    public async Task<AccountDeactivateResponse> Deactivate(AccountDeactivateRequest request)
    {
        var sessionGuid = request.SessionGuid;
        var webUserInfo = await _repo.FetchUserSessionAsync(sessionGuid);
        if (webUserInfo.user_seq == 0)
        {
            return new AccountDeactivateResponse
            {
                ErrorCode = ErrorCode.NotFoundCharBySeesionGuid,
                SessionGuid = sessionGuid,
            };
        }

        var lockTableName = RedisLockNameCollection.Member;
        await using var redisLock = await RedisLockGuardAsync.Of(webUserInfo.user_seq, lockTableName);
        var locked = redisLock.Entered;
        if (false == locked)
        {
            return new AccountDeactivateResponse
            {
                ErrorCode = ErrorCode.DbTableLock,
                SessionGuid = sessionGuid,
            };
        }

        try
        {
            var errorCode = ErrorCode.Succeed;
            var userSeq = webUserInfo.user_seq;
            var charSeq = webUserInfo.char_seq;
            var reasons = request.ReasonIds;

            var tblMember = await _repo.FetchMemberAsync(userSeq);
            if(tblMember.user_seq == 0 )
            {                
                errorCode = ErrorCode.NotFoundAccount;
            }
            else if(tblMember.deactive_date != null)
            {
                // 이미 비활성화 신청한 계정
                errorCode = ErrorCode.AlreadyReactivateAccount;
            }
            else
            {
                errorCode = await _repo.Deactivate(userSeq, charSeq, reasons);
            }


            var response = new AccountDeactivateResponse
            {
                ErrorCode = errorCode,
                SessionGuid = sessionGuid,
            };
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"failed DeleteAccount", ex);            
            
            return new AccountDeactivateResponse
            {
                ErrorCode = ErrorCode.DbUpdatedError,
                SessionGuid = sessionGuid,
            };
        }

        
    }
    //{
    //    var response = new AccountDeletedResponse { };

    //    return response;
    //}
}
