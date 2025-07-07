using Akka.Actor;
using Akka.IO;
using CommunityServer.Component.DataBase;
using CommunityServer.Helper;
using CommunityServer.InfoRepository;
using Library.AkkaActors;
using Library.Component;
using Library.DTO;
using Library.ECSSystem;
using Library.Helper;
using Library.Logger;
using Library.messages;
using Library.Repository.ConcurrentUser;
using log4net;
using Messages;
using NatsMessages;
using System.Collections.Concurrent;
using System.Reflection;

namespace CommunityServer.User
{
    public class UserCordiatorActor : UntypedActor, IECSSystemManager, IComponentManager
    {
        private class CharElementInfo
        {
            public string RemoteAddress { get; set; } = string.Empty;
            public ulong UserSeq { get; set; } = 0;
            public ulong CharSeq { get; set; } = 0;
        }
        private class ConcurrentUserCheckedTimer
        {
            public static ConcurrentUserCheckedTimer Instance { get; } = new();

        }


        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private Dictionary<System.Type, Func<object, IActorRef, bool>> _funcHandlers = null!;
        private Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool, bool>> _onOtherServerHandlers = null!;

        private readonly ConcurrentDictionary<string, IActorRef> _sessions = new();
        private readonly ConcurrentDictionary<IActorRef, CharElementInfo> _sessionRefs = new();

        private readonly IActorRef _listenerRef;
        private readonly IActorRef _worldRef;

        // Component 패턴
        protected readonly ECSSystemManager _systemManager = new ECSSystemManager();
        protected readonly ComponentManager _components = new ComponentManager();
        private ICancelable? _cancelable = null!;

        public static IActorRef ActorOf(IUntypedActorContext context, IActorRef listenerRef, IActorRef worldRef)
        {
            var prop = Props.Create(() => new UserCordiatorActor(listenerRef, worldRef));
            return context.ActorOf(prop, ActorPaths.UserCordiator.Name);
        }

        public UserCordiatorActor()
        {
            _listenerRef = ActorRefs.Nobody;
            _worldRef = ActorRefs.Nobody;

            InitMessengerHandler();

        }

        public UserCordiatorActor(IActorRef listenerActor, IActorRef worldRef)
        {
            _listenerRef = listenerActor;
            _worldRef = worldRef;

            InitMessengerHandler();
        }
        private void InitMessengerHandler()
        {
            _funcHandlers = new Dictionary<System.Type, Func<object, IActorRef, bool>>
            {
                // 캐릭터 관련 메시지
                {typeof(U2UCMessage.EnterCommunityRequest), (data, sender) => OnEnterCommunityRequest((U2UCMessage.EnterCommunityRequest)data, sender)},
                {typeof(U2UCMessage.RoomUserInvitedRequest), (data, sender) => OnRoomUserInvitedRequest((U2UCMessage.RoomUserInvitedRequest)data, sender)},
                {typeof(U2UCMessage.ZoneUserInvitedRequest), (data, sender) => OnZoneUserInvitedRequest((U2UCMessage.ZoneUserInvitedRequest)data, sender)},

                {typeof(U2UCMessage.MessageFromOtherServer), (data, sender) => OnMessageFromOtherServer((U2UCMessage.MessageFromOtherServer)data, sender)},
                {typeof(U2UCMessage.UserNoticeNoti), (data, sender) => OnUserNoticeNoti((U2UCMessage.UserNoticeNoti)data, sender)},
            };

            // 다른 서버에서 온 핸들러
            _onOtherServerHandlers = new Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool, bool>>
            {
                {NatsMessageWrapper.PayloadOneofCase.RoomUserInvitedRequest, (data, sender, calledTdd) => OnRecvRoomUserInvitedRequest(data, sender, calledTdd)},
                {NatsMessageWrapper.PayloadOneofCase.ZoneUserInvitedRequest, (data, sender, calledTdd) => OnRecvZoneUserInvitedRequest(data, sender, calledTdd)},

                {NatsMessageWrapper.PayloadOneofCase.ChatRoomCreatedNoti, (data, sender, calledTdd) => OnRecvChatRoomCreatedNoti(data, sender, calledTdd)},
                {NatsMessageWrapper.PayloadOneofCase.ChatRoomAddedChatNoti, (data, sender, calledTdd) => OnRecvChatRoomAddedChatNoti(data, sender, calledTdd)},
                {NatsMessageWrapper.PayloadOneofCase.ChatRoomLeavedNoti, (data, sender, calledTdd) => OnRecvChatRoomLeavedNoti(data, sender, calledTdd)},
                {NatsMessageWrapper.PayloadOneofCase.ChatRoomAddedReadNoti, (data, sender, calledTdd) => OnRecvChatRoomAddedReadNoti(data, sender, calledTdd)},

                {NatsMessageWrapper.PayloadOneofCase.AlertChangedNoti, (data, sender, calledTdd) => OnRecvAlertChangedNoti(data, sender, calledTdd)},

                {NatsMessageWrapper.PayloadOneofCase.GmtoolUserKickNoti, (data, sender, calledTdd) => OnRecvGmtoolUserKick(data, sender, calledTdd)},
                {NatsMessageWrapper.PayloadOneofCase.GmtoolChatNoti, (data, sender, calledTdd) => OnRecvGmtoolChatNoti(data, sender, calledTdd)},

                {NatsMessageWrapper.PayloadOneofCase.FeatureBlockChangedNoti, (data, sender, calledTdd) => OnRecvFeatureBlockChangedNoti(data, sender, calledTdd)},
                {NatsMessageWrapper.PayloadOneofCase.EmergencyNoticeChangedNoti, (data, sender, calledTdd) => OnRecvEmergencyNoticeChangedNoti(data, sender, calledTdd)},
            };
        }
        /// <summary>
        /// Component 관리
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public T AddSystem<T>(T component) where T : class, IECSSystem
        {
            return _systemManager.AddSystem<T>(component);
        }
        public T? GetSystem<T>() where T : class, IECSSystem
        {
            return _systemManager.GetSystem<T>();
        }

