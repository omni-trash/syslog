
namespace Syslog.Transport
{
    /// <summary>
    /// Erros for <see cref="UdpSender.OnError"/> event
    /// </summary>
    public class UdpSenderError : Exception
    {
        public enum ErrorCodeEnum
        {
            NotConnectedError,
            AddressError,
            PortRangeError,
            RemoteHostEmptyError
        };

        public ErrorCodeEnum ErrorCode { get; private set; }

        private struct ErrorMessage
        {
            public const string NotConnectedError       = "The client is not connected to remote host";
            public const string AddressErrorMessage     = "Unable to resolve '{0}'";
            public const string PortRangeErrorMessage   = "Port {0} is out of range";
            public const string RemoteHostEmptyMessage  = "The remote hostname is empty";
        }

        private UdpSenderError(string? message)
            : base(message)
        {

        }

        private UdpSenderError(string? message, Exception? innerExcepötion)
            : base(message, innerExcepötion)
        {
        }

        /// <summary>
        /// Not connected to remote host
        /// </summary>
        /// <returns></returns>
        public static UdpSenderError NotConnectedError() =>
            new(ErrorMessage.NotConnectedError)
            {
                ErrorCode = ErrorCodeEnum.NotConnectedError
            };

        /// <summary>
        /// No address
        /// </summary>
        /// <returns></returns>
        public static UdpSenderError AddressError(string host) =>
            new(string.Format(ErrorMessage.AddressErrorMessage, host))
            {
                ErrorCode = ErrorCodeEnum.AddressError
            };

        /// <summary>
        /// Port out of range
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static UdpSenderError PortRangeError(int port) =>
            new(string.Format(ErrorMessage.PortRangeErrorMessage, port))
            {
                ErrorCode = ErrorCodeEnum.PortRangeError
            };

        /// <summary>
        /// Remote host was not set
        /// </summary>
        /// <returns></returns>
        public static UdpSenderError RemoteHostEmptyError() =>
            new(ErrorMessage.RemoteHostEmptyMessage)
            {
                ErrorCode = ErrorCodeEnum.RemoteHostEmptyError
            };
    }
}
