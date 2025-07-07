using Library.Connector;
using Library.DTO;
using MySqlConnector;

namespace Library.Component;

public class MySqlDbCommonRepo
{
    public MySqlConnection? ConnectionFactory(DbConnectionType dbConnectionType)
    {
        //MySqlConnection의 연결 풀링 기능은 기본적으로 활성화되어 있습니다.
        //따라서 별도로 설정할 필요 없이, 연결 문자열을 사용하여 
        //MySqlConnection 객체를 만들면 자동으로 연결 풀링이 활용됩니다.
        // 아래와 같이 해야 MySql에서 설정한 DB 풀링 기능을 사용할 수 있습니다.
        // 싱글턴 객체로 가지고 있으면 연결 풀링 기능을 쓸 수 없다.
        return MySqlConnectionHelper.Instance.ConnectionFactory(dbConnectionType);
    }
    public async Task<MySqlConnection?> ConnectionFactoryAsync(DbConnectionType dbConnectionType)
    {
        var result = await MySqlConnectionHelper.Instance.ConnectionFactoryAsync(dbConnectionType);
        return result;
    }
}
