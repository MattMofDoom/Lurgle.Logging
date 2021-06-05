using System.Collections.Generic;

namespace Lurgle.Logging
{
    /// <summary>
    /// Level interface
    /// </summary>
    public interface ILevel : IHideObjectMembers
    {
        /// <summary>
        /// Add an additional property for logging context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IAddProperty AddProperty(string name, object value);

        /// <summary>
        /// Add an additional set of properties for logging context
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <returns></returns>
        IAddProperty AddProperty(Dictionary<string, object> propertyPairs);

        /// <summary>
        /// Add a new log entry and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        void Add(string logTemplate, params object[] args);
    }
}
