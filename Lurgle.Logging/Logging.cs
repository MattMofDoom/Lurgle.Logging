using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lurgle.Logging
{
    /// <summary>
    /// Static Blorp.Logging instance that provides an interface to properties and methods for logging
    /// </summary>
    public static class Logging
    {
        public static LoggingConfig Config { get; private set; } = LoggingConfig.GetConfig();
        public static Logger LogWriter { get; private set; } = null;
        public static Dictionary<LogType, FailureReason> LogFailures { get; private set; }
        public static List<LogType> EnabledLogs { get; private set; }
        public static readonly string AppName = "AppName";
        public static readonly string AppVersion = "AppVersion";
        public static readonly string CorrelationId = "CorrelationId";
        public static readonly string MethodName = "MethodName";
        public static readonly string SourceFile = "SourceFile";
        public static readonly string LineNumber = "LineNumber";
        public static readonly string LogMethod = "{0} - {1}";
        public static readonly string LogNameDate = "{0}-{1}";
        public static readonly string LogTemplate = "{0}-";
        public static readonly string DateIso = "yyyyMMdd";
        public static readonly string Initialising = "Initialising event sources ...";

        static Logging()
        {
            Init();
        }

        /// <summary>
        /// Flush logs and dispose the logging interface. Used for application shutdown. <para/>
        /// 
        /// If this is called and then an attempt is made to write to the log, the log will be automatically initialised again.
        /// </summary>
        public static void Close()
        {
            if (LogWriter != null)
                LogWriter.Dispose();

            LogWriter = null;
        }

        /// <summary>
        /// Return today's logfile for file logs
        /// </summary>
        /// <returns></returns>
        public static string GetLogFile()
        {
            return Path.Combine(Config.LogFolder,
                string.Format(LogNameDate, Config.LogName, DateTime.Now.ToString(DateIso)));
        }

        /// <summary>
        /// Retrieve a logging configuration with enrichers and minimum log level
        /// </summary>
        /// <returns></returns>
        private static LoggerConfiguration GetLogConfig()
        {
            LoggerConfiguration config = new LoggerConfiguration();
            switch (Config.LogLevel)
            {
                case LogLevel.Debug:
                    config.MinimumLevel.Debug();
                    break;
                case LogLevel.Error:
                    config.MinimumLevel.Error();
                    break;
                case LogLevel.Fatal:
                    config.MinimumLevel.Fatal();
                    break;
                case LogLevel.Information:
                    config.MinimumLevel.Information();
                    break;
                case LogLevel.Warning:
                    config.MinimumLevel.Warning();
                    break;
                default:
                    config.MinimumLevel.Verbose();
                    break;

            }

            return config
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMemoryUsage()
                .Enrich.WithProperty(AppName, Config.AppName)
                .Enrich.WithProperty(AppVersion, Config.AppVersion);

        }

        /// <summary>
        /// Test that the configured log type can be used
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="correlationId"></param>
        /// <param name="fileName"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        private static bool TestLogConfig(LogType logType, string correlationId = null, string fileName = null, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            LoggerConfiguration testConfig = GetLogConfig();
            try
            {
                switch (logType)
                {
                    case LogType.Console:
                        testConfig.WriteTo.Console((LogEventLevel)Config.LogLevelConsole, Config.LogFormatConsole, theme: SystemConsoleTheme.Literate);
                        break;
                    case LogType.EventLog:
                        try
                        {
                            //First test whether we can create new event source .. should also work if the source exists
                            testConfig.WriteTo.EventLog(Config.LogEventSource, Config.LogEventName, ".", true, Config.LogFormatEvent, null, LogEventLevel.Verbose);
                        }
                        catch
                        {
                            testConfig.WriteTo.EventLog(Config.LogEventSource, Config.LogEventName, ".", false, Config.LogFormatEvent, null, LogEventLevel.Verbose);
                        }
                        break;
                    case LogType.File:
                        testConfig.WriteTo.File(fileName, LogEventLevel.Verbose, Config.LogFormatFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: Config.LogDays,
                                shared: true, buffered: false, flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                        break;
                    case LogType.Seq:
                        if (!string.IsNullOrEmpty(Config.LogSeqApiKey))
                            testConfig.WriteTo.Seq(Config.LogSeqServer, LogEventLevel.Verbose, apiKey: Config.LogSeqApiKey);
                        else
                            testConfig.WriteTo.Seq(Config.LogSeqServer, LogEventLevel.Verbose);
                        break;
                }

                Logger testWriter = testConfig.CreateLogger();
                if (!string.IsNullOrEmpty(correlationId))
                    testWriter
                        .ForContext(Logging.CorrelationId, correlationId)
                        .ForContext(MethodName, methodName)
                        .ForContext(LineNumber, sourceLineNumber)
                        .ForContext(SourceFile, sourceFilePath)
                        .Debug(Initialising);
                else
                    testWriter
                        .ForContext(MethodName, methodName)
                        .ForContext(LineNumber, sourceLineNumber)
                        .ForContext(SourceFile, sourceFilePath)
                        .Debug(Initialising);

                testWriter.Dispose();
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Initialise the logging interface. Checks that the configured log types are available and alerts if they aren't.
        /// </summary>
        public static void Init(string correlationId = null, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            bool manageSource = true;

            string logFolder = Config.LogFolder;
            string fileName = string.Empty;

            List<LogType> logTypes = Config.LogType.ToList();
            LogFailures = new Dictionary<LogType, FailureReason>();
            EnabledLogs = new List<LogType>();

            //If event log is enabled, test that we can create sources and/or write logs
            if (logTypes.Contains(LogType.EventLog) && !TestLogConfig(LogType.EventLog, correlationId, methodName: methodName, sourceLineNumber: sourceLineNumber, sourceFilePath: sourceFilePath))
            {
                logTypes.Remove(LogType.EventLog);
                LogFailures.Add(LogType.EventLog, FailureReason.LogTestFailed);
            }


            //If file is enabled, test that folder and file access is okay
            if (logTypes.Contains(LogType.File))
            {
                if (string.IsNullOrEmpty(logFolder))
                {
                    LogFailures.Add(LogType.File, FailureReason.DirectoryConfigEmpty);
                    logTypes.Remove(LogType.File);
                }
                else if (!Directory.Exists(logFolder))
                    try
                    {
                        Directory.CreateDirectory(logFolder);
                    }
                    catch
                    {
                        logTypes.Remove(LogType.File);
                        LogFailures.Add(LogType.File, FailureReason.DirectoryNotFound);
                    }
                else
                {
                    //Configure the file path and name
                    fileName = Path.Combine(logFolder, string.Format(string.Concat(LogTemplate, Config.LogExtension), Config.LogName));

                    if (!TestLogConfig(LogType.File, correlationId, fileName, methodName, sourceFilePath, sourceLineNumber))
                    {
                        LogFailures.Add(LogType.File, FailureReason.LogTestFailed);
                        logTypes.Remove(LogType.File);
                    }
                }
            }

            if (logTypes.Contains(LogType.Seq) && !TestLogConfig(LogType.Seq, correlationId, methodName: methodName, sourceLineNumber: sourceLineNumber, sourceFilePath: sourceFilePath))
            {
                LogFailures.Add(LogType.Seq, FailureReason.LogTestFailed);
                logTypes.Remove(LogType.Seq);
            }

            //With all that out of the way, we can create the final log config

            if (!logTypes.Count.Equals(0))
            {
                LoggerConfiguration logConfig = GetLogConfig();

                if (logTypes.Contains(LogType.Console))
                    logConfig.WriteTo.Console((LogEventLevel)Config.LogLevelConsole, Config.LogFormatConsole, theme: SystemConsoleTheme.Literate);

                if (logTypes.Contains(LogType.Seq))
                {
                    if (!string.IsNullOrEmpty(Config.LogSeqApiKey))
                        logConfig.WriteTo.Seq(Config.LogSeqServer, (LogEventLevel)Config.LogLevelSeq, apiKey: Config.LogSeqApiKey);
                    else
                        logConfig.WriteTo.Seq(Config.LogSeqServer, (LogEventLevel)Config.LogLevelSeq);
                }

                if (logTypes.Contains(LogType.File))
                    logConfig.WriteTo.File(fileName, (LogEventLevel)Config.LogLevelFile, Config.LogFormatFile, rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: Config.LogDays, shared: true, buffered: false, flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));

                if (logTypes.Contains(LogType.EventLog))
                    logConfig.WriteTo.EventLog(Config.LogEventSource, Config.LogEventName, ".",
                        manageSource, Config.LogFormatEvent, null, (LogEventLevel)Config.LogLevelEvent);
                LogWriter = logConfig.CreateLogger();

                EnabledLogs = logTypes;
            }
            else
            {
                LogWriter = null;
                throw new Exception("No logs were configured, or initialising logs failed!");
            }
        }

    }
}
