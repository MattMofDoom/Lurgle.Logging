using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Serilog.Sinks.SystemConsole.Themes;

namespace Lurgle.Logging
{
    // ReSharper disable UnusedMember.Global
    /// <summary>
    ///     Logging configuration. Loaded from AppSettings if available but can be configured from code.
    /// </summary>
    public class LoggingConfig
    {
        /// <summary>
        ///     LoggingConfig constructor
        /// </summary>
        private LoggingConfig()
        {
        }

        /// <summary>
        ///     Constructor that permits passing a config and optional overrides of any property
        /// </summary>
        /// <param name="config"></param>
        /// <param name="enableMethodNameProperty"></param>
        /// <param name="enableSourceFileProperty"></param>
        /// <param name="enableLineNumberProperty"></param>
        /// <param name="logWriteInit"></param>
        /// <param name="appName"></param>
        /// <param name="appVersion"></param>
        /// <param name="logType"></param>
        /// <param name="logMaskProperties"></param>
        /// <param name="logMaskPattern"></param>
        /// <param name="logMaskPolicy"></param>
        /// <param name="logMaskCharacter"></param>
        /// <param name="logMaskDigit"></param>
        /// <param name="logConsoleTheme"></param>
        /// <param name="logFolder"></param>
        /// <param name="logName"></param>
        /// <param name="logExtension"></param>
        /// <param name="logEventSource"></param>
        /// <param name="logEventName"></param>
        /// <param name="logLevel"></param>
        /// <param name="logLevelConsole"></param>
        /// <param name="logLevelFile"></param>
        /// <param name="logLevelEvent"></param>
        /// <param name="logLevelSeq"></param>
        /// <param name="logFileType"></param>
        /// <param name="logDays"></param>
        /// <param name="logFlush"></param>
        /// <param name="logShared"></param>
        /// <param name="logBuffered"></param>
        /// <param name="logSeqServer"></param>
        /// <param name="logSeqApiKey"></param>
        /// <param name="logFormatConsole"></param>
        /// <param name="logFormatEvent"></param>
        /// <param name="logFormatFile"></param>
        public LoggingConfig(LoggingConfig config = null, bool? enableMethodNameProperty = null,
            bool? enableSourceFileProperty = null,
            bool? enableLineNumberProperty = null, bool? logWriteInit = null, string appName = null,
            string appVersion = null,
            List<LogType> logType = null,
            List<string> logMaskProperties = null, string logMaskPattern = null, MaskPolicy? logMaskPolicy = null,
            string logMaskCharacter = null, string logMaskDigit = null, ConsoleThemeType? logConsoleTheme = null,
            string logFolder = null, string logName = null, string logExtension = null, string logEventSource = null,
            string logEventName = null,
            LurgLevel? logLevel = null, LurgLevel? logLevelConsole = null,
            LurgLevel? logLevelFile = null, LurgLevel? logLevelEvent = null, LurgLevel? logLevelSeq = null,
            LogFileFormat? logFileType = null, int? logDays = null, int? logFlush = null, bool? logShared = null,
            bool? logBuffered = null, string logSeqServer = null, string logSeqApiKey = null,
            string logFormatConsole = null, string logFormatEvent = null, string logFormatFile = null)

