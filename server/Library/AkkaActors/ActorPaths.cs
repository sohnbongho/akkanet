namespace Library.AkkaActors;

public static class ActorPaths
{
    public static string System = "Server";

    // console Writer        
    public static readonly ActorMetaData ReaderConsole = new ActorMetaData("readerConsole", string.Empty);
    public static readonly ActorMetaData WriterConsole = new ActorMetaData("writerConsole", string.Empty);

    /*--------------------------------------
     * System관련
     --------------------------------------*/
    // MySql DB Actor
    public static readonly ActorMetaData DbCordiator = new ActorMetaData("dbcordiator", string.Empty);
    public static readonly ActorMetaData GameDb = new ActorMetaData("gamedb", "dbCordiator");

    // Redis Actor
    public static readonly ActorMetaData RedisCordiator = new ActorMetaData("rediscordiator", string.Empty);
    public static readonly ActorMetaData Redis = new ActorMetaData("redis", "dbCordiator");

    // MongoDB Actor
    public static readonly ActorMetaData MongdDbCordiator = new ActorMetaData("mongodbcordiator", string.Empty);

    // Nats Actor
    public static readonly ActorMetaData Nats = new ActorMetaData("nats", string.Empty);

    // Rabbit Actor
    public static readonly ActorMetaData RabbitMQActor = new ActorMetaData("rabbit", string.Empty);

    // Remote MessageQueue Actor
    public static readonly ActorMetaData AkkaRemoteActor = new ActorMetaData("akkaremoteactor", string.Empty);

    // Kafka Actor
    public static readonly ActorMetaData Kafka = new ActorMetaData("kafka", string.Empty);
    public static readonly ActorMetaData KafkaProducer = new ActorMetaData("kafkaproducer", "kafka");
    public static readonly ActorMetaData KafkaConsumer = new ActorMetaData("kafkaconsumer", "kafka");

    // 유저 Session Actor
    public static readonly ActorMetaData Listener = new ActorMetaData("listener", string.Empty);
    public static readonly ActorMetaData UserCordiator = new ActorMetaData("usercordiator", "listener");
    public static readonly ActorMetaData UserSession = new ActorMetaData("usersession", $@"listener/usercordiator");

    // 서버 관리 Session Actor
    public static readonly ActorMetaData ServerManagedListener = new ActorMetaData("servermanagedlistener", string.Empty);
    public static readonly ActorMetaData ServerManagedCordiator = new ActorMetaData("servercordiator", "serverManagedListener");

    // Lobby Server 와 연결하는 액터
    public static readonly ActorMetaData LobbyClient = new ActorMetaData("lobbyclient", string.Empty);


    /*--------------------------------------
     * World관련
     --------------------------------------*/
    public static readonly ActorMetaData World = new ActorMetaData("world", string.Empty);

    // zone
    public static readonly ActorMetaData ZoneCordiator = new ActorMetaData("zonecordiator", "world");
    public static readonly ActorMetaData Zone = new ActorMetaData("zone", $@"world/zonecordiator");

    // room
    public static readonly ActorMetaData RoomCordiator = new ActorMetaData("roomcordiator", "world");
    public static readonly ActorMetaData Room = new ActorMetaData("room", $@"world/roomCordiator");

    // room
    public static readonly ActorMetaData ChatRoomCordiator = new ActorMetaData("chatroomcordiator", "world");
    public static readonly ActorMetaData ChatRoom = new ActorMetaData("chatroom", $@"world/chatroomcordiator");

    // 메시지 큐 액터 이름
    public static string MessageQueuePath => ActorPaths.Nats.Path;
    
    /// <summary>
    /// 멀티 클라이언트 전용
    /// </summary>
    public static readonly ActorMetaData ZoneMultiClientManager = new ActorMetaData("zonemulticlientmanager", string.Empty);
    public static readonly ActorMetaData RoomMultiClientManager = new ActorMetaData("roommulticlientmanager", string.Empty);

    public static readonly ActorMetaData DummyClient = new ActorMetaData("dummyclient", string.Empty);
    public static readonly ActorMetaData SessionGuidGenerator = new ActorMetaData("sessionuuidgenerator", string.Empty);

    // 공지 관련 처리
    public static readonly ActorMetaData UserNotice = new ActorMetaData("usernotice", string.Empty);
}


