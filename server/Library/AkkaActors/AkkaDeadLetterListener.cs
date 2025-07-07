using Akka.Actor;
using Akka.Event;
using Library.Helper;
using Library.Logger;
using Library.messages;
using log4net;
using Messages;
using System.Reflection;

namespace Library.AkkaActors;

public class AkkaDeadLetterListener : ReceiveActor
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private ulong _deadLetterCount = 0;

    public static IActorRef ActorOf(ActorSystem system)
    {
        var actorRef = system.ActorOf(Props.Create(() => new AkkaDeadLetterListener()));
        return actorRef;
    }

    public AkkaDeadLetterListener()
    {
        Receive<DeadLetter>(deadLetter =>
        {
            OnDeadLetter(deadLetter);
        });
    }

    private void OnDeadLetter(DeadLetter deadLetter)
    {
        try
        {
            if (deadLetter.Message is Session.Unicast sessionMessage)
            {
                if (sessionMessage.Message.PayloadCase == MessageWrapper.PayloadOneofCase.MoveNoti)
                {
                    // 이동은 찍지 않음
                }
                else
                {                    
                    _logger.DebugEx(() => $"DeadLetter [{++_deadLetterCount}] received: {deadLetter.Message} wrapperName:{sessionMessage.Message.PayloadCase.ToString()} json:[{PacketLogHelper.Instance.GetLogJson(sessionMessage.Message)}] from {deadLetter.Sender} to {deadLetter.Recipient}");
                }
                //if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
                //{
                //    userCordiatorActor.Tell(new S2SMessage.UserDeadLetter
                //    {
                //        Message = deadLetter.Message,
                //        Sender = deadLetter.Sender,
                //        UserRecipientRef = deadLetter.Recipient,
                //    });
                //}
            }
            else
            {
                //var settings = new JsonSerializerSettings
                //{
                //    ContractResolver = new IgnoreAkkaContractResolver(),
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore // 순환 참조 무시 설정 추가
                //};

                //var jsonStr = JsonConvert.SerializeObject(deadLetter.Message, settings);
                //_logger.Warn($"DeadLetter [{++_deadLetterCount}] received: {deadLetter.Message} json:[{jsonStr}] from {deadLetter.Sender} to {deadLetter.Recipient}");
                //_logger.Warn($"DeadLetter [{++_deadLetterCount}] received: {deadLetter.Message} from {deadLetter.Sender} to {deadLetter.Recipient}");
            }
        }
        catch (Exception ex)
        {
            _logger.Warn($"OnDeadLetter DeadLetter [{++_deadLetterCount}] received: {deadLetter.Message} from {deadLetter.Sender} to {deadLetter.Recipient}", ex);
        }
    }
}
