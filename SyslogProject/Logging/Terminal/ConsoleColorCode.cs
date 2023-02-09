using System.Text.RegularExpressions;

namespace Logging.Terminal
{
    /// <summary>
    /// ANSI Color Codes with 16 colors we did support
    /// </summary>
    public static class ConsoleColorCode
    {
        public static object SyncRoot { get; } = new object();

        // Select Foreground Color
        // Here it means use the foreground color
        // which was active when entered WriteToConsole
        public const string Default         = "\u001b[38m";

        // Foreground
        public const string Black           = "\u001b[30m";
        public const string DarkRed         = "\u001b[31m";
        public const string DarkGreen       = "\u001b[32m";
        public const string DarkYellow      = "\u001b[33m";
        public const string DarkBlue        = "\u001b[34m";
        public const string DarkMagenta     = "\u001b[35m";
        public const string DarkCyan        = "\u001b[36m";
        public const string Gray            = "\u001b[37m";
        public const string DarkGray        = "\u001b[90m";
        public const string Red             = "\u001b[91m";
        public const string Green           = "\u001b[92m";
        public const string Yellow          = "\u001b[93m";
        public const string Blue            = "\u001b[94m";
        public const string Magenta         = "\u001b[95m";
        public const string Cyan            = "\u001b[96m";
        public const string White           = "\u001b[97m";

        // Select Background Color
        // Here it means use the background color
        // which was active when entered WriteToConsole
        public const string DefaultBack     = "\u001b[48m";

        // Background
        public const string BlackBack       = "\u001b[40m";
        public const string DarkRedBack     = "\u001b[41m";
        public const string DarkGreenBack   = "\u001b[42m";
        public const string DarkYellowBack  = "\u001b[43m";
        public const string DarkBlueBack    = "\u001b[44m";
        public const string DarkMagentaBack = "\u001b[45m";
        public const string DarkCyanBack    = "\u001b[46m";
        public const string GrayBack        = "\u001b[47m";
        public const string DarkGrayBack    = "\u001b[100m";
        public const string RedBack         = "\u001b[101m";
        public const string GreenBack       = "\u001b[102m";
        public const string YellowBack      = "\u001b[103m";
        public const string BlueBack        = "\u001b[104m";
        public const string MagentaBack     = "\u001b[105m";
        public const string CyanBack        = "\u001b[106m";
        public const string WhiteBack       = "\u001b[107m";

        // Split on Color Code or Linebreak but do not remove
        private static Regex SplitPattern { get; } = new Regex(@"(\u001b\[\d+m|\n)");

        // Foreground to ConsoleColor
        static readonly Dictionary<string, ConsoleColor> FgColorCodeConsoleColorMap = new()
        {
            { Black,          ConsoleColor.Black },
            { DarkBlue,       ConsoleColor.DarkBlue },
            { DarkGreen,      ConsoleColor.DarkGreen },
            { DarkCyan,       ConsoleColor.DarkCyan },
            { DarkRed,        ConsoleColor.DarkRed },
            { DarkMagenta,    ConsoleColor.DarkMagenta },
            { DarkYellow,     ConsoleColor.DarkYellow },
            { Gray,           ConsoleColor.Gray },
            { DarkGray,       ConsoleColor.DarkGray },
            { Blue,           ConsoleColor.Blue },
            { Green,          ConsoleColor.Green },
            { Cyan,           ConsoleColor.Cyan },
            { Red,            ConsoleColor.Red },
            { Magenta,        ConsoleColor.Magenta },
            { Yellow,         ConsoleColor.Yellow },
            { White,          ConsoleColor.White }
        };

        // Background to ConsoleColor
        static readonly Dictionary<string, ConsoleColor> BgColorCodeConsoleColorMap = new()
        {
            { BlackBack,       ConsoleColor.Black },
            { DarkBlueBack,    ConsoleColor.DarkBlue },
            { DarkGreenBack,   ConsoleColor.DarkGreen },
            { DarkCyanBack,    ConsoleColor.DarkCyan },
            { DarkRedBack,     ConsoleColor.DarkRed },
            { DarkMagentaBack, ConsoleColor.DarkMagenta },
            { DarkYellowBack,  ConsoleColor.DarkYellow },
            { GrayBack,        ConsoleColor.Gray },
            { DarkGrayBack,    ConsoleColor.DarkGray },
            { BlueBack,        ConsoleColor.Blue },
            { GreenBack,       ConsoleColor.Green },
            { CyanBack,        ConsoleColor.Cyan },
            { RedBack,         ConsoleColor.Red },
            { MagentaBack,     ConsoleColor.Magenta },
            { YellowBack,      ConsoleColor.Yellow },
            { WhiteBack,       ConsoleColor.White }
        };

        /// <summary>
        /// Detect color code and set the console color
        /// </summary>
        /// <param name="color">ANSI Color Code</param>
        /// <param name="foregroundDefault"></param>
        /// <param name="backgroundDefault"></param>
        /// <returns>true if color was set</returns>
        private static bool SetConsoleColor(string color, ConsoleColor foregroundDefault, ConsoleColor backgroundDefault)
        {
            if (color.Length == 0)
            {
                return false;
            }

            if (color[0] != '\u001b')
            {
                return false;
            }

            if (color == Default)
            {
                Console.ForegroundColor = foregroundDefault;
                return true;
            }

            if (color == DefaultBack)
            {
                Console.BackgroundColor = backgroundDefault;
                return true;
            }

            ConsoleColor consoleColor;

            if (FgColorCodeConsoleColorMap.TryGetValue(color, out consoleColor))
            {
                Console.ForegroundColor = consoleColor;
                return true;
            }

            if (BgColorCodeConsoleColorMap.TryGetValue(color, out consoleColor))
            {
                Console.BackgroundColor = consoleColor;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes the text token to console window
        /// </summary>
        /// <param name="token">text token to write</param>
        /// <param name="backgroundDefault"></param>
        private static void WriteTextToken(string token, ConsoleColor backgroundDefault)
        {
            // handle linebreak
            if (token == "\n")
            {
                ConsoleColor backgroundCurrent = Console.BackgroundColor;

                // when we write linebreaks the new line will be filled out
                // with the current background color.
                //
                // to prevent that behaviour we will set the background to
                // the color which was set when WriteToConsole was called.
                //
                // that is a visual issue.
                //
                // TODO: same problem when resize the console window ... solution?

                Console.BackgroundColor = backgroundDefault;
                Console.Write(token);
                Console.BackgroundColor = backgroundCurrent;
                return;
            }

            // write to console as is
            Console.Write(token);
        }

        /// <summary>
        /// Windows Console dont have yet full ANSI Color Code support, so we have to workaround
        /// </summary>
        /// <param name="args">messages and colors</param>
        public static void WriteToConsole(params object?[] args)
        {
            lock (SyncRoot)
            {
                // save
                ConsoleColor foregroundDefault = Console.ForegroundColor;
                ConsoleColor backgroundDefault = Console.BackgroundColor;

                // each parameter
                foreach (object? arg in args)
                {
                    // need a string
                    string? input = arg?.ToString();

                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }

                    // tokenize into text, color code and linebreaks
                    string[] tokens = SplitPattern.Split(input);

                    foreach (string token in tokens)
                    {
                        if (token.Length == 0)
                        {
                            continue;
                        }

                        if (SetConsoleColor(token, foregroundDefault, backgroundDefault))
                        {
                            continue;
                        }

                        WriteTextToken(token, backgroundDefault);
                    }
                }

                // restore
                Console.ForegroundColor = foregroundDefault;
                Console.BackgroundColor = backgroundDefault;
            }
        }
    }
}