        {
            if (config != null)
            {
                EnableMethodNameProperty = config.EnableMethodNameProperty;
                EnableSourceFileProperty = config.EnableSourceFileProperty;
                EnableLineNumberProperty = config.EnableLineNumberProperty;
                LogWriteInit = config.LogWriteInit;
                AppName = config.AppName;
                AppVersion = config.AppVersion;
                LogType = config.LogType;
                LogMaskProperties = config.LogMaskProperties;
                LogMaskPattern = config.LogMaskPattern;
                LogMaskPolicy = config.LogMaskPolicy;
                LogMaskCharacter = config.LogMaskCharacter;
                LogMaskDigit = config.LogMaskDigit;
                LogConsoleTheme = config.LogConsoleTheme;
                LogFolder = config.LogFolder;
                LogName = config.LogName;
                LogExtension = config.LogExtension;
                LogEventSource = config.LogEventSource;
                LogEventName = config.LogEventName;
                LogLevel = config.LogLevel;
                LogLevelConsole = config.LogLevelConsole;
                LogLevelFile = config.LogLevelFile;
                LogLevelEvent = config.LogLevelEvent;
                LogLevelSeq = config.LogLevelSeq;
                LogFileType = config.LogFileType;
                LogDays = config.LogDays;
                LogFlush = config.LogFlush;
                LogShared = config.LogShared;
                LogBuffered = config.LogBuffered;
                LogSeqServer = config.LogSeqServer;
                LogSeqApiKey = config.LogSeqApiKey;
                LogFormatConsole = config.LogFormatConsole;
                LogFormatEvent = config.LogFormatEvent;
                LogFormatFile = config.LogFormatFile;
            }

            if (enableMethodNameProperty != null)
                EnableMethodNameProperty = (bool) enableMethodNameProperty;
            if (enableSourceFileProperty != null)
                EnableSourceFileProperty = (bool) enableSourceFileProperty;
            if (enableLineNumberProperty != null)
                EnableLineNumberProperty = (bool) enableLineNumberProperty;
            if (logWriteInit != null)
                LogWriteInit = (bool) logWriteInit;
            if (!string.IsNullOrEmpty(appName))
                AppName = appName;
            if (!string.IsNullOrEmpty(appVersion))
                AppVersion = appVersion;
            if (logType != null)
                LogType = logType;
            if (logMaskProperties != null)
                LogMaskProperties = logMaskProperties;
            if (!string.IsNullOrEmpty(logMaskPattern))
                LogMaskPattern = logMaskPattern;
            if (logMaskPolicy != null)
                LogMaskPolicy = (MaskPolicy) logMaskPolicy;
            if (!string.IsNullOrEmpty(logMaskCharacter))
                LogMaskCharacter = logMaskCharacter;
            if (!string.IsNullOrEmpty(logMaskDigit))
                LogMaskDigit = logMaskDigit;
            if (logConsoleTheme != null)
                LogConsoleTheme = GetConsoleTheme((ConsoleThemeType) logConsoleTheme);
            if (!string.IsNullOrEmpty(logFolder))
                LogFolder = logFolder;
            if (!string.IsNullOrEmpty(logName))
                LogName = logName;
            if (!string.IsNullOrEmpty(logExtension))
                LogExtension = logExtension;
            if (logEventSource != null)
                LogEventSource = logEventSource;
            if (logEventName != null)
                LogEventName = logEventName;
            if (logLevel != null)
                LogLevel = (LurgLevel) logLevel;
            if (logLevelConsole != null)
                LogLevelConsole = (LurgLevel) logLevelConsole;
            if (logLevelFile != null)
                LogLevelFile = (LurgLevel) logLevelFile;
            if (logLevelEvent != null)
                LogLevelEvent = (LurgLevel) logLevelEvent;
            if (logLevelSeq != null)
                LogLevelSeq = (LurgLevel) logLevelSeq;
            if (logFileType != null)
                LogFileType = (LogFileFormat) logFileType;
            if (logDays != null)
                LogDays = (int) logDays;
            if (logFlush != null)
                LogFlush = (int) logFlush;
            if (logShared != null)
                LogShared = (bool) logShared;
            if (logBuffered != null)
                LogBuffered = (bool) logBuffered;
            if (!string.IsNullOrEmpty(logSeqServer))
                LogSeqServer = logSeqServer;
            if (!string.IsNullOrEmpty(logSeqApiKey))
                LogSeqApiKey = logSeqApiKey;
            if (!string.IsNullOrEmpty(logFormatConsole))
                LogFormatConsole = logFormatConsole;
            if (!string.IsNullOrEmpty(logFormatFile))
                LogFormatEvent = logFormatEvent;
            if (!string.IsNullOrEmpty(logFormatFile))
                LogFormatFile = logFormatFile;
        }

        /// <summary>
        ///     Set to false to disable the MethodName property
        /// </summary>
        public bool EnableMethodNameProperty { get; private set; } = true;

        /// <summary>
        ///     Set to false to disable the SourceFile property
        /// </summary>
        public bool EnableSourceFileProperty { get; private set; } = true;

        /// <summary>
        ///     Set to false to disable the LineNumber property
        /// </summary>
        public bool EnableLineNumberProperty { get; private set; } = true;

