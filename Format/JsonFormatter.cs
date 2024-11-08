using LarLogger.Format.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;



namespace LarLogger.Format
{
    public sealed class JsonFormatterConfig : FormatterOptions
    {

    }

    public sealed class JsonFormatter : Formatter, IDisposable
    {
        private JsonFormatterConfig _config;

        //用来接受/释放配置文件更新监听器
        private readonly IDisposable _onChangeToken;

        public JsonFormatter(IOptionsMonitor<JsonFormatterConfig> config) : base(FormatterType.Json)
        {
            _config = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig =>
            {
                _config = updatedConfig;
            });
        }

        private JsonSerializerOptions _serializerOptions;
        public override void Format(in LarLoggerEntry entry, in LarLoggerOptions loggerOptions, TextWriter textWriter)
        {
            _serializerOptions = _serializerOptions ?? new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(entry, _serializerOptions);
            textWriter.WriteLine(Encoding.UTF8.GetString(jsonUtf8Bytes));
        }

        public void Dispose()
        {
            _onChangeToken?.Dispose();
        }

     
    }
}
