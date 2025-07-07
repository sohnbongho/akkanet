using Akka.Actor;
using Google.Protobuf;
using Library.Helper;
using Library.Logger;
using Library.messages;
using log4net;
using Messages;
using NatsMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using static Library.messages.S2SMessage;

namespace Library.AkkaActors.MessageQueue;

public class RabbitMQActor : UntypedActor
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool>> _userHandlers;
    private Dictionary<Type, Func<object, IActorRef, bool>> _innerMsgHandlers;

    private IConnection _connection = null!;
    private IModel _channel = null!;
    private static readonly string _exchangeName = "direct_exchange";

    private readonly ConnectionFactory _factory;
    private readonly object _syncRoot = new();
    private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(10);
    private readonly int _serverId = 0;

    public static IActorRef ActorOf(ActorSystem actorSystem, int serverId)
    {
        var props = Props.Create(() => new RabbitMQActor(serverId));
        return actorSystem.ActorOf(props, ActorPaths.RabbitMQActor.Name);
    }

    public RabbitMQActor(int serverId)
    {
        _serverId = serverId;
        _userHandlers = new Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool>>
        {
            { NatsMessageWrapper.PayloadOneofCase.ConnectedResponse, (data, sender) => OnConnectedResponse(data.ConnectedResponse, sender) },
            { NatsMessageWrapper.PayloadOneofCase.RoomUserInvitedRequest, (data, sender) => OnRoomUserInvitedRequest(data.RoomUserInvitedRequest, sender) },
            { NatsMessageWrapper.PayloadOneofCase.ZoneUserInvitedRequest, (data, sender) => OnZoneUserInvitedRequest(data.RoomUserInvitedRequest, sender) },
        };

        _innerMsgHandlers = new Dictionary<Type, Func<object, IActorRef, bool>>();

        _factory = new ConnectionFactory()
        {
            //Uri = new Uri("amqp://dance:crazy1234@52.141.29.196:5672/"),
            HostName = "52.141.29.196",
            Port = 5672,
            UserName = "test",  // 사용자 이름을 여기에 입력하십시오
            Password = "test",  // 비밀번호를 여기에 입력하십시오
            AutomaticRecoveryEnabled = true,
            //NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            //RequestedConnectionTimeout = ConnectionFactory.DefaultConnectionTimeout,  // 연결 시간 초과 설정 
            //RequestedChannelMax = 100,  // 최대 채널 수
            //RequestedFrameMax = 65536,  // 최대 프레임 크기 (바이트)            
            //RequestedHeartbeat = ConnectionFactory.DefaultHeartbeat,     // 하트비트 인터벌             
        };
    }

    protected override void PreStart()
    {
        base.PreStart();
        ActorRefsHelper.Instance.Actors[ActorPaths.RabbitMQActor.Path] = Self;
        Connect();
    }

    private void Connect()
    {
        var self = Self;
        try
        {
            lock (_syncRoot)
            {
                _connection = _factory.CreateConnection();
                _connection.ConnectionShutdown += OnConnectionShutdown;

                _channel = _connection.CreateModel();

                var serverId = _serverId;

                // 교환기 선언
                _channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");

                // 큐 선언
                var serverQueueName = $"{ConstInfo.MqServerSubject}{serverId}";
                _channel.QueueDeclare(queue: serverQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: serverQueueName, exchange: _exchangeName, routingKey: serverQueueName);

                // 전체 메시지를 받을수 있는 큐 선언
                var allServerQueueName = ConstInfo.MqBroadcastTopic;
                _channel.QueueDeclare(queue: allServerQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: allServerQueueName, exchange: _exchangeName, routingKey: allServerQueueName);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = NatsMessageWrapper.Parser.ParseFrom(body);
                        var json = PacketLogHelper.Instance.GetNatsMessageToJson(message);

                        _logger.DebugEx(() => $"Received: {json}");
                        self.Tell(message);

                        // 메시지 확인 (삭제)
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"RabbitMQActor MessageHandler exception: {ex.Message}");
                    }

                };

                _channel.BasicConsume(queue: serverQueueName, autoAck: false, consumer: consumer);
                _channel.BasicConsume(queue: allServerQueueName, autoAck: false, consumer: consumer);
            }
            _logger.InfoEx(() => "Connected Rabbit MQ");
        }
        catch (Exception ex)
        {
            _logger.Error($"failed to conntect Rabbit MQ", ex);
        }

    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.Error($"Connection lost: {e.ReplyText}. Attempting to reconnect...");
        Reconnect();
    }

    private void Reconnect()
    {
        lock (_syncRoot)
        {
            var maxTryConnectCount = 1000; // 최대 시도 횟수 설정
            var currentDelay = _reconnectDelay; // 초기 재시도 간격

            for (var i = 0; i < maxTryConnectCount; ++i)
            {
                try
                {
                    if (_connection != null && _connection.IsOpen)
                        break;

                    _logger.InfoEx(() => "Attempting to reconnect to RabbitMQ...");
                    Connect();
                    _logger.InfoEx(() => "Reconnected to RabbitMQ.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Reconnect attempt failed: {ex.Message}. Retrying in {currentDelay.TotalSeconds} seconds.");
                    Thread.Sleep(currentDelay);

                    // 지수 백오프를 적용하여 재시도 간격을 증가시킵니다.
                    currentDelay += TimeSpan.FromSeconds(10); // 간격을 점진적으로 증가
                }
            }
        }
    }


    protected override void PostStop()
    {
        try
        {
            if (_channel != null)
            {
                _channel.Close();
            }
            if (_connection != null)
            {
                _connection.Close();
            }
        }
        finally
        {
            base.PostStop();
        }
    }

    protected override void OnReceive(object message)
    {
        if (message is NatsPubliish natsPubliish)
        {
            var body = natsPubliish.NatsMessageWrapper.ToByteArray();
            var toServerId = natsPubliish.NatsMessageWrapper.ToServerId;

            var routingKey = toServerId > 0
                ? $"{ConstInfo.MqServerSubject}{toServerId}" : ConstInfo.MqBroadcastTopic;

            _channel.BasicPublish(exchange: _exchangeName, routingKey: routingKey, basicProperties: null, body: body);
        }
        else if (message is NatsMessageWrapper wrapper)
        {
            if (_userHandlers.TryGetValue(wrapper.PayloadCase, out var handler))
            {
                handler(wrapper, Sender);
                return;
            }
            DefaultUserHandler(wrapper, Sender);
        }
    }

    private bool OnConnectedResponse(ConnectedResponse wrapper, IActorRef sessionRef)
    {
        return true;
    }

    private bool OnRoomUserInvitedRequest(RoomUserInvitedRequest request, IActorRef sender)
    {
        if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
        {
            userCordiatorActor.Tell(new U2UCMessage.MessageFromOtherServer
            {
                NatsMessageWrapper = new NatsMessageWrapper
                {
                    RoomUserInvitedRequest = request,
                },
            });
        }
        return true;
    }

    private bool OnZoneUserInvitedRequest(RoomUserInvitedRequest request, IActorRef sender)
    {
        if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
        {
            userCordiatorActor.Tell(new U2UCMessage.MessageFromOtherServer
            {
                NatsMessageWrapper = new NatsMessageWrapper
                {
                    RoomUserInvitedRequest = request,
                },
            });
        }
        return true;
    }

    private bool DefaultUserHandler(NatsMessageWrapper wrapper, IActorRef sessionRef)
    {
        if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
        {
            userCordiatorActor.Tell(new U2UCMessage.MessageFromOtherServer
            {
                NatsMessageWrapper = wrapper,
            });
        }
        return true;
    }
}
