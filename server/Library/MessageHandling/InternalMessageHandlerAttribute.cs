namespace Library.MessageHandling;

[AttributeUsage(AttributeTargets.Method)]
public class InternalMessageHandlerAttribute : Attribute
{
    public System.Type MessageType { get; }

    public InternalMessageHandlerAttribute(System.Type messageType)
    {
        MessageType = messageType;
    }
}
