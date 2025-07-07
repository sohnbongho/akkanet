using Akka.Actor;
using log4net;
using System.Reflection;
using CommunityServer.Helper;
using Library.Helper;
using NatsMessages;
using static Library.messages.S2SMessage;
using Google.Protobuf;
using Confluent.Kafka;
using Library.AkkaActors;
using Library.Logger;

namespace CommunityServer.MessageQueue
{
    public class KafkaProducerActor : UntypedActor
    {
        private sealed class ReconnectTimer
        {
            public static ReconnectTimer Instance { get; } = new();

        }

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool>> _userHandlers;                
        
        private IProducer<Null, byte[]>?     _producer = null;
        private List<NatsPubliish> _publiishs = new List<NatsPubliish>();
        private ICancelable? _cancelable = null!;

        public static IActorRef ActorOf(IUntypedActorContext context)
        {
            var prop = Props.Create(() => new KafkaProducerActor());
            return context.ActorOf(prop, ActorPaths.KafkaProducer.Name);
        }
        public KafkaProducerActor()
        {
            _userHandlers = new Dictionary<NatsMessageWrapper.PayloadOneofCase, Func<NatsMessageWrapper, IActorRef, bool>>
            {
                //{NatsMessageWrapper.PayloadOneofCase.ConnectedResponse, (data, sender) => OnConnectedResponse(data.ConnectedResponse, sender)},                
            };

        }
        private void Init()
        {   
            if (_producer != null)
            {
                _producer?.Dispose();
                _producer = null;
            }

            var serverId = ConfigHelper.Instance.ServerId;
            
            var allTopicName = ConstInfo.MqBroadcastTopic;
            var topicName = $"{ConstInfo.MqServerSubject}{serverId}";
            
            var bootstrapServers = ConfigHelper.Instance.KafkaConnectString; // Kafka 서버 주소            
            
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = bootstrapServers,
                    SecurityProtocol = SecurityProtocol.Plaintext,
                    SaslMechanism = SaslMechanism.Plain,
                    // 기본으로 설치하면 kafka_server_jaas.conf 없으므로 누구나 접속 가능
                    //SaslUsername = "$ConnectionString",
                    //SaslPassword = "<Your Event Hubs namespace connection string>"
                    SaslUsername = "$ConnectionString",
                    SaslPassword = bootstrapServers,
                };

                _producer = new ProducerBuilder<Null, byte[]>(config).Build();
            }
        }
                

        protected override void PreStart()
        {
            base.PreStart();

            ActorRefsHelper.Instance.Actors[ActorPaths.KafkaProducer.Path] = Self;

            Init();
        }
        protected override void PostStop()
        {   
            _producer?.Dispose();
            _producer = null;

            _cancelable?.Cancel();
            _cancelable = null;

            base.PostStop();
        }
        protected override void OnReceive(object message)
        {
            if(message is NatsPubliish publiish)
            {
                var ret = ProduceAsync(publiish);
            }
            else if (message is ReconnectTimer)
            {
                _cancelable = null; 
                AttemptReconnect();
            }
        }
        private async Task<bool> ProduceAsync(NatsPubliish publiish)
        {
            var bootstrapServers = ConfigHelper.Instance.KafkaConnectString; // Kafka 서버 주소
            var serverId = ConfigHelper.Instance.ServerId;

            var topicName = $"{ConstInfo.MqServerSubject}{serverId}";
            if(publiish.NatsMessageWrapper.ToServerId <= 0)
            {
                // 0이면 전체에게 알림
                topicName = ConstInfo.MqBroadcastTopic;
            }

            // 메시지 보내기            
            try
            {
                if(_producer != null)
                {
                    var produceMessage = new Message<Null, byte[]>
                    {
                        Value = publiish.NatsMessageWrapper.ToByteArray(),
                    };
                    var result = await _producer.ProduceAsync(topicName, produceMessage);
                    _logger.DebugEx(()=>$"Message sent to partition {result.Partition}, offset {result.Offset}");
                }                
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.Error($"Failed to deliver message: {e.Message} [{e.Error.Code}]");
                if (_producer != null)
                {
                    _producer?.Dispose();
                    _producer = null;
                }

                // 10초 대기 후 재 연결 시도
                if (_cancelable != null)
                    _cancelable?.Cancel();

                _cancelable = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(10), Self, 
                    ReconnectTimer.Instance, Self);
                return false;
            }
            return true;
        }
        private void AttemptReconnect()
        {
            // 연결 재시도 로직
            // 예: 일정 시간 대기 후 Init 메서드 호출
            Init();            
        }
    }
}
