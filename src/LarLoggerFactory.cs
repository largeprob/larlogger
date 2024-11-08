using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using LarLogger.Provider;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LarLogger
{
    /// <summary>
    ///  创建 <see cref="ILogger"/> 工厂
    /// </summary>
    public class LarLoggerFactory : IDisposable
    {
        /// <summary>
        /// 内部的日志工厂
        /// </summary>
        private ILoggerFactory _loggerFactory;

        /// <summary>
        /// 内部的日志工厂
        /// </summary>
        private IServiceProvider _scopeServiceProvider;


        private ILoggerProvider _loggerProvider;

        /// <summary>
        /// 包装<see cref="ILoggerFactory"/>
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="scopeServiceProvider"></param>
        public LarLoggerFactory(IServiceProvider scopeServiceProvider, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _scopeServiceProvider = scopeServiceProvider;
            _thisFactory = this;
        }

        /// <summary>
        /// 释放 <see cref="ILoggerFactory"/>  
        /// </summary>
        public void Dispose()
        {
            _loggerFactory.Dispose();
            _scopeServiceProvider  = null;
        }


        /// <summary>
        /// 静态访问器 <see cref="LarLoggerFactory"/>
        /// </summary>
        private static LarLoggerFactory _thisFactory;

        /// <summary>
        /// 创建日志<see cref="ILogger"/>
        /// </summary>
        /// <typeparam name="T">类别类型</typeparam>
        /// <returns></returns>
        public static ILogger<T> CreateLogger<T>()
        {
            return _thisFactory._scopeServiceProvider.GetService<ILogger<T>>();
        }


        /// <summary>
        /// 添加日志支持--默认配置
        /// </summary>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLogging()
        {
            return AddLogging((configBuilder) => {
                configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });
        }

        /// <summary>
        /// 添加日志支持--默认配置
        /// </summary>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLogging(Action<IConfigurationBuilder> configBuilderAction)
        {
            //初始化配置文件容器
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilderAction.Invoke(configBuilder);
            var config = configBuilder.Build();

            //初始化容器
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);

            //添加日志系统
            ILoggingBuilder logging = default;
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(config.GetSection("Logging"));

                //添加默认logger
                builder.AddLarLogger();
                logging = builder;
            });

            //注册日志包装
            var larLoggerBuilder = new LarLoggerbuilder(logging);
            services.TryAddSingleton<ILarLoggerbuilder>(larLoggerBuilder);

            //创建单例工厂
            var scopeServiceProvider = services.BuildServiceProvider().CreateScope().ServiceProvider;
            services.TryAddSingleton(new LarLoggerFactory(scopeServiceProvider,
                scopeServiceProvider.GetService<ILoggerFactory>()
                ));
            return larLoggerBuilder;
        }
    }
}
