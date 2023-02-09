
namespace Syslog.Transport
{
    /// <summary>
    /// Message Sender for <see cref="SyslogClient"/>
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Open sender
        /// </summary>
        void Open();

        /// <summary>
        /// Close sender
        /// </summary>
        void Close();

        /// <summary>
        /// Send a message to remote host
        /// </summary>
        /// <param name="payload"></param>
        void Send(byte[] payload);
    }
}
