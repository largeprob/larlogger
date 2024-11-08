using LarLogger.Output.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LarLogger.Output
{
    public class FileLogger : OutLogger, IAsyncDisposable
    {
        private const int _maxLogQueue = 1000;

        //日志输出/持久化队列
        private ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private List<string> _messages = new List<string>();
        private Task _workThread;
        private CancellationTokenSource _cts;

        private int _taskDelay = 0;

        //用来接受/释放配置文件更新监听器
        private readonly IDisposable _onChangeToken;
        public FileLogger(IOptionsMonitor<LarLoggerOptions> loggerOptions) : base(loggerOptions, LarLoggerType.File)
        {
            _cts = new CancellationTokenSource();
            _workThread = Task.Factory.StartNew(Process, _cts.Token);
        }
        private async Task Process(object obj)
        {
            if (obj is null) return;
            CancellationToken token = (CancellationToken)obj;

            //如果线程没有被停止就一直读取队列数据
            while (!token.IsCancellationRequested)
            {
                while (_taskDelay == -1)
                {
                    //  Console.WriteLine("我一直在等待重新执行任务");
                    await Task.Delay(200);
                }

                //将队列数据全部入到临时的Msg
                while (_logQueue.TryDequeue(out string msg))
                {
                    _messages.Add(msg);
                }

                //写文件
                if (_messages.Count > 0)
                {
                    await WriteFileAsync(token);

                    _messages.Clear();

                    //数据操作完成应该暂停当前线程等待继续
                    Pause();
                }
            }
        }
        private async Task WriteFileAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_loggerOptions.BaseDirectory)) Directory.CreateDirectory(_loggerOptions.BaseDirectory);

            var realFileName = _loggerOptions.BaseDirectory + string.Format("{0}{1:yyyyMMdd}.0000.{2}", _loggerOptions.FileName, DateTime.Now, _loggerOptions.Format);
            //var files = Directory.GetFiles(_config.BaseDirectory, string.Format("{0}*.txt|*.json", _config.FileName));
            var prefixName = string.Format("{0}*", _loggerOptions.FileName);
            var files = Directory.GetFiles(_loggerOptions.BaseDirectory, prefixName).Where(x => x.EndsWith(".txt") || x.EndsWith(".json")).ToList();
            if (files.Count != 0)
            {
                realFileName = files.Max();
            }

            long allStream = 0;
            FileInfo fileInfo = new FileInfo(realFileName);
            if (fileInfo.Exists)
            {
                allStream = fileInfo.Length;
            }

            foreach (var msg in _messages)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var thisM = msg.AsMemory();
                allStream += thisM.Length;
                if (allStream > _loggerOptions.FileSizeLimit)
                {
                    var maxNo = string.Empty;
                    var match = Regex.Match(realFileName, $@"{_loggerOptions.FileName}\d+\.(\d+)\.txt$|.json");
                    if (match.Success)
                    {
                        maxNo = match.Groups[1].Value;
                    }
                    else
                    {
                        maxNo = "1";
                    }
                    var newMaxNo = (Convert.ToInt32(maxNo) + 1).ToString("0000");
                    string oldValue = string.Format(".{0}.{1}", maxNo, _loggerOptions.Format);
                    string newValue = string.Format(".{0}.{1}", newMaxNo, _loggerOptions.Format);
                    realFileName = realFileName.Replace(oldValue, newValue);
                    allStream = thisM.Length;

                    files = Directory.GetFiles(_loggerOptions.BaseDirectory, prefixName).Where(x => x.EndsWith(".txt") || x.EndsWith(".json")).ToList();
                    if (files.Count + 1 > _loggerOptions.RetainedFileLimitCount)
                    {
                        for (int i = 0; i < files.Count - _loggerOptions.RetainedFileLimitCount; i++)
                        {
                            new FileInfo(files[i]).Delete();
                        }
                    }
                }


#if NETFRAMEWORK
                var buffer = thisM.ToArray();
                using (StreamWriter writer = new StreamWriter(realFileName, true))
                {
                    await writer.WriteLineAsync(buffer, 0, buffer.Length);
                }
#endif

#if NET5_0_OR_GREATER
                using (StreamWriter writer = new StreamWriter(realFileName, true))
                {
                    await writer.WriteLineAsync(thisM, cancellationToken);
                }
#endif

            }
        }


        /// <summary>
        /// 销毁线程
        /// </summary>
        protected async Task Cancel()
        {
            _cts.Cancel();
            _cts.Dispose();
            await _workThread;
        }

        /// <summary>
        /// 暂停线程
        /// </summary>
        protected void Pause()
        {
            _taskDelay = -1;
        }

        /// <summary>
        /// 继续线程
        /// </summary>
        protected void Continue()
        {
            _taskDelay = 0;
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            _logQueue = new ConcurrentQueue<string>();
            _messages.Clear();
            await _workThread;
        }

        public override void WaitLog(string message)
        {
            var matchLevel = Regex.Match(message, PatternLevel);
            if (matchLevel.Success)
            {
                var levelStr = string.Format("[{0}]", GetLogLevelStr(matchLevel.Value));
                message = message.Replace(matchLevel.Value, levelStr);
            }
            //只要有新数据入队
            _logQueue.Enqueue(message);
            Continue();
        }
    }
}
