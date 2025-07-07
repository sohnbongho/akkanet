using Akka.Actor;
using Google.Protobuf;
using Library.Helper;
using Library.Logger;
using Library.messages;
using log4net;
using Messages;
using NATS.Client;
using NatsMessages;
using System.Reflection;
using static Library.messages.S2SMessage;

namespace Library.AkkaActors.MessageQueue;

public class NatsMQActor : UntypedActor
{
    private class GameConnectTimer
    {
        public static GameConnectTimer Instance { get; } = new();
    }


    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private Dictionary<System.Type, Func<object, IActorRef, bool>> _innerMsgHandlers = new();

    private IConnection _connection = null!;
    private IAsyncSubscription _allSubscription = null!;
    private IAsyncSubscription _serverSubscription = null!;

    private string _connectionString = string.Empty;
    private int _serverId = -1;

    private int _connectCount = 0;
    private int _maxConnectCount = 1000;

    private ICancelable? _cancelable = null!;

    public static IActorRef ActorOf(ActorSystem actorSystem, string natsConnecString, int serverId)
    {
        var consoleReaderProps = Props.Create(() => new NatsMQActor(natsConnecString, serverId));
        return actorSystem.ActorOf(consoleReaderProps, ActorPaths.Nats.Name);
    }
    public NatsMQActor(string natsConnecString, int serverId)
    {
        _connectionString = natsConnecString;
        _serverId = serverId;
        
        var innerMsgHandlers = new Dictionary<System.Type, Func<object, IActorRef, bool>>
        {
            {typeof(GameConnectTimer), (data, sender) => OnGameConnectTimer() },
        };

        foreach (var innerHandler in innerMsgHandlers)
        {
            _innerMsgHandlers[innerHandler.Key] = innerHandler.Value;
        }
    }

    protected override void PreStart()
    {
        base.PreStart();

        ActorRefsHelper.Instance.Actors[ActorPaths.Nats.Path] = Self;

        Connect();
    }
    private void Connect()
    {
        ++_connectCount;
        if (_connectCount > _maxConnectCount)
        {
            // 최대 접속 시도 횟수 초과
            _logger.Error($"failed to reconnect");
            return;
        }

        try
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = _connectionString;
            options.ReconnectWait = 5000;
            options.MaxReconnect = 10000;

            // 연결 끊김 이벤트 처리
            options.DisconnectedEventHandler += (sender, args) =>
            {
                _logger.Error($"Connection lost. Attempting to reconnect...:{args.Conn.ConnectedUrl}");
                // 연결 끊김 시 처리할 로직 (재연결 시도는 자동으로 수행됨)
            };
            options.ClosedEventHandler += (sender, args) =>
            {
                _logger.InfoEx(() => $"Connection Closed: {args.Conn.ConnectedUrl}");
            };

            // 재연결 성공 이벤트 처리
            options.ReconnectedEventHandler += (sender, args) =>
            {
                _logger.InfoEx(() => $"Reconnected to NATS server.{args.Conn.ConnectedUrl}");
            };

            _connection = new ConnectionFactory().CreateConnection(options);

            var self = Self;
            // 전체 서버
            {
                string subject = ConstInfo.MqBroadcastTopic;
                _allSubscription = _connection.SubscribeAsync(subject);

                _allSubscription.MessageHandler += (sender, e) =>
                {
                    try
                    {
                        var receivedMessage = e.Message.Data;
                        var s2sWrapper = NatsMessageWrapper.Parser.ParseFrom(receivedMessage);
                        var json = PacketLogHelper.Instance.GetNatsMessageToJson(s2sWrapper);

                        _logger.DebugEx(() => $"Recv All From({s2sWrapper.FromServerId})->To({s2sWrapper.ToServerId}) data:{json}");
                        self.Tell(s2sWrapper);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"NatsMQActor MessageHandler exception: {ex.Message}");
                    }
                };

                _allSubscription.Start();
            }