        /// <summary>
        ///     Write an "Initialising" event during Init's call to TestLogConfig
        /// </summary>
        public bool LogWriteInit { get; private set; }

        /// <summary>
        ///     Meaningful app name that is used for logging. Will be auto-set if not specified.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        ///     App version will be determined from the binary version, but can be overriden
        /// </summary>
        public string AppVersion { get; private set; }

        /// <summary>
        ///     List of valid log types. At least one must be specified or an exception will be raised.
        /// </summary>
        public List<LogType> LogType { get; private set; }

        /// <summary>
        ///     Define properties that should be masked in logging
        /// </summary>
        public List<string> LogMaskProperties { get; private set; } = new List<string>();

        /// <summary>
        ///     Define the applicable policy for masking - MaskWithString, MaskLettersAndNumbers
        /// </summary>
        public MaskPolicy LogMaskPolicy { get; private set; } = MaskPolicy.None;

        /// <summary>
        ///     Define the string pattern to use for masking if MaskWithString policy is used
        /// </summary>
        public string LogMaskPattern { get; private set; } = "XXXXXX";

        /// <summary>
        ///     Define the mask character to use for non-digit values in masking if MaskLettersAndNumbers is used
        /// </summary>
        public string LogMaskCharacter { get; private set; } = "X";

        /// <summary>
        ///     Define the mask character to use for digit values in masking if MaskLettersAndNumbers is used
        /// </summary>
        public string LogMaskDigit { get; private set; } = "*";

        /// <summary>
        ///     Select a console theme, defaults to Literate
        /// </summary>
        public ConsoleTheme LogConsoleTheme { get; private set; }

        /// <summary>
        ///     Log folder for file logs
        /// </summary>
        public string LogFolder { get; private set; }

        /// <summary>
        ///     Log filename prefix for file logs. A hyphen and date will be auto-appended.
        /// </summary>
        public string LogName { get; private set; }

        /// <summary>
        ///     Log file extension. Defaults to .log
        /// </summary>
        public string LogExtension { get; private set; }

        /// <summary>
        ///     Event Source name for Windows Event Logs
        /// </summary>
        public string LogEventSource { get; private set; }

        /// <summary>
        ///     Log name for Windows Event Logs (eg. Application)
        /// </summary>
        public string LogEventName { get; private set; }

        /// <summary>
        ///     Minimum overall log level that will be written - Verbose, Debug, Information, Warning, Error, Fatal
        /// </summary>
        public LurgLevel LogLevel { get; private set; }

        /// <summary>
        ///     Minimum log level accepted by the Console sink
        /// </summary>
        public LurgLevel LogLevelConsole { get; private set; }

        /// <summary>
        ///     Minimum log level accepted by the File sink
        /// </summary>
        public LurgLevel LogLevelFile { get; private set; }

        /// <summary>
        ///     Minimum log level accepted by the Event Log sink
        /// </summary>
        public LurgLevel LogLevelEvent { get; private set; }

        /// <summary>
        ///     Minimum log level accepted by the Seq sink
        /// </summary>
        public LurgLevel LogLevelSeq { get; private set; }

        /// <summary>
        ///     Output files as text or Compact Json - defaults to text. If set to Json, <see cref="LogFormatFile" /> will not be
        ///     used/>
        /// </summary>
        public LogFileFormat LogFileType { get; private set; }

        /// <summary>
        ///     How many days log files will be retained - default is 31
        /// </summary>
        public int LogDays { get; private set; }

        /// <summary>
        ///     How many seconds before log file writes are flushed to disk - default is 1
        /// </summary>
        public int LogFlush { get; private set; }

        /// <summary>
        ///     Allow log file to be accessed in shared mode. Cannot be used with buffered mode.
        /// </summary>
        public bool LogShared { get; private set; }

        /// <summary>
        ///     Allow log file to have buffered writes. Cannot be used with shared mode.
        /// </summary>
        public bool LogBuffered { get; private set; }

        /// <summary>
        ///     URL for the Seq server, eg. https://seq.domain.com
        /// </summary>
        public string LogSeqServer { get; private set; }

