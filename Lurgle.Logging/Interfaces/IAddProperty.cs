using System;
using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global

namespace Lurgle.Logging.Interfaces
{
    /// <summary>
    ///     Property interface
    /// </summary>
    public interface IAddProperty : IHideObjectMembers
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

        /// <summary>
        ///     Add an Information event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Information(string logTemplate, params object[] args);

        /// <summary>
        ///     Add an Information event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Information(Exception ex, string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Debug event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Debug(string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Debug event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Debug(Exception ex, string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Verbose event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Verbose(string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Verbose event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Verbose(Exception ex, string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Warning event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Warning(string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Warning event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Warning(Exception ex, string logTemplate, params object[] args);

        /// <summary>
        ///     Add an Error event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Error(string logTemplate, params object[] args);

        /// <summary>
        ///     Add an Error event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Error(Exception ex, string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Fatal event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Fatal(string logTemplate, params object[] args);

        /// <summary>
        ///     Add a Fatal event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Fatal(Exception ex, string logTemplate, params object[] args);
    }
}