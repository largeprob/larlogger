using LarLogger.Provider;
using System.IO;
using System.Xml.Linq;


namespace LarLogger.Format.Base
{
    /// <summary>
    /// 日志格式化
    /// </summary>
    public abstract class Formatter
    {
        public readonly string Name;

        protected Formatter(string name)
        {
            Name = name;
        }

        public abstract void Format(in LarLoggerEntry entry, in LarLoggerOptions loggerOptions, TextWriter textWriter);

    }
}
