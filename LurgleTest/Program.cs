using System;
using System.Collections.Generic;
using Lurgle.Logging;

namespace LurgleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {   
                Log.Level(LogLevel.Information).Add("{0:l} v{1:l} starting ...", Logging.Config.AppName, Logging.Config.AppVersion);
                Log.Level(LogLevel.Error).Add("Test");
                Log.Level(LogLevel.Information).Add("Configured Logs: {LogCount}, Enabled Logs: {EnabledCount}", Logging.Config.LogType.Count, Logging.EnabledLogs.Count);
                Log.Level(LogLevel.Information).Add("Configured Log List:");
                foreach (LogType logType in Logging.Config.LogType)
                    Log.Level(LogLevel.Information).Add(" - {LogType}", logType);
                Log.Level(LogLevel.Debug,"TestCorro").Add("Enabled Log List:");
                foreach (LogType logType in Logging.EnabledLogs)
                    Log.Level(LogLevel.Information).Add(" - {LogType}", logType);
                foreach (KeyValuePair<LogType, FailureReason> logFailure in Logging.LogFailures)
                    Log.Level(LogLevel.Error).Add("Failure - {LogType}: {FailureReason}", logFailure.Key, logFailure.Value);
            }
            catch (Exception ex)
            {
                Log.Exception(ex).Add(ex.ToString());
            }

            Logging.Close();
        }
    }
}
