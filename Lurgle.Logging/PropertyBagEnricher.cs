using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Lurgle.Logging
{
    /// <summary>
    ///     Add properties to logs - adapted from https://benfoster.io/blog/serilog-best-practices/
    /// </summary>
    public class PropertyBagEnricher : ILogEventEnricher
    {
        // ReSharper disable UnusedMember.Global
        private readonly Dictionary<string, Tuple<object, bool>> _properties;

        /// <summary>
        ///     Creates a new <see cref="PropertyBagEnricher" /> instance.
        /// </summary>
        public PropertyBagEnricher()
        {
            _properties = new Dictionary<string, Tuple<object, bool>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Enriches the <paramref name="logEvent" /> using the values from the property bag.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">The factory used to create the property.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var prop in _properties)
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(prop.Key, prop.Value.Item1,
                    prop.Value.Item2));
        }

        /// <summary>
        ///     Add a property that will be added to all log events enriched by this enricher.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <param name="destructureObject">
        ///     Whether to destructure the value. See https://github.com/serilog/serilog/wiki/Structured-Data
        /// </param>
        /// <returns>The enricher instance, for chaining Add operations together.</returns>
        public PropertyBagEnricher Add(string key, object value, bool destructureObject = false)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (!_properties.ContainsKey(key)) _properties.Add(key, Tuple.Create(value, destructureObject));

            return this;
        }

        /// <summary>
        ///     Add properties that will be added to all log events enriched by this enricher.
        /// </summary>
        /// <param name="valueList">The property dictionary</param>
        /// <param name="destructureObject">
        ///     Whether to destructure the value. See https://github.com/serilog/serilog/wiki/Structured-Data
        /// </param>
        /// <returns>The enricher instance, for chaining Add operations together.</returns>
        public PropertyBagEnricher Add(Dictionary<string, object> valueList, bool destructureObject = false)
        {
            foreach (var value in valueList.Where(value => !_properties.ContainsKey(value.Key)))
                _properties.Add(value.Key, Tuple.Create(value.Value, destructureObject));

            return this;
        }
    }
}