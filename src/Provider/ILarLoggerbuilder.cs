﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarLogger.Provider
{
    public interface ILarLoggerbuilder
    {
        ILoggingBuilder _loggingBuilder { get; }
    }
}