        public void RemoveSystem<T>() where T : class, IECSSystem
        {
            _systemManager.RemoveSystem<T>();
        }
        /// <summary>
        /// Component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public T AddComponent<T>(T component) where T : class, IECSComponent
        {
            return _components.AddComponent(component);
        }
        public T? GetComponent<T>() where T : class, IECSComponent
        {
            return _components.GetComponent<T>();
        }
        public void RemoveComponent<T>() where T : class, IECSComponent
        {
            _components.RemoveComponent<T>();
        }

        protected override void PreStart()
        {
            base.PreStart();
            try
            {
                ActorRefsHelper.Instance.Actors[ActorPaths.UserCordiator.Path] = Self;

                AddSystem<UserRepository>(new UserRepository()); // MySql연결
                AddSystem<UserSessionRepository>(new UserSessionRepository()); // Redis연결     
                AddSystem<RoomInfoDataSystem>(new RoomInfoDataSystem()); // Redis연결     
                AddSystem<ConcurrentUserSystem>(ConcurrentUserSystem.Of(new ConcurrentUserRepo()));

                AddComponent<ConcurrentUserComponent>(new ConcurrentUserComponent());

                OnConcurrentUserCheckedTimer();
            }
            catch (Exception ex)
            {
                _logger.Error("failed to PreStart", ex);
            }


        }

        protected override void PostStop()
        {
            try
            {
                _sessions.Clear();
                _sessionRefs.Clear();
                CharacterInfoRepository.Instance.CharacterInfos.Clear();
                CharacterInfoRepository.Instance.UserSeqToCharSeqs.Clear();

                // Compone 제거
                RemoveSystem<UserRepository>();
                RemoveSystem<UserSessionRepository>();
                RemoveSystem<RoomInfoDataSystem>();
                RemoveSystem<ConcurrentUserSystem>();
                _systemManager.Dispose();

                RemoveComponent<ConcurrentUserComponent>();
                _components.Dispose();

                _cancelable?.Cancel();
                _cancelable = null;
            }
            catch (Exception ex)
            {
                _logger.Error("failed to PostStop", ex);
            }
            finally
            {
                base.PostStop();
            }

        }

