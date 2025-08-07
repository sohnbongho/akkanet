using Akka.Actor;

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
        foreach (var userRef in _userRefs)
        {
            for (int i = 0; i < 10; i++)
            {
                userRef.Tell(new UserSessionActor.ThreadCheck());                
            }            
        }
    }
    private void OnTimer(UserManagerActor.TickTimer msg)
    {
        Console.WriteLine("---------------------");
        Console.WriteLine("");

        RequestUser();
    }
}
