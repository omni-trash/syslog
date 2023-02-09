
namespace Syslog.Serialization
{
    /// <summary>
    /// Lexer Errors
    /// </summary>
    public class LexerError : Exception
    {
        public enum ErrorCodeEnum
        {
            UnexpectedEnd,
            UnexpectedToken
        }

        private struct ErrorMessage
        {
            public const string UnexpectedEndMessage   = "Unexpected end at index {0} ";
            public const string UnexpectedTokenMessage = "Unexpected token '{0}' at index {1}";
        }

        public ErrorCodeEnum ErrorCode { get; private set; }

        public int Index { get; private set; }

        private LexerError(string? message)
            : base(message)
        {

        }

        /// <summary>
        /// Unexpected EOI at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static LexerError UnexpectedEndError(int index) =>
            new(string.Format(ErrorMessage.UnexpectedEndMessage, index))
            {
                ErrorCode = ErrorCodeEnum.UnexpectedEnd,
                Index = index
            };

        /// <summary>
        /// Unexpected token at index
        /// </summary>
        /// <param name="token"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static LexerError UnexpectedTokenError(char token, int index) =>
            new(string.Format(ErrorMessage.UnexpectedTokenMessage, token, index))
            {
                ErrorCode = ErrorCodeEnum.UnexpectedToken,
                Index = index
            };
    }
}
