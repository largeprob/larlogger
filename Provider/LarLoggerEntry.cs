using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Serialization;

namespace LarLogger.Provider
{
    public struct LarLoggerEntry
    {
        /// <summary>
        /// 日志发生时间
        /// </summary>
        [JsonIgnore]
        public DateTime Time { get; set; }

        /// <summary>
        /// 日志类别名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 事件ID
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        [JsonIgnore]
        public LogLevel LevelType { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        [JsonPropertyName("Level")]
        [JsonPropertyOrder(-1)]
        public string Level { get; set; }

        /// <summary>
        /// 日志输出时间
        /// </summary>
        public string NowTime { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// 请求端点
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// 作用域ID
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// 作用域信息
        /// </summary>
        public string ScopeInfo { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [JsonIgnore]
        public int Sort { get; set; }

        /// <summary>
        /// 作用域数据
        /// </summary>
        [JsonIgnore]
        public string State { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Log { get; set; }
    }
}
