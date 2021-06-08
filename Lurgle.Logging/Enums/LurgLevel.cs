using Serilog.Events;

namespace Lurgle.Logging
{
    /// <summary>
    ///     Outlines the supported log levels. Abstracts Serilog <see cref="LogEventLevel" /> so that it does not need to be
    ///     referenced outside of the <see cref="Logging" /> class.
    /// </summary>
    public enum LurgLevel
    {
        /// <summary>
        ///     Fatal
        /// </summary>
        Fatal = LogEventLevel.Fatal,

        /// <summary>
        ///     Error
        /// </summary>
        Error = LogEventLevel.Error,

        /// <summary>
        ///     Warning
        /// </summary>
        Warning = LogEventLevel.Warning,

        /// <summary>
        ///     Information
        /// </summary>
        Information = LogEventLevel.Information,

        /// <summary>
        ///     Debug
        /// </summary>
        Debug = LogEventLevel.Debug,

        /// <summary>
        ///     Verbose
        /// </summary>
        Verbose = LogEventLevel.Verbose
    }
}