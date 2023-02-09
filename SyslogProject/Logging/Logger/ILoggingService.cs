
namespace Logging.Logger
{
    /// <summary>
    /// Logging Service interface
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILoggingService Get(string name);

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILoggingService Get<T>();

        /// <summary>
        /// Writes an informational message
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);

        /// <summary>
        /// Writes a warning message
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);

        /// <summary>
        /// Writes an error message
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);

        /// <summary>
        /// Writes an error message
        /// </summary>
        /// <param name="exception"></param>
        void Error(Exception exception);
    }
}
