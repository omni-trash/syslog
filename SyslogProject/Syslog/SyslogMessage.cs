
namespace Syslog
{
    public enum Facility
    {
        /// <summary>
        /// kernel messages
        /// </summary>
        Kernel = 0,

        /// <summary>
        /// user-level messages
        /// </summary>
        User = 1,

        /// <summary>
        /// mail system
        /// </summary>
        Mail = 2,

        /// <summary>
        /// system daemons
        /// </summary>
        System = 3,

        /// <summary>
        /// security/authorization messages
        /// </summary>
        Auth = 4,

        /// <summary>
        /// internal syslog messages
        /// </summary>
        Syslog = 5,

        /// <summary>
        /// line printer subsystem
        /// </summary>
        Printer = 6,

        /// <summary>
        /// network news subsystem
        /// </summary>
        News = 7,

        /// <summary>
        /// UUCP subsystem
        /// </summary>
        UUCP = 8,

        /// <summary>
        /// clock daemon
        /// </summary>
        Clock = 9,

        /// <summary>
        /// security/authorization messages
        /// </summary>
        AuthPriv = 10,

        /// <summary>
        /// FTP damon
        /// </summary>
        FTP = 11,

        /// <summary>
        /// NTP subsystem
        /// </summary>
        NTP = 12,

        /// <summary>
        ///  log audit
        /// </summary>
        Audit = 13,

        /// <summary>
        ///  log alert
        /// </summary>
        Alert = 14,

        /// <summary>
        /// clock daemon (note 2)
        /// </summary>
        Clock15 = 15,

        //
        // local use
        //

        Local0 = 16,
        Local1 = 17,
        Local2 = 18,
        Local3 = 19,
        Local4 = 20,
        Local5 = 21,
        Local6 = 22,
        Local7 = 23
    }

    public enum Severity
    {
        Emergency = 0,
        Alert = 1,
        Critical = 2,
        Error = 3,
        Warning = 4,
        Notice = 5,
        Informational = 6,
        Debug = 7
    }

    /// <summary>
    /// Syslog Message to send
    /// </summary>
    public class SyslogMessage
    {
        public Facility FACILITY { get; set; }
        public Severity SEVERITY { get; set; }
        public int PRIORITY => ((int)FACILITY << 3) + (int)SEVERITY;
        public int VERSION { get; } = 1;
        public DateTimeOffset? TIMESTAMP { get; set; }
        public string? HOSTNAME { get; set; }
        public string? APPNAME { get; set; }
        public string? PROCID { get; set; } 
        public string? MSGID { get; set; }
        public IDictionary<string, IDictionary<string, string>?>? SDATA { get; set; }
        public string? MSG { get; set; }

        public override string ToString()
        {
            return MessageConverter.ToRFC5424String(this);
        }

        // Environment.ProcessId
        private static readonly int ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

        /// <summary>
        /// Creates prefilled client syslog message
        /// </summary>
        /// <returns></returns>
        public static SyslogMessage CreateClientMessage()
        {
            return new SyslogMessage
            {
                FACILITY  = Facility.User,
                SEVERITY  = Severity.Informational,
                TIMESTAMP = DateTimeOffset.Now,
                HOSTNAME  = Environment.MachineName,
                APPNAME   = AppDomain.CurrentDomain.FriendlyName,
                PROCID    = $"{ProcessId}"
            };
        }

        /// <summary>
        /// Creates prefilled client syslog message with MSG field
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static SyslogMessage CreateClientMessage(string? msg)
        {
            var message = CreateClientMessage();
            message.MSG = msg;
            return message;
        }

        /// <summary>
        /// Creates prefilled client syslog message with MSG and SEVERITY field
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        public static SyslogMessage CreateClientMessage(string? msg, Severity severity)
        {
            var message = CreateClientMessage(msg);
            message.SEVERITY = severity;
            return message;
        }
    }
}
