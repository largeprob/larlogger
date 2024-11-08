using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarLogger
{
    public interface ILarLoggerFactory
    {
        /// <summary>
        ///  创建<see cref="ILogger"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILogger CreateLogger<T>();
    }
}
