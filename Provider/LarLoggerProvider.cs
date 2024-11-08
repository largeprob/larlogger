using LarLogger.Format.Base;
using LarLogger.Output.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Xml.Linq;


namespace LarLogger.Provider
{
    /// <summary>
    /// 日志提供器-用以创建日志实例
    /// </summary>
    /// <!--[UnsupportedOSPlatform("browser")] 是一个编译器警告-那些不受支持的操作系统/平台-->
    /// <!--[ProviderAlias("LarLogger")] 按照默认约定LarLogger作为日志提供器的名称，用以映射appsettings.json配置-->
    //[UnsupportedOSPlatform("browser")]
    [ProviderAlias("LarLogger")]
    public class LarLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// 用来接受/释放配置文件更新监听器
        /// </summary>
        /// <remarks>NUll</remarks>
        private readonly IDisposable _onChangeToken;

        //用来接受/释放配置文件
        private LarLoggerOptions _currentConfig;
        // 备注:
        //      因为Provider是一个单例，我们需要缓存日志实例进行服用
        //      _loggers用来接受/释放日志实例
        private readonly ConcurrentDictionary<string, LarLogger> _loggers = new ConcurrentDictionary<string, LarLogger>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<LarLoggerType, OutLogger> _outLoggers = new ConcurrentDictionary<LarLoggerType, OutLogger>();





        private ConcurrentDictionary<string, Formatter> _formatters = new ConcurrentDictionary<string, Formatter>();

        public LarLoggerProvider(IOptionsMonitor<LarLoggerOptions> config,
            IEnumerable<Formatter> formatters, IEnumerable<OutLogger> outLoggers
            )
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig =>
            {
                _currentConfig = updatedConfig;
            });


            foreach (var item in formatters)
            {
                _formatters.TryAdd(item.Name, item);
            }

            foreach (var item in outLoggers)
            {
                _outLoggers.TryAdd(item.Name, item);
            }


        }

        /// <summary>
        /// 创建日志实例
        /// </summary>
        /// <param name="categoryName">
        /// 日志分类名
        /// 默认是注入的泛型完全限定名称
        /// </param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            // 备注:
            //      如果字典中存在就从字典获取否则new
            //      注意在new LarLogger 时配置文件是以方法传递，而不是引用传递
            //      因为你不能确定OnChange后的Config是否为原来的实例。
            //      而categoryName和_accessor是无法被修改为新的地址
            ILogger logger = _loggers.GetOrAdd(categoryName, name => new LarLogger(categoryName, GetCurrentConfig, _formatters[_currentConfig.Format], _outLoggers));
            return logger;
        }





        /// <summary>
        /// 获取配置的方法
        /// </summary>
        private LarLoggerOptions GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            // 备注:
            //      释放所有资源
            //_loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
