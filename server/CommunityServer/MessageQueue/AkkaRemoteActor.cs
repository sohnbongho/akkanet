using Akka.Actor;
using log4net;
using System.Reflection;
using static Library.messages.S2SMessage;
using Library.Component;
using Library.AkkaActors;
using Library.messages;
using Library.Repository.Mysql;
using Library.DBTables.MySql;

namespace CommunityServer.MessageQueue;

public class AkkaRemoteActor : ReceiveActor
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);    
    private Dictionary<int, TblServerList> _servers = new();
    private readonly ActorSystem _actorSystem;

    public AkkaRemoteActor(ActorSystem actorSystem)
    {
        _actorSystem = actorSystem;

        // 다른 서버로 보내는 메시지
        Receive<RemotePubliish>(message =>
        {
            OnRemotePubliish(message);
        });


        // 다른 서버에서 받은 메시지
        Receive<RemoteSub>(message =>
        {
            OnNatsMessageWrapper(message);
        });
    }

    public static IActorRef ActorOf(ActorSystem actorSystem)
    {
        var consoleReaderProps = Props.Create(() => new AkkaRemoteActor(actorSystem));
        return actorSystem.ActorOf(consoleReaderProps, ActorPaths.AkkaRemoteActor.Name);
    }    

    protected override void PreStart()
    {
        base.PreStart();

        ActorRefsHelper.Instance.Actors[ActorPaths.AkkaRemoteActor.Path] = Self;

        var mySqlDbCommon = new ServerInfoSharedRepo();
        
        var serverInfos = mySqlDbCommon.GetServerInfos();
        foreach (var serverInfo in serverInfos)
        {
            _servers[serverInfo.server_id] = serverInfo;
        }
    }

    protected override void PostStop()
    {
        base.PostStop();
    }
    
    /// <summary>
    /// 다른 서버로 보낼 메시지
    /// </summary>    
    public void OnRemotePubliish(RemotePubliish message)
    {
        try
        {
            //var toServerId = message.NatsMessageWrapper.ToServerId;
            //var actorName = ActorPaths.AkkaRemoteActor.Name;

            //if (_servers.TryGetValue(toServerId, out var server))
            //{
            //    var remoteAddress = Address.Parse(server.remote_actor_path);
            //    var remoteAkkaAddress = remoteAddress + $"/user/{actorName}";

            //    var remoteActor = _actorSystem.ActorSelection(remoteAkkaAddress);

            //    remoteActor.Tell(new RemoteSub
            //    {
            //        NatsMessageWrapper = message.NatsMessageWrapper,
            //    });
            //}
        }
        catch (Exception ex)
        {
            _logger.Error($"Exception OnRemotePubliish ", ex);
        }

    }

    /// <summary>
    /// 다른 서버에서 받은 메시지
    /// </summary>
    /// <param name="message"></param>
    private void OnNatsMessageWrapper(RemoteSub message)
    {
        try
        {
            if (ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
            {
                userCordiatorActor.Tell(new U2UCMessage.MessageFromOtherServer
                {
                    NatsMessageWrapper = message.NatsMessageWrapper,
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Exception OnNatsMessageWrapper ", ex);

        }
    }
}
