using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarLogger.Provider.Configuration
{
    internal class LarLoggerProviderConfiguration : ILarLoggerProviderConfiguration
    {
        private IConfiguration _configuration;

        public LarLoggerProviderConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }


    }
}