        /// <summary>
        ///     API key for Seq. If empty, an API key will not be used.
        /// </summary>
        public string LogSeqApiKey { get; private set; }

        /// <summary>
        ///     Logging format for the Console. Default is {Message}{NewLine}
        /// </summary>
        public string LogFormatConsole { get; private set; }

        /// <summary>
        ///     Logging format for the Event Log. Default is ({ThreadId}) {Message}{NewLine}{NewLine}{Exception}
        /// </summary>
        public string LogFormatEvent { get; private set; }

        /// <summary>
        ///     Logging format for File logs. Default is {Timestamp:yyyy-MM-dd HH:mm:ss}: ({ThreadId}) [{Level}] {Message}{NewLine}
        /// </summary>
        public string LogFormatFile { get; private set; }

        /// <summary>
        ///     Get a config. Optionally a LoggingConfig can be passed
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static LoggingConfig GetConfig(LoggingConfig config = null)
        {
            LoggingConfig loggingConfig;
            if (config == null)
                loggingConfig = new LoggingConfig
                {
                    EnableMethodNameProperty =
                        GetBool(ConfigurationManager.AppSettings["EnableMethodNameProperty"], true),
                    EnableSourceFileProperty =
                        GetBool(ConfigurationManager.AppSettings["EnableSourceFileProperty"], true),
                    EnableLineNumberProperty =
                        GetBool(ConfigurationManager.AppSettings["EnableLineNumberProperty"], true),
                    LogWriteInit =
                        GetBool(ConfigurationManager.AppSettings["LogWriteInit"]),
                    AppName = ConfigurationManager.AppSettings["AppName"],
                    LogType = GetLogType(ConfigurationManager.AppSettings["LogType"]),
                    LogMaskProperties = GetMaskProperties(ConfigurationManager.AppSettings["LogMaskProperties"]),
                    LogMaskPattern = ConfigurationManager.AppSettings["LogMaskPattern"],
                    LogMaskPolicy = GetMaskPolicy(ConfigurationManager.AppSettings["LogMaskPolicy"]),
                    LogMaskCharacter = GetChar(ConfigurationManager.AppSettings["LogMaskCharacter"]),
                    LogMaskDigit = GetChar(ConfigurationManager.AppSettings["LogMaskDigit"]),
                    LogConsoleTheme =
                        GetConsoleTheme(GetConsoleThemeType(ConfigurationManager.AppSettings["ConsoleTheme"])),
                    LogFolder = ConfigurationManager.AppSettings["LogFolder"],
                    LogName = ConfigurationManager.AppSettings["LogName"],
                    LogExtension = ConfigurationManager.AppSettings["LogExtension"],
                    LogEventSource = ConfigurationManager.AppSettings["LogEventSource"],
                    LogEventName = ConfigurationManager.AppSettings["LogEventName"],
                    LogLevel = GetEventLevel(ConfigurationManager.AppSettings["LogLevel"]),
                    LogLevelConsole = GetEventLevel(ConfigurationManager.AppSettings["LogLevelConsole"]),
                    LogLevelFile = GetEventLevel(ConfigurationManager.AppSettings["LogLevelFile"]),
                    LogLevelEvent = GetEventLevel(ConfigurationManager.AppSettings["LogLevelEvent"]),
                    LogLevelSeq = GetEventLevel(ConfigurationManager.AppSettings["LogLevelSeq"]),
                    LogFileType = GetLogFileType(ConfigurationManager.AppSettings["LogFileType"]),
                    LogDays = GetInt(ConfigurationManager.AppSettings["LogDays"]),
                    LogFlush = GetInt(ConfigurationManager.AppSettings["LogFlush"]),
                    LogShared = GetBool(ConfigurationManager.AppSettings["LogShared"]),
                    LogBuffered = GetBool(ConfigurationManager.AppSettings["LogBuffered"], true),
                    LogSeqServer = ConfigurationManager.AppSettings["LogSeqServer"],
                    LogSeqApiKey = ConfigurationManager.AppSettings["LogSeqApiKey"],
                    LogFormatConsole = ConfigurationManager.AppSettings["LogFormatConsole"],
                    LogFormatEvent = ConfigurationManager.AppSettings["LogFormatEvent"],
                    LogFormatFile = ConfigurationManager.AppSettings["LogFormatFile"]
                };
            else
                loggingConfig = config;

            var isSuccess = true;

            //If AppName is not specified in config, attempt to populate it. Populate AppVersion while we're at it.
            try
            {
                if (string.IsNullOrEmpty(loggingConfig.AppName))
                    loggingConfig.AppName = Assembly.GetEntryAssembly()?.GetName().Name;

                loggingConfig.AppVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
                if (string.IsNullOrEmpty(loggingConfig.LogFolder))
                    loggingConfig.LogFolder = Assembly.GetEntryAssembly()?.Location;
            }
            catch
            {
                isSuccess = false;
            }

            if (!isSuccess)
                try
                {
                    if (string.IsNullOrEmpty(loggingConfig.AppName))
                        loggingConfig.AppName = Assembly.GetExecutingAssembly().GetName().Name;

                    loggingConfig.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (string.IsNullOrEmpty(loggingConfig.LogFolder))
                        loggingConfig.LogFolder = Assembly.GetExecutingAssembly().GetName().CodeBase;
                }
                catch
                {
                    //We surrender ...
                    loggingConfig.AppVersion = string.Empty;
                }

            if (string.IsNullOrEmpty(loggingConfig.LogMaskPattern)) loggingConfig.LogMaskPattern = "XXXXXX";

            if (string.IsNullOrEmpty(loggingConfig.LogMaskCharacter)) loggingConfig.LogMaskCharacter = "X";

            if (string.IsNullOrEmpty(loggingConfig.LogMaskDigit)) loggingConfig.LogMaskDigit = "*";

            if (loggingConfig.LogDays.Equals(-1)) loggingConfig.LogDays = 31;

            if (loggingConfig.LogFlush.Equals(-1)) loggingConfig.LogFlush = 1;

            if (loggingConfig.LogBuffered && loggingConfig.LogShared) loggingConfig.LogShared = false;

            if (string.IsNullOrEmpty(loggingConfig.LogName)) loggingConfig.LogName = loggingConfig.AppName;

            if (string.IsNullOrEmpty(loggingConfig.LogExtension)) loggingConfig.LogExtension = ".log";

            if (string.IsNullOrEmpty(loggingConfig.LogEventSource))
                loggingConfig.LogEventSource = loggingConfig.AppName;

            if (string.IsNullOrEmpty(loggingConfig.LogEventName)) loggingConfig.LogEventName = "Application";

            if (string.IsNullOrEmpty(loggingConfig.LogFormatConsole))
                loggingConfig.LogFormatConsole = "{Message}{NewLine}";

            if (string.IsNullOrEmpty(loggingConfig.LogFormatEvent))
                loggingConfig.LogFormatEvent = "({ThreadId}) {Message}{NewLine}{NewLine}{Exception}";

            if (string.IsNullOrEmpty(loggingConfig.LogFormatFile))
                loggingConfig.LogFormatFile =
                    "{Timestamp:yyyy-MM-dd HH:mm:ss}: ({ThreadId}) [{Level}] {Message}{NewLine}";

            return loggingConfig;
        }

