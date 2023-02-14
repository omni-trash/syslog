using Logging.Logger.Default;

namespace Logging.Logger
{
    /// <summary>
    /// Log Util
    /// </summary>
    public static class LogUtil
    {
        /// <summary>
        /// Creates a <see cref="Default.Logger"/> with the <see cref="TraceLogger"/> 
        /// as logging service and with full name of the given type as prefix
        /// </summary>
        /// <typeparam name="T">take full name from that type</typeparam>
        /// <returns></returns>
        public static ILogger GetLogger<T>()
            => GetLogger(typeof(T));

        /// <summary>
        /// Creates a <see cref="Default.Logger"/> with the <see cref="TraceLogger"/> 
        /// as logging service and with full name of the given type as prefix
        /// </summary>
        /// <param name="type">take full name from that type</param>
        /// <returns></returns>
        public static ILogger GetLogger(Type type)
            => new Default.Logger(TraceLogger.Get($"{type}"));
    }
}
