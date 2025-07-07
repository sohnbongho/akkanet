using Dapper;
using Library.Component;
using Library.DTO;
using log4net;
using System.Reflection;

namespace Library.Repository.ConcurrentUser;

public interface IConcurrentUserRepo
{
    void Insert(int serverId, int userCount);
}
public class ConcurrentUserRepo : MySqlDbCommonRepo, IConcurrentUserRepo
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public void Insert(int serverId, int userCount)
    {
        try
        {
            using (var db = ConnectionFactory(DbConnectionType.Game))
            {
                if (db == null)
                    return;

                var query = $"INSERT INTO tbl_log_concurrent_user VALUES(NULL, @server_id, @count, CURRENT_TIMESTAMP);";
                var affected = db.ExecuteAsync(query, new
                {
                    server_id = serverId,
                    count = userCount,
                });
            }
        }
        catch(Exception ex)
        {
            _logger.Error("fail add ConcurrentUserRepo.", ex);
        }
        
    }
}
