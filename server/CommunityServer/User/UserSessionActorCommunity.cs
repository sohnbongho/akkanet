using Akka.Actor;
using CommunityServer.Component.DataBase;
using CommunityServer.Helper;
using Library.AkkaActors;
using Library.AkkaActors.Socket;
using Library.DBTables;
using Library.DTO;
using Library.Helper;
using Library.Logger;
using Library.MessageHandling;
using Library.messages;
using Messages;

namespace CommunityServer.User
{
    public partial class UserSessionActor : SessionActor
    {
        private RedisCommonQuery.UserSessionInfo? _webUserInfo = null!;
        private bool _registered = false;

        private ulong _connectedUserSeq = 0;
        private ulong _connectedCharSeq = 0;

        private void InitCommunityInfo()
        {
            _webUserInfo = new RedisCommonQuery.UserSessionInfo();
            _registered = false;

        }
        private void DisposeCommunity()
        {
            _webUserInfo = null;
        }

        /// <summary>
        /// TDD 관련 메시지
        /// </summary>        
        [InternalMessageHandler(typeof(TDDMessage.Message))]
        private bool OnRecvTddUserMessageRequest(object objRequest, IActorRef sender)
        {
            var data = objRequest as TDDMessage.Message;
            if (data == null)
                return true;

            var wrapper = data.MessageWrapper;
            if (_sessionHandlerManager != null)
            {
                var handle = _sessionHandlerManager.HandleMessage(wrapper, Sender, true);
                return handle;
            }
            return true;
        }

        /// <summary>
        /// Community Server 입장
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="sessionRef"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>        
        [SessionMessageHandler(MessageWrapper.PayloadOneofCase.CommnunityServerEnterRequest)]
        private bool OnRecvCommnunityServerEnterRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
        {
            var request = wrapper.CommnunityServerEnterRequest;
            IActorRef calledTddActorRef = ActorRefs.Nobody;
            if (calledTdd)
            {
                calledTddActorRef = sessionRef;
            }

            var db = GetSystem<UserRepository>();
            var redis = GetSystem<UserSessionRepository>();

            if (_registered)
            {
                var response = new MessageWrapper
                {
                    CommnunityServerEnterResponse = new CommnunityServerEnterResponse
                    {
                        ErrorCode = (int)ErrorCode.AlreadyEnteredZone, // 이미 등록
                    }
                };

                TellClient(response, calledTddActorRef);

                if (calledTdd == false)
                {
                    ForceCloseConnection(); // 강제로 연결종료
                }

                return true;
            }
            if (redis == null)
                return true;
            if (db == null)
                return true;

            var sessionGuid = request.SessionGuid;
            _webUserInfo = redis.FetchUserSession(sessionGuid);

            if (0 == _webUserInfo.char_seq)
            {
                var response = new MessageWrapper
                {
                    CommnunityServerEnterResponse = new CommnunityServerEnterResponse
                    {
                        ErrorCode = (int)ErrorCode.NotFoundCharacter
                    }
                };

                TellClient(response, calledTddActorRef);
                if (calledTdd == false)
                {
                    ForceCloseConnection(); // 강제로 연결종료
                }

                return true;
            }

            _connectedUserSeq = _webUserInfo.user_seq;
            _connectedCharSeq = _webUserInfo.char_seq;
            var serverId = ConfigHelper.Instance.ServerId;
            db.StoreUserConnectedCommunityServerId(_webUserInfo.user_seq, _webUserInfo.char_seq, serverId); // 접속한 커뮤니티 서버 저장

            SetSessionUserSeq(_webUserInfo.user_seq);


            if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
            {
                userCordiatorActor.Tell(new U2UCMessage.EnterCommunityRequest
                {
                    UserSeq = _webUserInfo.user_seq,
                    CharSeq = _webUserInfo.char_seq,
                    CharActorRef = Self,
                    CalledTddActorRef = calledTddActorRef,
                });
            }

            return true;
        }

        [InternalMessageHandler(typeof(U2UCMessage.EnterCommunityResponse))]
        private bool OnRecvEnterCommunityResponse(object objRequest, IActorRef sender)
        {
            var data = objRequest as U2UCMessage.EnterCommunityResponse;
            if (data == null)
                return true;

            var calledTddActorRef = data.CalledTddActorRef;
            if (ErrorCode.Succeed == data.ErrorCode)
            {
                _registered = true;
            }

            {
                var response = new MessageWrapper
                {
                    CommnunityServerEnterResponse = new CommnunityServerEnterResponse
                    {
                        ErrorCode = (int)data.ErrorCode,
                    }
                };

                TellClient(response, calledTddActorRef);
            }
            if (ErrorCode.Succeed != data.ErrorCode)
            {
                ForceCloseConnection(); // 강제로 연결종료
            }
            return true;
        }

        /// <summary>
        /// 룸 서버 초대
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="sessionRef"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>
        [SessionMessageHandler(MessageWrapper.PayloadOneofCase.RoomUserInvitedRequest)]
        private bool OnRecvRoomUserInvitedRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
        {
            IActorRef calledTddActorRef = ActorRefs.Nobody;
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
                    RoomUserInvitedResponse = new RoomUserInvitedResponse
                    {
                        ErrorCode = (int)ErrorCode.NotFoundCharacter
                    }
                };

                TellClient(response, calledTddActorRef);
                return true;
            }
            var request = wrapper.RoomUserInvitedRequest;
            var gameModeType = ConvertHelper.ToEnum<GameModeType>(request.GameMode);

