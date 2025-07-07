using Akka.Actor;
using CommunityServer.Helper;
using CommunityServer.Repository;
using Library.AkkaActors;
using Library.Component;
using Library.ECSSystem;
using Library.Helper;
using Library.messages;
using log4net;
using System.Reflection;

namespace CommunityServer.World.Notice;

public class UserNoticeActor : ReceiveActor, IECSSystemManager
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    // Component 패턴
    protected ECSSystemManager _systemManager = new ECSSystemManager();
    private ICancelable? _cancelable = null!;

    private class CheckNotice
    {
        public static CheckNotice Instance { get; set; } = new CheckNotice();

    }

    public UserNoticeActor()
    {
        Receive<CheckNotice>(_ =>
        {
            OnCheckNotice();
        });
    }

    public static IActorRef ActorOf(ActorSystem actorSystem)
    {
        var consoleWriterProps = Props.Create(() => new UserNoticeActor());
        return actorSystem.ActorOf(consoleWriterProps, ActorPaths.UserNotice.Name);
    }
    protected override void PreStart()
    {
        base.PreStart();

        AddSystem<UserNoticeSystem>(new UserNoticeSystem()); // MySql연결

        var context = Context;
        var self = Self;

        _cancelable = ScheduleFactory.Start(context, ConstInfo.CheckedUserNoticeTime, self, CheckNotice.Instance);
    }

    protected override void PostStop()
    {

        try
        {
            Dispose();
        }
        catch (Exception ex)
        {
            _logger.Error("failed to PostStop", ex);
        }
        finally
        {
            base.PostStop();
        }
    }

    public T AddSystem<T>(T component) where T : class, IECSSystem
    {
        return _systemManager.AddSystem<T>(component);
    }
    public T? GetSystem<T>() where T : class, IECSSystem
    {
        return _systemManager.GetSystem<T>();
    }

    public void RemoveSystem<T>() where T : class, IECSSystem
    {
        _systemManager.RemoveSystem<T>();
    }
    public void Dispose()
    {
        _cancelable?.Cancel();
        _cancelable = null;

        RemoveSystem<UserNoticeSystem>(); // MySql연결
        _systemManager.Dispose();

    }
    private void OnCheckNotice()
    {
        _cancelable = null;

        var context = Context;
        var self = Self;

        try
        {
            if (false == ActorRefsHelper.Instance.Actors.TryGetValue(ActorPaths.UserCordiator.Path, out var userCordiatorActor))
                return;

            // 공지가 있는지 체크
            var repo = GetSystem<UserNoticeSystem>();
            if (repo == null)
                return;

            var now = DateTimeHelper.Now;
            var serverId = ConfigHelper.Instance.ServerId;
            var notices = repo.FetchNoticeImmediate(serverId, now);

            foreach (var notice in notices)
            {
                userCordiatorActor.Tell(new U2UCMessage.UserNoticeNoti
                {
                    Title = notice.title,
                    Content = notice.content,
                });
            }


        }
        catch (Exception ex)
        {
            _logger.Error("failed to OnCheckNotice", ex);
        }
        finally
        {
            _cancelable = ScheduleFactory.Start(context, ConstInfo.CheckedUserNoticeTime, self, CheckNotice.Instance);
        }
    }
}