        // here we are overriding the default SupervisorStrategy
        // which is a One-For-One strategy w/ a Restart directive
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // duration
                x =>
                {
                    return Directive.Restart;

                    ////Maybe we consider ArithmeticException to not be application critical
                    ////so we just ignore the error and keep going.
                    //if (x is ArithmeticException) return Directive.Resume;

                    ////Error that we cannot recover from, stop the failing actor
                    //else if (x is NotSupportedException) return Directive.Stop;

                    ////In all other cases, just restart the failing actor
                    //else return Directive.Restart;
                });
        }
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Session.RegisteredRequest registeredRequest:
                    {
                        OnReceiveRegister(registeredRequest);
                        break;
                    }
                case Session.ClosedRequest closedRequest:
                    {
                        var remoteAddress = closedRequest.RemoteAdress;
                        if (_sessions.TryGetValue(remoteAddress, out var session))
                        {
                            Context.Unwatch(session);
                            Context.Stop(session);
                            if (_sessionRefs.TryGetValue(session, out var characterInfo))
                            {
                                if (characterInfo.CharSeq > 0)
                                {
                                    var instance = CharacterInfoRepository.Instance;
                                    instance.Remove(characterInfo.UserSeq, characterInfo.CharSeq);
                                }

                                _sessionRefs.TryRemove(session, out var _);
                            }
                            // remove the actor reference from the dictionary
                            _sessions.TryRemove(remoteAddress, out _);

                            var redisSystem = GetSystem<UserSessionRepository>();
                            var serverId = ConfigHelper.Instance.ServerId;
                            if (redisSystem != null)
                            {
                                redisSystem.DecreaseServerUserCount(serverId);
                            }

                            var userComponent = GetComponent<ConcurrentUserComponent>();
                            if (userComponent != null)
                            {
                                userComponent.Decrement();
                            }
                        }
                        break;
                    }
                case Session.Broadcast broadcast:
                    {
                        var sendMessage = new Session.Unicast
                        {
                            Message = broadcast.Message
                        };

                        foreach (var sessionRef in _sessions.Values)
                        {
                            sessionRef.Tell(sendMessage);
                        }
                        break;
                    }
                case Tcp.WritingResumed writingResumed:
                    {
                        break;
                    }
                case Terminated terminated:
                    {
                        // Here, handle the termination of the watched actor.
                        // For example, you might want to create a new actor or simply log the termination.
                        if (_sessionRefs.TryGetValue(terminated.ActorRef, out var character))
                        {
                            var remoteAdress = character.RemoteAddress;
                            _logger.DebugEx(() => $"client disconnected:{remoteAdress}");

                            if (_sessions.TryGetValue(remoteAdress, out var session))
                            {
                                Context.Unwatch(session);
                                if (character.CharSeq > 0)
                                {
                                    var instance = CharacterInfoRepository.Instance;

                                    instance.CharacterInfos.TryRemove(character.CharSeq, out var _);
                                    instance.UserSeqToCharSeqs.TryRemove(character.UserSeq, out var _);
                                }
                                _sessionRefs.TryRemove(session, out var _);

                                // remove the actor reference from the dictionary
                                _sessions.TryRemove(remoteAdress, out _);

                                var redisSystem = GetSystem<UserSessionRepository>();
                                var serverId = ConfigHelper.Instance.ServerId;
                                if (redisSystem != null)
                                {
                                    redisSystem.DecreaseServerUserCount(serverId);
                                }

                                var userComponent = GetComponent<ConcurrentUserComponent>();
                                if (userComponent != null)
                                {
                                    userComponent.Decrement();
                                }

                            }

                        }
                        break;
                    }
                case S2SMessage.UserDeadLetter userDeadLetter:
                    {
                        OnUserDeadLetter(userDeadLetter);
                        break;
                    }
                case ConcurrentUserCheckedTimer _:
                    {
                        OnConcurrentUserCheckedTimer();
                        break;
                    }
                default:
                    {
                        try
                        {
                            if (true == _funcHandlers.TryGetValue(message.GetType(), out var handler))
                            {
                                handler(message, Sender);
                                return;
                            }
                            _logger.Error($"not found messageType:{message.GetType()}");
                            Unhandled(message);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Exception OnReceive ", ex);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 원격 세션 추가
        /// </summary>
        /// <param name="message"></param>
        private void OnReceiveRegister(Session.RegisteredRequest message)
        {
            // create a new session actor
            var remoteSender = message.Sender;
            var packetEncrypt = ConfigHelper.Instance.PacketEncrypt;

            var sessionProp = Props.Create(() => new UserSessionActor(Self, message.RemoteAdress, remoteSender, packetEncrypt));
            var sessionRef = Context.ActorOf(sessionProp);
            remoteSender.Tell(new Tcp.Register(sessionRef));

            // 자식 Session이 PostStop일때 Terminated 이벤트를 받을 수 있다.
            Context.Watch(sessionRef);

            // session관리에 넣어주자
            _sessions.TryAdd(message.RemoteAdress, sessionRef);
            _sessionRefs.TryAdd(sessionRef, new CharElementInfo
            {
                CharSeq = 0,
                UserSeq = 0,
                RemoteAddress = message.RemoteAdress
            });

            var redisSystem = GetSystem<UserSessionRepository>();
            var serverId = ConfigHelper.Instance.ServerId;
            if (redisSystem != null)
            {
                redisSystem.IncreaseServerUserCount(serverId);
            }

            var userComponent = GetComponent<ConcurrentUserComponent>();
            if (userComponent != null)
            {
                userComponent.Increment();
            }

        }

        /// <summary>
        /// 커뮤니티 서버 입장
        /// </summary>        
        private bool OnEnterCommunityRequest(U2UCMessage.EnterCommunityRequest data, IActorRef sender)
        {
            var charActorRef = data.CharActorRef;
            var userSeq = data.UserSeq;
            var charSeq = data.CharSeq;
            var tddActorRef = data.CalledTddActorRef;

            if (tddActorRef == null || tddActorRef == ActorRefs.Nobody) // tdd에서 호출한 것이 아니다.
            {
                if (false == _sessionRefs.TryGetValue(charActorRef, out var sessionRef))
                {
                    charActorRef.Tell(new U2UCMessage.EnterCommunityResponse
                    {
                        ErrorCode = ErrorCode.NotFoundCharacter,
                        CalledTddActorRef = data.CalledTddActorRef,
                    });
                    return true;
                }
                sessionRef.UserSeq = userSeq;
                sessionRef.CharSeq = charSeq;
            }

            var instance = CharacterInfoRepository.Instance;

            {
                instance.Add(userSeq, charSeq, charActorRef);
            }

            charActorRef.Tell(new U2UCMessage.EnterCommunityResponse
            {
                ErrorCode = ErrorCode.Succeed,
                CalledTddActorRef = data.CalledTddActorRef,
            });


            return true;
        }

        /// <summary>
        /// 룸 초대
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        /// <returns></returns>        
        private bool OnRoomUserInvitedRequest(U2UCMessage.RoomUserInvitedRequest data, IActorRef sender)
        {
            var targetCharSeq = data.TargetCharSeq;
            var charActorRef = data.CharActorRef;
            var fromCharSeq = data.FromCharSeq;
            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                charActorRef.Tell(new U2UCMessage.RoomUserInvitedResponse
                {
                    ErrorCode = ErrorCode.Succeed,
                    CalledTddActorRef = data.CalledTddActorRef,
                });

                if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.MessageQueuePath, out var natsActor))
                {
                    // 현재 커뮤니티 서버에서 유저를 못찾았으면 다른 커뮤니티 서버에 메시지를 날리자                    
                    var db = GetSystem<UserRepository>();
                    if (db != null)
                    {
                        var (finded, targetCharSession) = db.FetchUserSessionInfoByCharSeq(targetCharSeq);
                        var toServerId = targetCharSession.connected_community_serverid;

                        if (finded && toServerId > 0)
                        {
                            natsActor.Tell(new S2SMessage.NatsPubliish
                            {
                                NatsMessageWrapper = new NatsMessages.NatsMessageWrapper
                                {
                                    FromServerId = ConfigHelper.Instance.ServerId,
                                    ToServerId = toServerId,
                                    TargetCharSeq = targetCharSeq.ToString(),

                                    RoomUserInvitedRequest = new RoomUserInvitedRequest
                                    {
                                        RoomSeq = data.RoomSeq,
                                        RoomServerInfo = data.RoomServerInfo,
                                        FromCharSeq = data.FromCharSeq,
                                        TargetCharSeq = targetCharSeq,
                                        GameMode = (int)data.GameModeType,
                                    }
                                }
                            });
                        }
                    }
                }
                return true;
            }

            // 초대한 유저에게 성공을 보냄
            {
                charActorRef.Tell(new U2UCMessage.RoomUserInvitedResponse
                {
                    ErrorCode = ErrorCode.Succeed,
                    CalledTddActorRef = data.CalledTddActorRef,
                });
            }

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.RoomUserInvitedNoti
            {
                RoomSeq = data.RoomSeq,
                RoomServerInfo = data.RoomServerInfo,
                FromCharSeq = data.FromCharSeq,
                GameModeType = data.GameModeType,
            });

            return true;
        }

        /// <summary>
        /// Zone 서버 입장
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private bool OnZoneUserInvitedRequest(U2UCMessage.ZoneUserInvitedRequest data, IActorRef sender)
        {
            var targetCharSeq = data.TargetCharSeq;
            var charActorRef = data.CharActorRef;
            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                charActorRef.Tell(new U2UCMessage.ZoneUserInvitedResponse
                {
                    ErrorCode = ErrorCode.Succeed,
                    CalledTddActorRef = data.CalledTddActorRef,
                });
                if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.MessageQueuePath, out var natsActor))
                {
                    // 현재 커뮤니티 서버에서 유저를 못찾았으면 다른 커뮤니티 서버에 메시지를 날리자                    
                    var db = GetSystem<UserRepository>();
                    if (db != null)
                    {
                        var (finded, targetCharSession) = db.FetchUserSessionInfoByCharSeq(targetCharSeq);
                        var toServerId = targetCharSession.connected_community_serverid;

                        _logger.DebugEx(() => $"UserSession OnZoneUserInvitedRequest toServerId:{toServerId} targetCharSeq:{targetCharSeq}");

                        if (finded && toServerId > 0)
                        {
                            natsActor.Tell(new S2SMessage.NatsPubliish
                            {
                                NatsMessageWrapper = new NatsMessages.NatsMessageWrapper
                                {
                                    FromServerId = ConfigHelper.Instance.ServerId,
                                    ToServerId = toServerId,
                                    TargetCharSeq = targetCharSeq.ToString(),

                                    ZoneUserInvitedRequest = new ZoneUserInvitedRequest
                                    {
                                        ZoneServerInfo = data.ZoneServerInfo,
                                        FromCharSeq = data.FromCharSeq,
                                        TargetCharSeq = data.TargetCharSeq,
                                        MapIndex = data.MapIndex,
                                        ZoneIndex = data.ZoneIndex,
                                        GameMode = (int)data.GameModeType,
                                    }
                                }
                            });
                        }
                    }

                }
                return true;
            }

            // 초대한 유저에게 성공을 보냄
            {
                charActorRef.Tell(new U2UCMessage.ZoneUserInvitedResponse
                {
                    ErrorCode = ErrorCode.Succeed,
                    CalledTddActorRef = data.CalledTddActorRef,
                });
            }

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.ZoneUserInvitedNoti
            {
                FromCharSeq = data.FromCharSeq,
                ZoneServerInfo = data.ZoneServerInfo,
                MapIndex = data.MapIndex,
                ZoneIndex = data.ZoneIndex,
                GameModeType = data.GameModeType,
            });

            return true;
        }
        /// <summary>
        /// 다른 서버에 온 메시지
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        /// <returns></returns>        
        private bool OnMessageFromOtherServer(U2UCMessage.MessageFromOtherServer data, IActorRef sender)
        {
            try
            {
                _logger.DebugEx(() => $"OnMessageFromOtherServer From({data.NatsMessageWrapper.FromServerId})->To({data.NatsMessageWrapper.ToServerId}) data:{PacketLogHelper.Instance.GetNatsMessageToJson(data.NatsMessageWrapper)}");

                if (_onOtherServerHandlers.TryGetValue(data.NatsMessageWrapper.PayloadCase, out var handler))
                {
                    handler(data.NatsMessageWrapper, sender, false);
                }
                else
                {
                    _logger.DebugEx(() => $"not found OnMessageFromOtherServer");
                }
                return true;
            }

            catch (Exception ex)
            {
                _logger.Error("OnMessageFromOtherServer", ex);
                return true;
            }
        }

        /// <summary>
        /// 다른 서버에 온 룸 초대
        /// </summary>        
        private bool OnRecvRoomUserInvitedRequest(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var data = request.RoomUserInvitedRequest;

            var targetCharSeq = data.TargetCharSeq;
            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
                return true;

            var gameModeType = ConvertHelper.ToEnum<GameModeType>(data.GameMode);

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.RoomUserInvitedNoti
            {
                RoomSeq = data.RoomSeq,
                RoomServerInfo = data.RoomServerInfo,
                FromCharSeq = data.FromCharSeq,
                GameModeType = gameModeType,
            });

            return true;
        }
        private bool OnRecvZoneUserInvitedRequest(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var data = request.ZoneUserInvitedRequest;
            var targetCharSeq = data.TargetCharSeq;
            var instance = CharacterInfoRepository.Instance;

            _logger.DebugEx(() => $"OnRecvZoneUserInvitedRequest targetCharSeq:{targetCharSeq}");

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                return true;
            }

            var gameMode = ConvertHelper.ToEnum<GameModeType>(data.GameMode);
            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.ZoneUserInvitedNoti
            {
                FromCharSeq = data.FromCharSeq,
                ZoneServerInfo = data.ZoneServerInfo,
                MapIndex = data.MapIndex,
                ZoneIndex = data.ZoneIndex,
                GameModeType = gameMode,

            });

            return true;
        }

        /// <summary>
        /// 채팅 메시지
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sender"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>        
        private bool OnRecvChatRoomCreatedNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var targetCharSeq = ulong.Parse(request.TargetCharSeq);

            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                return true;
            }
            var chatRoomCreatedNoti = request.ChatRoomCreatedNoti;

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.UserNoti
            {
                Noti = new MessageWrapper
                {
                    ChatRoomCreatedNoti = chatRoomCreatedNoti,
                },

            });

            return true;
        }

        /// <summary>
        /// 룸 채팅 추가
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sender"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>
        private bool OnRecvChatRoomAddedChatNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var targetCharSeq = ulong.Parse(request.TargetCharSeq);

            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                return true;
            }
            var noti = request.ChatRoomAddedChatNoti;

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.UserNoti
            {
                Noti = new MessageWrapper
                {
                    ChatRoomAddedChatNoti = noti,
                },

            });

            return true;
        }

        /// <summary>
        /// 룸에서 나오기
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sender"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>
        private bool OnRecvChatRoomLeavedNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var targetCharSeq = ulong.Parse(request.TargetCharSeq);

            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                return true;
            }
            var noti = request.ChatRoomLeavedNoti;

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.UserNoti
            {
                Noti = new MessageWrapper
                {
                    ChatRoomLeavedNoti = noti,
                },

            });

            return true;
        }

        /// <summary>
        /// 읽기 추가
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sender"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>        
        private bool OnRecvChatRoomAddedReadNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var targetCharSeq = ulong.Parse(request.TargetCharSeq);

            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                return true;
            }
            var noti = request.ChatRoomAddedReadNoti;

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.UserNoti
            {
                Noti = new MessageWrapper
                {
                    ChatRoomAddedReadNoti = noti,
                },

            });

            return true;
        }

        /// <summary>
        /// 알림 변경
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sender"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>        
        private bool OnRecvAlertChangedNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            var targetCharSeq = ulong.Parse(request.TargetCharSeq);

            var instance = CharacterInfoRepository.Instance;

            if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
            {
                return true;
            }
            var noti = request.AlertChangedNoti;

            // 대상에게 알림
            targetCharActorRef.Tell(new U2UCMessage.UserNoti
            {
                Noti = new MessageWrapper
                {
                    AlertChangedNoti = noti,
                },

            });

            return true;
        }

        /// <summary>
        /// 유저 mailbox deadLetter발생
        /// </summary>        
        private bool OnUserDeadLetter(S2SMessage.UserDeadLetter data)
        {
            var userActor = data.UserRecipientRef;
            if (_sessionRefs.ContainsKey(userActor))
            {
                // 정보가 userSession이면
                userActor.Tell(data);
            }
            return true;
        }

        /// <summary>
        /// 유저 킥
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sender"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>        
        private bool OnRecvGmtoolUserKick(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            try
            {
                var targetCharSeq = ulong.Parse(request.TargetCharSeq);

                var instance = CharacterInfoRepository.Instance;

                if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
                {
                    return true;
                }
                var noti = request.GmtoolUserKickNoti;

                // 대상에게 알림
                targetCharActorRef.Tell(new U2UCMessage.UserNoti
                {
                    Noti = new MessageWrapper
                    {
                        GmtoolUserKickNoti = noti.Clone(),
                    },

                });
            }
            catch (Exception ex)
            {
                _logger.Error("OnRecvGmtoolUserKick", ex);
            }
            return true;
        }

        private bool OnRecvGmtoolChatNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            try
            {
                // 전체 유저에게 알림
                var instance = CharacterInfoRepository.Instance;
                var allCharSeqs = instance.CharacterInfos.Keys.ToList(); // 전체 유저
                foreach (var targetCharSeq in allCharSeqs)
                {
                    if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
                        continue;

                    var noti = request.GmtoolChatNoti;

                    // 대상에게 알림
                    targetCharActorRef.Tell(new U2UCMessage.UserNoti
                    {
                        Noti = new MessageWrapper
                        {
                            GmtoolChatNoti = noti.Clone(),
                        },

                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("OnRecvGmtoolChatNoti", ex);
            }

            return true;
        }

        /// <summary>
        /// 유저 공지
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        private bool OnUserNoticeNoti(U2UCMessage.UserNoticeNoti data, IActorRef sender)
        {
            try
            {
                // 전체 유저에게 알림
                var instance = CharacterInfoRepository.Instance;
                var allCharSeqs = instance.CharacterInfos.Keys.ToList(); // 전체 유저
                foreach (var targetCharSeq in allCharSeqs)
                {
                    if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
                        continue;

                    // 대상에게 알림
                    targetCharActorRef.Tell(new U2UCMessage.UserNoti
                    {
                        Noti = new MessageWrapper
                        {
                            GmtoolUserNoticeNoti = new GmtoolUserNoticeNoti
                            {
                                Title = data.Title,
                                Content = data.Content,
                            },
                        },

                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("OnUserNoticeNoti", ex);
            }

            return true;
        }

        private bool OnRecvFeatureBlockChangedNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            try
            {
                // 전체 유저에게 알림
                var instance = CharacterInfoRepository.Instance;
                var allCharSeqs = instance.CharacterInfos.Keys.ToList(); // 전체 유저
                foreach (var targetCharSeq in allCharSeqs)
                {
                    if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
                        continue;

                    // 대상에게 알림
                    targetCharActorRef.Tell(new U2UCMessage.UserNoti
                    {
                        Noti = new MessageWrapper
                        {
                            FeatureBlockChangedNoti = new FeatureBlockChangedNoti
                            {
                            },
                        },

                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("OnRecvFeatureBlockChangedNoti", ex);
            }

            return true;
        }
        private bool OnRecvEmergencyNoticeChangedNoti(NatsMessageWrapper request, IActorRef sender, bool calledTdd)
        {
            try
            {
                // 전체 유저에게 알림
                var instance = CharacterInfoRepository.Instance;
                var allCharSeqs = instance.CharacterInfos.Keys.ToList(); // 전체 유저
                foreach (var targetCharSeq in allCharSeqs)
                {
                    if (false == instance.CharacterInfos.TryGetValue(targetCharSeq, out var targetCharActorRef))
                        continue;

                    // 대상에게 알림
                    targetCharActorRef.Tell(new U2UCMessage.UserNoti
                    {
                        Noti = new MessageWrapper
                        {
                            EmergencyNoticeChangedNoti = new EmergencyNoticeChangedNoti
                            {
                            },
                        },

                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("OnRecvEmergencyNoticeChangedNoti", ex);
            }

            return true;
        }
        /// <summary>
        /// User 수 관련 저장 타이머
        /// </summary>
        private void OnConcurrentUserCheckedTimer()
        {
            _cancelable = null;

            var serverId = ConfigHelper.Instance.ServerId;

            var userSystem = GetSystem<ConcurrentUserSystem>();
            var userComponent = GetComponent<ConcurrentUserComponent>();
            if (userSystem != null)
            {
                userSystem.Save(serverId, userComponent);
            }

            var context = Context;
            var self = Self;

            _cancelable = ScheduleFactory.Start(context,
                ConstInfo.CheckedConcurrentUserCountTime, self, ConcurrentUserCheckedTimer.Instance);

        }
    }

}
