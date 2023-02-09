using Logging.Terminal;
using System.Diagnostics;
using System.Text;

namespace Logging.Tracing.Adapters
{
    /// <summary>
    /// Writes trace messages to text file
    /// </summary>
    public class TextFileTraceAdapter : ITraceAdapter
    {
        // text writer to use
        TextWriter? writer = null;

        // curren logfile
        string? fullpath;

        /// <summary>
        /// Current logfile
        /// </summary>
        public string? Logfile => fullpath;

        //
        // Auto generated filename
        //

        /// <summary>
        /// Is true when the adapter should create a filename automatically
        /// </summary>
        public bool AutoGenFileEnable { get; set; } = false;

        /// <summary>
        /// The base folder for automatically created files.
        /// <see cref="AutoGenFileEnable"/> must be set to true.
        /// </summary>
        public string AutoGenRootFolder { get; set; } = "~/Logs";

        /// <summary>
        /// Deletes automatically created logfiles when expired.
        /// <see cref="AutoGenFileEnable"/> must be set to true.
        /// </summary>
        public int AutoDeleteFilesAfterDays { get; set; } = 7;

        // next time to generate a filename
        DateTime? ensureLogfileNext = null;

        /// <summary>
        /// Initializes a new instance, where <see cref="AutoGenFileEnable"/> is set to true
        /// </summary>
        public TextFileTraceAdapter()
        {
            this.AutoGenFileEnable = true;
        }

        /// <summary>
        /// Initializes a new instance, which does not create filenames and does not delelete automatically
        /// </summary>
        /// <param name="fullpath"></param>
        public TextFileTraceAdapter(string fullpath)
        {
            this.fullpath = fullpath;
        }

        /// <summary>
        /// Writes a message to the text file
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="message">message to write</param>
        public void WriteLine(TraceEventType eventType, string message)
        {
            try
            {
                EnsureFile();

                if (!EnsureWriter())
                {
                    return;
                }

                string cat;

                switch (eventType)
                {
                    case TraceEventType.Critical:
                    case TraceEventType.Error:
                        cat = "ERR";
                        break;
                    case TraceEventType.Warning:
                        cat = "WRN";
                        break;
                    default:
                        cat = "INF";
                        break;
                }

                string messageToWrite = TextUtil.EscapeControls(message)!;

                writer?.WriteLine($"[{DateTime.Now:HH:mm:ss}] {cat}: {messageToWrite}");
                writer?.Flush();
            }
            catch (Exception error)
            {
                Close();
                Trace.TraceError($"{error}");
            }
        }

        /// <summary>
        /// Close the text writer
        /// </summary>
        public void Close()
        {
            try
            {
                writer?.Flush();
                writer?.Close();
            }
            catch
            {
                // whatever
                // maybe Trace.Close -> AdapterTraceListener.Close -> TextFileTraceAdapter.Close
                // so dont Trace.WriteError here
            }

            writer   = null;
            fullpath = null;
        }

        /// <summary>
        /// Creates a filename if required, delete outdated logfiles, only valid when <see cref="AutoGenFileEnable"/> was set to true
        /// </summary>
        public void EnsureFile()
        {
            if (!AutoGenFileEnable)
            {
                // fullpath as is
                return;
            }

            //
            // auto generate filename
            //

            if (ensureLogfileNext != null && ensureLogfileNext > DateTime.Now)
            {
                // no need
                return;
            }

            // tomorrow
            DateTime nextDate = DateTime.Now.AddDays(1);
            ensureLogfileNext = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, 0, 0, 0);

            //
            // prepare appname
            //

            string friendlyName   = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            char[] invalFileChars = Path.GetInvalidFileNameChars();
            char[] invalPathChars = Path.GetInvalidPathChars();

            // cleanup friendlyName
            string appnameToUse = string.Concat(
                    friendlyName.ToCharArray()
                    .Where(c => !invalFileChars.Contains(c))
                    .Where(c => !invalPathChars.Contains(c)));

            appnameToUse = appnameToUse.Replace(' ', '_');

            // create a filename from guid if empty
            if (string.IsNullOrWhiteSpace(appnameToUse))
            {
                appnameToUse = Guid.NewGuid().ToString();
            }

            // <PC>_<APPNAME>
            appnameToUse  = $"{Environment.MachineName}_{appnameToUse}";

            //
            // prepare folder and filename
            //

            string folder = this.AutoGenRootFolder;
            string file   = $"{appnameToUse}_{DateTime.Now:yyyyMMdd}.log";

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "~/Logs";
            }

            if (folder.StartsWith("~/"))
            {
                // Good choice for IIS Web Application and Windows Service
                // Should be the folder of running app
                string location = AppDomain.CurrentDomain.BaseDirectory;

                if (string.IsNullOrWhiteSpace(location))
                {
                    // Fallback
                    location = Environment.CurrentDirectory;
                }

                folder = Path.Combine(location, folder.Substring(2));
            }

            Directory.CreateDirectory(folder);

            // swap file
            Close();
            fullpath = Path.Combine(folder, file);
            EnsureWriter();

            // delete outdated logfiles async
            if (AutoDeleteFilesAfterDays > 0)
            {
                Task.Run(() =>
                {
                    new DirectoryInfo(folder)
                        .GetFiles($"{appnameToUse}_*.log", SearchOption.TopDirectoryOnly)
                        .Where(file => file.CreationTime.AddDays(AutoDeleteFilesAfterDays) < DateTime.Now)
                        .ToList()
                        .ForEach(file =>
                        {
                            try
                            {
                                file.Delete();
                                Trace.TraceInformation($"log file deleted {file.FullName}");
                            }
                            catch (Exception error)
                            {
                                Trace.TraceError($"{error}");
                            }
                        });
                });
            }
        }

        /// <summary>
        /// Create a text writer if required
        /// </summary>
        /// <returns></returns>
        bool EnsureWriter()
        {
            if (writer != null)
            {
                return true;
            }

            if (fullpath == null)
            {
                return false;
            }

            try
            {
                writer = new StreamWriter(fullpath, append: true, encoding: Encoding.UTF8);
                return true;
            }
            catch (Exception error)
            {
                Close();
                Trace.TraceError($"{error}");
            }

            return false;
        }
    }
}
