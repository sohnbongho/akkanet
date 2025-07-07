using Messages;

namespace Library.MessageHandling;

[AttributeUsage(AttributeTargets.Method)]
public class SessionMessageHandlerAttribute : Attribute
{
    public MessageWrapper.PayloadOneofCase MessageType { get; }

    public SessionMessageHandlerAttribute(MessageWrapper.PayloadOneofCase messageType)
    {
        MessageType = messageType;
    }
}
