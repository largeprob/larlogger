using LarLogger.Format.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LarLogger.Format
{
    /// <summary>
    /// 格式化配置
    /// </summary>
    public sealed class TxtFormatterConfig : FormatterOptions
    {

    }

    /// <summary>
    /// 控制台输出格式化
    /// </summary>
    public sealed class TxtFormatter : Formatter, IDisposable
    {
        private TxtFormatterConfig _config;
        //用来接受/释放配置文件更新监听器
        private readonly IDisposable _onChangeToken;
        public TxtFormatter(IOptionsMonitor<TxtFormatterConfig> config) : base(FormatterType.Txt)
        {
            _config = config.CurrentValue;

            _onChangeToken = config.OnChange(updatedConfig =>
            {
                _config = updatedConfig;
            });
        }

        public override void Format(in LarLoggerEntry entry, in LarLoggerOptions loggerOptions, TextWriter textWriter)
        {
            var mesageFormat = _config.MesageFormat;
            if (!string.IsNullOrEmpty(mesageFormat))
            {
                var matches = Regex.Matches(mesageFormat, @"\$\{(\w+(:[^\}]+)?)\}");
                foreach (Match match in matches)
                {
                    var splIndex = match.Groups[1].Value.IndexOf(":");

                    //无需格式化字段的模板
                    if (splIndex == -1)
                    {
                        var field = match.Groups[1].Value;
                        var property = entry.GetType().GetProperty(field);
                        if (property != null)
                        {
                            var value = string.Format("{0}", property.GetValue(entry));
                            mesageFormat = mesageFormat.Replace(match.Value, value);
                        }
                    }
                    else
                    {
                        var field = match.Groups[1].Value.Substring(0, splIndex);
                        var property = entry.GetType().GetProperty(field);
                        if (property != null)
                        {
                            var value = string.Format("{0}", property.GetValue(entry));
                            if (property.DeclaringType == typeof(DateTime))
                            {
                                value = Convert.ToDateTime(value).ToString(match.Groups[2].Value.Substring(1));
                            }
                            mesageFormat = mesageFormat.Replace(match.Value, value);
                        }
                    }
                }
             
                textWriter.Write(mesageFormat);
            }
            else
            {
                //日志类型
                textWriter.Write(entry.Level);
                textWriter.Write(string.Format(" {0} [{1}] {2} {3} {4} {5} {6}",
                    entry.CategoryName,
                    entry.EventId,
                    entry.Time.ToString(_config.TimeFormat),
                    entry.Request,
                    entry.Endpoint,
                    entry.ScopeId,
                    entry.ScopeInfo
                ));
                textWriter.Write("\n       " + entry.Log);
            }
        }

        public void Dispose()
        {
            _onChangeToken?.Dispose();
        }
    }
}
