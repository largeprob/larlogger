using LarLogger.Output.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Options;
using System;

namespace LarLogger.Output
{
    public class HttpLogger : OutLogger, IDisposable
    {
        public HttpLogger(IOptionsMonitor<LarLoggerOptions> loggerOptions) : base(loggerOptions, LarLoggerType.Http)
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException("尚未实现");
        }

        public override void WaitLog(string message)
        {
            throw new NotImplementedException("尚未实现");
        }
    }
}
