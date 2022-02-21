using System;
using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global

namespace Lurgle.Logging.Interfaces
{
    /// <summary>
    ///     Explicit level interface
    /// </summary>
    public interface IExplicitLevel : IHideObjectMembers
    {
        // ReSharper disable UnusedMember.Global
        /// <summary>
        ///     Add an additional property for logging context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="destructure"></param>
        /// <param name="keepEmptyValue"></param>
        /// <returns></returns>
        IAddProperty AddProperty(string name, object value, bool destructure = false, bool keepEmptyValue = true);

        /// <summary>
        ///     Add an additional set of properties for logging context
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <param name="destructure"></param>
        /// <param name="keepEmptyValue"></param>
        /// <returns></returns>
        IAddProperty AddProperty(Dictionary<string, object> propertyPairs, bool destructure = false,
            bool keepEmptyValue = true);

        /// <summary>
        ///     Set the log timestamp
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        IAddProperty SetTimestamp(DateTimeOffset timeStamp);

        /// <summary>
        ///     Add a new log entry and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Add(string logTemplate, params object[] args);
    }
}