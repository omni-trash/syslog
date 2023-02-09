using System.Text;

namespace Syslog.Serialization
{

    /// <summary>
    /// Converts <see cref="string"/> to <see cref="SyslogMessage"/>
    /// </summary>
    internal class RFC5424StringReader
    {
        /// <summary>
        /// Syslog message from payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="LexerError"></exception>
        public static SyslogMessage FromPayload(string? payload)
        {
            // C# 10 ArgumentNullException.ThrowIfNull(payload);
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            // Tokenizer
            var tokens = RFC5424Lexer.Tokenize(payload);

            SyslogMessage message = new();

            // Structured Data (not yet happy with that)
            // NOTE: the lexer ensures the correct order by design
            IDictionary<string, IDictionary<string, string>?>? sdata = null;
            IDictionary<string, string>? sd_param = null;
            string? sd_param_name = null;

            foreach (var token in tokens)
            {
                switch (token.Type_)
                {
                    case RFC5424TokenType.PRIVAL:
                        int prival   = int.Parse(token.Content);
                        int facility = prival >> 3;
                        int severity = prival  & 7;

                        message.FACILITY = (Facility)facility;
                        message.SEVERITY = (Severity)severity;
                        break;
                    case RFC5424TokenType.VERSION:
                        int version = int.Parse(token.Content);
                            
                        if (version != message.VERSION)
                        {
                            // incorrect version
                        }
                        break;
                    case RFC5424TokenType.TIMESTAMP:
                        message.TIMESTAMP = DateTimeOffset.Parse(token.Content);
                        break;
                    case RFC5424TokenType.HOSTNAME:
                        message.HOSTNAME = token.Content;
                        break;
                    case RFC5424TokenType.APPNAME:
                        message.APPNAME = token.Content;
                        break;
                    case RFC5424TokenType.PROCID:
                        message.PROCID = token.Content;
                        break;
                    case RFC5424TokenType.MSGID:
                        message.MSGID = token.Content;
                        break;
                    case RFC5424TokenType.MSG:
                        message.MSG = token.Content;
                        break;
                    case RFC5424TokenType.SDID:
                        string sd_id = token.Content;
                        sdata  ??= message.SDATA = new Dictionary<string, IDictionary<string, string>?>();
                        sd_param = sdata[sd_id]  = new Dictionary<string, string>();
                        break;
                    case RFC5424TokenType.PARAMNAME:
                        sd_param_name = token.Content;
                        break;
                    case RFC5424TokenType.PARAMVALUE:
                        if (sd_param      == null) throw new ArgumentNullException(nameof(sd_param));
                        if (sd_param_name == null) throw new ArgumentNullException(nameof(sd_param_name));

                        string sd_param_value   = UnescapeParamValue(token.Content);
                        sd_param[sd_param_name] = sd_param_value;
                        break;
                }
            }

            return message;
        }

        /// <summary>
        /// Unescape characters of PARAM-VALUE
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string UnescapeParamValue(string value)
        {
            StringBuilder builder = new();
            bool esc = false;

            foreach (var c in value)
            {
                if (esc)
                {
                    if (!RFC5424.IS_SD_PARAM_VALUE_ESCAPE_CHAR(c))
                    {
                        // wrong escape
                        builder.Append('\\');
                    }

                    esc = false;
                    builder.Append(c);
                    continue;
                }

                if (c == '\\')
                {
                    esc = true;
                    continue;
                }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
