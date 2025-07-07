using Library.AkkaActors;
using Library.Connector;
using Library.DTO;
using Library.Helper;
using Library.Repository.Mysql;
using log4net;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace CommunityServer.Helper
{
    /// <summary>
    /// Config를 관리하는 싱글턴 객체
    /// </summary>
    public sealed class ConfigHelper
    {
        private static readonly Lazy<ConfigHelper> lazy = new Lazy<ConfigHelper>(() => new ConfigHelper());

        public static ConfigHelper Instance { get { return lazy.Value; } }

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private JObject _jsonObj = null!;

        //World
        private int _serverId = 0;
        private int _port = 0;

        // Lobby 서버 정보
        private string _lobbyServerIp = string.Empty;
        private int _lobbyServerOpPort = 0;

        private bool _packetEncrypt = true;

        private string _gameDbConnectionString = null!;

        private string _redisConnectString = null!;
        private int _redisPoolCount;

        public int WorldId { get; private set; } = 0;
        public int ServerId => _serverId;
        public int Port => _port;
        public string LobbyServerIp => _lobbyServerIp;
        public int LobbyServerOpPort => _lobbyServerOpPort;

        public bool PacketEncrypt => _packetEncrypt;

        public string GameDbConnectionString => _gameDbConnectionString;

        public string RedisConnectString => _redisConnectString;
        public int RedisPoolCount => _redisPoolCount;

        private ServerInfoSharedRepo _db = null!;
        public int SwaggerPort { get; private set; } = 5000;

        // Nat 관련 
        public string NatsConnectString => _natsConnectString;

        private string _natsConnectString = string.Empty;

        // Kafka관련
        public string KafkaConnectString => _kafkaConnectString;
        private string _kafkaConnectString = string.Empty;

        // 몽고 DB
        private string _mongodbConnectString = string.Empty;

        // HTTPS사용유무
        public bool UsedHttps => _usedHttps;
        private bool _usedHttps = false;

        private ConfigHelper()
        {
        }
        public bool Load()
        {
            var fullPath = Assembly.GetExecutingAssembly().Location;
            var directoryPath = Path.GetDirectoryName(fullPath);

            string filePath = $@"{directoryPath}/Config.json5"; // 수정해야 할 부분
            string jsonString = File.ReadAllText(filePath);

            // Parse JSON string to JObject using Newtonsoft.Json
            _jsonObj = JObject.Parse(jsonString);

#pragma warning disable CS8602, CS8604 // null 가능 참조에 대한 역참조입니다.			
            _serverId = _jsonObj["world"]["serverId"].Value<int>();

            _packetEncrypt = _jsonObj["remote"]["encrypt"].Value<bool>();

            _gameDbConnectionString = _jsonObj["db"]["mySql"]["connectString"]["gameDb"].ToString();
            MySqlConnectionHelper.Instance.GameDbConnectionString = _gameDbConnectionString;

            _redisConnectString = _jsonObj["db"]["redis"]["connectString"].ToString();
            _redisPoolCount = _jsonObj["db"]["redis"]["poolCount"].Value<int>();
            RedisConnectionPool.Instance.Init(_redisConnectString, _redisPoolCount);

            _mongodbConnectString = _jsonObj["db"]["mongodb"]["connectString"].ToString();
            MongoDbConnectorHelper.ConnectionString = _mongodbConnectString;

            SwaggerPort = _jsonObj["swagger"]["port"].Value<int>();
            _usedHttps = _jsonObj["swagger"]["https"].Value<bool>();

            _natsConnectString = _jsonObj["nats"]["connectString"].ToString();

            var systemDbConnectionString = _jsonObj["db"]["mySql"]["connectString"]["system"].ToString();
            var designDbConnectionString = _jsonObj["db"]["mySql"]["connectString"]["design"].ToString();
            MySqlConnectionHelper.Instance.SystemDbConnectionString = systemDbConnectionString;
            MySqlConnectionHelper.Instance.DesighDbConnectionString = designDbConnectionString;
#pragma warning restore CS8602, CS8604 // null 가능 참조에 대한 역참조입니다.

            _db = new ServerInfoSharedRepo();

            ActorPaths.System = "CommunityServer";

            if (false == LoadWorldInfo())
                return false;

            return true;
        }

        public bool LoadForTDD()
        {
            var fullPath = Assembly.GetExecutingAssembly().Location;
            var directoryPath = Path.GetDirectoryName(fullPath);

            string filePath = $@"{directoryPath}/Config.json5"; // 수정해야 할 부분
            string jsonString = File.ReadAllText(filePath);

            // Parse JSON string to JObject using Newtonsoft.Json
            _jsonObj = JObject.Parse(jsonString);
#pragma warning disable CS8602, CS8604 // null 가능 참조에 대한 역참조입니다.			
            _serverId = _jsonObj["world"]["serverId"].Value<int>();

            _packetEncrypt = _jsonObj["remote"]["encrypt"].Value<bool>();

            _gameDbConnectionString = _jsonObj["db"]["mySql"]["connectString"]["gameDb"].ToString();
            MySqlConnectionHelper.Instance.GameDbConnectionString = _gameDbConnectionString;

            _redisConnectString = _jsonObj["db"]["redis"]["connectString"].ToString();
            _redisPoolCount = _jsonObj["db"]["redis"]["poolCount"].Value<int>();
            RedisConnectionPool.Instance.Init(_redisConnectString, _redisPoolCount);

            _mongodbConnectString = _jsonObj["db"]["mongodb"]["connectString"].ToString();
            MongoDbConnectorHelper.ConnectionString = _mongodbConnectString;

            SwaggerPort = _jsonObj["swagger"]["port"].Value<int>();
            _usedHttps = false;

            _natsConnectString = _jsonObj["nats"]["connectString"].ToString();

            var systemDbConnectionString = _jsonObj["db"]["mySql"]["connectString"]["system"].ToString();
            var designDbConnectionString = _jsonObj["db"]["mySql"]["connectString"]["design"].ToString();
            MySqlConnectionHelper.Instance.SystemDbConnectionString = systemDbConnectionString;
            MySqlConnectionHelper.Instance.DesighDbConnectionString = designDbConnectionString;
#pragma warning restore CS8602, CS8604 // null 가능 참조에 대한 역참조입니다.

            _db = new ServerInfoSharedRepo();

            if (false == LoadWorldInfoForTDD())
                return false;

            return true;
        }

        /// <summary>
        /// World정보를 읽어온다.
        /// </summary>
        private bool LoadWorldInfo()
        {
            var serverId = ServerId;
            var serverInfo = _db.GetServerInfo(serverId);
            if (serverInfo == null)
            {
                _logger.Error($"failed LoadWorldInfo()");
                return false;
            }
            if (serverInfo.server_type != (int)ServerType.Community)
            {
                _logger.Error($"incorrected server type:{serverInfo.server_type}");
                return false;
            }

            WorldId = serverInfo.world_id;
            _port = serverInfo.port;

            var tblLobby = _db.GetLobbyServerInfo(WorldId);
            if (0 == tblLobby.op_port)
            {
                _logger.Error($"not found lobby server. world_id :{WorldId}");
                return false;
            }
            _lobbyServerIp = tblLobby.ipaddr;
            _lobbyServerOpPort = tblLobby.op_port;

            // 고유한 서버ID
            SnowflakeIdGenerator.Instance.SetWorkerId(serverInfo.server_id);

            _logger.Warn($"Load Config Success {serverInfo.server_id}");

            return true;
        }

        /// <summary>
        /// TDD를 위한 World정보 로드
        /// </summary>
        /// <returns></returns>
        private bool LoadWorldInfoForTDD()
        {
            var serverId = ServerId;
            var serverInfo = _db.GetServerInfo(serverId);
            if (serverInfo == null)
            {
                _logger.Error($"failed LoadWorldInfo()");
                return false;
            }


            WorldId = serverInfo.world_id;
            _port = serverInfo.port;

            var tblLobby = _db.GetLobbyServerInfo(WorldId);
            if (0 == tblLobby.op_port)
            {
                _logger.Error($"not found lobby server. world_id :{WorldId}");
                return false;
            }
            _lobbyServerIp = tblLobby.ipaddr;
            _lobbyServerOpPort = tblLobby.op_port;

            // 고유한 서버ID
            SnowflakeIdGenerator.Instance.SetWorkerId(serverInfo.server_id);

            _logger.Warn($"Load Config Success {serverInfo.server_id}");

            return true;
        }
    }
}