            // 특정 서버
            if (_serverId > 0)
            {
                var serverId = _serverId;
                string subject = $"{ConstInfo.MqServerSubject}{serverId}";
                _serverSubscription = _connection.SubscribeAsync(subject);

                _serverSubscription.MessageHandler += (sender, e) =>
                {
                    try
                    {
                        var receivedMessage = e.Message.Data;
                        var s2sWrapper = NatsMessageWrapper.Parser.ParseFrom(receivedMessage);
                        var json = PacketLogHelper.Instance.GetNatsMessageToJson(s2sWrapper);

                        _logger.DebugEx(() => $"Recv From({s2sWrapper.FromServerId})->To({s2sWrapper.ToServerId}) data:{json}");
                        self.Tell(s2sWrapper);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"NatsMQActor MessageHandler exception: {ex.Message}");
                    }
                };

                _serverSubscription.Start();
            }
            _logger.InfoEx(() => $"Connected NatsMq.");
        }
        catch (Exception ex)
        {
            // 5초후 재시작
            _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(5),
                Self,
                GameConnectTimer.Instance,
                Self);

            _logger.Error($"failed to connect NatsMq connectionString:{_connectionString}", ex);
        }
    }

    /// <summary>
    /// 재접속 시도
    /// </summary>
    /// <returns></returns>
    private bool OnGameConnectTimer()
    {
        _logger.WarnEx(() => $"Try connect NatsMq connectionString:{_connectionString} Now:{DateTimeHelper.Now} ");

        _cancelable = null;
        Connect();
        return true;
    }

    protected override void PostStop()
    {
        try
        {
            _allSubscription.Unsubscribe();
            _serverSubscription.Unsubscribe();
            if (_connection != null)
            {
                _connection.Close();
            }
            
            _innerMsgHandlers.Clear();

            if (_cancelable != null)
            {
                _cancelable.Cancel();
                _cancelable = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("failed to PostStip", ex);
        }
        finally
        {
            base.PostStop();
        }
    }
    protected override void OnReceive(object message)
    {
        try
        {
            if (message is NatsPubliish natsPubliish)
            {
                SendNatsPubliish(natsPubliish);
                
            }
            else if (message is NatsMessageWrapper wrapper)
            {
                OnRecvNatsMessageWrapper(wrapper);
                
            }
            else
            {
                if (true == _innerMsgHandlers.TryGetValue(message.GetType(), out var handler))
                {
                    handler(message, Sender);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("fail to OnReceive", ex);
        }
    }
    private void SendNatsPubliish(NatsPubliish natsPubliish)
    {
        var payload = natsPubliish.NatsMessageWrapper.PayloadCase;
        try
        {
            _logger.DebugEx(() => $"SendNatsPubliish payloadCase:{payload} json:{PacketLogHelper.Instance.GetNatsMessageToJson(natsPubliish.NatsMessageWrapper)}");

            // 메시지 보내기
            var subject = ConstInfo.MqBroadcastTopic;
            var toServerId = natsPubliish.NatsMessageWrapper.ToServerId;
            if (toServerId > 0)
            {
                subject = $"{ConstInfo.MqServerSubject}{toServerId}";
            }
            var publishByteArray = natsPubliish.NatsMessageWrapper.ToByteArray();
            if (_connection != null)
            {
                _connection.Publish(subject, publishByteArray);
            }
        }
        catch(Exception ex)
        {
            _logger.Error($"SendNatsPubliish payload :{payload.ToString()}", ex);
        }
    }
    private void OnRecvNatsMessageWrapper(NatsMessageWrapper wrapper)
    {
        var payload = wrapper.PayloadCase;
        try
        {            
            _logger.DebugEx(() => $"OnRecvNatsMessageWrapper payloadCase:{wrapper.PayloadCase} json:{PacketLogHelper.Instance.GetNatsMessageToJson(wrapper)}");
            if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
            {
                userCordiatorActor.Tell(new U2UCMessage.MessageFromOtherServer
                {
                    NatsMessageWrapper = wrapper.Clone(),
                });
            }
            else
            {
                _logger.Error($"not found UserCoirdator Actor");
            }
        }
        catch(Exception ex)
        {
            _logger.Error($"OnRecvNatsMessageWrapper payload:{payload}", ex);
        }
    }

}

