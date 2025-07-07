using Messages;
using Akka.Actor;
using Akka.TestKit.NUnit;
using CommunityServer.World;
using CommunityServer.User;
using Library.DTO;
using Library.messages;

namespace ApiServer.Tests.CommunityServer;
public partial class CommunityConrollerTest : TestKit
{
    private int _defaultActorSleep = 2000;
    private TimeSpan _durationTime = TimeSpan.FromMinutes(10);

    private void EnterServerUsers(IActorRef[] userActors)
    {
        var i = 0;
        foreach (var actor in userActors)
        {
            if (i >= _maxAccount)
                break;

            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    CommnunityServerEnterRequest = new CommnunityServerEnterRequest
                    {
                        SessionGuid = _defaultSessoinGuid[i],
                    }
                },
            };
            actor.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.CommnunityServerEnterResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));

            ++i;
        }

    }

    [Test]
    public void Should_ChatRoom_Created()
    {
        bool packetEncrypt = false;
        var worldActor = Sys.ActorOf(Props.Create(() => new WorldActor()));
        var userCordiatorActor = Sys.ActorOf(Props.Create(() => new UserCordiatorActor()));
        var userActor = Sys.ActorOf(Props.Create(() => new UserSessionActor(packetEncrypt)));

        Thread.Sleep(_defaultActorSleep); // redis 초기화를 위한 대기

        EnterServerUsers(new[] { userActor });

        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomCreatedRequest = new ChatRoomCreatedRequest
                    {

                    }
                },
            };
            userActor.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomCreatedResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.InvalidRoomName));
        }

        // 정상 적인 룸생성
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomCreatedRequest = new ChatRoomCreatedRequest
                    {
                        SessionGuid = _defaultSessoinGuid[0],
                        RoomName = "room1",
                        TargetUserSeq = _defaultUserSeq[1],
                    }
                },
            };
            userActor.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomCreatedResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
        }
    }

    [Test]
    public void Should_ChatRoom_AddedChat()
    {
        var packetEncrypt = false;
        var worldActor = Sys.ActorOf(Props.Create(() => new WorldActor()));
        var userCordiatorActor = Sys.ActorOf(Props.Create(() => new UserCordiatorActor()));
        var userActor = Sys.ActorOf(Props.Create(() => new UserSessionActor(packetEncrypt)));

        Thread.Sleep(_defaultActorSleep); // redis 초기화를 위한 대기

        EnterServerUsers(new[] { userActor });

        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomAddedChatRequest = new ChatRoomAddedChatRequest
                    {

                    }
                },
            };
            userActor.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomAddedChatResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundRoom));
        }

        // 정상 적인 룸생성
        var roomId = string.Empty;
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomCreatedRequest = new ChatRoomCreatedRequest
                    {
                        SessionGuid = _defaultSessoinGuid[0],
                        RoomName = "room1",
                        TargetUserSeq = _defaultUserSeq[1],
                    }
                },
            };
            userActor.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomCreatedResponse.ErrorCode;

            roomId = response.ChatRoomCreatedResponse.RoomId;
        }

        // 정상적인 채팅 추가
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomAddedChatRequest = new ChatRoomAddedChatRequest
                    {
                        RoomId = roomId,
                        Chat = "test",
                        ChatType = 0,
                        JsonData = "{}",                        
                    }
                },
            };
            userActor.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomAddedChatResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
        }
    }

    [Test]
    public void Should_ChatRoom_LeaveChatRoom()
    {
        var packetEncrypt = false;

        var worldActor = Sys.ActorOf(Props.Create(() => new WorldActor()));
        var userCordiatorActor = Sys.ActorOf(Props.Create(() => new UserCordiatorActor()));
        var userActor1 = Sys.ActorOf(Props.Create(() => new UserSessionActor(packetEncrypt)));
        var userActor2 = Sys.ActorOf(Props.Create(() => new UserSessionActor(packetEncrypt)));

        Thread.Sleep(_defaultActorSleep); // redis 초기화를 위한 대기

        EnterServerUsers(new[] { userActor1, userActor2 });

        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomLeavedRequest = new ChatRoomLeavedRequest
                    {

                    }
                },
            };
            userActor1.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomLeavedResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundRoom));
        }

        // 정상 적인 룸생성
        var roomId = string.Empty;
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomCreatedRequest = new ChatRoomCreatedRequest
                    {
                        SessionGuid = _defaultSessoinGuid[0],
                        RoomName = "room1",
                        TargetUserSeq = _defaultUserSeq[1],
                    }
                },
            };
            userActor1.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomCreatedResponse.ErrorCode;

            roomId = response.ChatRoomCreatedResponse.RoomId;
        }

        //정상적인 채팅
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomAddedChatRequest = new ChatRoomAddedChatRequest
                    {
                        RoomId = roomId,
                        Chat = "test",
                        ChatType = 0,
                        JsonData = "{}",
                    }
                },
            };
            userActor1.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomAddedChatResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
        }

        // 정상적인 룸 탈퇴 (1유저)
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomLeavedRequest = new ChatRoomLeavedRequest
                    {
                        RoomId = roomId,                        
                    }
                },
            };
            userActor1.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomLeavedResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
        }

        // 정상적인 룸 탈퇴 (2유저)
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomLeavedRequest = new ChatRoomLeavedRequest
                    {
                        RoomId = roomId,
                    }
                },
            };
            userActor2.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomLeavedResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
        }

        // 정상적으로 룸이 사라졌는지 체크
        {
            var request = new TDDMessage.Message
            {
                MessageWrapper = new MessageWrapper
                {
                    ChatRoomAddedChatRequest = new ChatRoomAddedChatRequest
                    {
                        RoomId = roomId,
                        Chat = "test",
                        ChatType = 0,
                        JsonData = "{}",
                    }
                },
            };
            userActor1.Tell(request);

            // 
            var response = ExpectMsg<MessageWrapper>(_durationTime);
            var errorCode = response.ChatRoomAddedChatResponse.ErrorCode;
            Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundRoom));
        }
    }

}

