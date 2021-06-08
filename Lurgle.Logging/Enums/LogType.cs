﻿using System.ComponentModel;

namespace Lurgle.Logging
{
    /// <summary>
    ///     Supported log types
    /// </summary>
    public enum LogType
    {
        /// <summary>
        ///     Console
        /// </summary>
        [Description("Console")] Console = 1,

        /// <summary>
        ///     File
        /// </summary>
        [Description("File")] File = 2,

        /// <summary>
        ///     Windows Event Log
        /// </summary>
        [Description("Windows Event Log")] EventLog = 4,

        /// <summary>
        ///     Seq
        /// </summary>
        [Description("Seq")] Seq = 8,

        /// <summary>
        ///     All logs - can be used to return all logs being disabled
        /// </summary>
        [Description("All")] All = -1
    }
}