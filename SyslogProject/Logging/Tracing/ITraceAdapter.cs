using System.Diagnostics;

namespace Logging.Tracing
{
    /// <summary>
    /// Simple Trace Adapter for <see cref="AdapterTraceListener"/>
    /// </summary>
    public interface ITraceAdapter
    {
        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="message"></param>
        void WriteLine(TraceEventType eventType, string? message);

        /// <summary>
        /// Close the adapter
        /// </summary>
        void Close();
    }
}
