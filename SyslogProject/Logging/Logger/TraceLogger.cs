using System.Diagnostics;
using System.Xml.Linq;

namespace Logging.Logger
{
    /// <summary>
    /// Logging Service using Trace
    /// </summary>
    public class TraceLogger : ILoggingService
    {
        /// <summary>
        /// Log preffix for each message
        /// </summary>
        string name { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref=""/>
        /// </summary>
        /// <param name="name"></param>
        private TraceLogger(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a new logging service
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ILoggingService Get(string name)
        {
            return new TraceLogger(name);
        }

        /// <summary>
        /// Creates a new logging service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILoggingService Get<T>()
        {
            return Get(typeof(T).Name);
        }

        /// <summary>
        /// Creates a new logging service
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILoggingService ILoggingService.Get(string name)
        {
            return Get($"{this.name}.{name}");
        }

        /// <summary>
        /// Creates a new logging service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILoggingService ILoggingService.Get<T>()
        {
            return (this as ILoggingService).Get(typeof(T).Name);
        }

        /// <summary>
        /// Writes an informational message using <see cref="Trace.TraceInformation"/>
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            Trace.TraceInformation(Format(message));
        }

        /// <summary>
        /// Writes a warning message using <see cref="Trace.TraceWarning"/>
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message)
        {
            Trace.TraceWarning(Format(message));
        }

        /// <summary>
        /// Writes an error message using <see cref="Trace.TraceError"/>
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            Trace.TraceError(Format(message));
        }

        /// <summary>
        /// Writes an error message using <see cref="Trace.TraceError"/>
        /// </summary>
        /// <param name="exception"></param>
        public void Error(Exception exception)
        {
            Error($"{exception}");
        }

        private string Format(string message) => $"{name}:> {message}";
    }
}
