using Akka.Actor;
using Akka.Configuration;
using log4net;
using Microsoft.Extensions.Hosting;
using LobbyServer.ConsoleActor;
using LobbyServer.Helper;
using LobbyServer.World;
using System.Reflection;
using LobbyServer.DataBase.MySql;
using LobbyServer.DataBase.Redis;
using LobbyServer.DataBase.MongoDb;
using Library.AkkaActors.MessageQueue;
using Library.Logger;
using Akka.Event;
using Library.AkkaActors;

namespace LobbyServer;

public class AkkaHostedService : IHostedService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private ActorSystem _actorSystem;

    public AkkaHostedService(ActorSystem actorSystem)
    {
        _actorSystem = actorSystem;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // DeadLetterListener 등록                
        var deadLetterActorRef = AkkaDeadLetterListener.ActorOf(_actorSystem);
        _actorSystem.EventStream.Subscribe(deadLetterActorRef, typeof(DeadLetter));

        // Configure your actors here...
        // text console창에 적는 actor                
        var consoleWriterActor = ConsoleWriterActor.ActorOf(_actorSystem);

        // text를 읽는 actor                
        var consoleReaderActor = ConsoleReaderActor.ActorOf(_actorSystem, consoleWriterActor);
        consoleReaderActor.Tell(ConsoleReaderActor.StartCommand); // begin processing

        // Db actor                
        var dbActor = DbServiceCordiatorActor.ActorOf(_actorSystem);

        // Redis actor                
        var redisActor = RedisServiceCordiatorActor.ActorOf(_actorSystem);

        // mongDb actor 생성
        var mongDbActor = MongoDbCordiatorActor.ActorOf(_actorSystem);

        // nats acotr
        var natsConnectionString = ConfigHelper.Instance.NatsConnectString;
        var serverId = ConfigHelper.Instance.ServerId;
        var natsActor = NatsMQActor.ActorOf(_actorSystem, natsConnectionString, serverId);

        // RabbitActor
        //var rabbitActor = RabbitMQActor.ActorOf(_actorSystem);

        // AkkaRemoteActor acotr
        //var akkaRemoteActor = AkkaRemoteActor.ActorOf(_actorSystem);

        // World Actor 생성                
        var worldActor = WorldActor.ActorOf(_actorSystem);

        // Akka.IO로 초기화                
        //var listener = ListenerActor.ActorOf(_actorSystem, worldActor, ConfigHelper.Instance.Port);
        //var zoneListener = ZoneSessionListenerActor.ActorOf(_actorSystem, worldActor, ConfigHelper.Instance.OpPort);

        _logger.InfoEx(()=>$@"ServerManager Doing. ""exit"" is exit");

        return Task.CompletedTask;
    }
    public static Config LoadAkkaHconConfig()
    {
        var fullPath = Assembly.GetExecutingAssembly().Location;
        var directoryPath = Path.GetDirectoryName(fullPath);

        string path = $@"{directoryPath}/AkkaHCON.conf"; // 수정해야 할 부분

        // 파일이 존재하는지 확인
        if (File.Exists(path) == false)
        {
            _logger.Error($"not found file : {path}");
            Config tmpConfig = new Config();
            return tmpConfig;
        }
        // 파일 내용 읽기
        string content = File.ReadAllText(path);

        var config = ConfigurationFactory.ParseString(content);
        return config;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _actorSystem.Terminate();
    }
}
