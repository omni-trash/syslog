
namespace Logging.Terminal
{
    public static class TextUtil
    {
        /// <summary>
        /// Escapes control characters except CR, LF and HT
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string? EscapeControls(string? value)
        {
            return EscapeControls(value, EscapeAsHex);
        }

        /// <summary>
        /// Escapes control characters
        /// </summary>
        /// <param name="value">string with control characters</param>
        /// <param name="escape">escape control character</param>
        /// <returns></returns>
        public static string? EscapeControls(string? value, Func<char, string> escape)
        {
            if (value == null)
            {
                return null;
            }

            char[] chars = value.ToCharArray();

            var accepted = chars.Select(c =>
            {
                if (char.IsControl(c))
                {
                    return escape(c);
                }

                // BOM
                if (c == '\ufeff')
                {
                    return escape(c);
                }

                return $"{c}";
            }).ToArray();

            return string.Concat(accepted);
        }

        /// <summary>
        /// Escape control character except CR, LF and HT
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        internal static string EscapeAsHex(char control)
        {
            switch (control)
            {
                case '\r':
                case '\n':
                case '\t':
                    // we accept these controls
                    return $"{control}";
                default:
                    // show as hex number
                    return $@"\u{(int)control:x4}";
            }
        }
    }
}
