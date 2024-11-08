using LarLogger.Provider;
using Microsoft.Extensions.Options;
using System;

namespace LarLogger.Output.Base
{
    public abstract class OutLogger
    {
        public readonly LarLoggerType Name;

        //用来接受/释放配置文件更新监听器
        private readonly IDisposable _onChangeToken;
        //用来接受/释放配置文件
        protected LarLoggerOptions _loggerOptions;

        public OutLogger(IOptionsMonitor<LarLoggerOptions> loggerOptions, LarLoggerType name)
        {
            _loggerOptions = loggerOptions.CurrentValue;
            _onChangeToken = loggerOptions.OnChange(updatedConfig =>
               _loggerOptions = updatedConfig
            );
            Name = name;
        }

        protected string PatternLevel = @"\@\<[\d]+\>";
        protected string GetLogLevelStr(string input)
        {
            var result = string.Empty;
            switch (input)
            {
                case "@<0>":
                    result = "Trac";
                    break;
                case "@<1>":
                    result = "Dbug";
                    break;
                case "@<2>":
                    result = "Info";
                    break;
                case "@<3>":
                    result = "Warn";
                    break;
                case "@<4>":
                    result = "Fail";
                    break;
                case "@<5>":
                    result = "Crit";
                    break;
                case "@<6>":
                    result = "None";
                    break;
                default:
                    result = "NULL";
                    break;
            }
            return result;
        }

        public abstract void WaitLog(string message);

    }
}
