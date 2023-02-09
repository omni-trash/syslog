using Syslog.Transport;
using System.Diagnostics;
using System.Text;

namespace Syslog
{
    /// <summary>
    /// Simple Syslog Client to send RFC 5424 messages
    /// </summary>
    public class SyslogClient
    {
        readonly AsyncBufferSender buffer;

        bool closed = false;

        /// <summary>
        /// Number of pending messages to hold, before removing
        /// messages on overlow to keep the latest in the queue.
        /// The default value is 50.
        /// </summary>
        public int PendingLimit
        {
            get => buffer.PendingLimit;
            set => buffer.PendingLimit = value;
        }

        /// <summary>
        /// Returns true when the syslog client is stop sending any messages
        /// </summary>
        public bool Closed => closed;

        /// <summary>
        /// OnError Event, you can reopen after a period of time through <see cref="Reset"/>
        /// </summary>
        public event EventHandler<Exception>? OnError;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="sender">MessageSender to use</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SyslogClient(IMessageSender sender)
        {
            buffer = new(sender);
            buffer.OnError += Buffer_OnError;

            // warm up System.Net for net47
            NetworkUtil.TryGetAddress("localhost");
        }

        /// <summary>
        /// Tries to open the transport to ensure network is up
        /// and error messages are written to console.
        /// </summary>
        public void Open()
        {
            buffer.TryOpen();
        }

        /// <summary>
        /// Buffer OnError Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Buffer_OnError(object? sender, Exception e)
        {
            Close();
            Trace.TraceError($"{e}");
            OnError?.Invoke(this, e);
        }

        /// <summary>
        /// Send Text with Severity
        /// </summary>
        /// <param name="text"></param>
        /// <param name="severity"></param>
        public void SendText(string? text, Severity severity)
        {
            SendMessage(SyslogMessage.CreateClientMessage(text, severity));
        }

        /// <summary>
        /// Send Informational Message
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string? text)
        {
            SendText(text, Severity.Informational);
        }

        /// <summary>
        /// Send Warning Message
        /// </summary>
        /// <param name="text"></param>
        public void SendWarning(string? text)
        {
            SendText(text, Severity.Warning);
        }

        /// <summary>
        /// Send Error Message
        /// </summary>
        /// <param name="text"></param>
        public void SendError(string? text)
        {
            SendText(text, Severity.Error);
        }

        /// <summary>
        /// Sends the syslog message as is
        /// </summary>
        /// <param name="message"></param>
        private void SendMessage(SyslogMessage message)
        {
            if (closed)
            {
                return;
            }

            string? payload = MessageConverter.ToRFC5424String(message);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return;
            }

            buffer.SendData(Encoding.UTF8.GetBytes(payload));
        }

        /// <summary>
        /// Close syslog client
        /// </summary>
        public void Close()
        {
            closed = true;
            buffer.Close();
        }

        /// <summary>
        /// Resets the operational state
        /// </summary>
        public void Reset()
        {
            buffer.Reset();
            closed = false;
        }
    }
}
