//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using System.IO;
using System.Text;
using System.Threading;

#if ENCRYPT
namespace JointCode.Internals
#else
using JointCode.Common.Extensions;
namespace JointCode.Common.Logging
#endif
{
    ///// <summary>
    ///// A Logging class implementing the Singleton pattern and an internal Queue to be flushed perdiodically
    ///// </summary>
    //public class LogWriter
    //{
    //    private static LogWriter instance;
    //    private static Queue<Log> logQueue;
    //    private static string logDir = <Path to your Log Dir or Config Setting>;
    //    private static string logFile = <Your Log File Name or Config Setting>;
    //    private static int maxLogAge = int.Parse(<Max Age in seconds or Config Setting>);
    //    private static int queueSize = int.Parse(<Max Queue Size or Config Setting);
    //    private static DateTime LastFlushed = DateTime.Now;

    //    /// <summary>
    //    /// Private constructor to prevent instance creation
    //    /// </summary>
    //    private LogWriter() { }

    //    ~LogWriter()
    //    {
    //        FlushLog();
    //    }

    //    /// <summary>
    //    /// An LogWriter instance that exposes a single instance
    //    /// </summary>
    //    public static LogWriter Instance
    //    {
    //        get
    //        {
    //            // If the instance is null then create one and init the Queue
    //            if (instance == null)
    //            {
    //                instance = new LogWriter();
    //                logQueue = new Queue<Log>();
    //            }
    //            return instance;
    //        }
    //    }

    //    /// <summary>
    //    /// The single instance method that writes to the log file
    //    /// </summary>
    //    /// <param name="message">The message to write to the log</param>
    //    public void WriteToLog(string message)
    //    {
    //        // Lock the queue while writing to prevent contention for the log file
    //        lock (logQueue)
    //        {
    //            // Create the entry and push to the Queue
    //            Log logEntry = new Log(message);
    //            logQueue.Enqueue(logEntry);

    //            // If we have reached the Queue Size then flush the Queue
    //            if (logQueue.Count >= queueSize || DoPeriodicFlush())
    //            {
    //                FlushLog();
    //            }
    //        }            
    //    }

    //    private bool DoPeriodicFlush()
    //    {
    //        TimeSpan logAge = DateTime.Now - LastFlushed;
    //        if (logAge.TotalSeconds >= maxLogAge)
    //        {
    //            LastFlushed = DateTime.Now;
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// Flushes the Queue to the physical log file
    //    /// </summary>
    //    private void FlushLog()
    //    {
    //        string logPath = logDir + "log" + "_" + logFile;

    //        // This could be optimised to prevent opening and closing the file for each write
    //        using (var fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
    //        {
    //            using (var log = new StreamWriter(fs))
    //            {
    //                while (logQueue.Count > 0)
    //                {
    //                    Log entry = logQueue.Dequeue();
    //                    log.WriteLine(string.Format("{0}\t{1}",entry.LogTime,entry.Message));
    //                }
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// A Log class to store the message and the Date and Time the log entry was created
    ///// </summary>
    //public class Log
    //{
    //    public string Message { get; set; }
    //    public string LogTime { get; set; }
    //    public string LogDate { get; set; }

    //    public Log(string message)
    //    {
    //        Message = message;
    //        LogDate = DateTime.Now.ToString("yyyy-MM-dd");
    //        LogTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
    //    }
    //}

    ///// <summary>
    ///// 多线程写文本文件
    ///// </summary>
    //public class AsynFileHelp
    //{
    //    static Dictionary<long, long> lockDic = new Dictionary<long, long>();
    //    string fileName;

    //    /// <summary> 
    //    /// 获取或设置文件名称 
    //    /// </summary> 
    //    public string FileName
    //    {
    //        get { return fileName; }
    //        set { fileName = value; }
    //    }

    //    /// <summary> 
    //    /// 构造函数 
    //    /// </summary> 
    //    /// <param name="byteCount">每次开辟位数大小，这个直接影响到记录文件的效率</param> 
    //    /// <param name="fileName">文件全路径名</param> 
    //    public AsynFileHelp(string filename)
    //    {
    //        fileName = filename;
    //    }

    //    /// <summary> 
    //    /// 创建文件 
    //    /// </summary> 
    //    /// <param name="fileName"></param> 
    //    public void Create(string fileName)
    //    {
    //        if (!File.Exists(fileName))
    //        {
    //            using (FileStream fs = File.Create(fileName))
    //            {
    //                fs.Close();
    //            }
    //        }
    //    }

    //    /// <summary> 
    //    /// 写入文本 
    //    /// </summary> 
    //    /// <param name="content">文本内容</param> 
    //    private void Write(string content, string newLine)
    //    {
    //        if (string.IsNullOrEmpty(fileName))
    //            throw new Exception("文件名不能为空！");

    //        using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8, FileOptions.Asynchronous))
    //        {
    //            Byte[] dataArray = Encoding.Default.GetBytes(content + newLine);
    //            bool flag = true;
    //            long slen = dataArray.Length;
    //            long len = 0;
    //            while (flag)
    //            {
    //                try
    //                {
    //                    if (len >= fs.Length)
    //                    {
    //                        fs.Lock(len, slen);
    //                        lockDic[len] = slen;
    //                        flag = false;
    //                    }
    //                    else
    //                    {
    //                        len = fs.Length;
    //                    }
    //                }
    //                catch
    //                {
    //                    while (!lockDic.ContainsKey(len))
    //                    {
    //                        len += lockDic[len];
    //                    }
    //                }
    //            }
    //            fs.Seek(len, SeekOrigin.Begin);
    //            fs.Write(dataArray, 0, dataArray.Length);
    //            fs.Close();
    //        }
    //    }

    //    /// <summary> 
    //    /// 写入文件内容 
    //    /// </summary> 
    //    /// <param name="content"></param> 
    //    public void WriteLine(string content)
    //    {
    //        Write(content, Environment.NewLine);
    //    }

    //    /// <summary> 
    //    /// 写入文件 
    //    /// </summary> 
    //    /// <param name="content"></param> 
    //    public void Write(string content)
    //    {
    //        Write(content, "");
    //    }
    //}

    sealed class DefaultFileTarget : ILogTarget
    {
        const int MaxRetryTime = 10;
        readonly object _syncObj = new object();

        static string _logFile; //log file
        static FileLogSetting _logSetting;
        static DefaultFileTarget _singleton; // use a singleton to prevent read/write to the same log file at the same time.

        DefaultFileTarget() { }

        /// <summary>
        /// To avoid multiple file appenders acting on the same log file, this class must be singleton.
        /// </summary>
        public static DefaultFileTarget GetInstance(FileLogSetting logSetting)
        {
            if (_singleton != null)
                return _singleton;

            var singleton = new DefaultFileTarget();
            if (Interlocked.CompareExchange(ref _singleton, singleton, null) == null)
                _logSetting = logSetting;

            return _singleton;
        }

        static void CheckFileSystem()
        {
            if (!Directory.Exists(_logSetting.LogDirectory))
                Directory.CreateDirectory(_logSetting.LogDirectory);

            var time = DateTime.Now;
            var curDate = time.ToString("yyyy-MM-dd");
            var logFileName = _logSetting.LogFileName.IsNullOrWhiteSpace() 
                ? curDate + "_" + AppDomain.CurrentDomain.Id + ".log" 
                : _logSetting.LogFileName + "_" + curDate + "_" + AppDomain.CurrentDomain.Id + ".log";
            _logFile = Path.Combine(_logSetting.LogDirectory, logFileName);
        }

        /// <summary>
        /// Write the message into the log file
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(string message)
        {
            CheckFileSystem();

            var retryTime = 0;
            while (true)
            {
                try
                {
                    //Wait for resource to be free
                    lock (_syncObj)
                    {
                        using (var file = new FileStream(_logFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                        {
                            using (var writer = new StreamWriter(file, Encoding.UTF8))
                                writer.Write(message);
                        }
                    }
                    break;
                }
                catch(IOException)
                {
                    if (retryTime > MaxRetryTime)
                        throw;
                    //File not available, conflict with other class instances or application. Let's wait and retry.
                    retryTime++;
                    Thread.Sleep(200);
                }
            }
        }
    }
}
