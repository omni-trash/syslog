
namespace Syslog.Serialization
{
    /// <summary>
    /// Defines
    /// </summary>
    internal static class RFC5424
    {
        /// <summary>
        /// Empty Header Value
        /// </summary>
        public const char NILVALUE_CHAR = '-';

        /// <summary>
        /// Empty Header Value
        /// </summary>
        public const string NILVALUE_STRING = "-";

        /// <summary>
        /// Format to render TIMESTAMP
        /// </summary>
        public const string TIMESTAMP_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffK";

        /// <summary>
        /// SD-NAME = 1*32PRINTUSASCII ; except '=', SP, ']', %d34 (")
        /// </summary>
        static readonly char[] SD_NAME_RESERVED_CHARS = { '=', ' ', ']', '\"' };

        /// <summary>
        /// PARAM-VALUE = UTF-8-STRING ; characters '"', '\' and; ']' MUST be escaped.
        /// </summary>
        static readonly char[] SD_PARAM_VALUE_ESCAPE_CHARS = { '\"', '\\', ']' };

        public const int MAXLEN_HOSTNAME = 255;
        public const int MAXLEN_APPNAME  = 48;
        public const int MAXLEN_PROCID   = 128;
        public const int MAXLEN_MSGID    = 32;
        public const int MAXLEN_SDNAME   = 32;

        /// <summary>
        /// NILVALUE = "-"
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_NILVALUE(char c)
            => (c == NILVALUE_CHAR);

        /// <summary>
        /// SP = %d32
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_SP(char c)
            => (c == ' ');

        /// <summary>
        /// NONZERO-DIGIT = %d49-57
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_NONZERODIGIT(char c)
            => "123456789".Contains(c);

        /// <summary>
        /// DIGIT = %d48 / NONZERO-DIGIT
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_DIGIT(char c) 
            => "0123456789".Contains(c);

        /// <summary>
        /// PRINTUSASCII = %d33-126
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_PRINTUSASCII(char c)
            => (c >= 33 && c <= 126);

        /// <summary>
        /// PRINTUSASCII ; except '=', SP, ']', %d34 (")
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_SD_NAME(char c)
            => (IS_PRINTUSASCII(c) && !SD_NAME_RESERVED_CHARS.Contains(c));

        /// <summary>
        /// PARAM-VALUE = UTF-8-STRING ; characters '"', '\' and; ']' MUST be escaped.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IS_SD_PARAM_VALUE_ESCAPE_CHAR(char c)
            => SD_PARAM_VALUE_ESCAPE_CHARS.Contains(c);
    }
}