            if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
            {
                userCordiatorActor.Tell(new U2UCMessage.RoomUserInvitedRequest
                {
                    RoomSeq = request.RoomSeq,
                    RoomServerInfo = request.RoomServerInfo,
                    FromCharSeq = sessionCharSeq,
                    TargetCharSeq = request.TargetCharSeq,
                    GameModeType = gameModeType,

                    CharActorRef = Self,
                    CalledTddActorRef = calledTddActorRef,
                });
            }
            return true;
        }

        [InternalMessageHandler(typeof(U2UCMessage.RoomUserInvitedResponse))]
        private bool OnRecvRoomUserInvitedResponse(object objRequest, IActorRef sender)
        {
            var data = objRequest as U2UCMessage.RoomUserInvitedResponse;
            if (data == null)
                return true;

            var calledTddActorRef = data.CalledTddActorRef;

            {
                var response = new MessageWrapper
                {
                    RoomUserInvitedResponse = new RoomUserInvitedResponse
                    {
                        ErrorCode = (int)data.ErrorCode,
                    }
                };

                TellClient(response, calledTddActorRef);
            }

            return true;
        }

        [InternalMessageHandler(typeof(U2UCMessage.RoomUserInvitedNoti))]
        private bool OnRecvRoomUserInvitedNoti(object objRequest, IActorRef sender)
        {
            var data = objRequest as U2UCMessage.RoomUserInvitedNoti;
            if (data == null)
                return true;

            // 받은 유저에게 보냄
            var fromCharSeq = data.FromCharSeq;
            var db = GetSystem<UserRepository>();
            if (db == null)
            {
                return true;
            }

            var fromCharater = db.FetchCharacter(fromCharSeq);

            var fromCharNickname = fromCharater.nickname != null ? fromCharater.nickname : string.Empty;

            {
                var response = new MessageWrapper
                {
                    RoomUserInvitedNoti = new RoomUserInvitedNoti
                    {
                        FromCharSeq = data.FromCharSeq,
                        FromCharNickname = fromCharNickname,

                        RoomSeq = data.RoomSeq,
                        RoomServerInfo = data.RoomServerInfo,
                        GameMode = (int)data.GameModeType,
                    }
                };

                TellClient(response);
            }
            return true;
        }

        /// <summary>
        /// 존 서버 초대
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="sessionRef"></param>
        /// <param name="calledTdd"></param>
        /// <returns></returns>
        [SessionMessageHandler(MessageWrapper.PayloadOneofCase.ZoneUserInvitedRequest)]
        private bool OnRecvZoneUserInvitedRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
        {
            IActorRef calledTddActorRef = ActorRefs.Nobody;
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
                    ZoneUserInvitedResponse = new ZoneUserInvitedResponse
                    {
                        ErrorCode = (int)ErrorCode.NotFoundCharacter
                    }
                };

                TellClient(response, calledTddActorRef);
                return true;
            }

            var request = wrapper.ZoneUserInvitedRequest;
            var gameMode = ConvertHelper.ToEnum<GameModeType>(request.GameMode);

            if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
            {
                _logger.DebugEx(() => $"UserSession OnRecvZoneUserInvitedRequest FromCharSeq:{sessionCharSeq} TargetCharSeq:{request.TargetCharSeq}");

                userCordiatorActor.Tell(new U2UCMessage.ZoneUserInvitedRequest
                {
                    ZoneServerInfo = request.ZoneServerInfo,
                    FromCharSeq = sessionCharSeq,
                    TargetCharSeq = request.TargetCharSeq,
                    MapIndex = request.MapIndex,
                    ZoneIndex = request.ZoneIndex,
                    GameModeType = gameMode,

                    CharActorRef = Self,
                    CalledTddActorRef = calledTddActorRef,
                });
            }

            return true;
        }

        [InternalMessageHandler(typeof(U2UCMessage.ZoneUserInvitedResponse))]
        private bool OnRecvZoneUserInvitedResponse(object objRequest, IActorRef sender)
        {
            var data = objRequest as U2UCMessage.ZoneUserInvitedResponse;
            if (data == null)
                return true;

            var calledTddActorRef = data.CalledTddActorRef;

            {
                var response = new MessageWrapper
                {
                    ZoneUserInvitedResponse = new ZoneUserInvitedResponse
                    {
                        ErrorCode = (int)data.ErrorCode,
                    }
                };

                TellClient(response, calledTddActorRef);
            }


            return true;
        }

        [InternalMessageHandler(typeof(U2UCMessage.ZoneUserInvitedNoti))]
        private bool OnRecvZoneUserInvitedNoti(object objRequest, IActorRef sender)
        {
            var data = objRequest as U2UCMessage.ZoneUserInvitedNoti;
            if (data == null)
                return true;

            // 받은 유저에게 보냄
            var fromCharSeq = data.FromCharSeq;
            var db = GetSystem<UserRepository>();
            if (db == null)
            {
                return true;
            }

            var fromCharater = db.FetchCharacter(fromCharSeq);
            var fromCharNickname = fromCharater.nickname != null ? fromCharater.nickname : string.Empty;

            {
                var response = new MessageWrapper
                {
                    ZoneUserInvitedNoti = new ZoneUserInvitedNoti
                    {
                        FromCharSeq = data.FromCharSeq,
                        FromCharNickname = fromCharNickname,

                        MapIndex = data.MapIndex,
                        ZoneIndex = data.ZoneIndex,
                        GameMode = (int)data.GameModeType,
                        ZoneServerInfo = data.ZoneServerInfo,

                    }
                };

                TellClient(response);
            }
            return true;
        }

        /// <summary>
        /// 다른 서버에서 온 noti
        /// </summary>        
        [InternalMessageHandler(typeof(U2UCMessage.UserNoti))]
        private bool OnRecvUserNoti(object objRequest, IActorRef sender)
        {
            var data = objRequest as U2UCMessage.UserNoti;
            if (data == null)
                return true;

            var response = data.Noti;
            TellClient(response);

            return true;

        }
    }

}
