using CommunityServer.Helper;
using Dapper;
using Library.Component;
using Library.DBTables.MySql;
using Library.DTO;
using Library.ECSSystem;
using Library.Repository.Mysql;
using Library.Repository.Redis;

namespace CommunityServer.Component.DataBase;

public class UserRepository : MySqlDbCommonRepo, IECSSystem
{
    private UserSharedRepo _userRepository = UserSharedRepo.Of();
    private UserSessionSharedRepo _userSessionRepository = UserSessionSharedRepo.Of();
    private UserInfoSessionSharedRepo _userInfoRedisRepo = UserInfoSessionSharedRepo.Of();


    public TblMember FetchMember(ulong userSeq)
    {
        return _userRepository.FetchMember(userSeq);
    }
    public TblCharacter FetchCharacter(ulong charSeq)
    {
        return _userRepository.FetchCharacter(charSeq);
    }

    /// <summary>
    /// UserSession
    /// </summary>
    /// <param name="communityServerid"></param>
    public void ClearUserConnectedCommunityServer(int communityServerid)
    {
        _userSessionRepository.ClearUserConnectedCommunityServer(communityServerid);
    }
    public void StoreUserConnectedCommunityServerId(ulong userSeq, ulong charSeq, int communityServerid)
    {
        var currentServerId = ConfigHelper.Instance.ServerId;
        _userSessionRepository.StoreUserConnectedCommunityServerId(userSeq, charSeq, communityServerid, currentServerId);
    }
    public (bool, TblMemberSession) FetchUserSessionInfoByUserSeq(ulong userSeq)
    {
        return _userSessionRepository.FetchUserSessionInfoByUserSeq(userSeq);
    }
    public (bool, TblMemberSession) FetchUserSessionInfoByCharSeq(ulong charSeq)
    {
        return _userSessionRepository.FetchUserSessionInfoByCharSeq(charSeq);
    }

    public List<TblServerList> GetCommunityServerInfos(int worldId)
    {
        using (var db = ConnectionFactory(DbConnectionType.System))
        {
            if (db == null)
            {
                return new();
            }

            var serverType = (int)ServerType.Community;
            var query = $"select * from tbl_server_list where world_id={worldId} and server_type = {serverType}";

            var result = db.Query<TblServerList>(query);
            return result.ToList();
        }
    }
    public void ClearServerUserCount(int serverId)
    {
        _serverInfoRedisRepo.ClearServerUserCount(serverId);
    }
    public void IncreaseServerUserCount(int serverId)
    {
        _serverInfoRedisRepo.IncreaseServerUserCount(serverId);
    }
    public void DecreaseServerUserCount(int serverId)
    {
        _serverInfoRedisRepo.DecreaseServerUserCount(serverId);
    }
}
