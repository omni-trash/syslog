using System.Text;

namespace Syslog.Serialization
{
    /// <summary>
    /// Converts <see cref="SyslogMessage"/> to <see cref="string"/>
    /// </summary>
    internal class RFC5424StringWriter
    {
        /// <summary>
        /// Syslog message to payload
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToPayload(SyslogMessage? message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            int     PRIORITY    = message.PRIORITY;
            int     VERSION     = message.VERSION;
            string  TIMESTAMP   = NilvalueIfNullOrEmpty(message.TIMESTAMP?.ToString(RFC5424.TIMESTAMP_FORMAT));
            string  HOSTNAME    = HeaderString(message.HOSTNAME,  RFC5424.MAXLEN_HOSTNAME);
            string  APPNAME     = HeaderString(message.APPNAME,   RFC5424.MAXLEN_APPNAME);
            string  PROCID      = HeaderString(message.PROCID,    RFC5424.MAXLEN_PROCID);
            string  MSGID       = HeaderString(message.MSGID,     RFC5424.MAXLEN_MSGID);
            string  SDATA       = StructuredDataString(message.SDATA);
            string? MSG         = message.MSG;

            return $"<{PRIORITY}>{VERSION} {TIMESTAMP} {HOSTNAME} {APPNAME} {PROCID} {MSGID} {SDATA} {MSG}".Trim();
        }

        /// <summary>
        /// Ensures valid header values for HOSTNAME, APPNAME, PROCID or MSGID
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private static string HeaderString(string? text, int maxLength)
        {
            // HOSTNAME        = NILVALUE / 1*255PRINTUSASCII
            // APP-NAME        = NILVALUE / 1*48PRINTUSASCII
            // PROCID          = NILVALUE / 1*128PRINTUSASCII
            // MSGID           = NILVALUE / 1*32PRINTUSASCII

            text = PrintUsAsciiOnlyString(text);
            text = TruncateString(text, maxLength);
            text = NilvalueIfNullOrEmpty(text);

            return text;
        }

        /// <summary>
        /// NILVALUE when string is empty
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string NilvalueIfNullOrEmpty(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return RFC5424.NILVALUE_STRING;
            }

            return text!;
        }

        /// <summary>
        /// PRINTUSASCII %d33-126
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string? PrintUsAsciiOnlyString(string? text)
        {
            if (text == null)
            {
                return null;
            }

            return string.Concat(text.ToCharArray().Where(RFC5424.IS_PRINTUSASCII));
        }

        /// <summary>
        /// Limit string length
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private static string? TruncateString(string? text, int maxLength)
        {
            if (text == null)
            {
                return null;
            }

            return StringUtil.Range(text, 0, Math.Min(text.Length, maxLength));
        }

        /// <summary>
        /// Ensures valid string for SDID and PRAM-NAME
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string ParamNameString(string value)
        {
            // SD-ID           = SD-NAME
            // PARAM-NAME      = SD-NAME
            // SD-NAME         = 1*32PRINTUSASCII
            //                   ; except '=', SP, ']', %d34 (")

            string name = string.Concat(value.ToCharArray().Where(RFC5424.IS_SD_NAME));
            return TruncateString(name, RFC5424.MAXLEN_SDNAME)!;
        }

        /// <summary>
        /// Escapes characters for PARAM-VALUE
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string ParamValueString(string value)
        {
            // PARAM-VALUE     = UTF-8-STRING ; characters '"', '\' and
            //                                ; ']' MUST be escaped.

            StringBuilder builder = new();

            foreach (var c in value)
            {
                if (RFC5424.IS_SD_PARAM_VALUE_ESCAPE_CHAR(c))
                {
                    builder.Append('\\');
                }

                builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Formats the STRUCTURED-DATA Element
        /// </summary>
        /// <param name="sdata"></param>
        /// <returns></returns>
        private static string StructuredDataString(IDictionary<string, IDictionary<string, string>?>? sdata)
        {
            // STRUCTURED-DATA = NILVALUE / 1*SD-ELEMENT
            // SD-ELEMENT      = "[" SD-ID *(SP SD-PARAM) "]"
            // SD-PARAM        = PARAM-NAME "=" %d34 PARAM-VALUE %d34
            // SD-ID           = SD-NAME
            // PARAM-NAME      = SD-NAME
            // PARAM-VALUE     = UTF-8-STRING ; characters '"', '\' and
            //                                ; ']' MUST be escaped.
            // SD-NAME         = 1*32PRINTUSASCII
            //                   ; except '=', SP, ']', %d34 (")

            if (sdata == null)
            {
                return RFC5424.NILVALUE_STRING;
            }

            if (sdata.Count == 0)
            {
                return RFC5424.NILVALUE_STRING;
            }

            StringBuilder builder = new();

            foreach (var sd_element in sdata)
            {
                string sd_id = ParamNameString(sd_element.Key);

                builder.AppendFormat("[{0}", sd_id);

                // parameters are optional, so they can be null or empty
                if (sd_element.Value?.Count > 0)
                {
                    foreach (var sd_param in sd_element.Value)
                    {
                        string sd_param_name  = ParamNameString(sd_param.Key);
                        string sd_param_value = ParamValueString(sd_param.Value);

                        if (sd_param_name.Length > 0)
                        {
                            builder.AppendFormat(" {0}=\"{1}\"", sd_param_name, sd_param_value);
                        }
                    }
                }

                builder.Append(']');
            }

            return builder.ToString();
        }
    }
}
