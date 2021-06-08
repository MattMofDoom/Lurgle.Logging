namespace Lurgle.Logging
{
    /// <summary>
    ///     Return a reason why a given log type failed
    /// </summary>
    public enum FailureReason
    {
        /// <summary>
        ///     Could not find log file directory
        /// </summary>
        DirectoryNotFound,

        /// <summary>
        ///     Log file directory not set
        /// </summary>
        DirectoryConfigEmpty,

        /// <summary>
        ///     Failed to initialise logging for this log type
        /// </summary>
        LogTestFailed
    }
}