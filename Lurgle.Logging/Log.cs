using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lurgle.Logging
{
    public sealed class Log : ILog, ILevel, IAddProperty, IDisposable
    {
        private bool IsMethod { get; set; }
        private string MethodName { get; set; }
        private Dictionary<string, object> EventProperties { get; set; } = new Dictionary<string, object>();
        private LurgLevel LogLevel { get; set; }
        private Exception ErrorInfo { get; set; }

        private Log(LurgLevel logLevel, string correlationId, bool showMethod, string methodName, string sourceFilePath, int sourceLineNumber)
        {
            LogLevel = logLevel;
            IsMethod = showMethod;
            MethodName = methodName;
            if (Logging.LogWriter == null)
            {
                Logging.Init(correlationId, methodName, sourceFilePath, sourceLineNumber);
            }

            EventProperties = Logging.GetBaseProperties(correlationId, methodName, sourceFilePath, sourceLineNumber);
        }

        private Log(Exception ex, LurgLevel logLevel, string correlationId, bool showMethod, string methodName, string sourceFilePath, int sourceLineNumber)
        {
            ErrorInfo = ex;
            LogLevel = logLevel;
            IsMethod = showMethod;
            MethodName = methodName;

            if (Logging.LogWriter == null)
            {
                Logging.Init(correlationId, methodName, sourceFilePath, sourceLineNumber);
            }
            EventProperties = Logging.GetBaseProperties(correlationId, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Log an event with the specified level. Defaults to Information. 
        /// 
        /// CorrelationId can optionally be specified.
        /// 
        /// Optionally, you can embed the method name in the event log text.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel Level(LurgLevel logLevel = LurgLevel.Information, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Log(logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Log an Exception with the specified level. Defaults to Error. 
        /// 
        /// CorrelationId can optionally be specified. 
        /// 
        /// Optionally, you can embed the method name in the  event log text.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logLevel"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns></returns>
        public static ILevel Exception(Exception ex, LurgLevel logLevel = LurgLevel.Error, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Log(ex, logLevel, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Simple log entry using <seealso cref="LurgLevel.Information"/> with no arguments.
        /// </summary>
        /// <param name="logTemplate"></param>
        /// <param name="correlationId"></param>
        /// <param name="showMethod"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        public static void Add(string logTemplate, string correlationId = null, bool showMethod = false,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Level(LurgLevel.Information, correlationId, showMethod, methodName, sourceFilePath, sourceLineNumber).Add(logTemplate);
        }

        /// <summary>
        /// Add an additional property for logging context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IAddProperty AddProperty(string name, object value)
        {
            if (!string.IsNullOrEmpty(name) && !EventProperties.ContainsKey(name))
            {
                EventProperties.Add(name, value);
            }

            return this;
        }

        /// <summary>
        /// Add an additional set of properties for logging context
        /// </summary>
        /// <param name="propertyPairs"></param>
        /// <returns></returns>
        public IAddProperty AddProperty(Dictionary<string, object> propertyPairs)
        {
            foreach (KeyValuePair<string, object> values in propertyPairs)
            {
                if (!string.IsNullOrEmpty(values.Key) && !EventProperties.ContainsKey(values.Key))
                {
                    EventProperties.Add(values.Key, values.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Add a new log entry and apply parameters to the supplied log template
        /// </summary>
        /// <param name="logTemplate">Log template that parameters will be applied to</param>
        /// <param name="args">Parameters for the log template</param>
        public void Add(string logTemplate, params object[] args)
        {
            string logText;
            if (IsMethod)
            {
                logText = string.Format(Logging.LogMethod, MethodName, logTemplate);
            }
            else
            {
                logText = logTemplate;
            }

            if (Logging.LogWriter != null)
            {
                if (ErrorInfo != null)
                {
                    Logging.LogWriter
                        .ForContext(new PropertyBagEnricher().Add(EventProperties))
                        .Write((LogEventLevel)LogLevel, ErrorInfo, logText, args);
                }
                else
                {
                    Logging.LogWriter
                        .ForContext(new PropertyBagEnricher().Add(EventProperties))
                        .Write((LogEventLevel)LogLevel, logText, args);
                }
            }
        }

        /// <summary>
        /// Ensure that we flush and dispose the logwriter
        /// </summary>
        public void Dispose()
        {
            Logging.Close();
        }
    }
}
