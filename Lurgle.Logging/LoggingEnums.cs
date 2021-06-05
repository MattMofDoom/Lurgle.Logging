using Serilog.Events;
using System.ComponentModel;

namespace Lurgle.Logging
{
    /// <summary>
    /// Supported log types
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Console
        /// </summary>
        [Description("Console")]
        Console = 1,
        /// <summary>
        /// File
        /// </summary>
        [Description("File")]
        File = 2,
        /// <summary>
        /// Windows Event Log
        /// </summary>
        [Description("Windows Event Log")]
        EventLog = 4,
        /// <summary>
        /// Seq
        /// </summary>
        [Description("Seq")]
        Seq = 8,
        /// <summary>
        /// Not used
        /// </summary>
        [Description("All")]
        All = -1
    }

    /// <summary>
    /// Control the format of text logs
    /// </summary>
    public enum LogFileFormat
    {
        /// <summary>
        /// Text logs
        /// </summary>
        Text,
        /// <summary>
        /// Json logs
        /// </summary>
        Json,
    }

    /// <summary>
    /// Return a reason why a given log type failed
    /// </summary>
    public enum FailureReason
    {
        /// <summary>
        /// Could not find log file directory
        /// </summary>
        DirectoryNotFound,
        /// <summary>
        /// Log file directory not set
        /// </summary>
        DirectoryConfigEmpty,
        /// <summary>
        /// Failed to initialise logging for this log type
        /// </summary>
        LogTestFailed,
    }

    /// <summary>
    /// Outlines the supported log levels. Abstracts Serilog's <see cref="LogEventLevel"/> so that it does not need to be referenced outside of the <see cref="Logging"/> class.
    /// </summary>
    public enum LurgLevel
    {
        /// <summary>
        /// Fatal
        /// </summary>
        Fatal = LogEventLevel.Fatal,
        /// <summary>
        /// Error
        /// </summary>
        Error = LogEventLevel.Error,
        /// <summary>
        /// Warning
        /// </summary>
        Warning = LogEventLevel.Warning,
        /// <summary>
        /// Information
        /// </summary>
        Information = LogEventLevel.Information,
        /// <summary>
        /// Debug
        /// </summary>
        Debug = LogEventLevel.Debug,
        /// <summary>
        /// Verbose
        /// </summary>
        Verbose = LogEventLevel.Verbose,
        /// <summary>
        /// None
        /// </summary>
        NotSet = -1
    }

    /// <summary>
    /// Masking policy to be used when masking properties
    /// </summary>
    public enum MaskPolicy
    {
        /// <summary>
        /// No masking
        /// </summary>
        None,
        /// <summary>
        /// Mask with static string
        /// </summary>
        MaskWithString,
        /// <summary>
        /// Mask characters and digits
        /// </summary>
        MaskLettersAndNumbers,
    }
}
