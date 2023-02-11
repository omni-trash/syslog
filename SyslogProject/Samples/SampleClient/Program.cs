using Logging.Logger;
using Logging.Logger.Default;
using Logging.Logger.Default.Generic;
using Logging.Terminal;
using Logging.Tracing;
using System.Diagnostics;
using System.Text;

namespace SampleClient
{
    internal class Program
    {
        // one ore more syslog servers
        readonly string syslogServers = "localhost dev game1";

        // logging service to use
        static readonly ILoggingService tracing = TraceLogger.Get("SampleClient")!;

        // logger to use
        readonly ILogger logger = new Logger<Program>(tracing);

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        private void Run(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            ConsoleColorCode.WriteToConsole("Syslog Client Sample ", ConsoleColorCode.DarkGray, "v1.0.0", Environment.NewLine);
            ConsoleColorCode.WriteToConsole(ConsoleColorCode.Green, "press any key to quit...", Environment.NewLine, Environment.NewLine);

            AddTraceListeners();

            Trace.TraceInformation("Program start");

            // Run the test
            TestCase(logger.GetLogger(nameof(TestCase)));

            Trace.TraceInformation("Program exit");
            Trace.Close();
        }

        /// <summary>
        /// Adds the trace listeners we want to use
        /// </summary>
        private void AddTraceListeners()
        {
            // Trace to console with color
            TraceUtil.AddConsoleColorCodeToTrace();

            // Trace to text file
            TraceUtil.AddTextFileToTrace(null, 1);

            // Trace to syslog
            TraceUtil.AddSyslogToTrace(syslogServers);
        }

        /// <summary>
        /// Writes some messages during infinity loop
        /// </summary>
        private static void TestCase(ILogger logger)
        {
            ManualResetEvent quit = new(false);
            ConsoleTerminate(quit);

            string BOM = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

            do
            {
                // "SampleClient.Program.TestCase:> Hello World"
                logger.Info("Hello World");

                Trace.TraceInformation($"Hallo Welt, das ist ein Test ... öäüß");

                Trace.TraceInformation($"Hallo Welt ... TraceInformation");
                Trace.TraceWarning($"Hallo Welt ... TraceWarning");
                Trace.TraceError($"Hallo Welt ... TraceError");

                Trace.TraceError($"");
                Trace.TraceInformation("\u001b[91mHello World");
                Trace.TraceInformation($"{BOM}message with BOM");
                
            } while (!quit.WaitOne(TimeSpan.FromSeconds(10)));
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