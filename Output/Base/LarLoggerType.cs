using System;

namespace LarLogger.Output.Base
{
    /// <summary>
    /// 日志输出方式
    /// </summary>
    [Flags]
    public enum LarLoggerType
    {
        /// <summary>
        /// 控制台打印
        /// </summary>
        Console = 0x0000,
        /// <summary>
        /// 输出到文件
        /// </summary>
        File = 0x0001,
        /// <summary>
        /// 输出到远程地址
        /// </summary>
        Http = 0x0002,
        /// <summary>
        /// 输出到持久化数据库
        /// </summary>
        DataBase = 0x0004,
    }
}
