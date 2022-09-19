using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

// ReSharper disable InconsistentNaming

namespace Lurgle.Logging.Enrichers
{
    /// <summary>
    ///     Mask event properties based on the configured masking policy
    /// </summary>
    internal class MaskingEnricher : ILogEventEnricher
    {
        // ReSharper disable UnusedMember.Global
        private readonly List<string> _maskProperties = new List<string>();

        /// <summary>
        ///     Process incoming events and mask their values if a match is found in the configured properties
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="propertyFactory"></param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (Logging.Config.LogMaskPolicy.Equals(MaskPolicy.None)) return;
            foreach (var propertyName in _maskProperties)
            foreach (var logProperty in logEvent.Properties)
                if (logProperty.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    if (logProperty.Value is ScalarValue sv && sv.Value is string rawValue)
                        logEvent
                            .AddOrUpdateProperty
                            (
                                new LogEventProperty
                                (
                                    propertyName,
                                    new ScalarValue(Logging.MaskProperty(rawValue))
                                )
                            );
                    else
                        logEvent
                            .AddOrUpdateProperty
                            (
                                new LogEventProperty
                                (
                                    propertyName,
                                    new ScalarValue(Logging.MaskProperty(logProperty.Value))
                                )
                            );
                    break;
                }
        }

        /// <summary>
        ///     Add a list of properties to the masking enricher
        /// </summary>
        /// <param name="maskProperties"></param>
        /// <returns></returns>
        public MaskingEnricher Add(IEnumerable<string> maskProperties)
        {
            _maskProperties.AddRange(maskProperties);

            return this;
        }

        /// <summary>
        ///     Add a single property to the masking enricher
        /// </summary>
        /// <param name="maskProperty"></param>
        /// <returns></returns>
        public MaskingEnricher Add(string maskProperty)
        {
            _maskProperties.Add(maskProperty);
            return this;
        }
    }
}