using Akka.Actor;
using log4net;
using System.Reflection;

namespace Library.MessageHandling;

public class InternalMessageHandlerManager
{
    private Dictionary<System.Type, Func<object, IActorRef, bool>>? _handlers;
    private object? _instance;
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static InternalMessageHandlerManager Of(object instance)
    {
        var hanlder = new InternalMessageHandlerManager(instance);
        hanlder.InitializeHandlers();
        return hanlder;
    }

    private InternalMessageHandlerManager(object instance)
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
                var attribute = method.GetCustomAttribute<InternalMessageHandlerAttribute>();
                if (attribute != null)
                {
                    var parameters = method.GetParameters();

                    // ✅ 매개변수 개수 및 타입 검증
                    if (parameters.Length != 2 ||
                        parameters[0].ParameterType != typeof(object) || 
                        parameters[1].ParameterType != typeof(IActorRef))
                    {
                        var errorStr = $"❌ {method.Name} wrong parameter";
                        _logger.Error(errorStr);
                        throw new InvalidOperationException(errorStr);
                    }

                    // ✅ Func<>으로 Delegate 변환하여 직접 호출 가능                    
                    var handler = (Func<object, IActorRef, bool>)
                        method.CreateDelegate(typeof(Func<object, IActorRef, bool>), _instance);

                    _handlers[attribute.MessageType] = handler;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("InitializeHandlers", ex);
        }
        
    }

    public bool HandleMessage(object message, IActorRef sender)
    {
        if (_instance == null)
            return false;

        if (_handlers == null)
            return false;

        if (_handlers.TryGetValue(message.GetType(), out var hanlder))
        {
            try
            {
                hanlder(message, sender);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error executing for message '{message.GetType()}': {ex.Message}");
                return false;
            }
        }
        else
        {
            _logger.Error($"⚠ No handler found for message type: {message.GetType()}");
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
