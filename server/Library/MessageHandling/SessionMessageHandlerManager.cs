using Akka.Actor;
using Library.MessageHandling;
using log4net;
using Messages;
using System.Reflection;

public class SessionMessageHandlerManager : IDisposable
{    
    private Dictionary<MessageWrapper.PayloadOneofCase, Func<MessageWrapper, IActorRef, bool, bool>>? _handlers;

    private object? _instance;
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static SessionMessageHandlerManager Of(object instance)
    {
        var hanlder = new SessionMessageHandlerManager(instance);
        hanlder.InitializeHandlers();
        return hanlder;
    }

    private SessionMessageHandlerManager(object instance)
    {
        _instance = instance;        
    }
    private void InitializeHandlers()
    {
        if (_instance == null)
            return;

        var methods = _instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        _handlers = new();

        try
        {
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<SessionMessageHandlerAttribute>();
                if (attribute != null)
                {
                    var parameters = method.GetParameters();

                    // ✅ 매개변수 개수 및 타입 검증
                    if (parameters.Length != 3 ||
                        parameters[0].ParameterType != typeof(MessageWrapper) ||
                        parameters[1].ParameterType != typeof(IActorRef) ||
                        parameters[2].ParameterType != typeof(bool))
                    {
                        var errorStr = $"❌ {method.Name} wrong parameter";
                        _logger.Error(errorStr);
                        throw new InvalidOperationException(errorStr);
                    }

                    // ✅ Func<>으로 Delegate 변환하여 직접 호출 가능
                    var handler = (Func<MessageWrapper, IActorRef, bool, bool>)
                        method.CreateDelegate(typeof(Func<MessageWrapper, IActorRef, bool, bool>), _instance); ;

                    _handlers[attribute.MessageType] = handler;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("InitializeHandlers", ex);
        }

        
    }

    public bool HandleMessage(MessageWrapper wrapper, IActorRef sessionRef, bool calledTdd)
    {
        if (_instance == null)
            return false;
        
        if (_handlers == null)
            return false;

        if (_handlers.TryGetValue(wrapper.PayloadCase, out var handler))
        {
            try
            {
                return handler(wrapper, sessionRef, calledTdd); // ✅ Func<>을 사용하여 직접 호출                
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error executing for message '{wrapper.PayloadCase}': {ex.Message}");
                return false;
            }
        }
        else
        {
            _logger.Error($"⚠ No handler found for message type: {wrapper.PayloadCase}");
            return false;
        }
    }

    public void Dispose()
    {
        if(_handlers != null)
        {
            _handlers.Clear();
            _handlers = null;
        }       

        _instance = null;
    }
}
