using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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

    public enum FailureReason
    {
        DirectoryNotFound,
        DirectoryConfigEmpty,
        LogTestFailed,
    }

    /// <summary>
    /// Outlines the supported log levels. Abstracts Serilog's <see cref="LogEventLevel"/> so that it does not need to be referenced outside of the <see cref="Logging"/> class.
    /// </summary>
    public enum LogLevel
    {
        Fatal = LogEventLevel.Fatal,
        Error = LogEventLevel.Error,
        Warning = LogEventLevel.Warning,
        Information = LogEventLevel.Information,
        Debug = LogEventLevel.Debug,
        Verbose = LogEventLevel.Verbose,
        NotSet = -1
    }
}