        /// <summary>
        ///     Parse the configured log level <see cref="string" /> into a <see cref="LogLevel" /> value
        /// </summary>
        /// <param name="configValue">Setting string</param>
        /// <returns></returns>
        private static LurgLevel GetEventLevel(string configValue)
        {
            if (string.IsNullOrEmpty(configValue)) return LurgLevel.Verbose;
            return Enum.TryParse(configValue, true, out LurgLevel eventLevel) ? eventLevel : LurgLevel.Verbose;
        }

        /// <summary>
        ///     Parse a comma delimited list of property names to mask
        /// </summary>
        /// <param name="configValue"></param>
        /// <returns></returns>
        private static List<string> GetMaskProperties(string configValue)
        {
            return (configValue ?? "")
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }

        /// <summary>
        ///     Parse the configured mask policy into a <see cref="MaskPolicy" />
        /// </summary>
        /// <param name="configValue"></param>
        /// <returns></returns>
        private static MaskPolicy GetMaskPolicy(string configValue)
        {
            if (string.IsNullOrEmpty(configValue)) return MaskPolicy.None;
            return Enum.TryParse(configValue, true, out MaskPolicy maskPolicy) ? maskPolicy : MaskPolicy.None;
        }

        /// <summary>
        ///     Parse a config value into a single character string
        /// </summary>
        /// <param name="configValue"></param>
        /// <returns></returns>
        private static string GetChar(string configValue)
        {
            return !string.IsNullOrEmpty(configValue) ? configValue[0].ToString() : string.Empty;
        }

