using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace Syslog
{
    public static class NetworkUtil
    {
        /// <summary>
        /// Resolves IP or Hostname
        /// </summary>
        /// <param name="hostname">IP or hostname to resolve</param>
        /// <returns></returns>
        public static IPAddress? TryGetAddress(string hostname)
        {
            try
            {
                return GetAddress(hostname);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resolves IP or Hostname
        /// </summary>
        /// <param name="hostname">IP or hostname to resolve</param>
        /// <returns></returns>
        /// <exception cref="SocketException"></exception>
        public static IPAddress? GetAddress(string hostname)
        {
            // check predefined names
            switch (hostname.Trim().ToLowerInvariant())
            {
                case "":
                    return null;
                case "$loopback":
                    // local loopback adapter
                    // - 127.0.0.1
                    return IPAddress.Loopback;
                case "$broadcast":
                    // local broadcast
                    // - not routed
                    // - valid in broadcast domain
                    // - 255.255.255.255
                    return IPAddress.Broadcast;
                case "$subnet":
                    // directed broadcast
                    // - routed to foreign networks
                    // - 192.168.2.255
                    return GetSubnetAddress();
            }

            // ok now find out the IP adress of hostname to use
            var addresses = Dns.GetHostAddresses(hostname);

            IPAddress? remoteAddr = null;

            if (remoteAddr == null && Socket.OSSupportsIPv4)
            {
                remoteAddr = addresses.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);
            }

            if (remoteAddr == null && Socket.OSSupportsIPv6)
            {
                remoteAddr = addresses.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetworkV6);
            }

            return remoteAddr;
        }

        /// <summary>
        /// Get the broadcast IP of local subnet
        /// </summary>
        /// <returns></returns>
        private static IPAddress? GetSubnetAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in interfaces)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                {
                    // is not active
                    continue;
                }

                if (adapter.IsReceiveOnly)
                {
                    // we need to send
                    continue;
                }

                IPInterfaceProperties props = adapter.GetIPProperties();

                IPAddress? gateway = props.GatewayAddresses
                    .Select(inf => inf.Address)
                    .FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);

                if (gateway == null)
                {
                    // has no gateway
                    continue;
                }

                foreach (UnicastIPAddressInformation unicast in props.UnicastAddresses)
                {
                    if (unicast.IsTransient)
                    {
                        // is a cluster
                        continue;
                    }

                    if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        // is not IPv4
                        continue;
                    }

                    if (unicast.PrefixOrigin == PrefixOrigin.WellKnown)
                    {
                        // do not
                        continue;
                    }

                    if (unicast.SuffixOrigin == SuffixOrigin.WellKnown)
                    {
                        // do not
                        continue;
                    }

#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    // 192.168.2.0 + 255.255.255.0 -> 192.168.2.255
                    UInt32 addr = (UInt32)unicast.Address.Address;
                    UInt32 mask = (UInt32)unicast.IPv4Mask.Address;
                    UInt32 subn = addr & mask | ~mask;
                    return new IPAddress(subn);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                }
            }

            return null;
        }

        /// <summary>
        /// True if port is in supported range
        /// </summary>
        /// <see cref="UdpClient.Connect"/>
        /// <see cref="ValidationHelper.ValidateTcpPort"/>
        /// <param name="port">Port number to check</param>
        /// <returns></returns>
        public static bool IsValidPort(int port)
        {
            return port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort;
        }
    }
}