//{
//    [TestFixture]
//    public partial class CommunityConrollerTest : TestKit
//    {
//        [Test]
//        public void Should_AddedRead()
//        {
//            var facade = new DbServiceFacade();
//            var service = new ChatService();
//            var controller = new ChatController(facade, service);

//            {
//                var request = new ChatRoomAddedReadRequest
//                {
//                };
//                var rtn = controller.AddedRead(request).Result as BadRequestObjectResult;
//                var response = rtn.Value as ChatRoomAddedReadResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundCharBySeesionGuid));
//            }

//            // 읽기 항목 완료
//            {
//                var request = new ChatRoomAddedReadRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[1],
//                    RoomId = "65f40f9ca07e6d83979c39d0",
//                };                
//                request.ChatIds.Add("65f40f9ca07e6d83979c39d1");

//                var rtn = controller.AddedRead(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomAddedReadResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
//            }
//        }

//        [Test]
//        public void Should_ChatRoomList()
//        {
//            var facade = new DbServiceFacade();
//            var service = new ChatService();
//            var controller = new ChatController(facade, service);

//            // 잘못된 파라미터
//            {
//                var request = new ChatRoomListRequest
//                {
//                };
//                var rtn = controller.ChatRoomList(request).Result as BadRequestObjectResult;
//                var response = rtn.Value as ChatRoomListResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundCharBySeesionGuid));
//            }
//            // 룸 추가
//            var roomId = string.Empty;
//            {
//                var request = new ChatRoomCreatedRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[0],
//                    RoomName = "room1",
//                    TargetUserSeq = _defaultUserSeq[1],

//                };
//                var rtn = controller.CreatedRoom(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomCreatedResponse;
//                var errorCode = response.ErrorCode;                
//                roomId  = response.RoomId;                
//            }

//            // 채팅 추가
//            {
//                var request = new ChatRoomAddedChatRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[0],
//                    RoomId = roomId,
//                    Chat = "test",
//                    ChatType = 0,
//                    JsonData = "{}",
//                };

//                var rtn = controller.AddedChat(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomAddedChatResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
//            }

//            // user:1이 안읽은 메시지 수
//            {
//                var request = new ChatRoomListRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[1],
//                };

//                var rtn = controller.ChatRoomList(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomListResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
//            }
//        }



//        [Test]
//        public void Should_FetchChats()
//        {
//            var facade = new DbServiceFacade();
//            var service = new ChatService();
//            var controller = new ChatController(facade, service);

//            // 잘못된 파라미터
//            {
//                var request = new ChatRoomChatsRequest
//                {
//                };
//                var rtn = controller.FetchChats(request).Result as BadRequestObjectResult;
//                var response = rtn.Value as ChatRoomChatsResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundCharBySeesionGuid));
//            }

//            var roomId = string.Empty;
//            // 채팅방 생성
//            {
//                var request = new ChatRoomCreatedRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[0],
//                    RoomName = "room1",
//                    TargetUserSeq = _defaultUserSeq[1],

//                };
//                var rtn = controller.CreatedRoom(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomCreatedResponse;
//                var errorCode = response.ErrorCode;

//                roomId = response.RoomId;
//            }

//            // 채팅 추가
//            {
//                var request = new ChatRoomAddedChatRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[0],
//                    RoomId = roomId,
//                    Chat = "test",
//                    ChatType = 1,
//                    JsonData = "{}",
//                };

//                var rtn = controller.AddedChat(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomAddedChatResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
//            }

//            {
//                var request = new ChatRoomChatsRequest
//                {
//                    SessionGuid = _defaultSessoinGuid[0],
//                    RoomId = roomId,

//                    Offset = 0,
//                    Limit = 10,
//                };

//                var rtn = controller.FetchChats(request).Result as OkObjectResult;
//                var response = rtn.Value as ChatRoomChatsResponse;
//                var errorCode = response.ErrorCode;
//                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
//                Assert.That(response.Chats.Count, Is.EqualTo(1));
//            }
//        }


//    }
//}
