using Akka.Actor;
using System.Collections.Concurrent;

namespace Library.AkkaActors;

public class ActorRefsHelper
{
    private static readonly Lazy<ActorRefsHelper> lazy = new Lazy<ActorRefsHelper>(() => new ActorRefsHelper());
    public static ActorRefsHelper Instance { get { return lazy.Value; } }

    public ConcurrentDictionary<string, IActorRef> Actors { get; set; } = new ConcurrentDictionary<string, IActorRef>();

    public void Set(string actorName, IActorRef actor)
    {
        Actors[actorName] = actor;
    }

    public IActorRef Get(string actorName)
    {
        if(Actors.TryGetValue(actorName, out var actorRef))
        {
            return actorRef;
        }
        return ActorRefs.Nobody;
    }
}
