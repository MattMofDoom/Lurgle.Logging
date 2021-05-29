using Serilog.Events;
using System;
using System.Runtime.CompilerServices;

namespace Lurgle.Logging
{
    public sealed class Log : ILogging, IDisposable
    {
        private bool IsMethod { get; set; }
        private string CorrelationId { get; set; }
        private string MethodName { get; set; }
        private int SourceLineNumber { get; set; }
        private string SourceFilePath { get; set; }
        private LogLevel LogLevel { get; set; }
        private Exception ErrorInfo { get; set; }

        private Log(LogLevel logLevel, string correlationId, bool showMethod, string methodName, string sourceFilePath, int sourceLineNumber)
        {
            LogLevel = logLevel;
            IsMethod = showMethod;
            CorrelationId = correlationId;
            MethodName = methodName;
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;

            if (Logging.LogWriter == null)
            {
                Logging.Init(correlationId, methodName, sourceFilePath, sourceLineNumber);
            }
        }

        private Log(Exception ex, LogLevel logLevel, string correlationId, bool showMethod, string methodName, string sourceFilePath, int sourceLineNumber)
        {
            ErrorInfo = ex;
            LogLevel = logLevel;
            IsMethod = showMethod;
            CorrelationId = correlationId;
            MethodName = methodName;
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;

            if (Logging.LogWriter == null)
            {
                Logging.Init(correlationId, methodName, sourceFilePath, sourceLineNumber);
            }
        }
        /// <summary>
        /// Log an event with the specified level. Defaults to Information. 
        /// 
        /// CorrelationId can optionally be specified.
        /// 
        /// Optionally, you can embed the method name in the event log text.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILogging Level(LogLevel logLevel = LogLevel.Information, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Log(logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Log an Exception with the specified level. Defaults to Error. 
        /// 
        /// CorrelationId can optionally be specified. 
        /// 
        /// Optionally, you can embed the method name in the  event log text.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logLevel"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILogging Exception(Exception ex, LogLevel logLevel = LogLevel.Error, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Log(ex, logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Add a new log entry and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate">Log template that parameters will be applied to</param>
        /// <param name="args">Parameters for the log template</param>
        public void Add(string logTemplate, params object[] args)
        {
            string logText;
            if (IsMethod)
            {
                logText = string.Format(Logging.LogMethod, MethodName, logTemplate);
            }
            else
            {
                logText = logTemplate;
            }

            if (Logging.LogWriter != null)
            {
                if (ErrorInfo != null)
                {
                    if (!string.IsNullOrEmpty(CorrelationId))
                    {
                        Logging.LogWriter
                            .ForContext(Logging.CorrelationId, CorrelationId)
                            .ForContext(Logging.MethodName, MethodName)
                            .ForContext(Logging.LineNumber, SourceLineNumber)
                            .ForContext(Logging.SourceFile, SourceFilePath)
                            .Write((LogEventLevel)LogLevel, ErrorInfo, logText, args);
                    }
                    else
                    {
                        Logging.LogWriter
                            .ForContext(Logging.MethodName, MethodName)
                            .ForContext(Logging.LineNumber, SourceLineNumber)
                            .ForContext(Logging.SourceFile, SourceFilePath)
                            .Write((LogEventLevel)LogLevel, ErrorInfo, logText, args);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(CorrelationId))
                    {
                        Logging.LogWriter
                            .ForContext(Logging.CorrelationId, CorrelationId)
                            .ForContext(Logging.MethodName, MethodName)
                            .ForContext(Logging.LineNumber, SourceLineNumber)
                            .ForContext(Logging.SourceFile, SourceFilePath)
                            .Write((LogEventLevel)LogLevel, logText, args);
                    }
                    else
                    {
                        Logging.LogWriter
                            .ForContext(Logging.MethodName, MethodName)
                            .ForContext(Logging.LineNumber, SourceLineNumber)
                            .ForContext(Logging.SourceFile, SourceFilePath)
                            .Write((LogEventLevel)LogLevel, logText, args);
                    }
                }
            }
        }

        /// <summary>
        /// Ensure that we flush and dispose the logwriter
        /// </summary>
        public void Dispose()
        {
            Logging.Close();
        }
    }
}
