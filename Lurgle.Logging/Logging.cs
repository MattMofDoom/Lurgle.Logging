using Serilog;
using Serilog.Core;
using Serilog.Events;
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
        public static LoggingConfig Config { get; private set; }
        public static Logger LogWriter { get; private set; } = null;
        private static Dictionary<string, object> CommonProperties { get; set; } = new Dictionary<string, object>();
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

        /// <summary>
        /// Flush logs and dispose the logging interface. Used for application shutdown. <para/>
        /// 
        /// If this is called and then an attempt is made to write to the log, the log will be automatically initialised again.
        /// </summary>
        public static void Close()
        {
            if (LogWriter != null)
            {
                LogWriter.Dispose();
            }

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
                case LurgLevel.Debug:
                    config.MinimumLevel.Debug();
                    break;
                case LurgLevel.Error:
                    config.MinimumLevel.Error();
                    break;
                case LurgLevel.Fatal:
                    config.MinimumLevel.Fatal();
                    break;
                case LurgLevel.Information:
                    config.MinimumLevel.Information();
                    break;
                case LurgLevel.Warning:
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
                .Enrich.WithMemoryUsage()
                .Enrich.WithProperty(AppName, Config.AppName)
                .Enrich.WithProperty(AppVersion, Config.AppVersion);

        }

        /// <summary>
        /// Return a dictionary comprised of the base properties that we pass to each event
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetBaseProperties(string correlationId = null,
            string methodName = null, string sourceFilePath = null, int sourceLineNumber = -1)
        {
            //Automatically include static common properties
            Dictionary<string, object> propertyValues = CommonProperties.ToDictionary(p => p.Key, p => p.Value);

            if (!string.IsNullOrEmpty(correlationId))
            {
                propertyValues.Add(CorrelationId, correlationId);
            }

            if (Config.EnableMethodNameProperty && !string.IsNullOrEmpty(methodName))
            {
                propertyValues.Add(MethodName, methodName);
            }

            if (Config.EnableSourceFileProperty && !string.IsNullOrEmpty(sourceFilePath))
            {
                propertyValues.Add(SourceFile, sourceFilePath);
            }

            if (Config.EnableLineNumberProperty && sourceLineNumber > 0)
            {
                propertyValues.Add(LineNumber, sourceLineNumber);
            }

            return propertyValues;
        }

        /// <summary>
        /// Add an additional static property for logging
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void AddCommonProperty(string name, object value)
        {
            if (!string.IsNullOrEmpty(name) && !CommonProperties.ContainsKey(name))
            {
                CommonProperties.Add(name, value);
            }
        }

        /// <summary>
        /// Add an additional set of static properties for logging
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <returns></returns>
        public static void AddCommonProperty(Dictionary<string, object> propertyPairs)
        {
            foreach (KeyValuePair<string, object> values in propertyPairs)
            {
                if (!string.IsNullOrEmpty(values.Key) && !CommonProperties.ContainsKey(values.Key))
                {
                    CommonProperties.Add(values.Key, values.Value);
                }
            }
        }

        /// <summary>
        /// Clear the list of static properties
        /// </summary>
        public static void ResetCommonProperties()
        {
            CommonProperties.Clear();
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
        private static bool TestLogConfig(LogType logType, string correlationId = null, bool manageSource = true, string fileName = null,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            LoggerConfiguration testConfig = GetLogConfig();
            try
            {
                switch (logType)
                {
                    case LogType.Console:
                        testConfig
                            .WriteTo
                            .Console((LogEventLevel)Config.LogLevelConsole, Config.LogFormatConsole, theme: Config.LogConsoleTheme);
                        break;
                    case LogType.EventLog:
                        try
                        {
                            testConfig
                                .WriteTo
                                .EventLog(Config.LogEventSource, Config.LogEventName, ".", manageSource, Config.LogFormatEvent, null,
                                (LogEventLevel)Config.LogLevelEvent);
                        }
                        catch (System.TypeInitializationException)
                        {
                            return false;
                        }
                        catch (System.Security.SecurityException)
                        {
                            return false;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                        break;
                    case LogType.File:
                        if (Config.LogFileType.Equals(LogFileFormat.Text))
                        {
                            testConfig
                            .WriteTo
                            .File(fileName, (LogEventLevel)Config.LogLevelFile, Config.LogFormatFile, rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: Config.LogDays, shared: Config.LogShared, buffered: Config.LogBuffered,
                            flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                        }
                        else
                        {
                            testConfig
                                .WriteTo
                                .File(new Serilog.Formatting.Compact.CompactJsonFormatter(), fileName, (LogEventLevel)Config.LogLevelFile,
                                rollingInterval: RollingInterval.Day, retainedFileCountLimit: Config.LogDays, shared: Config.LogShared,
                                buffered: Config.LogBuffered, flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                        }

                        break;
                    case LogType.Seq:
                        if (!string.IsNullOrEmpty(Config.LogSeqApiKey))
                        {
                            testConfig
                                .WriteTo
                                .Seq(Config.LogSeqServer, (LogEventLevel)Config.LogLevelSeq, apiKey: Config.LogSeqApiKey);
                        }
                        else
                        {
                            testConfig
                                .WriteTo
                                .Seq(Config.LogSeqServer, (LogEventLevel)Config.LogLevelSeq);
                        }

                        break;
                }

                Logger testWriter = testConfig.CreateLogger();
                testWriter
                    .ForContext(new PropertyBagEnricher().Add(GetBaseProperties(correlationId, methodName, sourceFilePath, sourceLineNumber)))
                    .Verbose(Initialising);

                testWriter.Dispose();
                return true;
            }
            catch (System.TypeInitializationException)
            {
                return false;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Set the logging config. This will only set/update the config if there is no LogWriter currently set. 
        /// </summary>
        public static void SetConfig(LoggingConfig logConfig = null)
        {
            if (LogWriter == null)
            {
                Config = LoggingConfig.GetConfig(logConfig);
            }
        }

        /// <summary>
        /// Initialise the logging interface. Checks that the configured log types are available and alerts if they aren't.
        /// </summary>
        public static void Init(string correlationId = null, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            //We can only initialise on a running config. If one is not set via Logging.SetConfig, we will attempt to read from app.config.
            if (Config == null)
            {
                SetConfig();
            }

            bool manageSource = true;

            string logFolder = Config.LogFolder;
            string fileName = string.Empty;

            List<LogType> logTypes = Config.LogType.ToList();
            LogFailures = new Dictionary<LogType, FailureReason>();
            EnabledLogs = new List<LogType>();

            //If event log is enabled, test that we can create sources and/or write logs
            if (logTypes.Contains(LogType.EventLog) && !TestLogConfig(LogType.EventLog, correlationId, methodName: methodName, sourceLineNumber: sourceLineNumber, sourceFilePath: sourceFilePath))
            {
                manageSource = false;
                if (!TestLogConfig(LogType.EventLog, correlationId, methodName: methodName, sourceLineNumber: sourceLineNumber, sourceFilePath: sourceFilePath))
                {
                    logTypes.Remove(LogType.EventLog);
                    LogFailures.Add(LogType.EventLog, FailureReason.LogTestFailed);
                }
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
                {
                    try
                    {
                        Directory.CreateDirectory(logFolder);
                    }
                    catch
                    {
                        logTypes.Remove(LogType.File);
                        LogFailures.Add(LogType.File, FailureReason.DirectoryNotFound);
                    }
                }
                else
                {
                    //Configure the file path and name
                    fileName = Path.Combine(logFolder, string.Format(string.Concat(LogTemplate, Config.LogExtension), Config.LogName));

                    if (!TestLogConfig(LogType.File, correlationId, false, fileName, methodName, sourceFilePath, sourceLineNumber))
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
                {
                    logConfig
                        .WriteTo
                        .Console((LogEventLevel)Config.LogLevelConsole, Config.LogFormatConsole, theme: Config.LogConsoleTheme);
                }

                if (logTypes.Contains(LogType.Seq))
                {
                    if (!string.IsNullOrEmpty(Config.LogSeqApiKey))
                    {
                        logConfig
                            .WriteTo
                            .Seq(Config.LogSeqServer, (LogEventLevel)Config.LogLevelSeq, apiKey: Config.LogSeqApiKey);
                    }
                    else
                    {
                        logConfig
                            .WriteTo
                            .Seq(Config.LogSeqServer, (LogEventLevel)Config.LogLevelSeq);
                    }
                }

                if (logTypes.Contains(LogType.File))
                {
                    if (Config.LogFileType.Equals(LogFileFormat.Text))
                    {
                        logConfig
                            .WriteTo
                            .File(fileName, (LogEventLevel)Config.LogLevelFile, Config.LogFormatFile, rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: Config.LogDays, shared: Config.LogShared, buffered: Config.LogBuffered, flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                    }
                    else
                    {
                        logConfig
                            .WriteTo
                            .File(new Serilog.Formatting.Compact.CompactJsonFormatter(), fileName, (LogEventLevel)Config.LogLevelFile,
                            rollingInterval: RollingInterval.Day, retainedFileCountLimit: Config.LogDays, shared: Config.LogShared, buffered: Config.LogBuffered,
                            flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                    }
                }

                if (logTypes.Contains(LogType.EventLog))
                {
                    logConfig.WriteTo.EventLog(Config.LogEventSource, Config.LogEventName, ".",
                        manageSource, Config.LogFormatEvent, null, (LogEventLevel)Config.LogLevelEvent);
                }

                LogWriter = logConfig.CreateLogger();
                EnabledLogs = logTypes;
            }
            else
            {
                LogWriter = null;
            }
        }

    }
}
