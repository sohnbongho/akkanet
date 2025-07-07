using Akka.Actor;
using CommunityServer.Component.DataBase;
using CommunityServer.Helper;
using CommunityServer.InfoRepository;
using Library.AkkaActors;
using Library.AkkaActors.Socket;
using Library.DBTables.MongoDb;
using Library.DTO;
using Library.Helper;
using Library.Logger;
using Library.MessageHandling;
using Library.messages;
using Messages;


namespace CommunityServer.User;

public partial class UserSessionActor : SessionActor
{
    /// <summary>
    /// 유저에게 알림
    /// </summary>        
    public bool UserNoti(string userSeqStr, MessageWrapper wrapper)
    {
        var db = GetSystem<UserRepository>();
        var mongoDb = GetSystem<RoomInfoDataSystem>();

        var instance = CharacterInfoRepository.Instance;
        var userSeq = ulong.Parse(userSeqStr);
        if (instance.UserSeqToCharSeqs.TryGetValue(userSeq, out var charSeq))
        {
            if (instance.CharacterInfos.TryGetValue(charSeq, out var userSession))
            {
                // 다른 서버에 있는 유저이다.
                userSession.Tell(new U2UCMessage.UserNoti
                {
                    Noti = wrapper,
                });
            }
        }
        else
        {
            if (db == null || mongoDb == null)
                return true;

            // 다른 서버에 있는 유저이다.
            var tblMember = db.FetchMember(userSeq);
            if (tblMember.char_seq <= 0)
            {
                _logger.Error($"ChatRoomInfoHelper.UserNoti not found char:{tblMember.char_seq}");
                return false;
            }

            var (finded, toCharSession) = db.FetchUserSessionInfoByUserSeq(tblMember.user_seq);
            if (false == finded || toCharSession.connected_community_serverid <= 0)
            {
                _logger.DebugEx(() => $"ChatRoomInfoHelper.UserNoti not found userSeq:{tblMember.user_seq} conneted_serverid:{toCharSession.connected_serverid} connected_community_serverid:{toCharSession.connected_community_serverid}");
                return false;
            }

            var toServerId = toCharSession.connected_community_serverid;
            if (false == ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.MessageQueuePath, out var natsActor))
            {
                _logger.Error($"ChatRoomInfoHelper.UserNoti not found natsActor");
                return false;
            }

            var natsMessageWrapper = new NatsMessages.NatsMessageWrapper
            {
                FromServerId = ConfigHelper.Instance.ServerId,
                ToServerId = toServerId,
                TargetCharSeq = tblMember.char_seq.ToString(),
            };
            if (wrapper.PayloadCase == MessageWrapper.PayloadOneofCase.ChatRoomAddedChatNoti)
            {
                natsMessageWrapper.ChatRoomAddedChatNoti = wrapper.ChatRoomAddedChatNoti;
            }
            else if (wrapper.PayloadCase == MessageWrapper.PayloadOneofCase.ChatRoomLeavedNoti)
            {
                natsMessageWrapper.ChatRoomLeavedNoti = wrapper.ChatRoomLeavedNoti;
            }
            else if (wrapper.PayloadCase == MessageWrapper.PayloadOneofCase.ChatRoomAddedReadNoti)
            {
                natsMessageWrapper.ChatRoomAddedReadNoti = wrapper.ChatRoomAddedReadNoti;
            }
            else
            {
                _logger.Error($"ChatRoomInfoHelper.UserNoti not found PayloadCase:{wrapper.PayloadCase.ToString()}");
                return false;
            }

            // 커뮤니티 서버에 룸 생성 알림
            natsActor.Tell(new S2SMessage.NatsPubliish
            {
                NatsMessageWrapper = natsMessageWrapper,

            });
        }

