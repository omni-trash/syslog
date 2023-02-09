using System.Diagnostics;

namespace Syslog.Serialization
{
    internal abstract class LexerBase
    {
        protected string input;
        protected int index = 0;

        protected LexerBase(string input)
        {
            this.input = input;
        }

        /// <summary>
        /// Returns true if end of input reached
        /// </summary>
        /// <returns></returns>
        protected bool IsEndOfInput()
        {
            return index >= input.Length;
        }

        /// <summary>
        /// Returns the token at current index
        /// </summary>
        /// <returns></returns>
        protected char Current()
        {
            return (index < input.Length ? input[index] : '\u0000');
        }

        /// <summary>
        /// Move to next index
        /// </summary>
        /// <exception cref="LexerError"></exception>
        protected void Next()
        {
            if (index < input.Length)
            {
                ++index;
                return;
            }

            ThrowIf(true);
        }

        /// <summary>
        /// Throws if condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <exception cref="LexerError"></exception>
        protected void ThrowIf(bool condition)
        {
            if (condition)
            {
                if (IsEndOfInput())
                {
                    throw LexerError.UnexpectedEndError(index);
                }

                throw LexerError.UnexpectedTokenError(Current(), index);
            }
        }

        /// <summary>
        /// Loop while all conditions are true and move next each loop
        /// </summary>
        /// <param name="conditions"></param>
        protected void NextWhile(params Func<char, bool>[] conditions)
        {
            if (conditions.Length == 0)
            {
                Trace.TraceError("NextWhile with no conditions");
                return;
            }

            char current = Current();

            while (!IsEndOfInput() && conditions.All(pred => pred(current)))
            {
                Next();
                current = Current();
            }
        }
    }
}
