using Akka.Actor;

namespace Library.Helper;

public static class ScheduleFactory
{
    public static ICancelable Start(IUntypedActorContext context, TimeSpan delay, IActorRef self, object message)
    {
        return context.System.Scheduler.ScheduleTellOnceCancelable(delay,  // 반복 간격
                    self,                     // 메시지를 받을 액터
                    message, // 보낼 메시지
                    self                    // 메시지를 보내는 주체
            );
    }
}