        return true;
    }

    /// <summary>
    /// 채팅룸 추가
    /// </summary>
    /// <param name="wrapper"></param>
    /// <param name="sessionRef"></param>
    /// <param name="calledTdd"></param>
    /// <returns></returns>
    [SessionMessageHandler(MessageWrapper.PayloadOneofCase.ChatRoomCreatedRequest)]
    private bool OnRecvChatRoomCreatedRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
    {
        var request = wrapper.ChatRoomCreatedRequest;
        IActorRef? calledTddActorRef = null;
        if (calledTdd)
        {
            calledTddActorRef = sessionRef;
        }
        if (_webUserInfo == null)
        {
            return true;
        }

        var sessionUserSeq = _webUserInfo.user_seq;
        var sessionCharSeq = _webUserInfo.char_seq;

        if (0 == sessionCharSeq)
        {
            var response = new MessageWrapper
            {
                ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                {
                    ErrorCode = (int)ErrorCode.NotFoundCharacter
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }


        var db = GetSystem<UserRepository>();
        var mongoDb = GetSystem<RoomInfoDataSystem>();

        try
        {
            if (db == null || mongoDb == null)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                    {
                        ErrorCode = (int)ErrorCode.DbInitializedError
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }
            var webUserInfo = _webUserInfo;

            var userSeqStr = webUserInfo.user_seq.ToString();
            var roomCount = mongoDb.CountRoom(userSeqStr);
            if (roomCount >= ConstInfo.MaxChatRoomCount)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                    {
                        ErrorCode = (int)ErrorCode.OverMaxChatRoom, // 이미 등록
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            var roomName = request.RoomName;
            if (string.IsNullOrEmpty(roomName))
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                    {
                        ErrorCode = (int)ErrorCode.InvalidRoomName, // 이미 등록
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            if (false == ulong.TryParse(request.TargetUserSeq, out var userId))
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                    {
                        ErrorCode = (int)ErrorCode.NotFoundCharacter, // 이미 등록
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }


            var fromUserSeqStr = webUserInfo.user_seq.ToString();
            var toUserSeqStr = request.TargetUserSeq;
            var toUserSeq = ulong.Parse(request.TargetUserSeq);

            var roomUserSeqs = new[] { fromUserSeqStr, toUserSeqStr };
            var roomUsers = new List<MongoDbQuery.ChatRoomUser>();

            foreach (var roomUserSeq in roomUserSeqs)
            {
                var userSeq = ulong.Parse(roomUserSeq);

                var tblMember = db.FetchMember(userSeq);
                if (0 == tblMember.user_seq)
                    continue;

                var charSeq = tblMember.char_seq;
                var tblCharacter = db.FetchCharacter(charSeq);
                if (0 == tblCharacter.char_seq)
                    continue;

                var chatRoomUser = new MongoDbQuery.ChatRoomUser
                {
                    Name = tblCharacter.nickname,
                    UserSeq = userSeq.ToString(),
                    CharSeq = charSeq.ToString(),
                    ProfileImage = string.Empty,
                };
                roomUsers.Add(chatRoomUser);
            }

            var serverHost = string.Empty;
            var serverPort = 0;
            var worldId = ConfigHelper.Instance.WorldId;
            var serverInfos = db.GetCommunityServerInfos(worldId);
            if (serverInfos.Count > 0)
            {
                Random rnd = new Random();
                int randomInt = rnd.Next(0, serverInfos.Count); // 0 이상 X 미만

                serverHost = serverInfos[randomInt].ipaddr;
                serverPort = serverInfos[randomInt].port;
            }

            // 채트룸 추가
            var (added, chatRoom) = mongoDb.AddChatRoom(roomName, roomUsers, serverHost, serverPort);
            if (false == added)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                    {
                        ErrorCode = (int)ErrorCode.DbInsertedError, // 이미 등록
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            var roomId = chatRoom.Id.ToString();

            var chatRoomCreatedNoti = new ChatRoomCreatedNoti
            {
                RoomId = roomId,
                RoomName = chatRoom.Name,

                ChatServerHost = serverHost,
                ChatServerPort = serverPort,
            };
            var response = new MessageWrapper
            {
                ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                {
                    ErrorCode = (int)ErrorCode.Succeed,

                    RoomId = roomId,
                    RoomName = roomName,

                    ChatServerHost = serverHost,
                    ChatServerPort = serverPort,
                }
            };

            foreach (var roomUser in roomUsers)
            {
                if (false == ulong.TryParse(roomUser.UserSeq, out var userSeq))
                    continue;

                var tblMember = db.FetchMember(userSeq);

                var chatRoomUserInfo = new ChatRoomUserInfo
                {
                    Name = roomUser.Name,
                    UserSeq = roomUser.UserSeq,
                    CharSeq = roomUser.CharSeq,
                    ProfileImage = tblMember.image_url,
                };

                response.ChatRoomCreatedResponse.Users.Add(chatRoomUserInfo);
                chatRoomCreatedNoti.Users.Add(chatRoomUserInfo);
            }

            // 상대방에게 알림
            var (finded, toCharSession) = db.FetchUserSessionInfoByUserSeq(toUserSeq);
            if (finded && toCharSession.connected_community_serverid != 0)
            {
                var toServerId = toCharSession.connected_community_serverid;
                var toCharSeq = toCharSession.char_seq;

                if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.MessageQueuePath, out var natsActor))
                {
                    // 커뮤니티 서버에 룸 생성 알림
                    natsActor.Tell(new S2SMessage.NatsPubliish
                    {
                        NatsMessageWrapper = new NatsMessages.NatsMessageWrapper
                        {
                            FromServerId = ConfigHelper.Instance.ServerId,
                            ToServerId = toServerId,
                            TargetCharSeq = toCharSeq.ToString(),

                            ChatRoomCreatedNoti = chatRoomCreatedNoti,
                        }
                    });
                }
            }

            TellClient(response, calledTddActorRef);
            return true;

        }
        catch (Exception ex)
        {
            _logger.Error("fail to CreatedRoom", ex);

            var response = new MessageWrapper
            {
                ChatRoomCreatedResponse = new ChatRoomCreatedResponse
                {
                    ErrorCode = (int)ErrorCode.DbInsertedError, // 이미 등록
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }
    }

    /// <summary>
    /// 채팅 추가
    /// </summary>
    /// <param name="wrapper"></param>
    /// <param name="sessionRef"></param>
    /// <param name="calledTdd"></param>
    /// <returns></returns>        
    [SessionMessageHandler(MessageWrapper.PayloadOneofCase.ChatRoomAddedChatRequest)]
    private bool OnRecvChatRoomAddedChatRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
    {
        var request = wrapper.ChatRoomAddedChatRequest;
        IActorRef? calledTddActorRef = null;
        if (calledTdd)
        {
            calledTddActorRef = sessionRef;
        }
        if(_webUserInfo == null)
        {
            return true;
        }

        var charSeq = _webUserInfo.char_seq;
        if (0 == charSeq)
        {
            var response = new MessageWrapper
            {
                ChatRoomAddedChatResponse = new ChatRoomAddedChatResponse
                {
                    ErrorCode = (int)ErrorCode.NotFoundCharacter
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }

        var db = GetSystem<UserRepository>();
        var mongoDb = GetSystem<RoomInfoDataSystem>();
        var webUserInfo = _webUserInfo;
        var sessionUserSeq = _webUserInfo.user_seq;
        var sessionCharSeq = _webUserInfo.char_seq;

        var roomId = request.RoomId;
        try
        {
            if (db == null || mongoDb == null)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomAddedChatResponse = new ChatRoomAddedChatResponse
                    {
                        ErrorCode = (int)ErrorCode.DbInitializedError
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            var chatType = request.ChatType; ;
            var now = DateTimeHelper.Now;
            var writedUserSeqStr = sessionUserSeq.ToString();

            var (find, roomInfo) = mongoDb.FetchRoom(roomId);
            if (find == false)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomAddedChatResponse = new ChatRoomAddedChatResponse
                    {
                        ErrorCode = (int)ErrorCode.NotFoundRoom
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            var roomUsers = roomInfo.Users.Select(x => x.UserSeq).ToList();

            var readedUsers = new List<string> { writedUserSeqStr };
            var unReadedUsers = roomInfo.Users.Select(x => x.UserSeq).Except(readedUsers).ToList();

            var chatMessage = new MongoDbQuery.ChatMessage
            {
                RoomId = roomId,
                ReadUserSeqs = readedUsers,
                UnreadUserSeqs = unReadedUsers,
                WritedUserSeq = writedUserSeqStr,
                Chat = request.Chat,

                ChatType = chatType,
                JsonData = request.JsonData,

                CreatedDate = now,
            };

            var (updated, chatId) = mongoDb.AddedChat(roomId, chatMessage);

            var noti = new ChatRoomAddedChatNoti
            {
                RoomId = roomId,

                UserSeq = sessionUserSeq,
                Chat = request.Chat,

                ChatType = chatType,
                JsonData = request.JsonData,

                ChatId = chatId,
                WritedUserSeq = writedUserSeqStr,
            };
            unReadedUsers.ForEach(x => noti.UnreadUserSeqs.Add(x.ToString()));
            readedUsers.ForEach(x => noti.ReadUserSeqs.Add(x.ToString()));

            var writedUserSeq = ulong.Parse(writedUserSeqStr);
            foreach (var userSeqStr in roomUsers)
            {
                UserNoti(userSeqStr, new MessageWrapper
                {
                    ChatRoomAddedChatNoti = noti,
                });
            }
            var response = new MessageWrapper
            {
                ChatRoomAddedChatResponse = new ChatRoomAddedChatResponse
                {
                    ErrorCode = (int)ErrorCode.Succeed,
                    ChatId = chatId,
                }
            };
            TellClient(response, calledTddActorRef);
            return true;

        }
        catch (Exception ex)
        {
            _logger.Error("fail to AddedChat", ex);
            var response = new MessageWrapper
            {
                ChatRoomAddedChatResponse = new ChatRoomAddedChatResponse
                {
                    ErrorCode = (int)ErrorCode.DbInsertedError,
                }
            };
            TellClient(response, calledTddActorRef);
            return true;
        }
    }

    /// <summary>
    /// 채팅룸 떠나기
    /// </summary>            
    [SessionMessageHandler(MessageWrapper.PayloadOneofCase.ChatRoomLeavedRequest)]
    private bool OnRecvChatRoomLeavedRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
    {
        var request = wrapper.ChatRoomLeavedRequest;
        IActorRef? calledTddActorRef = null;
        if (calledTdd)
        {
            calledTddActorRef = sessionRef;
        }
        if (_webUserInfo == null)
            return true;

        var sessionUserSeq = _webUserInfo.user_seq;
        var sessionCharSeq = _webUserInfo.char_seq;

        if (0 == sessionCharSeq)
        {
            var response = new MessageWrapper
            {
                ChatRoomLeavedResponse = new ChatRoomLeavedResponse
                {
                    ErrorCode = (int)ErrorCode.NotFoundCharacter
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }

        var db = GetSystem<UserRepository>();
        var mongoDb = GetSystem<RoomInfoDataSystem>();
        var webUserInfo = _webUserInfo;

        try
        {
            if (db == null || mongoDb == null)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomLeavedResponse = new ChatRoomLeavedResponse
                    {
                        ErrorCode = (int)ErrorCode.DbInitializedError
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            var roomId = request.RoomId;
            var userSeqStr = sessionUserSeq.ToString();

            // 룸을 찾는다.
            var (find1, roomInfo1) = mongoDb.FetchRoom(roomId);
            if (false == find1)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomLeavedResponse = new ChatRoomLeavedResponse
                    {
                        ErrorCode = (int)ErrorCode.NotFoundRoom
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }


            var removed = mongoDb.RemoveUserFromChatRoom(roomId, userSeqStr);
            //var removed = mongoDb.RemoveUserFromChatRoomWithNoTransaction(roomId, userSeqStr);

            // 유저를 지우고 룸에 인원이 0명이면 방도 지워지기에,
            // 방에 0명이 아닌 상황에서는 남은 유저들에게 알림을 보내야 한다.
            var (find, roomInfo) = mongoDb.FetchRoom(roomId);
            if (find)
            {
                var roomUsers = roomInfo.Users.Select(x => x.UserSeq).ToList();
                var removedUserSeq = sessionUserSeq;

                foreach (var notiUserSeqStr in roomUsers)
                {
                    UserNoti(notiUserSeqStr, new MessageWrapper
                    {
                        ChatRoomLeavedNoti = new ChatRoomLeavedNoti
                        {
                            RoomId = roomId,
                            LeavedUserSeq = removedUserSeq,
                        }
                    });
                }
            }

            var response = new MessageWrapper
            {
                ChatRoomLeavedResponse = new ChatRoomLeavedResponse
                {
                    ErrorCode = (int)ErrorCode.Succeed,
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("fail to LeaveChatRoom", ex);

            var response = new MessageWrapper
            {
                ChatRoomLeavedResponse = new ChatRoomLeavedResponse
                {
                    ErrorCode = (int)ErrorCode.DbInsertedError,
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }
    }

    /// <summary>
    /// 채팅룸 새로 고침
    /// </summary>        
    [SessionMessageHandler(MessageWrapper.PayloadOneofCase.ChatRoomRefreshRequest)]
    private bool OnRecvChatRoomRefreshRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
    {
        var request = wrapper.ChatRoomRefreshRequest;
        IActorRef? calledTddActorRef = null;
        if (calledTdd)
        {
            calledTddActorRef = sessionRef;
        }
        if (_webUserInfo == null)
            return true;

        var sessionUserSeq = _webUserInfo.user_seq;
        var sessionCharSeq = _webUserInfo.char_seq;

        if (0 == sessionCharSeq)
        {
            var response = new MessageWrapper
            {
                ChatRoomRefreshResponse = new ChatRoomRefreshResponse
                {
                    ErrorCode = (int)ErrorCode.NotFoundCharacter
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }

        var db = GetSystem<UserRepository>();
        var mongoDb = GetSystem<RoomInfoDataSystem>();
        var webUserInfo = _webUserInfo;

        try
        {
            if (db == null || mongoDb == null)
            {
                var response1 = new MessageWrapper
                {
                    ChatRoomRefreshResponse = new ChatRoomRefreshResponse
                    {
                        ErrorCode = (int)ErrorCode.DbInitializedError
                    }
                };

                TellClient(response1, calledTddActorRef);
                return true;
            }

            var roomId = request.RoomId;
            var worldId = ConfigHelper.Instance.WorldId;
            var serverHost = string.Empty;
            var serverPort = 0;

            var serverInfos = db.GetCommunityServerInfos(worldId);
            if (serverInfos.Count > 0)
            {
                Random rnd = new Random();
                int randomInt = rnd.Next(0, serverInfos.Count); // 0 이상 X 미만

                serverHost = serverInfos[randomInt].ipaddr;
                serverPort = serverInfos[randomInt].port;
            }

            var updated = mongoDb.UpdateChatRoomEnteredInfo(roomId, serverHost, serverPort);

            var response = new MessageWrapper
            {
                ChatRoomRefreshResponse = new ChatRoomRefreshResponse
                {
                    ErrorCode = (int)ErrorCode.Succeed,

                    RoomId = roomId,
                    ChatServerPort = serverPort,
                    ChatServerHost = serverHost,
                }
            };

            TellClient(response, calledTddActorRef);
            return true;

        }
        catch (Exception ex)
        {
            _logger.Error("fail to FetchChats", ex);

            var response = new MessageWrapper
            {
                ChatRoomRefreshResponse = new ChatRoomRefreshResponse
                {
                    ErrorCode = (int)ErrorCode.DbInsertedError,
                }
            };

            TellClient(response, calledTddActorRef);
            return true;
        }
    }


}
