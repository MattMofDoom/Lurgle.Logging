using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Lurgle.Logging.Interfaces;
using Serilog.Events;

namespace Lurgle.Logging
{
    /// <summary>
    ///     Log an event with Lurgle.Logging
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public sealed class Log : ILog, ILevel, IAddProperty, IDisposable
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedParameter.Global
        private Log(LurgLevel logLevel, string correlationId, bool showMethod, string methodName, string sourceFilePath,
            int sourceLineNumber)
        {
            LogLevel = logLevel;
            IsMethod = showMethod;
            MethodName = methodName;
            if (Logging.LogWriter == null) Logging.Init(correlationId);

            EventProperties = Logging.GetBaseProperties(correlationId, methodName, sourceFilePath, sourceLineNumber);
        }

        private Log(Exception ex, LurgLevel logLevel, string correlationId, bool showMethod, string methodName,
            string sourceFilePath, int sourceLineNumber)
        {
            ErrorInfo = ex;
            LogLevel = logLevel;
            IsMethod = showMethod;
            MethodName = methodName;

            if (Logging.LogWriter == null) Logging.Init(correlationId);
            EventProperties = Logging.GetBaseProperties(correlationId, methodName, sourceFilePath, sourceLineNumber);
        }

        private bool IsMethod { get; }
        private string MethodName { get; }
        private Dictionary<string, object> EventProperties { get; }
        private LurgLevel LogLevel { get; }
        private Exception ErrorInfo { get; }

        /// <summary>
        ///     Ensure that we flush and dispose the log writer
        /// </summary>
        public void Dispose()
        {
            Logging.Close();
        }

        /// <summary>
        ///     Add an additional property for logging context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IAddProperty AddProperty(string name, object value)
        {
            var exists = false;
            if (string.IsNullOrEmpty(name)) return this;
            if (EventProperties.Any(property => property.Key.Equals(name, StringComparison.OrdinalIgnoreCase)))
                exists = true;

            if (exists) return this;
            if (Logging.Config.LogMaskPolicy.Equals(MaskPolicy.None))
            {
                EventProperties.Add(name, value);
            }
            else
            {
                var isMask = Logging.Config.LogMaskProperties.Any(maskProperty =>
                    maskProperty.Equals(name, StringComparison.OrdinalIgnoreCase));

                EventProperties.Add(name, isMask ? Logging.MaskProperty(value) : value);
            }


            return this;
        }

        /// <summary>
        ///     Add an additional set of properties for logging context
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <returns></returns>
        public IAddProperty AddProperty(Dictionary<string, object> propertyPairs)
        {
            foreach (var values in from values in propertyPairs
                where !string.IsNullOrEmpty(values.Key)
                let exists = EventProperties.Any(property =>
                    property.Key.Equals(values.Key, StringComparison.OrdinalIgnoreCase))
                where !exists
                select values)
                if (Logging.Config.LogMaskPolicy.Equals(MaskPolicy.None))
                {
                    EventProperties.Add(values.Key, values.Value);
                }
                else
                {
                    var isMask = Logging.Config.LogMaskProperties.Any(maskProperty =>
                        maskProperty.Equals(values.Key, StringComparison.OrdinalIgnoreCase));

                    EventProperties.Add(values.Key, isMask ? Logging.MaskProperty(values.Value) : values.Value);
                }

            return this;
        }

        /// <summary>
        ///     Add a new log entry and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate">Log template that parameters will be applied to</param>
        /// <param name="args">Parameters for the log template</param>
        public void Add(string logTemplate, params object[] args)
        {
            var logText = IsMethod ? string.Format(Logging.LogMethod, MethodName, logTemplate) : logTemplate;

            if (Logging.LogWriter == null) return;
            if (ErrorInfo != null)
                Logging.LogWriter
                    .ForContext(new PropertyBagEnricher().Add(EventProperties))
                    .Write((LogEventLevel) LogLevel, ErrorInfo, logText, args);
            else
                Logging.LogWriter
                    .ForContext(new PropertyBagEnricher().Add(EventProperties))
                    .ForContext(new MaskingEnricher().Add(Logging.Config.LogMaskProperties))
                    .Write((LogEventLevel) LogLevel, logText, args);
        }

        /// <summary>
        ///     Log an event with the specified level. Defaults to <see cref="LurgLevel.Information" />.
        ///     CorrelationId can optionally be specified.
        ///     Optionally, you can embed the method name in the event log text.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel Level(LurgLevel logLevel = LurgLevel.Information, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Log(logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        ///     Log an Exception with the specified level. Defaults to <see cref="LurgLevel.Error" />.
        ///     CorrelationId can optionally be specified.
        ///     Optionally, you can embed the method name in the  event log text.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logLevel"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel Exception(Exception ex, LurgLevel logLevel = LurgLevel.Error, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Log(ex, logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        ///     Simple log entry using <seealso cref="LurgLevel.Information" /> with no arguments.
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        public static void Add(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            Level(LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate);
        }
    }
}