using Library.Component;
using Library.Connector;
using Library.DBTables.MongoDb;
using Library.ECSSystem;
using Library.Helper;
using Library.Logger;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;
using static Library.DBTables.MongoDb.MongoDbQuery;

namespace CommunityServer.Component.DataBase;

public class RoomInfoDataSystem : IECSSystem
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    /// <summary>
    /// DB Index 처리
    /// </summary>
    public bool InitIndexes()
    {
        var mongoDb = MongoDbConnectorHelper.Instance;

        //채팅
        {
            var databaseName = MongoDbDataBase.ChatApp;
            var collectionName = MongoDbCollection.ChatMessage;
            var database = mongoDb.GetDatabase(databaseName);
            var collection = database.GetCollection<ChatMessage>(collectionName);

            var indexKeysDefinition = Builders<ChatMessage>.IndexKeys.Ascending(x => x.RoomId);
            var createIndexOptions = new CreateIndexOptions();
            var indexModel = new CreateIndexModel<ChatMessage>(indexKeysDefinition, createIndexOptions);

            var indexName = collection.Indexes.CreateOne(indexModel);
            _logger.InfoEx(() => $"Created index: {indexName}");
        }
        // 채팅 룸 정보
        {
            var databaseName = MongoDbDataBase.ChatApp;
            var collectionName = "chatRoom";
            var database = mongoDb.GetDatabase(databaseName);
            var collection = database.GetCollection<ChatRoom>(collectionName);

            var indexKeysDefinition = Builders<ChatRoom>.IndexKeys.Ascending(x => x.Id);
            var createIndexOptions = new CreateIndexOptions();
            var indexModel = new CreateIndexModel<ChatRoom>(indexKeysDefinition, createIndexOptions);

            var indexName = collection.Indexes.CreateOne(indexModel);
            _logger.InfoEx(() => $"Created index: {indexName}");
        }

        //채팅 룸 정보
        {
            var databaseName = MongoDbDataBase.ChatApp;
            var collectionName = MongoDbCollection.ChatRoomForUser;
            var database = mongoDb.GetDatabase(databaseName);
            var collection = database.GetCollection<ChatRoomForUser>(collectionName);

            var indexKeysDefinition = Builders<ChatRoomForUser>.IndexKeys.Ascending(x => x.RoomId);
            var createIndexOptions = new CreateIndexOptions();
            var indexModel = new CreateIndexModel<ChatRoomForUser>(indexKeysDefinition, createIndexOptions);

            var indexName = collection.Indexes.CreateOne(indexModel);
            _logger.InfoEx(() => $"Created index: {indexName}");
        }

        // 채팅 정보 UserSeq로 정렬
        {
            var databaseName = MongoDbDataBase.ChatApp;
            var collectionName = MongoDbCollection.ChatRoomForUser;
            var database = mongoDb.GetDatabase(databaseName);
            var collection = database.GetCollection<ChatRoomForUser>(collectionName);

            var indexKeysDefinition = Builders<ChatRoomForUser>.IndexKeys.Ascending(x => x.UserSeq);
            var createIndexOptions = new CreateIndexOptions();
            var indexModel = new CreateIndexModel<ChatRoomForUser>(indexKeysDefinition, createIndexOptions);

            var indexName = collection.Indexes.CreateOne(indexModel);
            _logger.InfoEx(() => $"Created index: {indexName}");
        }

        return true;
    }

    /// <summary>
    /// 현재 룸 갯수 체크
    /// </summary>        
    /// <param name="userSeq"></param>
    /// <returns></returns>        
    public int CountRoom(string userSeq)
    {
        var mongoDb = MongoDbConnectorHelper.Instance;
        var databaseName = MongoDbDataBase.ChatApp;
        var database = mongoDb.GetDatabase(databaseName);

        var collectionName = MongoDbCollection.ChatRoomForUser;
        var collection = database.GetCollection<ChatRoomForUser>(collectionName);

        try
        {
            // Users 배열 내에 특정 UserSeq를 가진 문서를 찾는 필터를 생성합니다.
            var filter = Builders<ChatRoomForUser>.Filter.Eq(user => user.UserSeq, userSeq);

            // 해당 필터에 맞는 문서의 수를 세어 반환합니다.
            var count = collection.CountDocuments(filter);
            return (int)count;
        }
        catch (MongoConnectionException mEx)
        {
            MongoDbConnectorHelper.HandleMongoConnectionException(mEx);
            _logger.Error("reconnect to CountRoomAsync.");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error("faild to CountRoomAsync.", ex);
            return 0;
        }
    }

    /// <summary>
    /// 채팅 룸 추가
    /// </summary>
    public (bool, ChatRoom) AddChatRoom(string roomName, List<MongoDbQuery.ChatRoomUser> roomUsers, string serverHost, int serverPort)
    {
        var now = DateTimeHelper.Now;
        var chatRoom = new ChatRoom
        {
            Users = roomUsers,
            Name = roomName,
            LastestChat = string.Empty,

            Host = serverHost,
            Port = serverPort,

            UpdatedTime = now,
            CreatedTime = now,
        };

        try
        {
            var mongoDb = MongoDbConnectorHelper.Instance;
            var databaseName = MongoDbDataBase.ChatApp;
            var database = mongoDb.GetDatabase(databaseName);
            var chatRoomId = string.Empty;

            {
                var collectionName = MongoDbCollection.ChatRoom;
                var collection = database.GetCollection<ChatRoom>(collectionName);

                // 새 문서 삽입                
                collection.InsertOne(chatRoom);

                chatRoomId = chatRoom.Id.ToString();
            }

            // 유저가 들어가 있는 채팅룸
            foreach (var user in roomUsers)
            {
                var userSeq = user.UserSeq;

                var collectionName = MongoDbCollection.ChatRoomForUser;
                var collection = database.GetCollection<ChatRoomForUser>(collectionName);

                // 새 문서 삽입                
                collection.InsertOne(new ChatRoomForUser
                {
                    RoomId = chatRoomId,
                    UserSeq = userSeq,
                    UpdatedTime = now,
                    CreatedTime = now,
                });

            }
            return (true, chatRoom);
        }
        catch (MongoConnectionException mEx)
        {
            // 연결 관련 예외가 발생했을 때 재연결 시도
            MongoDbConnectorHelper.HandleMongoConnectionException(mEx);
            _logger.Error("reconnect to MongoDB.");
            return (false, chatRoom);
        }
        catch (Exception ex)
        {
            _logger.Error("faild to AddChatRoomAsync.", ex);
            return (false, chatRoom);
        }
    }

    /// <summary>
    /// 유저 삭제
    /// </summary>
    public bool RemoveUserFromChatRoom(string roomId, string userSeqToRemove)
    {
        // mongoDb는 MongoClient의 인스턴스입니다.
        var mongoDb = MongoDbConnectorHelper.Instance;
        var databaseName = "chatApp";
        var database = mongoDb.GetDatabase(databaseName);

        try
        {
            // ChatRoom정보에서 User삭제
            {
                var collectionName = "chatRoom";
                var collection = database.GetCollection<ChatRoom>(collectionName);

                // roomId에 해당하는 ChatRoom 문서를 찾아 UserSeqs에서 userSeqToRemove를 제거합니다.
                var roomObjectId = new ObjectId(roomId);
                var filter = Builders<ChatRoom>.Filter.Eq(x => x.Id, roomObjectId);
                var update = Builders<ChatRoom>.Update.PullFilter(x => x.Users,
                    Builders<ChatRoomUser>.Filter.Eq(x => x.UserSeq, userSeqToRemove));


                // 문서 업데이트 후 반환 받기
                var updatedChatRoom = collection.FindOneAndUpdate(filter,
                    update,
                    new FindOneAndUpdateOptions<ChatRoom, ChatRoom>
                    {
                        ReturnDocument = ReturnDocument.After
                    });

                // UserSeqs의 사용자 수가 0명인 경우 룸 삭제
                if (updatedChatRoom != null && updatedChatRoom.Users.Count == 0)
                {
                    // 룸을 지운다.
                    collection.DeleteOne(filter);

                    // 룸에 해당되는 채팅 메시지도 다 지운다.                        
                    {
                        var chatCollectionName = "chatMessage";
                        var chatCollection = database.GetCollection<ChatMessage>(chatCollectionName);
                        // RoomId가 주어진 값과 일치하는 모든 문서를 찾아 삭제합니다.
                        var chatFilter = Builders<ChatMessage>.Filter.Eq(m => m.RoomId, roomId);
                        var result = chatCollection.DeleteMany(chatFilter);
                    }
                }
            }

            // 유저정보에서 userSeq를 지운다.
            {
                var collectionName = MongoDbCollection.ChatRoomForUser;
                var collection = database.GetCollection<ChatRoomForUser>(collectionName);

                var filter = Builders<ChatRoomForUser>.Filter.And(
                            Builders<ChatRoomForUser>.Filter.Eq(x => x.RoomId, roomId),
                            Builders<ChatRoomForUser>.Filter.Eq(x => x.UserSeq, userSeqToRemove));

                // 필터에서 지운다.
                collection.DeleteOne(filter);
            }

            return true;
        }
        catch (MongoConnectionException mEx)
        {
            // 연결 관련 예외가 발생했을 때 재연결 시도
            MongoDbConnectorHelper.HandleMongoConnectionException(mEx);
            _logger.Error("reconnect to MongoDB.");
            return false;
        }
        catch (Exception ex)
        {
            // 오류가 발생한 경우 트랜잭션 취소            
            _logger.Error("Failed to RemoveUserFromChatRoomAsync in a transaction.", ex);
            return false;
        }
    }


    // Room정보를 가져오자        
    public (bool, ChatRoom) FetchRoom(string roomId)
    {
        try
        {
            var mongoDb = MongoDbConnectorHelper.Instance;
            var databaseName = MongoDbDataBase.ChatApp;
            var collectionName = MongoDbCollection.ChatRoom;
            var database = mongoDb.GetDatabase(databaseName);
            var collection = database.GetCollection<ChatRoom>(collectionName);

            var roomObjectId = new ObjectId(roomId);

            var filter = Builders<ChatRoom>.Filter.Eq(room => room.Id, roomObjectId);
            var find = collection.Find(filter).FirstOrDefault();

            if (find == null)
            {
                // 룸이 없음
                return (false, new ChatRoom());
            }

            return (true, find);
        }
        catch (MongoConnectionException mEx)
        {
            // 연결 관련 예외가 발생했을 때 재연결 시도
            MongoDbConnectorHelper.HandleMongoConnectionException(mEx);
            _logger.Error("reconnect to MongoDB.");
            return (false, new ChatRoom());
        }
        catch (Exception ex)
        {
            _logger.Error("faild to FetchRoomListAsync.", ex);
            return (false, new ChatRoom());
        }
    }

    public (bool, string) AddedChat(string roomId, MongoDbQuery.ChatMessage chatMessage)
    {
        var chatId = string.Empty;
        try
        {
            var mongoDb = MongoDbConnectorHelper.Instance;
            var databaseName = MongoDbDataBase.ChatApp;
            var database = mongoDb.GetDatabase(databaseName);

            // 채팅 추가
            {
                var collectionName = MongoDbCollection.ChatMessage;
                var collection = database.GetCollection<ChatMessage>(collectionName);
                collection.InsertOne(chatMessage);

                chatId = chatMessage.Id.ToString();
            }

            // 마지막 채팅 글씨 변경
            {
                var collectionName = MongoDbCollection.ChatRoom;
                var collection = database.GetCollection<ChatRoom>(collectionName);

                var roomObjectId = new ObjectId(roomId);
                var filter = Builders<ChatRoom>.Filter.Eq(x => x.Id, roomObjectId);
                var update = Builders<ChatRoom>.Update.Set(x => x.LastestChat, chatMessage.Chat)
                    .Set(x => x.UpdatedTime, DateTimeHelper.Now);

                var result = collection.UpdateOne(filter, update);

                var updated = result.IsAcknowledged && result.ModifiedCount == 1;
            }

            {
                var collectionName = MongoDbCollection.ChatRoomForUser;
                var collection = database.GetCollection<ChatRoomForUser>(collectionName);

                var filter = Builders<ChatRoomForUser>.Filter.Eq(x => x.RoomId, roomId);
                var update = Builders<ChatRoomForUser>.Update.Set(x => x.UpdatedTime, DateTimeHelper.Now);
                var result = collection.UpdateMany(filter, update);

                var updated = result.IsAcknowledged && result.ModifiedCount == 1;
            }


            return (true, chatId);

        }
        catch (MongoConnectionException mEx)
        {
            MongoDbConnectorHelper.HandleMongoConnectionException(mEx);
            _logger.Error("reconnect to MongoDB.");
            return (false, chatId);
        }
        catch (Exception ex)
        {
            _logger.Error("faild to UpdaetedLastChat.", ex);
            return (false, chatId);
        }
    }

    public bool UpdateChatRoomEnteredInfo(string roomId, string serverHost, int port)
    {
        var mongoDb = MongoDbConnectorHelper.Instance;
        var databaseName = MongoDbDataBase.ChatApp;
        var database = mongoDb.GetDatabase(databaseName);

        var collectionName = MongoDbCollection.ChatRoom;
        var collection = database.GetCollection<ChatRoom>(collectionName);

        try
        {
            var objectRoomId = new ObjectId(roomId);
            var now = DateTimeHelper.Now;
            var filter = Builders<ChatRoom>.Filter.Eq(message => message.Id, objectRoomId);
            var update = Builders<ChatRoom>.Update.Set(x => x.Host, serverHost)
                    .Set(x => x.Port, port)
                    .Set(x => x.UpdatedTime, now);

            var result = collection.UpdateOne(filter, update);
            var updated = result.IsAcknowledged && result.ModifiedCount == 1;
            return updated;

        }
        catch (MongoConnectionException mEx)
        {
            MongoDbConnectorHelper.HandleMongoConnectionException(mEx);
            _logger.Error("reconnect to UpdateChatRoomEnteredInfoAsync.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error("faild to UpdateChatRoomEnteredInfoAsync.", ex);
            return false;
        }
    }
}
