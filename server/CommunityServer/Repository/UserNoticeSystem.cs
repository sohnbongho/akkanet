using Dapper;
using Library.Component;
using Library.DBTables.MySql;
using Library.DTO;
using Library.ECSSystem;

namespace CommunityServer.Repository;

public class UserNoticeSystem : MySqlDbCommonRepo, IECSSystem
{
    /// <summary>
    /// 서버 Id에 해당되는 공지
    /// </summary>    
    public List<TblNoticeImmediate> FetchNoticeImmediate(int serverId, DateTime now)
    {
        using (var db = ConnectionFactory(DbConnectionType.Game))
        {
            if (db == null)
            {
                return new();
            }

            var query = $"select * from tbl_notice_immediate where server_id = @server_id and showed = @showed and notice_date < @notice_date";
            var result = db.Query<TblNoticeImmediate>(query, new
            {
                server_id = serverId,
                showed = 0,
                notice_date = now,
            });
            var tbls = result.ToList();
            var ids = tbls.Select(x => x.id).ToList();
            if (ids.Any())
            {
                var idsStr = string.Join(",", ids);

                query = $"UPDATE tbl_notice_immediate SET showed = @showed WHERE id in ({idsStr})";
                var affected = db.Execute(query, new
                {
                    showed = 1,
                });
            }


            return tbls;
        }
    }
}
