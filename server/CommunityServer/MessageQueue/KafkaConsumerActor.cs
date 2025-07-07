using Akka.Actor;
using log4net;
using System.Reflection;
using CommunityServer.Helper;
using Library.Helper;
using Messages;
using NatsMessages;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Library.AkkaActors;
using Library.Logger;

namespace CommunityServer.MessageQueue
{
    public class KafkaConsumerActor : UntypedActor
    {      
        private sealed class ConsumerTimer
        {
            public static ConsumerTimer Instance { get; } = new();

        }
        private sealed class ReconnectTimer
        {
            public static ReconnectTimer Instance { get; } = new();

        }

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool>> _userHandlers;                
                
        private IConsumer<Ignore, byte[]>?   _consumer = null;
        private CancellationTokenSource _cancelToken; // 취소 토큰 소스 추가

        private ICancelable? _cancelable = null!;

        public static IActorRef ActorOf(IUntypedActorContext context)
        {
            var consoleReaderProps = Props.Create(() => new KafkaConsumerActor());
            return context.ActorOf(consoleReaderProps, ActorPaths.KafkaConsumer.Name);
        }
        public KafkaConsumerActor()
        {
            _userHandlers = new Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool>>
            {
                {NatsMessageWrapper.PayloadOneofCase.ConnectedResponse, (data, sender) => OnConnectedResponse(data.ConnectedResponse, sender)},                
            };
            _cancelToken = new CancellationTokenSource(); // 취소 토큰 소스 초기화

        }
        private void Init()
        {            
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
            

            var serverId = ConfigHelper.Instance.ServerId;
            
            var allTopicName = ConstInfo.MqBroadcastTopic;
            var topicName = $"{ConstInfo.MqServerSubject}{serverId}";           
            
            var bootstrapServers = ConfigHelper.Instance.KafkaConnectString; // Kafka 서버 주소
            var consumerGroupId = ConstInfo.KafkaConsumerGroupId;

            CreateTopic(allTopicName); // Server Topic 생성
            CreateTopic(topicName); // Server Topic 생성


            // 서버 개별 topic
            {
                var conf = new ConsumerConfig
                {
                    GroupId = consumerGroupId,
                    BootstrapServers = bootstrapServers,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                _consumer = new ConsumerBuilder<Ignore, byte[]>(conf).SetValueDeserializer(Deserializers.ByteArray).Build();
                _consumer.Subscribe(new string[] { allTopicName, topicName });               
            }
            
        }
        private void CreateTopic(string topicName)
        {
            var bootstrapServers = ConfigHelper.Instance.KafkaConnectString; // Kafka 서버 주소            

            // 생성할 토픽 이름
            var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

            using (var adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    // 토픽 정보를 요청합니다.                        
                    // new List<string> { topicName }
                    var topicInfo = adminClient.DescribeTopicsAsync(TopicCollection.OfTopicNames(new string[] { topicName })).Result;

                    // 토픽이 존재한다면 여기에 도달합니다.                        
                    _logger.DebugEx(()=>$"Topic {topicName} exists.");

                }
                catch (Exception)
                {
                    try
                    {
                        adminClient.CreateTopicsAsync(new List<TopicSpecification> {
                                new TopicSpecification { Name = topicName, NumPartitions = 1, ReplicationFactor = 1 }
                                }).GetAwaiter().GetResult();

                        _logger.DebugEx(()=>$"Topic {topicName} created successfully. ");
                    }
                    catch (CreateTopicsException ce)
                    {
                        _logger.Error($"An error occured creating topic {topicName}: {ce.Results[0].Error.Reason}");
                    }
                }

            }
        }

        protected override void PreStart()
        {
            base.PreStart();

            Init();

            // 100ms마다 타이머
            _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(100), Self, 
                ConsumerTimer.Instance, Self);
        }
        protected override void PostStop()
        {
            _cancelToken.Cancel(); // 취소 요청
            _consumer?.Dispose();
            _consumer = null;

            _cancelable?.Cancel();
            _cancelable = null;

            base.PostStop();
        }
        protected override void OnReceive(object message)
        {
            if(message is ConsumerTimer)
            {
                _cancelable = null;

                ConsumeMessages();

                // 100ms마다 타이머
                if (_cancelable != null)
                    _cancelable?.Cancel();

                _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(100), 
                    Self, ConsumerTimer.Instance, Self);
            }            
            else if(message is ReconnectTimer)
            {
                Init();
            }
        }

        private void ConsumeMessages()
        {
            try
            {
                if (_consumer == null)
                    return;

                var cr = _consumer.Consume(_cancelToken.Token); // 취소 토큰 사용                    

                if (cr != null && cr.Message != null && cr.Message.Value != null)
                {
                    byte[] receivedMessage = cr.Message.Value;
                    var s2sWrapper = NatsMessageWrapper.Parser.ParseFrom(receivedMessage);
                    var json = PacketLogHelper.Instance.GetNatsMessageToJson(s2sWrapper);

                    _logger.DebugEx(()=>$"From({s2sWrapper.FromServerId})->To({s2sWrapper.ToServerId}) data:{json}");
                    // 받은 메시지
                    OnRecv(s2sWrapper);
                }

            }
            catch (OperationCanceledException)
            {
                // Consume 호출이 취소됐을 때 처리
                _logger.DebugEx(()=>"Kafka consumer consumption cancelled.");
            }
            catch (ConsumeException ce)
            {                
                _logger.Error($"KafkaConsumerActor Error occured: {ce.Error.Reason}");

                // 연결 재시도 로직
                // 예: 일정 시간 대기 후 Init 메서드 호출
                if (_cancelable != null)
                    _cancelable?.Cancel();

                _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(10), Self, 
                    ReconnectTimer.Instance, Self);
            }

            catch (Exception e)
            {
                _logger.Error($"KafkaConsumerActor Error occured: {e.Message}");
            }
        }        

        /// <summary>
        /// 전체 받은 메시지
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        private bool OnRecv(NatsMessageWrapper wrapper)
        {
            if (_userHandlers.TryGetValue(wrapper.PayloadCase, out var handler))
            {
                handler(wrapper, Sender);
                return false;
            }
            return true;
        }

        private bool OnConnectedResponse(ConnectedResponse wrapper, IActorRef sessionRef)
        {
            return true;
        }


    }
}
