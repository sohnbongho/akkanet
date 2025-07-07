using Akka.Actor;
using CommunityServer.Helper;
using NatsMessages;
using static Library.messages.S2SMessage;
using Library.AkkaActors;
using log4net;
using System.Reflection;
using Library.Logger;

namespace CommunityServer.ConsoleActor
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    class ConsoleWriterActor : UntypedActor
    {
        private Dictionary<string, Func<string[], bool>> _hanlder;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static IActorRef ActorOf(ActorSystem actorSystem)
        {
            var consoleWriterProps = Props.Create(() => new ConsoleWriterActor());
            return actorSystem.ActorOf(consoleWriterProps, ActorPaths.WriterConsole.Name);
        }        
        public ConsoleWriterActor()
        {
            _hanlder = new Dictionary<string, Func<string[], bool>>
            {
                { "/to", (message) => ToMessage(message) },
            };
        }
        protected override void OnReceive(object message)
        {
            if (message is string messageString)
            {
                // 공백을 기준으로 최대 10개의 부분으로 나눔
                string[] parts = messageString.Split(new char[] { ' ' }, 10);
                var consoleMessage = string.Empty;
                var foregroundColor = ConsoleColor.Red;

                if (parts.Length > 0 && _hanlder.TryGetValue(parts[0], out var handler))
                {
                    handler(parts);
                    consoleMessage = $"excute message: {parts[0]}";
                    foregroundColor = ConsoleColor.Green;
                }
                else
                {
                    consoleMessage = $"server message: {messageString}";
                    foregroundColor = ConsoleColor.Red;
                }
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(consoleMessage);
                Console.ResetColor();
                if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.ReaderConsole.Path, out var actor))
                {
                    actor.Tell(ConsoleReaderActor.UpdateCommand);
                }
            }
        }
        private bool ToMessage(string[] parts)
        {
            if (parts.Length < 2)
                return true;

            var fromServerId = ConfigHelper.Instance.ServerId;
            var toServerId = Int32.Parse(parts[1]);

            if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.MessageQueuePath, out var actor))
            {
                _logger.DebugEx(() => $"fromServerId:{fromServerId} toServerId:{toServerId}");
                actor.Tell(new NatsPubliish
                {
                    NatsMessageWrapper = new NatsMessageWrapper
                    {
                        FromServerId = fromServerId,
                        ToServerId = toServerId,

                        ZoneUserInvitedRequest = new Messages.ZoneUserInvitedRequest
                        {
                            ZoneIndex = 1,
                            
                        }
                    }
                });
            }
            else
            {
                _logger.Error($"not found nats actor");
            }

                
            return true;
        }
    }
}
