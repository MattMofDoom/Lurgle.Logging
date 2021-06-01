using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Lurgle.Logging
{
    /// <summary>
    /// Logging configuration. Loaded from AppSettings if available but can be configured from code.
    /// </summary>
    public class LoggingConfig
    {
        /// <summary>
        /// Set to false to disable the MethodName property
        /// </summary>
        public bool EnableMethodNameProperty { get; set; } = true;
        /// <summary>
        /// Set to false to disable the SourceFile property
        /// </summary>
        public bool EnableSourceFileProperty { get; set; } = true;
        /// <summary>
        /// Set to false to disable the LineNumber property
        /// </summary>
        public bool EnableLineNumberProperty { get; set; } = true;
        /// <summary>
        /// Meaningful app name that is used for logging. Will be auto-set if not specified.
        /// </summary>
        public string AppName { get; set; }
        /// <summary>
        /// App version will be determined from the binary version, but can be overriden
        /// </summary>
        public string AppVersion { get; set; }
        /// <summary>
        /// List of valid log types. At least one must be specified or an exception will be raised.
        /// </summary>
        public List<LogType> LogType { get; set; }
        /// <summary>
        /// Define properties that should be masked in logging
        /// </summary>
        public List<string> LogMaskProperties { get; set; } = new List<string>();
        /// <summary>
        /// Define the applicable policy for masking - MaskWithString, MaskLettersAndNumbers
        /// </summary>
        public MaskPolicy LogMaskPolicy { get; set; } = MaskPolicy.None;
        /// <summary>
        /// Define the string pattern to use for masking if MaskWithString policy is used
        /// </summary>
        public string LogMaskPattern { get; set; } = "XXXXXX";
        /// <summary>
        /// Define the mask character to use for non-digit values in masking if MaskLettersAndNumbers is used
        /// </summary>
        public string LogMaskCharacter { get; set; } = "X";
        /// <summary>
        /// Define the mask character to use for digit values in masking if MaskLettersAndNumbers is used
        /// </summary>
        public string LogMaskDigit { get; set; } = "*";
        /// <summary>
        /// Select a console theme, defaults to Literate
        /// </summary>
        public ConsoleTheme LogConsoleTheme { get; set; }
        /// <summary>
        /// Log folder for file logs
        /// </summary>
        public string LogFolder { get; set; }
        /// <summary>
        /// Log filename prefix for file logs. A hyphen and date will be auto-appended.
        /// </summary>
        public string LogName { get; set; }
        /// <summary>
        /// Log file extension. Defaults to .log
        /// </summary>
        public string LogExtension { get; set; }
        /// <summary>
        /// Event Source name for Windows Event Logs
        /// </summary>
        public string LogEventSource { get; set; }
        /// <summary>
        /// Log name for Windows Event Logs (eg. Application)
        /// </summary>
        public string LogEventName { get; set; }
        /// <summary>
        /// Minimum overall log level that will be written - Verbose, Debug, Information, Warning, Error, Fatal
        /// </summary>
        public LurgLevel LogLevel { get; set; }
        /// <summary>
        /// Minimum log level accepted by the Console sink
        /// </summary>
        public LurgLevel LogLevelConsole { get; set; }
        /// <summary>
        /// Minimum log level accepted by the File sink
        /// </summary>
        public LurgLevel LogLevelFile { get; set; }
        /// <summary>
        /// Minimum log level accepted by the Event Log sink
        /// </summary>
        public LurgLevel LogLevelEvent { get; set; }
        /// <summary>
        /// Minimum log level accepted by the Seq sink
        /// </summary>
        public LurgLevel LogLevelSeq { get; set; }
        /// <summary>
        /// Output files as text or Compact Json - defaults to text. If set to Json, <see cref="LogFormatFile" /> will not be used/>
        /// </summary>
        public LogFileFormat LogFileType { get; set; }
        /// <summary>
        /// How many days log files will be retained - default is 31
        /// </summary>
        public int LogDays { get; set; }
        /// <summary>
        /// How many seconds before log file writes are flushed to disk - default is 1
        /// </summary>
        public int LogFlush { get; set; }
        /// <summary>
        /// Allow log file to be accessed in shared mode. Cannot be used with buffered mode.
        /// </summary>
        public bool LogShared { get; set; }
        /// <summary>
        /// Allow log file to have buffered writes. Cannot be used with shared mode.
        /// </summary>
        public bool LogBuffered { get; set; }
        /// <summary>
        /// URL for the Seq server, eg. https://seq.domain.com
        /// </summary>
        public string LogSeqServer { get; set; }
        /// <summary>
        /// API key for Seq. If empty, an API key will not be used.
        /// </summary>
        public string LogSeqApiKey { get; set; }
        /// <summary>
        /// Logging format for the Console. Default is {Message}{NewLine}
        /// </summary>
        public string LogFormatConsole { get; set; }
        /// <summary>
        /// Logging format for the Event Log. Default is ({ThreadId}) {Message}{NewLine}{NewLine}{Exception}
        /// </summary>
        public string LogFormatEvent { get; set; }
        /// <summary>
        /// Logging format for File logs. Default is {Timestamp:yyyy-MM-dd HH:mm:ss}: ({ThreadId}) [{Level}] {Message}{NewLine}
        /// </summary>
        public string LogFormatFile { get; set; }

        /// <summary>
        /// Get a config. Optionally a LoggingConfig can be passed 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static LoggingConfig GetConfig(LoggingConfig config = null)
        {
            LoggingConfig loggingConfig;
            if (config == null)
            {
                loggingConfig = new LoggingConfig()
                {
                    EnableMethodNameProperty = GetBool(ConfigurationManager.AppSettings["EnableMethodNameProperty"], true),
                    EnableSourceFileProperty = GetBool(ConfigurationManager.AppSettings["EnableSourceFileProperty"], true),
                    EnableLineNumberProperty = GetBool(ConfigurationManager.AppSettings["EnableLineNumberProperty"], true),
                    AppName = ConfigurationManager.AppSettings["AppName"],
                    LogType = GetLogType(ConfigurationManager.AppSettings["LogType"]),
                    LogMaskProperties = GetMaskProperties(ConfigurationManager.AppSettings["LogMaskProperties"]),
                    LogMaskPattern = ConfigurationManager.AppSettings["LogMaskPattern"],
                    LogMaskPolicy = GetMaskPolicy(ConfigurationManager.AppSettings["LogMaskPolicy"]),
                    LogMaskCharacter = GetChar(ConfigurationManager.AppSettings["LogMaskCharacter"]),
                    LogMaskDigit = GetChar(ConfigurationManager.AppSettings["LogMaskDigit"]),
                    LogConsoleTheme = GetConsoleTheme(ConfigurationManager.AppSettings["ConsoleTheme"]),
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
                    LogDays = GetInt(ConfigurationManager.AppSettings["LogMonths"]),
                    LogFlush = GetInt(ConfigurationManager.AppSettings["LogFlush"]),
                    LogShared = GetBool(ConfigurationManager.AppSettings["LogShared"]),
                    LogBuffered = GetBool(ConfigurationManager.AppSettings["LogBuffered"], true),
                    LogSeqServer = ConfigurationManager.AppSettings["LogSeqServer"],
                    LogSeqApiKey = ConfigurationManager.AppSettings["LogSeqApiKey"],
                    LogFormatConsole = ConfigurationManager.AppSettings["LogFormatConsole"],
                    LogFormatEvent = ConfigurationManager.AppSettings["LogFormatEvent"],
                    LogFormatFile = ConfigurationManager.AppSettings["LogFormatFile"]
                };
            }
            else
            {
                loggingConfig = config;
            }

            bool isSuccess = true;

            //If AppName is not specified in config, attempt to populate it. Populate AppVersion while we're at it.
            try
            {
                if (string.IsNullOrEmpty(loggingConfig.AppName))
                {
                    loggingConfig.AppName = Assembly.GetEntryAssembly().GetName().Name;
                }

                loggingConfig.AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                if (string.IsNullOrEmpty(loggingConfig.LogFolder))
                {
                    loggingConfig.LogFolder = Assembly.GetEntryAssembly().Location;
                }
            }
            catch
            {
                isSuccess = false;
            }

            if (!isSuccess)
            {
                try
                {
                    if (string.IsNullOrEmpty(loggingConfig.AppName))
                    {
                        loggingConfig.AppName = Assembly.GetExecutingAssembly().GetName().Name;
                    }

                    loggingConfig.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (string.IsNullOrEmpty(loggingConfig.LogFolder))
                    {
                        loggingConfig.LogFolder = Assembly.GetExecutingAssembly().GetName().CodeBase;
                    }
                }
                catch
                {
                    //We surrender ...
                    loggingConfig.AppVersion = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(loggingConfig.LogMaskPattern))
            {
                loggingConfig.LogMaskPattern = "******";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogMaskCharacter))
            {
                loggingConfig.LogMaskCharacter = "X";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogMaskDigit))
            {
                loggingConfig.LogMaskDigit = "*";
            }

            if (loggingConfig.LogDays.Equals(-1))
            {
                loggingConfig.LogDays = 31;
            }

            if (loggingConfig.LogFlush.Equals(-1))
            {
                loggingConfig.LogFlush = 1;
            }

            if (loggingConfig.LogBuffered && loggingConfig.LogShared)
            {
                loggingConfig.LogShared = false;
            }

            if (string.IsNullOrEmpty(loggingConfig.LogName))
            {
                loggingConfig.LogName = "Lurgle";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogExtension))
            {
                loggingConfig.LogExtension = ".log";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogEventSource))
            {
                loggingConfig.LogEventSource = loggingConfig.AppName;
            }

            if (string.IsNullOrEmpty(loggingConfig.LogEventName))
            {
                loggingConfig.LogEventName = "Application";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogFormatConsole))
            {
                loggingConfig.LogFormatConsole = "{Message}{NewLine}";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogFormatEvent))
            {
                loggingConfig.LogFormatEvent = "({ThreadId}) {Message}{NewLine}{NewLine}{Exception}";
            }

            if (string.IsNullOrEmpty(loggingConfig.LogFormatFile))
            {
                loggingConfig.LogFormatFile = "{Timestamp:yyyy-MM-dd HH:mm:ss}: ({ThreadId}) [{Level}] {Message}{NewLine}";
            }

            return loggingConfig;
        }

        /// <summary>
        /// Parse the configured log level <see cref="string"/> into a <see cref="LogLevel"/> value
        /// </summary>
        /// <param name="configValue">Setting string</param>
        /// <returns></returns>
        private static LurgLevel GetEventLevel(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue))
            {
                if (Enum.TryParse(configValue, true, out LurgLevel eventLevel))
                {
                    return eventLevel;
                }
            }

            return LurgLevel.Verbose;
        }

        private static List<string> GetMaskProperties(string configValue)
        {
            return (configValue ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }

        /// <summary>
        /// Parse the configured mask policy into a <see cref="MaskPolicy"/>
        /// </summary>
        /// <param name="configValue"></param>
        /// <returns></returns>
        private static MaskPolicy GetMaskPolicy(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue))
            {
                if (Enum.TryParse(configValue, true, out MaskPolicy maskPolicy))
                {
                    return maskPolicy;
                }
            }

            return MaskPolicy.None;
        }

        private static string GetChar(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue))
            {
                return configValue[0].ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Parse a comma-delimited logType <see cref="string"/> into a list of <see cref="Lurgle.Logging.LogType"/>
        /// </summary>
        /// <param name="configValue">Setting string (comma-delimited)</param>
        /// <returns></returns>
        private static List<LogType> GetLogType(string configValue)
        {
            List<LogType> logTypes = new List<LogType>();

            if (!string.IsNullOrEmpty(configValue))
            {
                foreach (string logString in configValue.Split(','))
                {
                    if (Enum.TryParse(logString, true, out LogType logTypeValue))
                    {
                        logTypes.Add(logTypeValue);
                    }
                }
            }
            else
            {
                logTypes.Add(Lurgle.Logging.LogType.Console);
            }

            return logTypes;
        }

        private static ConsoleTheme GetConsoleTheme(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue))
            {
                switch (configValue.ToLowerInvariant())
                {
                    case "literate":
                        return SystemConsoleTheme.Literate;
                    case "grayscale":
                        return SystemConsoleTheme.Grayscale;
                    case "colored":
                        return SystemConsoleTheme.Colored;
                    case "ansiliterate":
                        return AnsiConsoleTheme.Literate;
                    case "ansigrayscale":
                        return AnsiConsoleTheme.Grayscale;
                    case "ansicode":
                        return AnsiConsoleTheme.Code;
                }
            }

            return SystemConsoleTheme.Literate;
        }

        private static LogFileFormat GetLogFileType(string configValue)
        {
            if (!string.IsNullOrEmpty(configValue) && Enum.TryParse(configValue, out LogFileFormat fileFormat))
            {
                return fileFormat;
            }

            return LogFileFormat.Text;
        }

        /// <summary>
        /// Convert the supplied <see cref="object"/> to an <see cref="int"/><para/>
        /// This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to an int</param>
        /// <returns></returns>
        public static int GetInt(object sourceObject)
        {
            string sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject))
            {
                sourceString = (string)sourceObject;
            }

            if (int.TryParse(sourceString, out int destInt))
            {
                return destInt;
            }

            return -1;
        }

        /// <summary>
        /// Convert the supplied <see cref="object"/> to a <see cref="bool"/><para/>
        /// This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to a bool</param>
        /// <pasram name=trueIfEmpty">Return true if the object is empty</param>
        /// <returns></returns>
        public static bool GetBool(object sourceObject, bool trueIfEmpty = false)
        {
            string sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject))
            {
                sourceString = (string)sourceObject;
            }

            if (bool.TryParse(sourceString, out bool destBool))
            {
                return destBool;
            }

            return trueIfEmpty;
        }
    }
}
