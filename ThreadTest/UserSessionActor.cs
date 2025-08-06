using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        Receive<UserSessionActor.ThreadCheck>(msg => OnThreadCheck(msg));
    }
    public void OnThreadCheck(UserSessionActor.ThreadCheck msg)
    {
        Console.WriteLine($"id:{_id} ThreadId:{Thread.CurrentThread.ManagedThreadId}");
    }
}
