using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

// ReSharper disable UnusedMember.Local

namespace Lurgle.Logging.Destructurers
{
    /// <summary>
    ///     Mask destructuring policy
    /// </summary>
    public class MaskDestructuringPolicy : IDestructuringPolicy
    {
        private readonly IDictionary<Type, Properties> _cache = new Dictionary<Type, Properties>();
        private readonly object _sync = new object();

        /// <summary>
        ///     Try destructuring the property
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyValueFactory"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
            out LogEventPropertyValue result)
        {
            if (value == null || value is IEnumerable)
            {
                result = null;
                return false;
            }

            result = Structure(value, propertyValueFactory);

            return true;
        }

        private static LogEventPropertyValue BuildLogEventProperty(object o,
            ILogEventPropertyValueFactory propertyValueFactory)
        {
            return o == null ? new ScalarValue(null) : propertyValueFactory.CreatePropertyValue(o, true);
        }

        private static object SafeGetPropertyValue(object o, PropertyInfo pi)
        {
            try
            {
                return pi.GetIndexParameters().Any() ? null : pi.GetValue(o);
            }
            catch (TargetInvocationException ex)
            {
                SelfLog.WriteLine("The property accessor {0} threw exception {1}", pi, ex);
                return "The property accessor threw an exception: " + ex.InnerException?.GetType().Name;
            }
        }

        private static Properties GetProperties(Type type)
        {
            var typeProperties = type.GetRuntimeProperties()
                .Where(p => p.CanRead);

            var entry = new Properties(typeProperties.ToArray(), new PropertyInfo[] { });

            return entry;
        }

        private Properties GetCachedProperties(Type type)
        {
            Properties entry;

            lock (_sync)
            {
                if (_cache.TryGetValue(type, out entry)) return entry;
            }

            var typeProperties = type.GetRuntimeProperties()
                .Where(p => p.CanRead);


            var propertyInfos = typeProperties.ToList();
            var includedProps = propertyInfos
                .Where(p => !Mask(p))
                .ToArray();

            var maskedProps = propertyInfos
                .Where(Mask)
                .ToArray();

            entry = new Properties(includedProps, maskedProps);

            lock (_sync)
            {
                _cache[type] = entry;
            }

            return entry;
        }

        private static bool Mask(MemberInfo p)
        {
            return Logging.Config.LogMaskPolicy != MaskPolicy.None &&
                   Logging.Config.LogMaskProperties.Contains(p.Name,
                       StringComparer.OrdinalIgnoreCase);
        }

        private LogEventPropertyValue Structure(object o, ILogEventPropertyValueFactory propertyValueFactory)
        {
            var type = o.GetType();

            var properties = GetCachedProperties(type);

            var structureProperties = (from p in properties.ToInclude
                let propertyValue = SafeGetPropertyValue(o, p)
                let logEventPropertyValue = BuildLogEventProperty(propertyValue, propertyValueFactory)
                select new LogEventProperty(p.Name, logEventPropertyValue)).ToList();
            structureProperties.AddRange(from p in properties.ToMask
                let propertyValue = SafeGetPropertyValue(o, p)
                let logEventPropertyValue =
                    BuildLogEventProperty(Logging.MaskProperty(propertyValue), propertyValueFactory)
                select new LogEventProperty(p.Name, logEventPropertyValue));

            return new StructureValue(structureProperties, type.Name);
        }

        private class Properties
        {
            public Properties(PropertyInfo[] toInclude, PropertyInfo[] toMask)
            {
                ToInclude = toInclude;
                ToMask = toMask;
            }

            public PropertyInfo[] ToInclude { get; }
            public PropertyInfo[] ToMask { get; }
        }
    }
}