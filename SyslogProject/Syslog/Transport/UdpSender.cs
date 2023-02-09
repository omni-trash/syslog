using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Syslog.Transport
{
    /// <summary>
    /// Transport UDP
    /// </summary>
    /// <seealso cref="SyslogClient"/>
    /// <seealso cref="SyslogTraceAdapter"/>
    public class UdpSender : IMessageSender
    {
        // RFC 5426
        // Syslog senders MUST support sending syslog message datagrams
        // to the UDP port 514, but MAY be configurable to send messages to a
        // different port.
        public const int SYSLOG_UPD_REMOTE_PORT = 514;

        // client to use
        UdpClient? client;

        /// <summary>
        /// Remote host to use
        /// </summary>
        public string RemoteHost { get; set; }

        /// <summary>
        /// Remote port to use
        /// </summary>
        public int RemotePort { get; set; }

        /// <summary>
        /// True if client is connected to remote host
        /// </summary>
        public bool Connected => this.client?.Client?.Connected == true;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="remoteHost"></param>
        public UdpSender(string remoteHost)
            : this(remoteHost, SYSLOG_UPD_REMOTE_PORT)
        {
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="remoteHost"></param>
        /// <param name="remotePort"></param>
        public UdpSender(string remoteHost, int remotePort)
        {
            this.RemoteHost = remoteHost;
            this.RemotePort = remotePort;
        }

        /// <summary>
        /// Opens the client connection to remote host
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="UdpSenderError"></exception>
        public void Open()
        {
            if (Connected)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(RemoteHost))
            {
                throw UdpSenderError.RemoteHostEmptyError();
            }

            if (!NetworkUtil.IsValidPort(RemotePort))
            {
                throw UdpSenderError.PortRangeError(RemotePort);
            }

            IPAddress? address = NetworkUtil.TryGetAddress(RemoteHost);

            if (address == null)
            {
                throw UdpSenderError.AddressError(RemoteHost);
            }

            IPEndPoint endpoint = new(address, RemotePort);

            UdpClient udp = new()
            {
                EnableBroadcast = true
            };

            udp.Connect(endpoint);
            this.client = udp;
        }

        /// <summary>
        /// Sends the payload to the connected target
        /// </summary>
        /// <param name="payload"></param>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="UdpSenderError"></exception>
        public void Send(byte[] payload)
        {
            if (payload == null)
            {
                return;
            }

            if (payload.Length == 0)
            {
                return;
            }

            if (!Connected)
            {
                throw UdpSenderError.NotConnectedError();
            }

            client!.Send(payload, payload.Length);
        }

        /// <summary>
        /// Close connection
        /// </summary>
        public void Close()
        {
            // shutdown
            client?.Close();
            client = null;
        }
    }
}
