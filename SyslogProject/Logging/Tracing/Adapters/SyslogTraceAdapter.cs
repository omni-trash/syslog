using Logging.Terminal;
using Syslog;
using System.Diagnostics;

namespace Logging.Tracing.Adapters
{
    public class SyslogTraceAdapter : ITraceAdapter
    {
        readonly SyslogClient syslog;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="syslog">SyslogClient to use</param>
        public SyslogTraceAdapter(SyslogClient syslog)
        {
            this.syslog = syslog;
        }

        /// <summary>
        /// Send syslog message
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="message"></param>
        public void WriteLine(TraceEventType eventType, string? message)
        {
            // NOTE:
            // message can have control characters,
            // so we have to escape or remove them
            // before submit to protect syslog servers
            string? escaped = TextUtil.EscapeControls(message);

            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    syslog.SendError(escaped);
                    break;
                case TraceEventType.Warning:
                    syslog.SendWarning(escaped);
                    break;
                default:
                    syslog.SendText(escaped);
                    break;
            }
        }

        /// <summary>
        /// Close Adapter
        /// </summary>
        public void Close()
        {
            syslog.Close();
        }
    }
}
