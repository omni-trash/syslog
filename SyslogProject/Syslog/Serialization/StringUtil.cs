using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syslog.Serialization
{
    internal static class StringUtil
    {
        /// <summary>
        /// Returns a substring
        /// </summary>
        /// <param name="str">input</param>
        /// <param name="start">input start</param>
        /// <param name="end">input end</param>
        /// <returns></returns>
        public static string Range(string str, int start, int end)
            => str.Substring(start, end - start);
    }
}
