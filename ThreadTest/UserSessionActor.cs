using Akka.Actor;

namespace ThreadTest;

public partial class UserSessionActor : ReceiveActor
{
    public class ThreadCheck
    {

    }
    private readonly int _id;
    public UserSessionActor(int id) : base()
    {
        _id = id;

        ReceiveAsync<UserSessionActor.ThreadCheck>(async msg => await OnThreadCheck(msg));
    }
    public async Task<bool> OnThreadCheck(UserSessionActor.ThreadCheck msg)
    {
        //start id:5 ThreadId: 20
        //start id:6 ThreadId: 23
        //start id:4 ThreadId: 21
        //start id:7 ThreadId: 22
        //start id:1 ThreadId: 22
        //start id:0 ThreadId: 21
        //start id:8 ThreadId: 20
        //start id:9 ThreadId: 23
        //start id:2 ThreadId: 20
        //start id:3 ThreadId: 23
        //end id:1 ThreadId: 21
        //end id:0 ThreadId: 23
        //end id:9 ThreadId: 20
        //end id:6 ThreadId: 22
        //end id:3 ThreadId: 21
        //end id:5 ThreadId: 23
        //end id:7 ThreadId: 21
        //end id:8 ThreadId: 23
        //end id:4 ThreadId: 20
        //end id:2 ThreadId: 22
        Console.WriteLine($"start id:{_id} ThreadId:{Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(1000);
        Console.WriteLine($"end id:{_id} ThreadId:{Thread.CurrentThread.ManagedThreadId}");

        return true;
    }
}
