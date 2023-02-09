using Logging.Terminal;
using Logging.Tracing;
using Syslog;
using Syslog.Serialization;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SampleServer
{
    internal class Program
    {
        ManualResetEvent quit = new(false);

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        private void Run(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            ConsoleColorCode.WriteToConsole("Syslog Server Sample ", ConsoleColorCode.DarkGray, "v1.0.0", Environment.NewLine);
            ConsoleColorCode.WriteToConsole(ConsoleColorCode.Green, "press any key to quit...", Environment.NewLine, Environment.NewLine);

            ConsoleTerminate(quit);

            TraceUtil.AddConsoleColorCodeToTrace();
            Trace.TraceInformation("Syslog is running");

            UdpServer server = new(Syslog.Transport.UdpSender.SYSLOG_UPD_REMOTE_PORT);

            server.OnError += Server_OnError;
            server.OnResult += Server_OnResult;
            server.Run(quit);

            Trace.TraceWarning("Syslog stopped");
            Trace.Close();
        }

        /// <summary>
        /// Server OnError Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_OnError(object? sender, Exception e)
        {
            // Display error and terminate
            Trace.TraceError($"{e}");
            quit.Set();
        }

        /// <summary>
        /// Server OnResult Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_OnResult(object? sender, UdpReceiveResult e)
        {
            HandleUdpPacketAsync(e);
        }

        /// <summary>
        /// Takes the syslog message from UDP result and displays formatted text on console window
        /// </summary>
        /// <param name="packet">the packet to handle</param>
        /// <returns></returns>
        private Task HandleUdpPacketAsync(UdpReceiveResult packet)
        {
            return Task.Run(() => HandleUdpPacket(packet));
        }

        /// <summary>
        /// Takes the syslog message from UDP result and displays formatted text on console window
        /// </summary>
        /// <param name="packet">the packet to handle</param>
        private void HandleUdpPacket(UdpReceiveResult packet)
        {
            if (packet.Buffer == null)
            {
                return;
            }

            if (packet.Buffer.Length == 0)
            {
                return;
            }

            DateTime timestamp = DateTime.Now;

            // NOTE:
            // packet can have control characters,
            // so we have to escape or remove them
            // before print some data on console
            string payload = Encoding.UTF8.GetString(packet.Buffer);
            string escaped = TextUtil.EscapeControls(payload)!;

            try
            {
                SyslogMessage message = MessageConverter.FromRFC5424String(escaped);
                DisplayMessage(timestamp, packet.RemoteEndPoint, message);
            }
            catch (LexerError)
            {
                DisplayMalformedMessage(timestamp, packet.RemoteEndPoint, escaped);
            }
            catch (Exception error)
            {
                Trace.TraceError($"{error}");
            }
        }

        /// <summary>
        /// Writes syslog message to console
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="from"></param>
        /// <param name="message"></param>
        private static void DisplayMessage(DateTime timestamp, IPEndPoint from, SyslogMessage message)
        {
            TextBuilder builder = new();

            string msgTextColor;

            switch (message.SEVERITY)
            {
                case Severity.Emergency:
                case Severity.Alert:
                case Severity.Critical:
                case Severity.Error:
                    msgTextColor = ConsoleColorCode.DarkRed;

                    builder.AddSection(
                        ConsoleColorCode.DarkRedBack,
                        ConsoleColorCode.Gray,
                        "ERR");
                    break;
                case Severity.Warning:
                case Severity.Notice:
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
                "] (",
                ConsoleColorCode.DarkGray,
                from.Address,
                ConsoleColorCode.Default,
                ":",
                ConsoleColorCode.DarkGray,
                from.Port,
                ConsoleColorCode.DarkCyan,
                "):> ");

            //
            // Write Message
            //

            if (string.IsNullOrEmpty(message.MSG))
            {
                builder.AddSection(
                    Environment.NewLine,
                    ConsoleColorCode.Black,
                    ConsoleColorCode.DarkGrayBack,
                    "<NO MESSAGE TEXT>",
                    Environment.NewLine,
                    Environment.NewLine);
            }
            else
            {
                builder.AddSection(
                    Environment.NewLine,
                    ">>> ",
                    msgTextColor,
                    message.MSG, 
                    Environment.NewLine,
                    Environment.NewLine);
            }

            //
            // Write Details
            //

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Timestamp: ",
                $"{message.TIMESTAMP:yyyy-MM-ddTHH:mm:ss.fffK}",
                Environment.NewLine);

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Facility : ",
                message.FACILITY,
                Environment.NewLine);

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Severity : ",
                msgTextColor,
                message.SEVERITY,
                Environment.NewLine);

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Hostname : ",
                message.HOSTNAME,
                Environment.NewLine);

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Appname  : ",
                message.APPNAME,
                Environment.NewLine);

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Proc ID  : ",
                message.PROCID,
                Environment.NewLine);

            builder.AddSection(
                ConsoleColorCode.DarkGray,
                "Msg ID   : ",
                message.MSGID,
                Environment.NewLine);

            builder.Add(Environment.NewLine);

            //
            // Write Structured Data
            //

            if (message.SDATA?.Count > 0)
            {
                foreach (var element in message.SDATA)
                {
                    // SDID
                    builder.AddSection(
                        ConsoleColorCode.DarkCyan,
                        element.Key);

                    if (element.Value?.Count  > 0)
                    {
                        foreach (var param in element.Value)
                        {
                            builder.AddSection(
                                Environment.NewLine,
                                ConsoleColorCode.DarkGray,
                                " → ",
                                param.Key,
                                ": \"",
                                param.Value,
                                "\"");
                        }
                    }

                    builder.Add(Environment.NewLine);
                }

                builder.Add(Environment.NewLine);
            }

            ConsoleColorCode.WriteToConsole(builder.ToArray());
        }

        /// <summary>
        /// Writes malformed syslog message to console
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="from"></param>
        /// <param name="escaped"></param>
        private static void DisplayMalformedMessage(DateTime timestamp, IPEndPoint from, string escaped)
        {
            TextBuilder builder = new();

            builder.AddSection(
                ConsoleColorCode.DarkYellowBack,
                ConsoleColorCode.Black,
                "MAL");

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
                "] (",
                ConsoleColorCode.DarkGray,
                from.Address,
                ConsoleColorCode.Default,
                ":",
                ConsoleColorCode.DarkGray,
                from.Port,
                ConsoleColorCode.DarkCyan,
                "):> MALFORMED MESSAGE OR RFC 3164 (NOT SUPPORTED)");

            //
            // Write Message
            //

            builder.AddSection(
                Environment.NewLine,
                ConsoleColorCode.DarkYellow,
                escaped,
                Environment.NewLine,
                Environment.NewLine);

            builder.Add(Environment.NewLine);
            ConsoleColorCode.WriteToConsole(builder.ToArray());
        }

        /// <summary>
        /// Terminates when user pressed a key
        /// </summary>
        /// <param name="quit"></param>
        private static void ConsoleTerminate(ManualResetEvent quit)
        {
            // on cancel event (ctrl+c, shutdown)
            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                quit.Set();
            };

            if (Console.IsInputRedirected)
            {
                return;
            }

            // on key pressed (user)
            Task.Run(() =>
            {
                Console.ReadKey(true);
                quit.Set();
            });
        }
    }
}