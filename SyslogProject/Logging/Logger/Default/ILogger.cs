using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Logger.Default
{
    using Generic;

    /// <summary>
    /// Default Logger interface used for DI
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Creates a new <see cref="ILogger"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILogger GetLogger(string name);

        /// <summary>
        /// Creates a new <see cref="ILogger{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILogger<T> GetLogger<T>();

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
