using System.Diagnostics;

namespace Logging.Tracing
{
    /// <summary>
    /// This class uses an adapter to write the message
    /// </summary>
    public sealed class AdapterTraceListener : TraceListener
    {
        TraceEventType eventType = TraceEventType.Information;

        bool closed = false;

        readonly ITraceAdapter adapter;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="adapter"></param>
        public AdapterTraceListener(ITraceAdapter adapter)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            // Close adapter here when u forgot to call Trace.Close in main
            AppDomain.CurrentDomain.ProcessExit  += (s, e) => Close();
            AppDomain.CurrentDomain.DomainUnload += (s, e) => Close();
        }

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string? message)
        {
            if (closed)
            {
                return;
            }

            adapter.WriteLine(eventType, message);
        }

        /// <summary>
        /// Close the output stream so that it no longer receives tracing output
        /// </summary>
        public override void Close()
        {
            if (closed)
            {
                return;
            }

            closed = true;
            adapter.Close();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string? message)
        {
            // unused
        }

        /// <summary>
        /// Remember TraceEventType
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <param name="message"></param>
        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
        {
            this.eventType = eventType;
            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Remember TraceEventType
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
        {
            this.eventType = eventType;
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        /// <summary>
        /// Remember TraceEventType
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id)
        {
            this.eventType = eventType;
            base.TraceEvent(eventCache, source, eventType, id);
        }
    }
}
