using LarLogger.Format.Base;
using LarLogger.Output.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;


namespace LarLogger.Provider
{
    /// <summary>
    /// 日志设置选项
    /// </summary>
    public sealed partial class LarLoggerOptions
    {
        private LarLoggerType _loggerType = LarLoggerType.Console;

        /// <summary>
        /// 日志输出类型
        /// </summary>
        public LarLoggerType LoggerType { get; set; }


        private string _format = FormatterType.Txt;

        /// <summary>
        /// 日志输出格式化类型
        /// </summary>
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
            }
        }


        private string _baseDirectory = Directory.GetCurrentDirectory() + "//LarLogs//";

        /// <summary>
        /// 文件输出地址
        /// 默认<c>./LarLogs/</c>
        /// </summary>
        public string BaseDirectory
        {
            get => _baseDirectory;
            set
            {
                if (string.IsNullOrEmpty(value)) return;


                if (Path.IsPathRooted(value))
                {
                    _baseDirectory = value;
                }
                else
                {
                    _baseDirectory = Directory.GetCurrentDirectory() + value;
                }
            }
        }


        private string _fileName = "larlog";

        /// <summary>
        /// 文件名称
        /// 默认<c>larlog</c>
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                _fileName = value;
            }
        }


        private int? _fileSizeLimit = 10 * 1024 * 1024;

        /// <summary>
        /// 文件单位大小：byte
        /// 默认<c>10M</c>
        /// </summary>
        public int? FileSizeLimit
        {
            get
            {
                return _fileSizeLimit;
            }
            set
            {
                if (value <= 0)
                {
                    throw new Exception($"无效的值{nameof(value)},{nameof(FileSizeLimit)}单位大小必须有效");
                }
                _fileSizeLimit = value;
            }
        }

        private const int _minFileCount = 1;
        private const int _maxFileCount = 999;
        private int _retainedFileLimitCount = _minFileCount;

        /// <summary>
        /// 文件最大数量按照同文件名
        /// 默认<c>1</c>
        /// 最大<c>999</c>
        /// </summary>
        public int RetainedFileLimitCount
        {
            get
            {
                return _retainedFileLimitCount;
            }
            set
            {
                if (value <= 0 || value > _maxFileCount)
                {
                    throw new Exception($"无效的值{nameof(value)},{nameof(RetainedFileLimitCount)}的范围必须为{_minFileCount}-{_maxFileCount}");
                }
                _retainedFileLimitCount = value;
            }
        }

        /// <summary>
        /// 是否启用范围日志输出
        /// 默认<c>false</c>
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// 打印色彩配置表-Rgb
        /// </summary>
        public Dictionary<string, string> LogLevelToColorRgb { get; set; } = new Dictionary<string, string>()
        {
            ["Trac"] = "250;250;250",
            ["Dbug"] = "250;250;250",
            ["Info"] = "51;150;30",
            ["Warn"] = "255;160;0",
            ["Fail"] = "213;0;0",
            ["Crit"] = "255;235;59",
        };

 
        private string _colorUrl = "30;159;255";
        /// <summary>
        /// 润色URL
        /// </summary>
        public string ColorUrl
        {
            get
            {
                return _colorUrl;
            }
            set
            {
                _colorUrl = value;
            }
        }

    }


    /// <summary>
    /// 日志设置选项扩展
    /// </summary>
    public sealed partial class LarLoggerOptions {

        /// <summary>
        /// 根据日志字符获取日志级别枚举
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static LogLevel GetLogLevel(string input)
        {
            LogLevel result = default;
            switch (input)
            {
                case "Trac":
                    result = LogLevel.Trace;
                    break;
                case "Dbug":
                    result = LogLevel.Debug;
                    break;
                case "Info":
                    result = LogLevel.Information;
                    break;
                case "Warn":
                    result = LogLevel.Warning;
                    break;
                case "Fail":
                    result = LogLevel.Error;
                    break;
                case "Crit":
                    result = LogLevel.Critical;
                    break;
                case "None":
                    result = LogLevel.None;
                    break;
                default:
                    result = LogLevel.None;
                    break;
            }
            return result;
        }

        /// <summary>
        /// 根据级别枚举获取日志字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetLogLevel(LogLevel input)
        {
            string result = string.Empty;
            switch (input)
            {
                case LogLevel.Trace:
                    result = "Trac";
                    break;
                case LogLevel.Debug:
                    result = "Dbug";
                    break;
                case LogLevel.Information:
                    result = "Info";
                    break;
                case LogLevel.Warning:
                    result = "Warn";
                    break;
                case LogLevel.Error:
                    result = "Fail";
                    break;
                case LogLevel.Critical:
                    result = "Crit";
                    break;
                case LogLevel.None:
                    result = "None";
                    break;
                default:
                    result = "Null";
                    break;
            }
            return result;
        }

    }
}
