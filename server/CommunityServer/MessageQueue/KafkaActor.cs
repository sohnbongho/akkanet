using Akka.Actor;
using log4net;
using System.Reflection;
using Library.AkkaActors;

namespace CommunityServer.MessageQueue
{
    public class KafkaActor : UntypedActor
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);        

        public static IActorRef ActorOf(ActorSystem actorSystem)
        {
            var consoleReaderProps = Props.Create(() => new KafkaActor());
            return actorSystem.ActorOf(consoleReaderProps, ActorPaths.Kafka.Name);
        }
        public KafkaActor()
        {            

        }
        
        protected override void PreStart()
        {
            base.PreStart();

            var producer = KafkaProducerActor.ActorOf(Context);
            var consumer = KafkaConsumerActor.ActorOf(Context);
            
        }
        protected override void PostStop()
        {   

            base.PostStop();
        }
        protected override void OnReceive(object message)
        {

        }

    }
}
