using LarLogger.Format.Base;
using LarLogger.Output.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace LarLogger.Provider
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public sealed class LarLogger : ILogger
    {
        //作用域日志列表
        private ConcurrentStack<ThisLoggerScope> _stackInfo = new ConcurrentStack<ThisLoggerScope>();
        //作用域ID
        private string _scopeId = string.Empty;


        private readonly string _categoryName;
        private readonly Func<LarLoggerOptions> _getCurrentConfig;


        //日志格式化处理
        private readonly Formatter _loggerFormatter;

        /// <summary>
        /// ILogger是单例多线程访问时保证写入数据唯一
        /// </summary>
        /// <remarks>NULL</remarks>
        [ThreadStatic]
        private static StringWriter _stringWriter;

        //日志输出程序
        private readonly ConcurrentDictionary<LarLoggerType, OutLogger> _outLoggers;

        internal LarLogger(string categoryName,
            Func<LarLoggerOptions> currentConfig, Formatter loggerFormatter,
            ConcurrentDictionary<LarLoggerType, OutLogger> outLoggers
            )
        {
            Debug.WriteLine($"实例化-》{nameof(LarLogger)}");
            _categoryName = categoryName;
            _getCurrentConfig = currentConfig;
            _loggerFormatter = loggerFormatter;
            _outLoggers = outLoggers;
        }

    

        public bool IsEnabled(LogLevel logLevel)
        {
            var loggerOptions = _getCurrentConfig();
            return loggerOptions.LogLevelToColorRgb.ContainsKey(
                LarLoggerOptions.GetLogLevel(logLevel)
                );
        }

        /// <summary>
        /// 开启作用域日志
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state) 
        {
            if (!_getCurrentConfig().IncludeScopes)
            {
                return default;
            }

            if (!_hasLoggerScope)
            {
                _scopeId = Guid.NewGuid().ToString();
            }

            var scope = new ThisLoggerScope(this, state);
            _stackInfo.Push(scope);
            return scope;
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var Level = string.Empty;
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Level = "@<0>";
                    break;
                case LogLevel.Debug:
                    Level = "@<1>";
                    break;
                case LogLevel.Information:
                    Level = "@<2>";
                    break;
                case LogLevel.Warning:
                    Level = "@<3>";
                    break;
                case LogLevel.Error:
                    Level = "@<4>";
                    break;
                case LogLevel.Critical:
                    Level = "@<5>";
                    break;
                case LogLevel.None:
                    Level = "@<6>";
                    break;
                default:
                    Level = "@<9>";
                    break;
            }

            var nowTime = DateTime.Now;
            var logEntry = new LarLoggerEntry()
            {
                LevelType = logLevel,
                Level = Level,
                CategoryName = _categoryName,
                Time = nowTime,
                NowTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", nowTime),
                ScopeId = _scopeId,
                Sort = _stackInfo.Count,
                State = string.Format("{0}", state),
                Log = formatter(state, exception),
            };
          
            if (_hasLoggerScope)
            {
                var sbScope = new StringBuilder();
                sbScope.AppendFormat(" [Scope]：{0} Linked：", _scopeId);
                var arr = _stackInfo.ToArray();
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    sbScope.Append(arr[i]._sate);
                    if (i > 0)
                    {
                        sbScope.Append("=>");
                    }
                }
                logEntry.ScopeInfo = sbScope.ToString();
                sbScope.Clear();
            }

            //格式化日志
            _stringWriter = _stringWriter ?? new StringWriter();
            var loggerOptions = _getCurrentConfig();
            _loggerFormatter.Format(logEntry, loggerOptions, _stringWriter);
            var sbWriter = _stringWriter.GetStringBuilder();
            if (sbWriter.Length == 0)
            {
                return;
            }
            var str = sbWriter.ToString();
            sbWriter.Clear();

            //输出日志
            foreach (var logger in _outLoggers)
            {
                logger.Value.WaitLog(str);
            }
        }

        /// <summary>
        /// 是否有作用域数据
        /// </summary>
        private bool _hasLoggerScope => _stackInfo.Count > 0;

        /// <summary>
        /// 释放作用域
        /// </summary>
        public void EndScope()
        {
            _stackInfo.TryPop(out ThisLoggerScope @this);

            if (!_hasLoggerScope)
            {
                _scopeId = string.Empty;
            }
        }


    }

    public sealed class ThisLoggerScope : IDisposable
    {
        private readonly LarLogger _logger;
        public readonly object _sate;
        public ThisLoggerScope(LarLogger logger, object sate)
        {
            _logger = logger;
            _sate = sate;
        }
        public void Dispose()
        {
            _logger.EndScope();
        }
    }
}
