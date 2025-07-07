using Messages;

namespace Library.AkkaActors.Socket.Handler;

public interface ISessionSendHanlder
{
    void Dispose();
    void Init();
    bool Tell(SessionInfoHandler session, MessageWrapper request);
}
