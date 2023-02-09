using Logging.Tracing.Adapters;
using Syslog;
using Syslog.Transport;
using System.Diagnostics;

namespace Logging.Tracing
{
    /// <summary>
    /// This class has some workloads to add commonly used trace outputs
    /// </summary>
    public static class TraceUtil
    {
        /// <summary>
        /// Adds ConsoleColorCodeTraceAdapter to Trace.Listeners
        /// Removes the ConsoleTraceListener from Trace.Listeners
        /// </summary>
        public static void AddConsoleColorCodeToTrace()
        {
            ConsoleColorCodeTraceAdapter adapter = new();
            AdapterTraceListener listener = new(adapter);
            Trace.Listeners.Add(listener);

            // Remove ConsoleTraceListener
            Trace.Listeners.OfType<ConsoleTraceListener>().ToList().ForEach(listener => Trace.Listeners.Remove(listener));
        }

        /// <summary>
        /// Adds SyslogTraceAdapter (UDP) to Trace.Listeners
        /// </summary>
        /// <param name="remoteHosts">one or more remote hosts, separated by space</param>
        public static void AddSyslogToTrace(string remoteHosts)
        {
            // Add Syslog Trace for each remote host
            string[] targets = remoteHosts.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            List<SyslogClient> clients = new List<SyslogClient>();

            foreach (var target in targets)
            {
                // Create Syslog Client
                UdpSender    sender = new(target);
                SyslogClient syslog = new(sender);

                // Optional
                syslog.OnError += Syslog_OnError;

                // Add Syslog Trace
                SyslogTraceAdapter   adapter  = new(syslog);
                AdapterTraceListener listener = new(adapter);
                Trace.Listeners.Add(listener);

                clients.Add(syslog);
            }

            // After all Listeners are added to ensure messages goes through the Trace pipe
            clients.ForEach(client => client.Open());
        }

        /// <summary>
        /// Syslog OnError Event Handler, tries to reopen after a long time (15 min)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Syslog_OnError(object? sender, Exception e)
        {
            // The syslog client shut downs automatically on sender error.
            // If that is ok, then do not reopen. The syslog client will not send any message anymore.
            // If that is not ok, than you can try to reopen through reset all states after a long time (!).
            // Dont reopen fast, it makes no sense on network failure.
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(15));
                (sender as SyslogClient)?.Reset();
            });
        }

        /// <summary>
        /// Adds TextFileTraceAdapter to Trace.Listeners
        /// </summary>
        /// <param name="fullpath">the logfile to use</param>
        public static void AddTextFileToTrace(string fullpath)
        {
            TextFileTraceAdapter adapter = new(fullpath)
            {
                AutoGenFileEnable = false
            };

            AdapterTraceListener listener = new(adapter);
            Trace.Listeners.Add(listener);
        }

        /// <summary>
        /// Adds TextFileTraceAdapter to Trace.Listeners
        /// The filenames are automatically created every day
        /// Deletes outdated logfiles afer a preriod of days
        /// </summary>
        /// <param name="root">the base folder for the logfiles</param>
        /// <param name="deleteOldFilesAfterDays">number of days when the logfiles should be deleted</param>
        public static void AddTextFileToTrace(string? root, int deleteOldFilesAfterDays)
        {
            TextFileTraceAdapter adapter = new()
            {
                AutoGenFileEnable = true,
                AutoDeleteFilesAfterDays = deleteOldFilesAfterDays
            };

            if (!string.IsNullOrWhiteSpace(root))
            {
                adapter.AutoGenRootFolder = root!;
            }

            AdapterTraceListener listener = new(adapter);
            Trace.Listeners.Add(listener);
        }
    }
}
