using Akka.Actor;
using CommunityServer.Component.DataBase;
using CommunityServer.Helper;
using Library.AkkaActors.Socket;
using Library.Data.Enums;
using log4net;
using System.Reflection;

namespace CommunityServer.User
{
    public partial class UserSessionActor : SessionActor
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public UserSessionActor(bool packetEncrypt) : base(packetEncrypt)
        {

        }

        public UserSessionActor(IActorRef userManagementActor, string remoteAdress, IActorRef connection, bool packetEncrypt)
            : base(userManagementActor, remoteAdress, connection, packetEncrypt)
        {
        }
        protected override void PreStart()
        {
            base.PreStart();

            AddSystem<UserRepository>(new UserRepository()); // MySql연결
            AddSystem<UserSessionRepository>(new UserSessionRepository()); // Redis연결     
            AddSystem<RoomInfoDataSystem>(new RoomInfoDataSystem()); // Redis연결     

            InitCommunityInfo();
        }

        /// <summary>
        /// actor 종료
        /// </summary>
        protected override void PostStop()
        {
            try
            {
                DispseConnectedServer();
                DisposeCommunity();

                RemoveSystem<UserRepository>();
                RemoveSystem<UserSessionRepository>();
                RemoveSystem<RoomInfoDataSystem>();
            }
            catch (Exception ex)
            {
                _logger.Error("Error during PostStop", ex);
            }
            finally
            {
                base.PostStop();
            }
        }

        private void DispseConnectedServer()
        {
            var db = GetSystem<UserRepository>();

            var serverId = ConfigHelper.Instance.ServerId;

            var userSeq = _connectedUserSeq;
            var charSeq = _connectedCharSeq;
            if (userSeq > 0 && charSeq > 0 && db != null)
            {
                // 연결이 끈기면 유저 접속 가능
                db.StoreUserConnectedCommunityServerId(userSeq, charSeq, 0);
            }
        }

        protected override void OnReceive(object message)
        {
            base.OnReceive(message);
        }

        /// <summary>
        /// socket이 끈김을 알림
        /// </summary>
        public override void ClosedSocket(SessionClosedType closedType)
        {
            base.ClosedSocket(closedType);
        }
        public override void CloseSocketOnTimeout()
        {
            ClosedSocket(SessionClosedType.KeepAliveTimeOut);
        }
    }
}
