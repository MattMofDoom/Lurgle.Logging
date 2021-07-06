using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using Lurgle.Logging.Classes;
using Lurgle.Logging.Destructurers;
using Lurgle.Logging.Enrichers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Lurgle.Logging
{
    /// <summary>
    ///     Static Lurgle.Logging instance that provides an interface to properties and methods for logging
    /// </summary>
    public static class Logging
    {
        private const string LogNameDate = "{0}-{1}";
        private const string LogTemplate = "{0}-";
        private const string DateIso = "yyyyMMdd";

        private const string
            Initialising = "Initialising event sources ..."; // ReSharper disable MemberCanBePrivate.Global

        /// <summary>
        ///     Correlation ID cache for individual threads
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static CorrelationCache Cache;

        // ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // ReSharper disable CollectionNeverQueried.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Global
        /// <summary>
        ///     String format for adding the method name to logs
        /// </summary>
        public const string LogMethod = "[{0}] - {1}";

        /// <summary>
        ///     Current Lurgle.Logging configuration
        /// </summary>
        public static LoggingConfig Config { get; private set; }

        /// <summary>
        ///     Currently configured Serilog Logger
        /// </summary>
        public static Logger LogWriter { get; private set; }

        private static List<LogProperty> CommonProperties { get; } = new List<LogProperty>();

        /// <summary>
        ///     Current Correlation Id
        /// </summary>
        public static string CorrelationId { get; private set; }

        /// <summary>
        ///     Dictionary of <see cref="FailureReason" /> for why a given <see cref="LogType" /> failed
        /// </summary>
        public static Dictionary<LogType, FailureReason> LogFailures { get; private set; }

        /// <summary>
        ///     List of enabled <see cref="LogType" />
        /// </summary>
        public static List<LogType> EnabledLogs { get; private set; }

        /// <summary>
        ///     Flush logs and dispose the logging interface. Used for application shutdown.
        ///     <para />
        ///     If this is called and then an attempt is made to write to the log, the log will be automatically initialised again.
        /// </summary>
        public static void Close()
        {
            LogWriter?.Dispose();

            LogWriter = null;
        }

        /// <summary>
        ///     Return today's logfile for file logs
        /// </summary>
        /// <returns></returns>
        public static string GetLogFile()
        {
            return Path.Combine(Config.LogFolder,
                string.Format(LogNameDate, Config.LogName, DateTime.Now.ToString(DateIso)));
        }

        /// <summary>
        ///     Retrieve a logging configuration with enrichers and minimum log level
        /// </summary>
        /// <returns></returns>
        private static LoggerConfiguration GetLogConfig()
        {
            var config = new LoggerConfiguration();
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
                .Destructure.WithMaskProperties()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithMemoryUsage()
                .Enrich.WithProperty(nameof(Config.AppName), Config.AppName)
                .Enrich.WithProperty(nameof(Config.AppVersion), Config.AppVersion);
        }

        /// <summary>
        ///     Generate or set the <see cref="CorrelationId" /> or add/update one in the <see cref="Logging.Cache" />
        ///     <para />
        ///     CorrelationId is a static property if <see cref="LoggingConfig.EnableCorrelationCache" /> is not enabled. />
        ///     <para />
        ///     You can generate a new CorrelationId with <see cref="NewCorrelationId()" />
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public static string SetCorrelationId(string correlationId = null)
        {
            if (Config.EnableCorrelationCache)
            {
                if (string.IsNullOrEmpty(correlationId))
                    return Cache.Contains(Thread.CurrentThread.ManagedThreadId)
                        ? Cache.Get(Thread.CurrentThread.ManagedThreadId)
                        : NewCorrelationId();
                Cache.Replace(Thread.CurrentThread.ManagedThreadId, correlationId);
                return correlationId;
            }

            if (!string.IsNullOrEmpty(correlationId))
            {
                CorrelationId = correlationId;

                return correlationId;
            }

            if (!string.IsNullOrEmpty(CorrelationId)) return CorrelationId;
            var corrId = NewCorrelationId();
            CorrelationId = corrId;

            return corrId;
        }

        /// <summary>
        ///     Return a new CorrelationId and update <see cref="CorrelationId" />
        /// </summary>
        /// <returns></returns>
        public static string NewCorrelationId()
        {
            if (Config.EnableCorrelationCache)
            {
                var cacheId = Guid.NewGuid().ToString();
                Cache.Replace(Thread.CurrentThread.ManagedThreadId, cacheId);
                return cacheId;
            }

            var corrId = Guid.NewGuid().ToString();
            CorrelationId = corrId;

            return corrId;
        }

        /// <summary>
        ///     Return a dictionary comprised of the base properties that we pass to each event
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static List<LogProperty> GetBaseProperties(string correlationId = null,
            string methodName = null, string sourceFilePath = null, int sourceLineNumber = -1)
        {
            //Automatically include static common properties
            var propertyValues = CommonProperties.ToList();

            propertyValues.Add(new LogProperty(nameof(CorrelationId), SetCorrelationId(correlationId)));

            if (Config.EnableMethodNameProperty && !string.IsNullOrEmpty(methodName))
                propertyValues.Add(new LogProperty("MethodName", methodName));

            if (Config.EnableSourceFileProperty && !string.IsNullOrEmpty(sourceFilePath))
                propertyValues.Add(new LogProperty("SourceFile", sourceFilePath));

            if (Config.EnableLineNumberProperty && sourceLineNumber > 0)
                propertyValues.Add(new LogProperty("LineNumber", sourceLineNumber));

            return propertyValues;
        }

        /// <summary>
        ///     Add an additional static property for logging
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="destructure"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static void AddCommonProperty(string name, object value, bool destructure = false, bool update = false)
        {
            if (string.IsNullOrEmpty(name)) return;
            foreach (var property in CommonProperties.Where(property =>
                property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                if (!update) return;
                var isMask = Config != null && Config.LogMaskProperties.Any(maskProperty =>
                    maskProperty.Equals(name, StringComparison.OrdinalIgnoreCase));
                property.Value = isMask ? MaskProperty(value) : value;
                property.Destructure = destructure;
            }

            if (Config == null || Config.LogMaskPolicy.Equals(MaskPolicy.None))
            {
                CommonProperties.Add(new LogProperty(name, value, destructure));
            }
            else
            {
                var isMask = Config.LogMaskProperties.Any(maskProperty =>
                    maskProperty.Equals(name, StringComparison.OrdinalIgnoreCase));

                CommonProperties.Add(new LogProperty(name, isMask ? MaskProperty(value) : value, destructure));
            }
        }

        /// <summary>
        ///     Add an additional set of static properties for logging
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <param name="destructure"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static void AddCommonProperty(Dictionary<string, object> propertyPairs, bool destructure = false,
            bool update = false)
        {
            foreach (var values in propertyPairs)
            {
                var exists = false;
                if (string.IsNullOrEmpty(values.Key)) continue;
                foreach (var property in CommonProperties.Where(property =>
                    property.Name.Equals(values.Key, StringComparison.OrdinalIgnoreCase)))
                    if (!update)
                    {
                        exists = true;
                    }
                    else
                    {
                        var isMask = Config != null && Config.LogMaskProperties.Any(maskProperty =>
                            maskProperty.Equals(values.Key, StringComparison.OrdinalIgnoreCase));

                        CommonProperties.Add(
                            new LogProperty(values.Key, isMask ? MaskProperty(values.Value) : values.Value));
                        property.Destructure = property.Destructure;
                    }

                if (exists) continue;
                if (Config == null || Config.LogMaskPolicy.Equals(MaskPolicy.None))
                {
                    CommonProperties.Add(new LogProperty(values.Key, values.Value, destructure));
                }
                else
                {
                    var isMask = Config.LogMaskProperties.Any(maskProperty =>
                        maskProperty.Equals(values.Key, StringComparison.OrdinalIgnoreCase));

                    CommonProperties.Add(
                        new LogProperty(values.Key, isMask ? MaskProperty(values.Value) : values.Value));
                }
            }
        }

        /// <summary>
        ///     Clear the list of static properties
        /// </summary>
        public static void ResetCommonProperties()
        {
            CommonProperties.Clear();
        }

        /// <summary>
        ///     Returns a masked property based on the masking policy
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public static object MaskProperty(object propertyValue)
        {
            switch (Config.LogMaskPolicy)
            {
                case MaskPolicy.MaskWithString:
                    return Config.LogMaskPattern;
                case MaskPolicy.MaskLettersAndNumbers:
                    var replaceValue = Regex.Replace(propertyValue.ToString(), "[A-Z]", Config.LogMaskCharacter,
                        RegexOptions.IgnoreCase);
                    return Regex.Replace(replaceValue, "\\d", Config.LogMaskDigit);
                default:
                    return propertyValue;
            }
        }

        /// <summary>
        ///     Test that the configured log type can be used
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="correlationId"></param>
        /// <param name="manageSource"></param>
        /// <param name="fileName"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        private static bool TestLogConfig(LogType logType, string correlationId = null, bool manageSource = true,
            string fileName = null,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var testConfig = GetLogConfig();
            try
            {
                switch (logType)
                {
                    case LogType.Console:
                        testConfig
                            .WriteTo
                            .Console((LogEventLevel) Config.LogLevelConsole, Config.LogFormatConsole,
                                theme: Config.LogConsoleTheme);
                        break;
                    case LogType.EventLog:
                        try
                        {
                            testConfig
                                .WriteTo
                                .EventLog(Config.LogEventSource, Config.LogEventName, ".", manageSource,
                                    Config.LogFormatEvent, null,
                                    (LogEventLevel) Config.LogLevelEvent);
                        }
                        catch (TypeInitializationException)
                        {
                            return false;
                        }
                        catch (SecurityException)
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
                            testConfig
                                .WriteTo
                                .File(fileName ?? string.Empty, (LogEventLevel) Config.LogLevelFile,
                                    Config.LogFormatFile,
                                    rollingInterval: RollingInterval.Day,
                                    retainedFileCountLimit: Config.LogDays, shared: Config.LogShared,
                                    buffered: Config.LogBuffered,
                                    flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                        else
                            testConfig
                                .WriteTo
                                .File(new CompactJsonFormatter(), fileName ?? string.Empty,
                                    (LogEventLevel) Config.LogLevelFile,
                                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: Config.LogDays,
                                    shared: Config.LogShared,
                                    buffered: Config.LogBuffered,
                                    flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));

                        break;
                    case LogType.Seq:
                        if (!string.IsNullOrEmpty(Config.LogSeqApiKey))
                            testConfig
                                .WriteTo
                                .Seq(Config.LogSeqServer, (LogEventLevel) Config.LogLevelSeq,
                                    apiKey: Config.LogSeqApiKey, messageHandler: new SeqClient());
                        else
                            testConfig
                                .WriteTo
                                .Seq(Config.LogSeqServer, (LogEventLevel) Config.LogLevelSeq,
                                    messageHandler: new SeqClient());

                        break;
                }

                var testWriter = testConfig.CreateLogger();
                // Only write the Initialising event if enabled
                if (Config.LogWriteInit)
                    testWriter
                        .ForContext(new PropertyBagEnricher().Add(GetBaseProperties(correlationId, methodName,
                            sourceFilePath, sourceLineNumber)))
                        .Verbose(Initialising);

                testWriter.Dispose();
                return true;
            }
            catch (TypeInitializationException)
            {
                return false;
            }
            catch (SecurityException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Set the logging config. This will only set/update the config if there is no LogWriter currently set.
        /// </summary>
        public static void SetConfig(LoggingConfig logConfig = null)
        {
            if (LogWriter == null) Config = LoggingConfig.GetConfig(logConfig);
        }

        /// <summary>
        ///     Initialise the logging interface. Checks that the configured log types are available and alerts if they aren't.
        /// </summary>
        public static void Init(string correlationId = null, [CallerMemberName] string methodName = null,
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            //We can only initialise on a running config. If one is not set via Logging.SetConfig, we will attempt to read from app.config.
            if (Config == null) SetConfig();

            var manageSource = true;

            if (Config == null) return;
            if (Config.EnableCorrelationCache)
                Cache = new CorrelationCache(Config.CorrelationCacheExpiry);
            var logFolder = Config.LogFolder;
            var fileName = string.Empty;

            var logTypes = Config.LogType.ToList();
            LogFailures = new Dictionary<LogType, FailureReason>();
            EnabledLogs = new List<LogType>();

            //If event log is enabled, test that we can create sources and/or write logs
            if (logTypes.Contains(LogType.EventLog) && !TestLogConfig(LogType.EventLog, correlationId))
            {
                manageSource = false;
                if (!TestLogConfig(LogType.EventLog, correlationId))
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
                    fileName = Path.Combine(logFolder,
                        string.Format(string.Concat(LogTemplate, Config.LogExtension), Config.LogName));

                    if (!TestLogConfig(LogType.File, correlationId, false, fileName))
                    {
                        LogFailures.Add(LogType.File, FailureReason.LogTestFailed);
                        logTypes.Remove(LogType.File);
                    }
                }
            }

            if (logTypes.Contains(LogType.Seq) && !TestLogConfig(LogType.Seq, correlationId))
            {
                LogFailures.Add(LogType.Seq, FailureReason.LogTestFailed);
                logTypes.Remove(LogType.Seq);
            }

            //With all that out of the way, we can create the final log config
            if (!logTypes.Count.Equals(0))
            {
                var logConfig = GetLogConfig();

                if (logTypes.Contains(LogType.Console))
                    logConfig
                        .WriteTo
                        .Console((LogEventLevel) Config.LogLevelConsole, Config.LogFormatConsole,
                            theme: Config.LogConsoleTheme);

                if (logTypes.Contains(LogType.Seq))
                {
                    if (!string.IsNullOrEmpty(Config.LogSeqApiKey))
                        logConfig
                            .WriteTo
                            .Seq(Config.LogSeqServer, (LogEventLevel) Config.LogLevelSeq, apiKey: Config.LogSeqApiKey,
                                messageHandler: new SeqClient());

                    else
                        logConfig
                            .WriteTo
                            .Seq(Config.LogSeqServer, (LogEventLevel) Config.LogLevelSeq,
                                messageHandler: new SeqClient());
                }

                if (logTypes.Contains(LogType.File))
                {
                    if (Config.LogFileType.Equals(LogFileFormat.Text))
                        logConfig
                            .WriteTo
                            .File(fileName, (LogEventLevel) Config.LogLevelFile, Config.LogFormatFile,
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: Config.LogDays, shared: Config.LogShared,
                                buffered: Config.LogBuffered, flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                    else
                        logConfig
                            .WriteTo
                            .File(new CompactJsonFormatter(), fileName, (LogEventLevel) Config.LogLevelFile,
                                rollingInterval: RollingInterval.Day, retainedFileCountLimit: Config.LogDays,
                                shared: Config.LogShared, buffered: Config.LogBuffered,
                                flushToDiskInterval: new TimeSpan(0, 0, Config.LogFlush));
                }

                if (logTypes.Contains(LogType.EventLog))
                    logConfig.WriteTo.EventLog(Config.LogEventSource, Config.LogEventName, ".",
                        manageSource, Config.LogFormatEvent, null, (LogEventLevel) Config.LogLevelEvent);

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