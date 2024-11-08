using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarLogger.Provider
{
    /// <summary>
    /// 此类的作用为了包装调用
    /// </summary>
    public class LarLoggerbuilder: ILarLoggerbuilder
    {
        public ILoggingBuilder _loggingBuilder { get; }

        public LarLoggerbuilder(ILoggingBuilder loggingBuilder)
        {
            _loggingBuilder = loggingBuilder;
        }
    }
}
