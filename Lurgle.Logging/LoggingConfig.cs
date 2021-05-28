using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace Lurgle.Logging
{
    /// <summary>
    /// Logging configuration. Loaded from AppSettings if available but can be configured from code.
    /// </summary>
    public class LoggingConfig
    {
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
        public LogLevel LogLevel { get; set; }
        /// <summary>
        /// Minimum log level accepted by the Console sink
        /// </summary>
        public LogLevel LogLevelConsole { get; set; }
        /// <summary>
        /// Minimum log level accepted by the File sink
        /// </summary>
        public LogLevel LogLevelFile { get; set; }
        /// <summary>
        /// Minimum log level accepted by the Event Log sink
        /// </summary>
        public LogLevel LogLevelEvent { get; set; }
        /// <summary>
        /// Minimum log level accepted by the Seq sink
        /// </summary>
        public LogLevel LogLevelSeq { get; set; }
        /// <summary>
        /// How many days log files will be retained - default is 31
        /// </summary>
        public int LogDays { get; set; }
        /// <summary>
        /// How many seconds before log file writes are flushed to disk - default is 1
        /// </summary>
        public int LogFlush { get; set; }
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

        public static LoggingConfig GetConfig()
        {
            LoggingConfig config = new LoggingConfig()
            {
                AppName = ConfigurationManager.AppSettings["AppName"],
                LogType = GetLogType(ConfigurationManager.AppSettings["LogType"]),
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
                LogDays = GetInt(ConfigurationManager.AppSettings["LogMonths"]),
                LogFlush = GetInt(ConfigurationManager.AppSettings["LogFlush"]),
                LogSeqServer = ConfigurationManager.AppSettings["LogSeqServer"],
                LogSeqApiKey = ConfigurationManager.AppSettings["LogSeqApiKey"],
                LogFormatConsole = ConfigurationManager.AppSettings["LogFormatConsole"],
                LogFormatEvent = ConfigurationManager.AppSettings["LogFormatEvent"],
                LogFormatFile = ConfigurationManager.AppSettings["LogFormatFile"]
            };

            bool isSuccess = true;

            //If AppName is not specified in config, attempt to populate it. Populate AppVersion while we're at it.
            try
            {
                if (string.IsNullOrEmpty(config.AppName))
                    config.AppName = Assembly.GetEntryAssembly().GetName().Name;
                config.AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                if (string.IsNullOrEmpty(config.LogFolder))
                    config.LogFolder = Assembly.GetEntryAssembly().Location;
            }
            catch
            {
                isSuccess = false;
            }

            if (!isSuccess)
                try
                {
                    if (string.IsNullOrEmpty(config.AppName))
                        config.AppName = Assembly.GetExecutingAssembly().GetName().Name;
                    config.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (string.IsNullOrEmpty(config.LogFolder))
                        config.LogFolder = Assembly.GetExecutingAssembly().GetName().CodeBase;
                }
                catch
                {
                    //We surrender ...
                    config.AppVersion = string.Empty;
                }

            if (config.LogDays.Equals(-1))
                config.LogDays = 31;

            if (config.LogFlush.Equals(-1))
                config.LogFlush = 1;

            if (string.IsNullOrEmpty(config.LogName))
                config.LogName = "Blorp";

            if (string.IsNullOrEmpty(config.LogExtension))
                config.LogExtension = ".log";

            if (string.IsNullOrEmpty(config.LogEventSource))
                config.LogEventSource = config.AppName;

            if (string.IsNullOrEmpty(config.LogEventName))
                config.LogEventName = "Application";

            if (string.IsNullOrEmpty(config.LogFormatConsole))
                config.LogFormatConsole = "{Message}{NewLine}";

            if (string.IsNullOrEmpty(config.LogFormatEvent))
                config.LogFormatEvent = "({ThreadId}) {Message}{NewLine}{NewLine}{Exception}";

            if (string.IsNullOrEmpty(config.LogFormatFile))
                config.LogFormatFile = "{Timestamp:yyyy-MM-dd HH:mm:ss}: ({ThreadId}) [{Level}] {Message}{NewLine}";

            return config;
        }

        /// <summary>
        /// Parse the configured log level <see cref="string"/> into a <see cref="LogLevel"/> value
        /// </summary>
        /// <param name="configValue">Setting string</param>
        /// <returns></returns>
        private static LogLevel GetEventLevel(string configValue)
        {
            LogLevel eventLevel;
            if (Enum.TryParse(configValue, true, out eventLevel))
                return eventLevel;

            return LogLevel.Verbose;
        }

        /// <summary>
        /// Parse a comma-delimited logType <see cref="string"/> into a list of <see cref="Lurgle.Logging.LogType"/>
        /// </summary>
        /// <param name="configValue">Setting string (comma-delimited)</param>
        /// <returns></returns>
        private static List<LogType> GetLogType(string configValue)
        {
            List<LogType> logTypes = new List<LogType>();

            foreach (string logString in configValue.Split(','))
            {
                LogType logTypeValue;
                if (Enum.TryParse(logString, true, out logTypeValue))
                    logTypes.Add(logTypeValue);
            }

            return logTypes;
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
            int destInt;

            if (!Convert.IsDBNull(sourceObject))
                sourceString = (string)sourceObject;

            if (int.TryParse(sourceString, out destInt))
                return destInt;

            return -1;
        }

        /// <summary>
        /// Convert the supplied <see cref="object"/> to a <see cref="bool"/><para/>
        /// This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to a bool</param>
        /// <returns></returns>
        public static bool GetBool(object sourceObject)
        {
            string sourceString = string.Empty;
            bool destBool;

            if (!Convert.IsDBNull(sourceObject))
                sourceString = (string)sourceObject;

            if (bool.TryParse(sourceString, out destBool))
                return destBool;

            return false;
        }
    }
}