        /// <summary>
        ///     Parse a comma-delimited logType <see cref="string" /> into a list of <see cref="Lurgle.Logging.LogType" />
        /// </summary>
        /// <param name="configValue">Setting string (comma-delimited)</param>
        /// <returns></returns>
        private static List<LogType> GetLogType(string configValue)
        {
            var logTypes = new List<LogType>();

            if (!string.IsNullOrEmpty(configValue))
            {
                foreach (var logString in configValue.Split(','))
                    if (Enum.TryParse(logString, true, out LogType logTypeValue))
                        logTypes.Add(logTypeValue);
            }
            else
            {
                logTypes.Add(Lurgle.Logging.LogType.Console);
            }

            return logTypes;
        }

        private static ConsoleThemeType GetConsoleThemeType(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue) && Enum.TryParse(configValue, out ConsoleThemeType themeType))
                return themeType;

            return ConsoleThemeType.Literate;
        }

        /// <summary>
        ///     Parse a configured theme to a <see cref="ConsoleTheme" />
        /// </summary>
        /// <param name="themeType"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static ConsoleTheme GetConsoleTheme(ConsoleThemeType themeType)
        {
            switch (themeType)
            {
                case ConsoleThemeType.Literate:
                    return SystemConsoleTheme.Literate;
                case ConsoleThemeType.Grayscale:
                    return SystemConsoleTheme.Grayscale;
                case ConsoleThemeType.Colored:
                    return SystemConsoleTheme.Colored;
                case ConsoleThemeType.AnsiLiterate:
                    return AnsiConsoleTheme.Literate;
                case ConsoleThemeType.AnsiGrayscale:
                    return AnsiConsoleTheme.Grayscale;
                case ConsoleThemeType.AnsiCode:
                    return AnsiConsoleTheme.Code;
                default:
                    return SystemConsoleTheme.Literate;
            }
        }

        /// <summary>
        ///     Return the <see cref="ConsoleThemeType" /> of a given <see cref="ConsoleTheme" />
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static ConsoleThemeType GetConsoleThemeType(ConsoleTheme theme)
        {
            if (theme == SystemConsoleTheme.Literate)
                return ConsoleThemeType.Literate;

            if (theme == SystemConsoleTheme.Colored)
                return ConsoleThemeType.Colored;

            if (theme == SystemConsoleTheme.Grayscale)
                return ConsoleThemeType.Grayscale;

            if (theme == AnsiConsoleTheme.Literate)
                return ConsoleThemeType.AnsiLiterate;

            return theme == AnsiConsoleTheme.Grayscale ? ConsoleThemeType.AnsiGrayscale : ConsoleThemeType.AnsiCode;
        }

        /// <summary>
        ///     Parse a config value to a <see cref="LogFileFormat" />
        /// </summary>
        /// <param name="configValue"></param>
        /// <returns></returns>
        private static LogFileFormat GetLogFileType(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue) && Enum.TryParse(configValue, out LogFileFormat fileFormat))
                return fileFormat;

            return LogFileFormat.Text;
        }

        /// <summary>
        ///     Convert the supplied <see cref="object" /> to an <see cref="int" />
        ///     <para />
        ///     This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to an int</param>
        /// <returns></returns>
        private static int GetInt(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            if (int.TryParse(sourceString, out var destInt)) return destInt;

            return -1;
        }

        /// <summary>
        ///     Convert the supplied <see cref="object" /> to a <see cref="bool" />
        ///     <para />
        ///     This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to a bool</param>
        /// <param name="trueIfEmpty">Return true if the object is empty</param>
        /// <returns></returns>
        private static bool GetBool(object sourceObject, bool trueIfEmpty = false)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            return bool.TryParse(sourceString, out var destBool) ? destBool : trueIfEmpty;
        }
    }
}