using Akka.Actor;
using CommunityServer.Component.DataBase;
using CommunityServer.Helper;
using Library.AkkaActors;
using Library.ECSSystem;
using Library.Logger;
using log4net;
using System.Reflection;
using static Library.messages.S2SMessage;

namespace CommunityServer.World
{
    /// <summary>
    /// 채팅 서버 액터
    /// </summary>
    public class WorldActor : UntypedActor, IECSSystemManager
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        // Component 패턴
        private ECSSystemManager _systemManager = new ECSSystemManager();


        public static IActorRef ActorOf(ActorSystem actorSystem)
        {
            var listenerProps = Props.Create(() => new WorldActor());
            return actorSystem.ActorOf(listenerProps, ActorPaths.World.Name);
        }

        /// <summary>
        /// Component 관리
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public T AddSystem<T>(T component) where T : class, IECSSystem
        {
            if (_systemManager == null)
            {
                _systemManager = new();
            }

            return _systemManager.AddSystem<T>(component);
        }

        public T? GetSystem<T>() where T : class, IECSSystem
        {
            if (_systemManager == null)
                return null;

            return _systemManager.GetSystem<T>();
        }

        public void RemoveSystem<T>() where T : class, IECSSystem
        {
            if (_systemManager == null)
                return;

            _systemManager.RemoveSystem<T>();
        }
        /// <summary>
        /// 생성자
        /// </summary>
        public WorldActor()
        {
            // 생존성 모니터링 시작
            //_heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
            //    TimeSpan.Zero, TimeSpan.FromSeconds(5), Self, new Messages.Heartbeat(), Self);
        }

        // <summary>
        // AssociationErrorEvent: 
        // 연결 설정이 실패했거나 연결 도중 오류가 발생했을 때 발생하는 이벤트입니다.
        // 이 이벤트는 소켓이 닫히는 상황에서도 발생할 수 있습니다.
        // </summary>        

        //private void HandleAssociationError(AssociationErrorEvent e)
        //{
        //    // AssociationErrorEvent contains the remote address in the RemoteAddress property
        //    var remoteAddress = e.RemoteAddress;
        //    // You can get the actor path from the remote address
        //    var remoteActorPath = remoteAddress + "/user/remoteActor";

        //    // handle error...
        //    _logger.InfoEx(()=>$"HandleAssociationError:{remoteActorPath}");           
        //}

        // <summary>
        // DisassociatedEvent: 연결이 끊어졌을 때 발생하는 이벤트입니다.
        // 연결이 강제로 끊기거나 상대방이 연결을 종료했을 때 발생합니다.
        // </summary>        
        //private void HandleDisassociation(DisassociatedEvent e)
        //{
        //    // DisassociatedEvent also contains the remote address in the RemoteAddress property
        //    var remoteAddress = e.RemoteAddress.ToString();
        //    // You can get the actor path from the remote address
        //    var remoteActorPath = remoteAddress + "/user/clientActor";

        //}

        protected override void PreStart()
        {
            //Context.System.EventStream.Subscribe(Self, typeof(AssociationErrorEvent));
            //Context.System.EventStream.Subscribe(Self, typeof(DisassociatedEvent));
            base.PreStart();

            AddSystem<UserRepository>(new UserRepository()); // MySql연결
            AddSystem<UserSessionRepository>(new UserSessionRepository()); // Redis연결                        

            // 기존에 접속하고 있는 유저들 초기화            
            {
                var serverId = ConfigHelper.Instance.ServerId;

                var redis = GetSystem<UserSessionRepository>();
                var db = GetSystem<UserRepository>();
                if (redis != null)
                {
                    redis.ClearServerUserCount(serverId);
                }
                if (db != null)
                {
                    db.ClearUserConnectedCommunityServer(serverId);
                }
            }
        }

        protected override void PostStop()
        {
            //Context.System.EventStream.Unsubscribe(Self, typeof(AssociationErrorEvent));
            //Context.System.EventStream.Unsubscribe(Self, typeof(DisassociatedEvent));

            // 생존성 모니터링 종료
            //_heartbeatTask?.Cancel();

            // Component 제거            
            // Component 제거
            RemoveSystem<UserRepository>();
            RemoveSystem<UserSessionRepository>();            
            _systemManager.Dispose();

            base.PostStop();
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

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case KeepAliveRequest _:
                    _logger.DebugEx(() => $"WorldActor KeepAliveRequest");
                    Sender.Tell(KeepAliveResponse.Instance);
                    break;
            }
        }
    }
}