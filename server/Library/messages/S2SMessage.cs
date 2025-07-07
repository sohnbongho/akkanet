using Akka.Actor;
using Akka.IO;
using Library.DTO;
using Messages;
using NatsMessages;
using System.Net.WebSockets;

namespace Library.messages
{
    /// <summary>
    /// Session관련 메시지
    /// </summary>
    public class Session
    {
        public class RegisteredRequest
        {
            public string RemoteAdress { get; set; } = null!;
            public IActorRef Sender { get; set; } = ActorRefs.Nobody;
        }
        public class ClosedRequest
        {
            public string RemoteAdress { get; set; } = null!;

        }

        public class SetUserCordiatorActor
        {
            public IActorRef UserCordiatorActor { get; set; } = ActorRefs.Nobody;

        }

        /// <summary>
        /// 대상들 에게 보내기
        /// </summary>
        public class Broadcast
        {
            public MessageWrapper Message { get; set; } = null!;
        }

        /// <summary>
        /// 단일 객체에게 보내기
        /// </summary>
        public class Unicast
        {
            public MessageWrapper Message { get; set; } = null!;
        }

        // 성공 적으로 메시지 보내기 성공
        public class Ack : Tcp.Event
        {
            public static Ack Instance { get; } = new();
        }
        /// <summary>
        ///  메시지 보내기
        /// </summary>
        public class SendMessageRequest
        {
            public byte[] Buffer { get; set; } = Array.Empty<byte>();
            public WebSocketMessageType WebSocketMessageType { get; set; }
        }
        public class SendMessageResponse
        {
            public bool Closed { get; set; } // 소켓을 닫아라 요청

        }
    }

    /// <summary>
    /// Server Manager <-> Servers(Login, Lobby, Zone)
    /// </summary>
    public class S2SMessage
    {
        /// <summary>
        /// 서버 생존 유무에 대해
        /// </summary>

        public class KeepAliveRequest
        {
            static public KeepAliveRequest Instance = new KeepAliveRequest();
        }

        public class KeepAliveResponse
        {
            static public KeepAliveResponse Instance = new KeepAliveResponse();
        }

        public class NatsPubliish
        {
            public NatsMessageWrapper NatsMessageWrapper { get; set; } = null!;
        }

        /// <summary>
        /// 외부에 전달
        /// </summary>
        public class RemotePubliish
        {
            public NatsMessageWrapper NatsMessageWrapper { get; set; } = null!;
        }
        public class RemoteSub
        {
            public NatsMessageWrapper NatsMessageWrapper { get; set; } = null!;
        }

        /// <summary>
        /// 메시지를 못받은 유저 Actor
        /// </summary>
        public class UserDeadLetter
        {
            public object Message { get; set; } = null!;
            public IActorRef Sender { get; set; } = ActorRefs.Nobody;
            public IActorRef UserRecipientRef { get; set; } = ActorRefs.Nobody;
        }
    }

    /// <summary>
    /// User,  UserCordiator
    /// </summary>
    public class U2UCMessage
    {
        /// <summary>
        /// 커뮤니티 서버 입장
        /// </summary>
        public class EnterCommunityRequest
        {
            public ulong UserSeq { get; set; }
            public ulong CharSeq { get; set; }
            public IActorRef CharActorRef { get; set; } = ActorRefs.Nobody;
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }
        public class EnterCommunityResponse
        {
            public ErrorCode ErrorCode { get; set; }
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }

        /// <summary>
        /// 룸 게임 중에 유저 초대
        /// </summary>
        public class RoomUserInvitedRequest
        {
            public ulong RoomSeq { get; set; }
            public RoomServerInfo RoomServerInfo { get; set; } = null!;
            public ulong FromCharSeq { get; set; }
            public ulong TargetCharSeq { get; set; }
            public GameModeType GameModeType { get; set; }

            public IActorRef CharActorRef { get; set; } = ActorRefs.Nobody;
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }
        public class RoomUserInvitedResponse
        {
            public ErrorCode ErrorCode { get; set; }
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }

        public class RoomUserInvitedNoti
        {
            public ulong RoomSeq { get; set; }
            public RoomServerInfo RoomServerInfo { get; set; } = null!;
            public ulong FromCharSeq { get; set; }
            public GameModeType GameModeType { get; set; }
        }

        /// <summary>
        /// 다른 서버에서 온 메시지
        /// </summary>
        public class MessageFromOtherServer
        {
            public NatsMessageWrapper NatsMessageWrapper { get; set; } = null!;
        }

        /// <summary>
        /// 존 유저 초대
        /// </summary>
        public class ZoneUserInvitedRequest
        {
            public ZoneServerInfo ZoneServerInfo { get; set; } = null!;
            public ulong FromCharSeq { get; set; }
            public ulong TargetCharSeq { get; set; }

            public int MapIndex { get; set; }
            public int ZoneIndex { get; set; }
            public GameModeType GameModeType { get; set; }
            public IActorRef CharActorRef { get; set; } = ActorRefs.Nobody;
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }

        public class ZoneUserInvitedResponse
        {
            public ErrorCode ErrorCode { get; set; }
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }
        public class ZoneUserInvitedNoti
        {
            public ulong FromCharSeq { get; set; }
            public ZoneServerInfo ZoneServerInfo { get; set; } = null!;

            public int MapIndex { get; set; }
            public int ZoneIndex { get; set; }
            public GameModeType GameModeType { get; set; } = GameModeType.None;
        }

        /// <summary>
        /// 다른 서버에서 온 유저 알림/유저에게 바로 알림
        /// </summary>
        public class UserNoti
        {
            public MessageWrapper Noti { get; set; } = null!;
        }
        /// <summary>
        /// 채팅 서버 입장
        /// </summary>
        public class EnterChatRoomServerRequest
        {
            public string RoomId { get; set; } = string.Empty;
            public ulong UserSeq { get; set; }
            public ulong CharSeq { get; set; }
            public IActorRef CharActorRef { get; set; } = ActorRefs.Nobody;
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }
        public class EnterChatRoomServerResponse
        {
            public ErrorCode ErrorCode { get; set; }

            public string RoomId { get; set; } = string.Empty;
            public ulong UserSeq { get; set; }
            public ulong CharSeq { get; set; }
            public IActorRef CharActorRef { get; set; } = ActorRefs.Nobody;
            public IActorRef CalledTddActorRef { get; set; } = ActorRefs.Nobody;
        }
        
        /// <summary>
        /// 유저 공지
        /// </summary>
        public class UserNoticeNoti
        {
            public string Title { get; set; } = string.Empty;
            public string Content{ get; set; } = string.Empty;            
        }
    }
}
