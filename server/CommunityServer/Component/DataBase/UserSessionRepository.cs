using Library.Component;
using Library.DBTables;
using Library.ECSSystem;
using Library.Repository.Redis;

namespace CommunityServer.Component.DataBase;

public class UserSessionRepository : RedisCacheCommonComponent, IECSSystem
{
    
    private ServerInfoRedisSharedRepo _serverInfoRedisRepo = new();
    public RedisCommonQuery.UserSessionInfo FetchUserSession(string sessionGuid)
    {
        return _userInfoRedisRepo.FetchUserSession(sessionGuid);
    }

   

}
