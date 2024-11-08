using LarLogger.Output.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Options;
using System;

namespace LarLogger.Output
{
    public class DataBaselogger : OutLogger, IDisposable
    {
        public DataBaselogger(IOptionsMonitor<LarLoggerOptions> loggerOptions) : base(loggerOptions, LarLoggerType.DataBase)
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
