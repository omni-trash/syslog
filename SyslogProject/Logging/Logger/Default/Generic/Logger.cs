
namespace Logging.Logger.Default.Generic
{
    /// <summary>
    /// Generic Default Logger Sample Implementation used for DI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Logger<T> : Logger, ILogger<T>
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="logging">logging service to use</param>
        public Logger(ILoggingService logging)
            : base(logging.Get<T>())
        {

        }
    }
}
