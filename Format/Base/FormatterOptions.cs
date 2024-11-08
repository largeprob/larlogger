namespace LarLogger.Format.Base
{
    public class FormatterOptions
    {
        /// <summary>
        /// 日期格式
        /// 如果设置了MesageFormat此项无效
        /// </summary>
        public string TimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 日志输出格式
        /// </summary>
        /// <example>
        /// <see cref="LarLoggerEntry"/>
        /// ${CategoryName}
        /// 
        /// ${EventId}
        /// </example>
        public string MesageFormat { get; set; }
    }
}
