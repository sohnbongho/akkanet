using Akka.Actor;
using Akka.IO;
using Library.AkkaActors.Socket.Handler;
using Library.Component;
using Library.Data.Enums;
using Library.ECSSystem;
using Library.Helper;
using Library.Logger;
using Library.MessageHandling;
using Library.messages;
using log4net;
using Messages;
using System.Reflection;

namespace Library.AkkaActors.Socket;

public abstract class SessionActor : UntypedActor, IComponentManager, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    protected readonly ECSEntity _entity;
    protected SessionInfoHandler? _sessionInfoHandler;

    //protected readonly SessionSendHandlerWithPool _sessionSendHandler;
    //private readonly SessionReadHandlerWithMemoryStream _sessionReadHandler;

    protected SessionSendHanlder? _sessionSendHandler;
    protected SessionReadHandler? _sessionReadHandler;
    protected bool _disposedValue;

    protected ICancelable? _cancelableKeepAlive = null!;

    protected ulong _sessionUserSeq = 0;
    protected SessionMessageHandlerManager? _sessionHandlerManager;
    protected InternalMessageHandlerManager? _internalHandlerManager;

    private class KeepAlive
    {
        public static KeepAlive Instance { get; } = new();
    }

    public class StopSessoinActor
    {
    }

    public SessionActor(bool packetEncrypt) : this(ActorRefs.Nobody, string.Empty, ActorRefs.Nobody, packetEncrypt)
    {
    }

    public SessionActor(IActorRef userManagementActor, string remoteAdress, IActorRef connection, bool packetEncrypt)
    {
        _entity = ECSEntity.Of();

        _sessionInfoHandler = new SessionInfoHandler(userManagementActor, remoteAdress, connection, packetEncrypt);
        _sessionSendHandler = new();
        _sessionReadHandler = new();
    }

    private SessionSendHanlder CreateSendHandler()
    {
        return new SessionSendHanlder();
    }

    /// <summary>
    /// Component 관리
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    public T AddComponent<T>(T component) where T : class, IECSComponent
    {
        return _entity.AddComponent(component);
    }
    public T? GetComponent<T>() where T : class, IECSComponent
    {
        return _entity.GetComponent<T>();
    }

    public void RemoveComponent<T>() where T : class, IECSComponent
    {
        _entity.RemoveComponent<T>();
    }

    /// <summary>
    /// System
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    public void AddSystem<T>(T component) where T : class, IECSSystem
    {
        _entity.AddSystem<T>(component);
    }
    public T? GetSystem<T>() where T : class, IECSSystem
    {
        return _entity.GetSystem<T>();
    }

    public void RemoveSystem<T>() where T : class, IECSSystem
    {
        _entity.RemoveSystem<T>();
    }

    /// <summary>
    /// Akka.IO관련 처리
    /// </summary>        
    protected override void PreStart()
    {
        base.PreStart();

        try
        {
            var context = Context;
            if (_sessionSendHandler == null)
                _sessionSendHandler = new();

            if (_sessionReadHandler == null)
                _sessionReadHandler = new();

            if (_sessionInfoHandler == null)
                _sessionInfoHandler = new();

            _internalHandlerManager = InternalMessageHandlerManager.Of(this);
            _sessionHandlerManager = SessionMessageHandlerManager.Of(this);

            _sessionSendHandler.Init();
            _sessionReadHandler.Init();
            _sessionInfoHandler.Init(context);

            // 연결 성공
            TellClient(new MessageWrapper
            {
                ConnectedResponse = new ConnectedResponse { }
            });

            /// 연결 유지 타이머 시작
            _cancelableKeepAlive = Context.System.Scheduler.ScheduleTellOnceCancelable(ConstInfo.SessionKeepAliveTime,  // 반복 간격                    
                        Self,                     // 메시지를 받을 액터
                        KeepAlive.Instance, // 보낼 메시지
                        Self                    // 메시지를 보내는 주체
                    );
        }
        catch (Exception ex)
        {
            _logger.Error("PreStart", ex);
        }
    }

    protected override void PostStop()
    {
        _logger.DebugEx(() => "SessionActor.PostStop");
        try
        {
            // 메모리 해제
            CleanupResources();
            Dispose();
        }
        catch (Exception ex)
        {
            _logger.Error("Error during PostStop", ex);
        }
        finally
        {
            base.PostStop();
        }
    }

    private void CleanupResources()
    {
        if (_sessionInfoHandler != null)
        {
            _sessionInfoHandler.Dispose();
            _sessionInfoHandler = null;
        }

        // 반드시 System을 먼저 Dispose하고 Component를 Dispose하자
        // 상호 참조에 이슈가 있을 수 있으므로, GC에 부담이 된다
        _entity.Dispose();

        if (_sessionSendHandler != null)
        {
            _sessionSendHandler.Dispose();
            _sessionSendHandler = null;
        }
        if (_sessionReadHandler != null)
        {
            _sessionReadHandler.Dispose();
            _sessionReadHandler = null;
        }
        if (_internalHandlerManager != null)
        {
            _internalHandlerManager.Dispose();
            _internalHandlerManager = null;
        }

        if (_sessionHandlerManager != null)
        {
            _sessionHandlerManager.Dispose();
            _sessionHandlerManager = null;
        }

        if (_cancelableKeepAlive != null)
        {
            _cancelableKeepAlive.Cancel();
            _cancelableKeepAlive = null;
        }
    }
    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case Tcp.Received received: // 메시지 받기
                {
                    if (_sessionInfoHandler != null)
                    {
                        //HandleReceivedAsync(received.Data.ToArray());
                        var data = received.Data.ToArray();
                        var remoteAddress = _sessionInfoHandler.RemoteAddress;
                        var packetEncrypt = _sessionInfoHandler.PacketEncrypt;
                        if (_sessionReadHandler != null && _sessionReadHandler.HandleReceived(data, packetEncrypt, Self, remoteAddress, _sessionUserSeq) == false)
                        {
                            _logger.Error("Tcp.Received Exception");
                            ClosedSocket(SessionClosedType.ClientClosed);
                            return;
                        }
                    }

                    break;
                }

            case Session.Unicast unicast:
                {
                    TellClient(unicast.Message);
                    break;
                }
            case Session.Ack _:
                {
                    // 패킷 전송 완료
                    SendPacket();
                    break;
                }
            case Session.Broadcast broadcast:
                {
                    if (_sessionInfoHandler != null)
                    {
                        _sessionInfoHandler.TellUserManagement(broadcast);
                    }
                    break;
                }
            case Tcp.ErrorClosed _:
                {
                    ClosedSocket(SessionClosedType.ClientClosed);
                    break;
                }
            case Tcp.PeerClosed closed:
            case Tcp.Closed _:
                {
                    ClosedSocket(SessionClosedType.ClientClosed);
                    break;
                }
            case Tcp.CommandFailed failed:
                {
                    // 패킷 전송 실패 처리
                    _logger.Error($"Tcp.CommandFailed: {failed.Cmd}");

                    // 재시도를 위해 패킷을 다시 전송
                    SendPacket();
                    break;
                }
            case Tcp.WritingResumed _:
                {
                    // 패킷 전송 완료 처리
                    //if (_sessionSendHandler.IsCompleted())
                    //{
                    //    _logger.DebugEx(() => "completed to send packet ");
                    //}
                    //else
                    //{
                    //    // 전체 패킷이 전송되지 않았을 경우 재시도
                    //    SendPacket();
                    //}
                    break;
                }
            case KeepAlive _:
                {
                    OnKeepAlive();
                    break;
                }
            case MessageWrapper messageWrapper:
                {
                    HandleReceivedPacket(messageWrapper);
                    break;
                }
            case S2SMessage.UserDeadLetter userDeadLetter:
                {
                    OnUserDeadLetter(userDeadLetter);
                    break;
                }
            case StopSessoinActor:
                {
                    OnStopActor();
                    break;
                }
            case Terminated terminated:
                {
                    if (_sessionInfoHandler != null)
                    {
                        var socketStatus = _sessionInfoHandler.IsTerminatedSocket(terminated);
                        if (socketStatus)
                        {
                            _sessionInfoHandler.OnClosedSocket(Context);
                        }
                    }
                    break;
                }
            default:
                {
                    if (_internalHandlerManager != null && true == _internalHandlerManager.HandleMessage(message, Sender))
                    {
                        return;
                    }
                    else
                    {
                        _logger.Error($"{Sender} Unhandled");
                        Unhandled(message);
                    }

                    break;
                }
        }
    }

    protected void OnKeepAlive()
    {
        _cancelableKeepAlive = null;
        var now = DateTimeHelper.Now;
        if (_sessionInfoHandler == null)
            return;

        var receivedKeepAliveTime = _sessionInfoHandler.ReceivedKeepAliveTime;
        if (now > receivedKeepAliveTime)
        {
            _logger.WarnEx(() => $"disconnect. because no keep alive. sessionUserSeq:{_sessionUserSeq}");
            // Alive 패킷 Time Over 
            //ForceCloseConnection();            
            CloseSocketOnTimeout();

            return;
        }

        _cancelableKeepAlive = Context.System.Scheduler.ScheduleTellOnceCancelable(ConstInfo.SessionKeepAliveTime,  // 반복 간격                    
            Self,                     // 메시지를 받을 액터
            KeepAlive.Instance, // 보낼 메시지
            Self                    // 메시지를 보내는 주체
        );
    }

    public virtual void CloseSocketOnTimeout()
    {
        ClosedSocket(SessionClosedType.KeepAliveTimeOut); // 강제로 유저 종료
    }

    [SessionMessageHandler(MessageWrapper.PayloadOneofCase.KeepAliveRequest)]
    public bool OnRecvKeepAliveRequest(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
    {
        if (_sessionInfoHandler == null)
            return true;

        var now = DateTimeHelper.Now;

        var receivedKeepAliveTime = now + ConstInfo.SessionKeepAliveTime;
        _sessionInfoHandler.SetReceivedKeepAliveTime(receivedKeepAliveTime);

        _logger.DebugEx(() => $"OnRecvKeepAliveRequest sessionUserSeq:{_sessionUserSeq} now:{now} checkdTime:{receivedKeepAliveTime}");

        return true;
    }

    private bool HandleReceivedPacket(MessageWrapper wrapper)
    {
        var wrapperName = string.Empty;
        try
        {
            wrapperName = wrapper.PayloadCase.ToString();
            //if (_userHandlers != null && _userHandlers.TryGetValue(wrapper.PayloadCase, out var handler))
            //{
            //    handler(wrapper, Sender, false);
            //    return true;
            //}            
            if (_sessionHandlerManager != null)
            {
                var handle = _sessionHandlerManager.HandleMessage(wrapper, Sender, false);
                return handle;
            }
            _logger.Error($"HandleReceivedPacket {wrapperName}");

        }
        catch (Exception ex)
        {
            _logger.Error($"HandleReceivedPacket {wrapperName}", ex);
        }

        return false;
    }
    /// <summary>
    /// socket이 끈김
    /// </summary>
    public virtual void ClosedSocket(SessionClosedType closedType)
    {
        //Self.Tell(new StopSessoinActor());        
        // userActor가 먼저 종료되고 나서 소켓 액터 종료
        Context.Stop(Self);
        if (_sessionInfoHandler != null)
        {
            _sessionInfoHandler.ClosedSocket(Context);
        }

    }


    private void OnStopActor()
    {
        // userActor가 먼저 종료되고 나서 소켓 액터 종료
        Context.Stop(Self);
        if (_sessionInfoHandler != null)
        {
            _sessionInfoHandler.ClosedSocket(Context);
        }
    }

    /// <summary>
    /// 현재 등록된 패킷을 보낸다.
    /// </summary>
    protected void SendPacket()
    {
        //var (succeed, packet) = _sessionSendHandler.DequeuePendingPacket();
        //if (succeed)
        //{
        //    _sessionInfoHandler.TellConnectedSocket(packet);
        //}
    }

    /// <summary>
    /// 메시지 전달
    /// </summary>        
    protected void TellClient(MessageWrapper message, IActorRef? calledTddActorRef = null)
    {
        //int threadId = Thread.CurrentThread.ManagedThreadId;
        //var json = PacketLogHelper.Instance.GetLogJson(message);
        //_logger.DebugEx(()=>$"Server->Client [{threadId}]- ip({_remoteAddress}) seq({SessionCharSeq}) type({message.PayloadCase}) data({json})");
        //_logger.DebugEx(()=>$"Server->Client [{threadId}]- ip({_remoteAddress}) seq({SessionCharSeq}) type({message.PayloadCase}) ");

        if (calledTddActorRef != null && calledTddActorRef != ActorRefs.Nobody)
        {
            // TDD 요청한 곳에 다시 보낸다.
            calledTddActorRef.Tell(message);
            return;
        }
        if (_sessionSendHandler != null && _sessionInfoHandler != null)
        {
            _sessionSendHandler.Tell(_sessionInfoHandler, message);
        }

        //_sessionSendHandler.EnqueuePendingPacket(message, _sessionInfoHandler.PacketEncrypt); // 대기 패킷에 등록
        //var (succeed, packet) = _sessionSendHandler.DequeuePendingPacket();
        //if (succeed)
        //{
        //    _sessionInfoHandler.TellConnectedSocket(packet);
        //}
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                CleanupResources();
            }

            // 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의합니다.
            // 큰 필드를 null로 설정합니다.
            _disposedValue = true;
        }
    }

    // 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
    // ~SessionActor()
    // {
    //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        Dispose(disposing: true);
        //GC.SuppressFinalize(this); // 실수로 누군가 메모리 해제를 빼먹으면 GC.SuppressFinalize(this)를 호출하지 않고 파이널라이저를 유지하는 것이 더 안전할 수 있음
    }

    /// <summary>
    /// 강제 연결 종료
    /// </summary>
    public void ForceCloseConnection()
    {
        _logger.DebugEx(() => "Forcefully closing connection.");
        if (_sessionInfoHandler != null)
        {
            _sessionInfoHandler.TellConnectedSocket(Tcp.Close.Instance);            // 
        }
    }

    /// <summary>
    /// 유저 mailbox deadLetter발생
    /// </summary>        
    private bool OnUserDeadLetter(S2SMessage.UserDeadLetter deadLetter)
    {
        if (_sessionInfoHandler == null)
            return true;

        var remoteAdress = _sessionInfoHandler.RemoteAddress;
        _sessionInfoHandler.IncreaseDeadLetterCount();
        var deadLetterCount = _sessionInfoHandler.DeadLetterCount;

        _logger.Error($"DeadLetter remoteAdress: {remoteAdress} _deadLetterCount:{deadLetterCount} received: {deadLetter.Message} from {deadLetter.Sender} ");

        if (deadLetterCount >= ConstInfo.MaxUserDeadLetterCount)
        {
            ForceCloseConnection(); // 강제 연결 종료
        }

        return true;
    }

    protected void SetSessionUserSeq(ulong sessionUserSeq)
    {
        _sessionUserSeq = sessionUserSeq;
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        // (자식 액터가 30초 동안 10번 이상 실패하면 더 이상 재시도하지 않습니다.)
        return new OneForOneStrategy(
            10, // maxNumberOfRetries 
            TimeSpan.FromSeconds(30), // duration
            x =>
            {
                // DDOS 로 비정상적인 행동을 하면 강제로 종료
                return Directive.Stop;

                ////Maybe we consider ArithmeticException to not be application critical
                ////so we just ignore the error and keep going.
                //if (x is ArithmeticException) return Directive.Resume;

                ////Error that we cannot recover from, stop the failing actor
                //else if (x is NotSupportedException) return Directive.Stop;

                ////In all other cases, just restart the failing actor
                //else return Directive.Restart;
            });
    }
}
