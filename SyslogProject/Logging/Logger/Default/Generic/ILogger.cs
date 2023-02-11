using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Logger.Default.Generic
{
    /// <summary>
    /// Generic Default Logger interface used for DI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILogger<T> : ILogger
    {

    }
}
