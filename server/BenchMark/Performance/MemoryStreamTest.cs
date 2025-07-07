using Akka.Actor;
using BenchmarkDotNet.Attributes;
using Library.AkkaActors.Socket.Handler;
using Library.Helper.Encrypt;
using Library.Memory.ProtoBuffer;
using Messages;

namespace BenchMark.Performance;

[MemoryDiagnoser]
public class MemoryStreamTest
{
    [Benchmark]
    public void StreamTest()
    {
        var handler = new SessionReadHandlerWithMemoryStream();
        handler.Init();

        for (int i = 0; i < 100; ++i)
        {
            var message = new MessageWrapper
            {
                MoveRequest = new MoveRequest
                {
                    Position = new Position(),
                    Velocity = new Position(),
                }
            };


            var bytes = message.ToByteArrayWithBuffer();
            var encrypt = CryptographyHelper.EncryptPacket(bytes);

            handler.HandleReceived(encrypt, true, ActorRefs.Nobody);
        }
    }

    [Benchmark]
    public void ListTest()
    {
        var handler = new SessionReadHandler();
        handler.Init();

        for (int i = 0; i < 100; ++i)
        {
            var message = new MessageWrapper
            {
                MoveRequest = new MoveRequest
                {
                    Position = new Position(),
                    Velocity = new Position(),
                }
            };


            var bytes = message.ToByteArrayWithBuffer();
            var encrypt = CryptographyHelper.EncryptPacket(bytes);

            handler.HandleReceived(encrypt, true, ActorRefs.Nobody, string.Empty, 0);
        }
    }
}
