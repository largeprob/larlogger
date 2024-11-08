using LarLogger.Output.Base;
using LarLogger.Provider;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace LarLogger.Output
{
    public class ConsoleLogger : OutLogger, IAsyncDisposable
    {
        //日志缓存队列
        private ConcurrentQueue<string> _consoleQueue = new ConcurrentQueue<string>();
        //临时日志输出列表
        private List<string> _consoleList = new List<string>();

        //后台线程
        private readonly Task _consoleThread;
        private CancellationTokenSource _cts;
        private ManualResetEvent _manualEvent = new ManualResetEvent(true);

        //队列最大缓冲长度
        private const int _maxQueuedLength = 1024;

        public ConsoleLogger(IOptionsMonitor<LarLoggerOptions> loggerOptions) : base(loggerOptions, LarLoggerType.Console)
        {
            _cts = new CancellationTokenSource();
            _consoleThread = Task.Factory.StartNew(Process, _cts.Token);
        }


        /// <summary>
        /// 工作区
        /// </summary>
        private void Process(object obj)
        {
            if (obj is null) return;
            CancellationToken token = (CancellationToken)obj;
            while (!token.IsCancellationRequested)
            {
                _manualEvent.WaitOne();

                //将队列数据全部入到临时的Msg
                while (_consoleQueue.TryDequeue(out string msg))
                {
                    _consoleList.Add(msg);
                }

                //等待列表中的消息全部消费完成
                lock (_consoleQueue)
                {
                    if (_consoleList.Count > 0)
                    {
                        ConsoleLog(token);
                    }
                }

                //暂停线程
                PauseLog();
            }
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="token"></param>
        private void ConsoleLog(CancellationToken token)
        {

            if (token.IsCancellationRequested) return;

            foreach (var msg in _consoleList)
            {
                Console.WriteLine(msg);
            }
            _consoleList.Clear();

            //当日志输出完成后使等待中的线程继续操作
            Monitor.PulseAll(_consoleQueue);
        }

        /// <summary>
        /// 销毁线程
        /// </summary>
        public async Task CancelLog()
        {
            _cts.Cancel();
            _cts.Dispose();
            await _consoleThread;
        }

        /// <summary>
        /// 暂停线程
        /// </summary>
        public void PauseLog()
        {
            _manualEvent.Reset();
        }

        /// <summary>
        /// 继续线程
        /// </summary>
        public void ContinueLog()
        {
            _manualEvent.Set();
        }

        /// <summary>
        /// 入队
        /// </summary>
        private bool TryEnqueue(string message)
        {
            //入队时加锁
            //如果消费者没有拿到锁则生产者基本上可以一直获取到锁，直到到达 _maxQueuedLength
            //使队列可以积压一定数量的消息
            lock (_consoleQueue)
            {
                //如果队列长度超长，就让所有拿到锁还没有入队的的线程等待队列消费
                Debug.WriteLine(
                    string.Format("{0}队列长度{1}", nameof(ConsoleLogger), _consoleQueue.Count)
                    );
                while (_consoleQueue.Count >= _maxQueuedLength)
                {
                    Monitor.Wait(_consoleQueue);
                }

                //入队
                _consoleQueue.Enqueue(message);

                //唤醒线程
                ContinueLog();

                return true;
            }
        }

        //private string _patternLevel = @"\@\\u003C[\d]+\\u003E";
        private string _patternUrl = "(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]";

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="content"></param>
        public override void WaitLog( string message)
        {
            //着色日志输出类型
            var matchLevel = Regex.Match(message, PatternLevel);
            if (matchLevel.Success)
            {
                var levelStr = GetLogLevelStr(matchLevel.Value);
                var levelColor = string.Format("\u001b[38;2;{0}m[{1}]\u001b[0m", _loggerOptions.LogLevelToColorRgb[levelStr], levelStr);
                message = message.Replace(matchLevel.Value, levelColor);
            }

            //着色URL
            MatchCollection matches = Regex.Matches(message, _patternUrl);
            foreach (Match match in matches)
            {
                message = message.Replace(match.Value,
                    string.Format("\u001b[38;2;{0}m{1}\u001b[0m",
                _loggerOptions.ColorUrl,
                 match.Value
                )
                    );
            }

            //入队
            TryEnqueue(message);
        }



        public async ValueTask DisposeAsync()
        {
            _consoleQueue = new ConcurrentQueue<string>();
            _consoleList.Clear();
            await CancelLog();
        }
    }
}
