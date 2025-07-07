using log4net;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Library.Logger;

public static class LoggerExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InfoEx(this ILog logger, Func<string> message)
    {
        if (logger.IsInfoEnabled)
        {
            logger.Info(message());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugEx(this ILog logger, Func<string> message)
    {
        if (logger.IsDebugEnabled)
        {
            logger.Debug(message());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarnEx(this ILog logger, Func<string> message)
    {
        if (logger.IsWarnEnabled)
        {
            logger.Warn(message());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WarnWithContentError(this ILog logger, int errorCode, object request, object response)
    {
        if (logger.IsWarnEnabled)
        {
            var requestClassName = request.GetType().Name;
            var serializedRequest = JsonConvert.SerializeObject(request);
            var serializedResponse = JsonConvert.SerializeObject(response);

            logger.Warn($"ContentError name:[{requestClassName}] errorCode:[{errorCode}] reqeust:[{serializedRequest}] -> response:[{serializedResponse}]");
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugEx(this ILog logger, object request, object response)
    {
        if (logger.IsDebugEnabled)
        {
            var requestClassName = request.GetType().Name;
            var serializedRequest = JsonConvert.SerializeObject(request);
            var serializedResponse = JsonConvert.SerializeObject(response);

            logger.Debug($"Content name:[{requestClassName}] reqeust:[{serializedRequest}] -> response:[{serializedResponse}]");
        }

    }
}
