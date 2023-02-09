
namespace Syslog.Serialization
{
    internal enum RFC5424TokenType
    {
        NILVALUE,
        SYSLOGMSG,
        HEADER,
        PRI,
        PRIVAL,
        VERSION,
        TIMESTAMP,
        HOSTNAME,
        APPNAME,
        PROCID,
        MSGID,
        STRUCTUREDDATA,
        MSG,

        // timestamp date
        FULLDATE,
        DATEFULLYEAR,
        DATEMONTH,
        DATEMDAY,

        // timestamp time
        FULLTIME,
        PARTIALTIME,
        TIMEHOUR,
        TIMEMINUTE,
        TIMESECOND,
        TIMESECFRAC,
        TIMEOFFSET,
        TIMENUMOFFSET,

        // structured data
        SDELEMENT,
        SDID,
        SDNAME,
        SDPARAM,
        PARAMNAME,
        PARAMVALUE,
    }

    internal class RFC5424Token
    {
        // Grammar
        public string Content { get; private set; }
        public RFC5424TokenType Type_ { get; private set; }

        private RFC5424Token(string content, RFC5424TokenType type_)
        {
            Content = content;
            Type_   = type_;
        }

        public static RFC5424Token Create(string content, RFC5424TokenType type) 
            => new(content, type);

        public override string ToString()
            => $"{Type_} \"{Content}\"";
    }
}
