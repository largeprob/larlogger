using LarLogger.Format.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
namespace LarLogger.Format
{
    internal class BaseFormatterOptions<Options> : IConfigureOptions<Options> where Options : FormatterOptions
    {
        private readonly Action<Options> _action;
        public BaseFormatterOptions(Action<Options> action)
        {
            _action = action;
        }
        public virtual void Configure(Options options)
        {
            _action?.Invoke(options);
        }
    }
    internal class BindFormatterOptions<Options> : BaseFormatterOptions<Options> where Options : FormatterOptions
    {
        private const string section = "FormatterConfig";
        public BindFormatterOptions(ILoggerProviderConfiguration<LarLoggerProvider> providerConfiguration)
            : base(op => providerConfiguration.Configuration.GetSection(section).Bind(op))
        {

        }

        //public BindFormatterOptions(ILoggerProviderConfiguration<LarLoggerProvider> providerConfiguration)
        //  : base(op => ConfigurationBinder.Bind(config, options)))
        //{

        //}
    }
}
