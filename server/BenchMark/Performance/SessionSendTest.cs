using Akka.Actor;
using BenchmarkDotNet.Attributes;
using Library.AkkaActors.Socket.Handler;
using Library.messages;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchMark.Performance;


[MemoryDiagnoser]
public class SessionSendTest
{
    [Benchmark]
    public void UseMemeryPool()
    {
        var message = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };

        var hanlder = new SessionSendHandlerWithPool();

        hanlder.EnqueuePendingPacket(message, true);
        hanlder.DequeuePendingPacket();
    }

    [Benchmark]
    public void Lite()
    {
        var message = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        var session = new SessionInfoHandler();
        var hanlder = new SessionSendHanlder();

        hanlder.Tell(session, message);
    }

    [Benchmark]
    public void LiteWithFixMemory()
    {
        var message = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        var session = new SessionInfoHandler();
        var hanlder = new SessionSendHanlderWithFixMemory();

        hanlder.Tell_Old(session, message);
    }
    
    [Benchmark]
    public void LiteWithFixMemory2()
    {
        var message = new MessageWrapper
        {
            ConnectedResponse = new ConnectedResponse
            {

            }
        };
        var session = new SessionInfoHandler();
        var hanlder = new SessionSendHanlderWithFixMemory();

        hanlder.Tell(session, message);
    }
}
