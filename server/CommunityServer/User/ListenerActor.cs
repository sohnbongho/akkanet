using Akka.Actor;
using Akka.IO;
using Library.AkkaActors;
using Library.Logger;
using Library.messages;
using log4net;
using System.Net;
using System.Reflection;

namespace CommunityServer.User
{
    public class ListenerActor : UntypedActor
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly int _port;

        private IActorRef _sessionCordiatorRef = ActorRefs.Nobody;
        private IActorRef _worldActorRef = ActorRefs.Nobody;
        private IActorRef _tcpListener = ActorRefs.Nobody;

        public static IActorRef ActorOf(ActorSystem actorSystem, IActorRef worldActor, int port)
        {
            var clientProps = Props.Create(() => new ListenerActor(worldActor, port));
            return actorSystem.ActorOf(clientProps, ActorPaths.Listener.Name);
        }

        public ListenerActor(IActorRef worldActor, int port)
        {
            _worldActorRef = worldActor;
            _port = port;

            Context.System.Tcp().Tell(new Tcp.Bind(Self, new IPEndPoint(IPAddress.Any, port)));
        }

        // here we are overriding the default SupervisorStrategy
        // which is a One-For-One strategy w/ a Restart directive
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // duration
                x =>
                {
                    return Directive.Restart;

                    ////Maybe we consider ArithmeticException to not be application critical
                    ////so we just ignore the error and keep going.
                    //if (x is ArithmeticException) return Directive.Resume;

                    ////Error that we cannot recover from, stop the failing actor
                    //else if (x is NotSupportedException) return Directive.Stop;

                    ////In all other cases, just restart the failing actor
                    //else return Directive.Restart;
                });
        }

        protected override void PreStart()
        {
            base.PreStart();

            // 세션을 관리해 주는 Actor 생성            
            _sessionCordiatorRef = UserCordiatorActor.ActorOf(Context, Self, _worldActorRef);
        }

        protected override void PostStop()
        {
            _logger.InfoEx(() => $"PostStop ListenerActor");

            _tcpListener.Tell(Tcp.Unbind.Instance, Self);
            _tcpListener.Tell(Tcp.Closed.Instance, Self);

            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Tcp.Bound bound:
                    {
                        _tcpListener = Sender;

                        _logger.DebugEx(() => $"Listening on {bound.LocalAddress}");
                        break;
                    }
                case Tcp.Connected connected:
                    {
                        _logger.DebugEx(() => $"Tcp.Connected on {connected.RemoteAddress.ToString()}");
                        var remoteAdress = connected.RemoteAddress.ToString() ?? string.Empty;
                        if (_sessionCordiatorRef != null)
                        {
                            _sessionCordiatorRef.Tell(new Session.RegisteredRequest
                            {
                                Sender = Sender,
                                RemoteAdress = remoteAdress,
                            });
                        }

                        break;
                    }
                case DeathPactException:
                    {
                        _logger.DebugEx(() => $"DeathPactException");
                        break;
                    }
                case Terminated terminated:
                    {
                        _logger.DebugEx(() => $"terminated {terminated}");

                        // Here, handle the termination of the watched actor.
                        // For example, you might want to create a new actor or simply log the termination.
                        break;
                    }
                default:
                    {
                        _logger.DebugEx(() => $"Unhandled type:{message.GetType()}");

                        Unhandled(message);
                        break;
                    }
            }
        }
    }

}
