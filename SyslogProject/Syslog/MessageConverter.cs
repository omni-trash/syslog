using Syslog.Serialization;

namespace Syslog
{
    /// <summary>
    /// Syslog message payload converter
    /// </summary>
    public static class MessageConverter
    {
        /// <summary>
        /// Syslog message to paylod
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToRFC5424String(SyslogMessage? message)
        {
            return RFC5424StringWriter.ToPayload(message);
        }

        /// <summary>
        /// Syslog message from payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="LexerError"></exception>
        public static SyslogMessage FromRFC5424String(string? payload)
        {
            return RFC5424StringReader.FromPayload(payload);
        }
    }
}
