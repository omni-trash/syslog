# Syslog Client

The syslog client sends syslog messages (RFC 5424 UDP) to a syslog server.

The client use ``Trace.TraceError`` when an error occurs, so u have to add a ``TraceListener`` to see that messages.

```csharp
using Logging.Tracing;
using Syslog;
using Syslog.Transport;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // someone should trace to console so u can see error messages
            TraceUtil.AddConsoleColorCodeToTrace();

            UdpSender    sender = new("localhost");
            SyslogClient syslog = new (sender);

            // try to connect
            syslog.Open();

            syslog.SendText("That is an informational message");
            syslog.SendWarning("That is a warning message");
            syslog.SendError("That is an error message");

            // flush and close
            syslog.Close();
        }
    }
}
```

The fields of the syslog message are prefilled automatically.

```
FACILITY  = Facility.User,
SEVERITY  = Severity.Informational,
TIMESTAMP = DateTimeOffset.Now,
HOSTNAME  = Environment.MachineName,
APPNAME   = AppDomain.CurrentDomain.FriendlyName,
PROCID    = Environment.ProcessId
```

The syslog client stops sending any message on network error.

If u want to reconnect to the remote syslog server, u have to listen on the ``OnError`` event and then u have to ``Reset`` the syslog client.

```csharp
using Logging.Tracing;
using Syslog;
using Syslog.Transport;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // someone should trace to console so u can see error messages
            TraceUtil.AddConsoleColorCodeToTrace();

            UdpSender    sender = new("localhost");
            SyslogClient syslog = new (sender);

            syslog.OnError += Syslog_OnError;

            // try to connect
            syslog.Open();

            syslog.SendText("That is an informational message");
            syslog.SendWarning("That is a warning message");
            syslog.SendError("That is an error message");

            // flush and close
            syslog.Close();
        }

        private static void Syslog_OnError(object? sender, Exception e)
        {
            // Reset the syslog client after 15 minutes
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(15));
                (sender as SyslogClient)?.Reset();
            });
        }
    }
}
```
