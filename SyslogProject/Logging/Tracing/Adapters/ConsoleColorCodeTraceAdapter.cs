using Logging.Terminal;
using System.Diagnostics;

namespace Logging.Tracing.Adapters
{
    public class ConsoleColorCodeTraceAdapter : ITraceAdapter
    {
        /// <summary>
        /// Writes to the console window with color
        /// </summary>
        /// <param name="message"></param>
        public void WriteLine(TraceEventType eventType, string? message)
        {
            DateTime timestamp = DateTime.Now;

            // NOTE:
            // message can have control characters,
            // so we have to escape or remove them
            // before print some data on console
            string? escaped = TextUtil.EscapeControls(message);

            TextBuilder builder = new();

            string? msgTextColor;

            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    msgTextColor = ConsoleColorCode.DarkRed;

                    builder.AddSection(
                        ConsoleColorCode.DarkRedBack,
                        ConsoleColorCode.Gray,
                        "ERR");
                    break;
                case TraceEventType.Warning:
                    msgTextColor = ConsoleColorCode.DarkYellow;

                    builder.AddSection(
                        ConsoleColorCode.DarkYellowBack,
                        ConsoleColorCode.Black,
                        "WRN");
                    break;
                default:
                    msgTextColor = ConsoleColorCode.Gray;

                    builder.AddSection(
                        ConsoleColorCode.DarkGrayBack,
                        ConsoleColorCode.Black,
                        "INF");
                    break;
            }

            //
            // Write Header
            //

            builder.AddSection(
                ConsoleColorCode.DarkCyan,
                " [",
                ConsoleColorCode.DarkGray,
                $"{timestamp:yyyy-MM-dd} ",
                ConsoleColorCode.Gray,
                $"{timestamp:HH:mm:ss}",
                ConsoleColorCode.DarkGray,
                $"{timestamp:.fff}",
                ConsoleColorCode.DarkCyan,
                "]:> ");

            //
            // Write Message
            //

            builder.AddSection(
                msgTextColor,
                escaped,
                Environment.NewLine);

            ConsoleColorCode.WriteToConsole(builder.ToArray());
        }

        /// <summary>
        /// Close adapter
        /// </summary>
        public void Close()
        {
            lock (ConsoleColorCode.SyncRoot)
            {
                // graceful shutdown
            }
        }
    }
}
