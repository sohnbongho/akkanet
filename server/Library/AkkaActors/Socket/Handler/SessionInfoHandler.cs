using Akka.Actor;
using Library.Helper;

namespace Library.AkkaActors.Socket.Handler;

public class SessionInfoHandler : IDisposable
{
    public bool PacketEncrypt => _packetEncrypt;
    public string RemoteAddress => _remoteAddress ?? string.Empty;
    public int DeadLetterCount => _deadLetterCount;

    private readonly string _remoteAddress = null!;

    private IActorRef? _userManagementActor = ActorRefs.Nobody;
    private IActorRef? _connectedSocket = ActorRefs.Nobody; // 연결 된 원격지 클라이언트 session 
    private bool _connected;

    // 패킷 암호화 여부
    private readonly bool _packetEncrypt = true;
    private int _deadLetterCount = 0;

    private DateTime _receivedKeepAliveTime = DateTimeHelper.Now;
    public DateTime ReceivedKeepAliveTime => _receivedKeepAliveTime;


    public SessionInfoHandler() : this(ActorRefs.Nobody, string.Empty, ActorRefs.Nobody, false)
    {
    }
    public SessionInfoHandler(IActorRef userManagementActor, string remoteAdress, IActorRef connection, bool packetEncrypt)
    {
        _userManagementActor = userManagementActor;
        _remoteAddress = remoteAdress;
        _connectedSocket = connection;
        _packetEncrypt = packetEncrypt;
        _connected = true;
    }

    public void Dispose()
    {
        _userManagementActor = null;
        _connectedSocket = null;
    }

    public void Init(IUntypedActorContext context)
    {
        _deadLetterCount = 0;
        // 액터가 완전히 시작된 후 Watch 호출
        if (_connectedSocket != null)
        {
            context.Watch(_connectedSocket);
        }
    }


    public void TellUserManagement(Object broadcast)
    {
        if(_userManagementActor != null)
        {
            _userManagementActor.Tell(broadcast);
        }
    }

    public void TellConnectedSocket(Object obj)
    {
        if (_connected == false || _connectedSocket == ActorRefs.Nobody || _connectedSocket == null)
            return;

        _connectedSocket.Tell(obj);
    }

    public void IncreaseDeadLetterCount()
    {
        _deadLetterCount++;
    }


    public bool IsTerminatedSocket(Terminated terminated)
    {
        if (terminated.ActorRef != _connectedSocket)
            return false;

        return true;
    }
    public void ClosedSocket(IUntypedActorContext context)
    {
        if (_connectedSocket != null)
        {
            context.Stop(_connectedSocket);
            context.Unwatch(_connectedSocket);
        }
        _connected = false;
    }
    public void OnClosedSocket(IUntypedActorContext context)
    {
        if (_connectedSocket != null)
        {
            context.Unwatch(_connectedSocket);
        }

        _connected = false;
    }
    public void SetReceivedKeepAliveTime(DateTime time)
    {
        _receivedKeepAliveTime = time;

    }

}
