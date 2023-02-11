using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Logger.Default
{
    using Generic;

    /// <summary>
    /// Default Logger Sample Implementation used for DI
    /// </summary>
    public class Logger : ILogger
    {
        readonly ILoggingService service;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="logging">logging service to use</param>
        public Logger(ILoggingService logging)
        {
            service = logging;
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ILogger GetLogger(string name)
            => new Logger(service.Get(name));

        /// <summary>
        /// Creates a new <see cref="ILogger{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ILogger<T> GetLogger<T>()
            => new Logger<T>(service);

        /// <summary>
        /// Writes an error message
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
            => service.Error(message);

        /// <summary>
        /// Writes an error message
        /// </summary>
        /// <param name="exception"></param>
        public void Error(Exception exception)
            => service.Error(exception);

        /// <summary>
        /// Writes an informational message
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
            => service.Info(message);

        /// <summary>
        /// Writes an warning message
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message)
            => service.Warn(message);
    }
}
