using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;

/* 项目“LarLogger (net7.0)”的未合并的更改
在此之前:
using LarLogger.Output;
在此之后:
using LarLogger.Output;
using LarLogger;
using LarLogger.Core;
*/

/* 项目“LarLogger (net7.0)”的未合并的更改
在此之前:
using LarLogger.Core.Format.Base;
在此之后:
using LarLogger.Core.Format.Base;
using LarLogger;
using LarLogger.Core;
using LarLogger;
*/
using LarLogger.Output;
using LarLogger.Format;
using LarLogger.Provider;
using LarLogger.Output.Base;
using LarLogger.Format.Base;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections;

namespace LarLogger
{
    public static class LarLoggerExtensions
    {
        /// <summary>
        /// 注册日志格式化程序
        /// </summary>
        /// <typeparam name="TOptions">格式化配置<see cref="FormatterOptions"/></typeparam>
        /// <typeparam name="TFormat">格式化程序<see cref="Formatter"/></typeparam>
        /// <param name="builder"><see cref="ILoggingBuilder"/></param>
        private static void AddLarLoggerFormat<TOptions, TFormat>(this ILoggingBuilder builder)
         where TFormat : Formatter
         where TOptions : FormatterOptions
        {
            builder.AddConfiguration();

            //注册日志格式化程序
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<Formatter, TFormat>());

            //注册配置文件初始化绑定到TOptions IConfigureOptions会被系统调用Configure，将配置文件的内容绑定到注册的Options
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TOptions>, BindFormatterOptions<TOptions>>());

            //注册TOptions配置文件修改监听器
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>, ConfigurationChangeTokenSource<TOptions>>());
            //builder.Services.AddSingleton<IConfigureOptions<TOptions>>(new BaseFormatterOptions<TOptions>(configure));
        }

        /// <summary>
        /// 注册日志提供程序
        /// </summary>
        /// <param name="builder"><see cref="ILoggingBuilder"/> </param>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLarLogger(this ILoggingBuilder builder)
        {
            //清除系统默认日志
            builder.ClearProviders();

            //添加输出程序
            builder.AddLarLoggerOut<ConsoleLogger>((options) => options.LoggerType = options.LoggerType | LarLoggerType.Console);
            builder.AddLarLoggerOut<FileLogger>((options) => options.LoggerType = options.LoggerType | LarLoggerType.File);

            //注册格式化日志程序
            AddLarLoggerFormat<TxtFormatterConfig, TxtFormatter>(builder);
            AddLarLoggerFormat<JsonFormatterConfig, JsonFormatter>(builder);

            //注册日志
            builder.Services.TryAddSingleton<ILoggerProvider, LarLoggerProvider>();

            //注册配置文件修改监听器
            LoggerProviderOptions.RegisterProviderOptions<LarLoggerOptions, LarLoggerProvider>(builder.Services);

            //注册包装
            var larLoggerBuilder = new LarLoggerbuilder(builder);
            builder.Services.TryAddSingleton<ILarLoggerbuilder>(larLoggerBuilder);

            return larLoggerBuilder;
        }

        /// <summary>
        /// 注册日志提供程序
        /// </summary>
        /// <param name="builder"><see cref="ILoggingBuilder"/> </param>
        /// <param name="configure">日志配置文件<see cref="LarLoggerOptions"/></param>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLarLogger(this ILoggingBuilder builder, Action<LarLoggerOptions> configure)
        {
            var larbuilder = builder.AddLarLogger();
            builder.Services.Configure(configure);
            return larbuilder;
        }

        /// <summary>
        /// 按名称注册日志格式化提供程序
        /// </summary>
        /// <param name="builder"><see cref="ILoggingBuilder"/></param>
        /// <param name="name"><see cref="FormatterType"/></param>
        /// <returns></returns>
        private static ILarLoggerbuilder AddLarLoggerFormatWithName<TOptions>(this ILarLoggerbuilder builder, string name, Action<TOptions> configure) where TOptions : FormatterOptions
        {
            builder._loggingBuilder.AddLarLogger((options) => options.Format = name);

            if (configure != null)
            {
                builder._loggingBuilder.Services.Configure(configure);
            }
            return builder;
        }


        /// <summary>
        /// 格式化日志、注册文本格式化
        /// </summary>
        /// <param name="builder"><see cref="ILoggingBuilder"/></param>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLarLoggerFormatTxt(this ILarLoggerbuilder builder)
        {
            return builder.AddLarLoggerFormatWithName<TxtFormatterConfig>(FormatterType.Txt, null);
        }

        /// <summary>
        /// 格式化日志、注册文本格式化
        /// </summary>
        /// <param name="builder"><see cref="ILoggingBuilder"/></param>
        /// <param name="configure">控制台格式化配置文件<see cref="TxtFormatterConfig"/></param>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLarLoggerFormatTxt(this ILarLoggerbuilder builder, Action<TxtFormatterConfig> configure)
        {
            return builder.AddLarLoggerFormatWithName<TxtFormatterConfig>(FormatterType.Txt, configure);
        }


        /// <summary>
        /// 格式化日志、注册Json格式化
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLarLoggerFormatJson(this ILarLoggerbuilder builder)
        {
            return builder.AddLarLoggerFormatWithName<JsonFormatterConfig>(FormatterType.Json, null);
        }

        /// <summary>
        /// 格式化日志、注册Json格式化
        /// </summary>
        /// <param name="builder"><see cref="ILoggingBuilder"/></param>
        /// <param name="configure">控制台格式化配置文件<see cref="JsonFormatterConfig"/></param>
        /// <returns></returns>
        public static ILarLoggerbuilder AddLarLoggerFormatJson(this ILarLoggerbuilder builder, Action<JsonFormatterConfig> configure)
        {
            return builder.AddLarLoggerFormatWithName<JsonFormatterConfig>(FormatterType.Json, configure);
        }




        private static ILoggingBuilder AddLarLoggerOut<TOut>(this ILoggingBuilder builder, Action<LarLoggerOptions> options) where TOut : OutLogger
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<OutLogger, TOut>());
            builder.Services.Configure(options);
            return builder;
        }

        private static ILarLoggerbuilder AddLarLoggerOutConsole(this ILarLoggerbuilder larBuilder)
        {
            var builder = larBuilder._loggingBuilder;
            builder.AddLarLoggerOut<ConsoleLogger>((options) => options.LoggerType = options.LoggerType | LarLoggerType.Console);
            return larBuilder;
        }

        public static ILarLoggerbuilder AddLarLoggerOutFile(this ILarLoggerbuilder larBuilder)
        {
            var builder = larBuilder._loggingBuilder;
            builder.AddLarLoggerOut<FileLogger>((options) => options.LoggerType = options.LoggerType | LarLoggerType.File);

            return larBuilder;


        }

        [Obsolete("This interface is not implemented yet.", true)]
        public static ILarLoggerbuilder AddLarLoggerOutHttp(this ILarLoggerbuilder larBuilder)
        {
            var builder = larBuilder._loggingBuilder;
            builder.AddLarLoggerOut<HttpLogger>((options) => options.LoggerType = options.LoggerType | LarLoggerType.Http);
            return larBuilder;
        }

        [Obsolete("This interface is not implemented yet.", true)]
        public static ILarLoggerbuilder AddLarLoggerOutDBContext(this ILarLoggerbuilder larBuilder)
        {
            var builder = larBuilder._loggingBuilder;
            builder.AddLarLoggerOut<DataBaselogger>((options) => options.LoggerType = options.LoggerType | LarLoggerType.DataBase);
            return larBuilder;
        }

 

        #region 适用于NETFramework


        #endregion


     
    }


    
}

