using Serilog.Events;
using System.ComponentModel;

namespace Lurgle.Logging
{
    /// <summary>
    /// Supported log types
    /// </summary>
    public enum LogType
    {
        [Description("Console")]
        Console = 1,
        [Description("File")]
        File = 2,
        [Description("Windows Event Log")]
        EventLog = 4,
        [Description("Seq")]
        Seq = 8,
        [Description("All")]
        All = -1
    }

    /// <summary>
    /// Control the format of text logs
    /// </summary>
    public enum LogFileFormat
    {
        Text,
        Json,
    }

    /// <summary>
    /// Return a reason why a given log type failed
    /// </summary>
    public enum FailureReason
    {
        DirectoryNotFound,
        DirectoryConfigEmpty,
        LogTestFailed,
    }

    /// <summary>
    /// Outlines the supported log levels. Abstracts Serilog's <see cref="LogEventLevel"/> so that it does not need to be referenced outside of the <see cref="Logging"/> class.
    /// </summary>
    public enum LurgLevel
    {
        Fatal = LogEventLevel.Fatal,
        Error = LogEventLevel.Error,
        Warning = LogEventLevel.Warning,
        Information = LogEventLevel.Information,
        Debug = LogEventLevel.Debug,
        Verbose = LogEventLevel.Verbose,
        NotSet = -1
    }

    /// <summary>
    /// Masking policy to be used when masking properties
    /// </summary>
    public enum MaskPolicy
    {
        None,
        MaskWithString,
        MaskLettersAndNumbers,
    }
}
