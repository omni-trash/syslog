
namespace Logging.Terminal
{
    /// <summary>
    /// 
    /// </summary>
    public class TextBuilder
    {
        readonly List<object?[]> list = new();

        /// <summary>
        /// Adds some objects to the text builder list
        /// </summary>
        /// <param name="args"></param>
        public void Add(params object?[] args)
        {
            list.Add(args);
        }

        /// <summary>
        /// Adds color codes to restore the foreground and background color
        /// </summary>
        /// <remarks>
        /// That is not the same as <see cref="Console.ResetColor"/> which resets to default colors.
        /// <see cref="ConsoleColorCode.Default"/> and <see cref="ConsoleColorCode.DefaultBack"/> uses the
        /// <see cref="Console.ForegroundColor"/> and <see cref="Console.BackgroundColor"/>
        /// when <see cref="ConsoleColorCode.WriteToConsole"/> was called.
        /// </remarks>
        public void RestoreColor()
        {
            Add(ConsoleColorCode.Default, ConsoleColorCode.DefaultBack);
        }

        /// <summary>
        /// Adds some objects to the text builder list and then adds <see cref="RestoreColor"/> 
        /// to restore to the colors when <see cref="ConsoleColorCode.WriteToConsole"/> was called
        /// </summary>
        /// <param name="args"></param>
        public void AddSection(params object?[] args)
        {
            list.Add(args);
            RestoreColor();
        }

        public object?[] ToArray()
        {
            return list.SelectMany(item => item).ToArray();
        }

        public override string ToString()
        {
            return string.Concat(ToArray());
        }
    }
}
