# Trace to File

If u want to write trace messages to a file u have to add a `TextWriterTraceListern` and u have to enable `TRACE`.

```csharp
using System.Diagnostics;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string root    = AppDomain.CurrentDomain.BaseDirectory;
            string logpath = Path.Combine(root, "Logs"); 
            string logfile = Path.Combine(logpath, "logfile.log");

            Trace.Listeners.Add(new TextWriterTraceListener(logfile));

            // ensure folder exists (!)
            Directory.CreateDirectory(logpath);

            Trace.TraceInformation("That is an informational message");
            Trace.TraceWarning("That is a warning message");
            Trace.TraceError("That is an error message");

            // flush and close (!)
            Trace.Close();
        }
    }
}
```

The trace messages are now written to the `logfile.log`.

```
ConsoleApp Information: 0 : That is an informational message
ConsoleApp Warning: 0 : That is a warning message
ConsoleApp Error: 0 : That is an error message
```

Now let us use the `TextFileTraceAdapter`. It will changes the output format.

```csharp
using Logging.Tracing.Adapters;
using Logging.Tracing;
using System.Diagnostics;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string root    = AppDomain.CurrentDomain.BaseDirectory;
            string logpath = Path.Combine(root, "Logs");
            string logfile = Path.Combine(logpath, "logfile.log");

            TextFileTraceAdapter adapter  = new(logfile);
            AdapterTraceListener listener = new(adapter);
            Trace.Listeners.Add(listener);

            // ensure folder exists (!)
            Directory.CreateDirectory(logpath);

            Trace.TraceInformation("That is an informational message");
            Trace.TraceWarning("That is a warning message");
            Trace.TraceError("That is an error message");

            Trace.Close();
        }
    }
}
```

The trace messages are now written to the `logfile.log`.

```
[10:09:15] INF: That is an informational message
[10:09:15] WRN: That is a warning message
[10:09:15] ERR: That is an error message
```

If u dont want to write that code manually u can use the `TraceUtil.AddTextFileToTrace`.

```csharp
using Logging.Tracing;
using System.Diagnostics;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string root    = AppDomain.CurrentDomain.BaseDirectory;
            string logpath = Path.Combine(root, "Logs");
            string logfile = Path.Combine(logpath, "logfile.log");

            TraceUtil.AddTextFileToTrace(logfile);

            // ensure folder exists (!)
            Directory.CreateDirectory(logpath);

            Trace.TraceInformation("That is an informational message");
            Trace.TraceWarning("That is a warning message");
            Trace.TraceError("That is an error message");

            Trace.Close();
        }
    }
}
```
> the log folder must exist

> the log file is growing

Now let us use the auto creation feature of the `TextFileTraceAdapter`.

```csharp
using Logging.Tracing.Adapters;
using Logging.Tracing;
using System.Diagnostics;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string root    = AppDomain.CurrentDomain.BaseDirectory;
            string logpath = Path.Combine(root, "Logs");

            // NOTE: no logfile argument (!)
            TextFileTraceAdapter adapter = new();

            // configure adapter for auto creation
            adapter.AutoGenFileEnable = true;
            adapter.AutoGenRootFolder = logpath;
            adapter.AutoDeleteFilesAfterDays = 7;

            AdapterTraceListener listener = new(adapter);
            Trace.Listeners.Add(listener);

            Trace.TraceInformation("That is an informational message");
            Trace.TraceWarning("That is a warning message");
            Trace.TraceError("That is an error message");

            Trace.Close();
        }
    }
}
```

> the log folder is created automatically

> the log file is created for every day

> logfiles older than 7 days will be deleted

The filename format is `<MachineName>_<AppName>_<yyyyMMdd>.log`.

``PC_ConsoleApp_20230211.log``

> the pattern to obtain the logfiles to delete is ``PC_ConsoleApp_*.log``

> if u dont want to delete logfiles automatically, set `adapter.AutoDeleteFilesAfterDays = -1`.

If u dont want to write that code manually u can use the `TraceUtil.AddTextFileToTrace`.

```csharp
using Logging.Tracing;
using System.Diagnostics;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TraceUtil.AddTextFileToTrace("~/Logs", 7);

            Trace.TraceInformation("That is an informational message");
            Trace.TraceWarning("That is a warning message");
            Trace.TraceError("That is an error message");

            Trace.Close();
        }
    }
}
```
