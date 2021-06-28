﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Lurgle.Logging.Interfaces;
using Serilog.Events;

// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable MethodOverloadWithOptionalParameter

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

        /// <summary>
        ///     Log level for this log
        /// </summary>
        public LurgLevel LogLevel { get; private set; }

        /// <summary>
        ///     Exception attached to this log
        /// </summary>
        public Exception ErrorInfo { get; private set; }

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
        ///     Add an additional property for logging context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(string name, object value, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Level(LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(name, value);
        }

        /// <summary>
        ///     Add an additional set of properties for logging context
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(Dictionary<string, object> propertyPairs, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Level(LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(propertyPairs);
        }

        /// <summary>
        ///     Add an additional property for logging context and pass an exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(Exception ex, string name, object value, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Exception(ex, LurgLevel.Error, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(name, value);
        }

        /// <summary>
        ///     Add an additional set of properties for logging context and pass an exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="propertyPairs"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(Exception ex, Dictionary<string, object> propertyPairs,
            string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Exception(ex, LurgLevel.Error, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(propertyPairs);
        }

        /// <summary>
        ///     Set log level and add an additional property for logging context
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(LurgLevel logLevel, string name, object value, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Level(logLevel, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(name, value);
        }

        /// <summary>
        ///     Set log level and add an additional set of properties for logging context
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="propertyPairs"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(LurgLevel logLevel, Dictionary<string, object> propertyPairs,
            string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Level(logLevel, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(propertyPairs);
        }

        /// <summary>
        ///     Set log level and exception, and add an additional property for logging context
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logLevel"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(Exception ex, LurgLevel logLevel, string name, object value,
            string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Exception(ex, logLevel, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(name, value);
        }

        /// <summary>
        ///     Set log level and exception,  and add an additional set of properties for logging context
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logLevel"></param>
        /// <param name="propertyPairs"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel AddProperty(Exception ex, LurgLevel logLevel, Dictionary<string, object> propertyPairs,
            string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return (ILevel) Exception(ex, logLevel, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .AddProperty(propertyPairs);
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
        ///     Add an Information event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Information(string logTemplate, params object[] args)
        {
            LogLevel = LurgLevel.Information;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add an Information event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Information(Exception ex, string logTemplate, params object[] args)
        {
            ErrorInfo = ex;
            LogLevel = LurgLevel.Information;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Debug event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Debug(string logTemplate, params object[] args)
        {
            LogLevel = LurgLevel.Debug;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Debug event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Debug(Exception ex, string logTemplate, params object[] args)
        {
            ErrorInfo = ex;
            LogLevel = LurgLevel.Debug;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Verbose event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Verbose(string logTemplate, params object[] args)
        {
            LogLevel = LurgLevel.Verbose;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Verbose event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Verbose(Exception ex, string logTemplate, params object[] args)
        {
            ErrorInfo = ex;
            LogLevel = LurgLevel.Verbose;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Warning event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Warning(string logTemplate, params object[] args)
        {
            LogLevel = LurgLevel.Warning;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Warning event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Warning(Exception ex, string logTemplate, params object[] args)
        {
            ErrorInfo = ex;
            LogLevel = LurgLevel.Warning;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add an Error event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Error(string logTemplate, params object[] args)
        {
            LogLevel = LurgLevel.Error;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add an Error event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Error(Exception ex, string logTemplate, params object[] args)
        {
            ErrorInfo = ex;
            LogLevel = LurgLevel.Error;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add a Fatal event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Fatal(string logTemplate, params object[] args)
        {
            LogLevel = LurgLevel.Fatal;
            Add(logTemplate, args);
        }

        /// <summary>
        ///     Add an Information event with an Exception and apply parameters
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="args"></param>
        public void Fatal(Exception ex, string logTemplate, params object[] args)
        {
            ErrorInfo = ex;
            LogLevel = LurgLevel.Fatal;
            Add(logTemplate, args);
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
        ///     Create a simple log entry with the specified level
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Add(LurgLevel logLevel, string logTemplate, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Create a simple log entry with the specified level and exception
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Add(Exception ex, LurgLevel logLevel, string logTemplate, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Create a simple Information log entry and apply parameters to the template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Add(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Create a simple Information log entry with exception and apply parameters to the template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Add(Exception ex, string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log an Information event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Information(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log an Information event with an exception and apply parameters to the supplied log template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Information(Exception ex, string logTemplate, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath,
                    sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Debug event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Debug(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Debug, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Debug event with an exception and apply parameters to the supplied log template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Debug(Exception ex, string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Debug, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Verbose event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Verbose(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Verbose, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Verbose event with an exception and apply parameters to the supplied log template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Verbose(Exception ex, string logTemplate, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Verbose, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Warning event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Warning(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Warning, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Warning event with an exception and apply parameters to the supplied log template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Warning(Exception ex, string logTemplate, string correlationId = null,
            bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Warning, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log an Error event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Error(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Error, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log an Error event with an exception and apply parameters to the supplied log template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Error(Exception ex, string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Error, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Fatal event and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Fatal(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Level(LurgLevel.Fatal, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }

        /// <summary>
        ///     Log a Fatal event with an exception and apply parameters to the supplied log template
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args"></param>
        public static void Fatal(Exception ex, string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0, params object[] args)
        {
            Exception(ex, LurgLevel.Fatal, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber)
                .Add(logTemplate, args);
        }
    }
}