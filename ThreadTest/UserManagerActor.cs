using Akka.Actor;
using Akka.IO;

namespace ThreadTest;

public class UserManagerActor : ReceiveActor
{
    private int _maxUserCount = 10;
    private List<IActorRef> _userRefs = new();

    private class TickTimer
    {

    }
    public UserManagerActor() : base()
    {
        Receive<UserManagerActor.TickTimer>(msg => OnTimer(msg));

    }
    protected override void PreStart()
    {
        base.PreStart();

        for (int i = 0; i < _maxUserCount; ++i) 
        {
            var sessionProp = Props.Create(() => new UserSessionActor(i));
            var userRef = Context.ActorOf(sessionProp);
            _userRefs.Add(userRef);
        }
        RequestUser();

        var self = Self;

        Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10),
            self,
            new TickTimer(),
            self);

    }
    private void RequestUser()
    {
        for (int i = 0; i < _maxUserCount; ++i)
        {
            var sessionProp = Props.Create(() => new UserSessionActor(i));
            var userRef = Context.ActorOf(sessionProp);
            _userRefs.Add(userRef);
            userRef.Tell(new UserSessionActor.ThreadCheck());
        }
        

    }
    private void OnTimer(UserManagerActor.TickTimer msg)
    {
        Console.WriteLine("---------------------");
        Console.WriteLine("");

        RequestUser();
    }
}
