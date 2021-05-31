using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lurgle.Logging;

namespace LurgleWebTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logging.AddCommonProperty("TestCommonProperty", "Common property for all log events");

            //Populate a small dictionary for testing per-event properties
            Dictionary<string, object> test = new Dictionary<string, object>();
            test.Add("TestDictKey", value: FailureReason.LogTestFailed);

            //Add a start log
            Log.Level(LurgLevel.Information).Add("{AppName:l} v{AppVersion:l} starting ...");
            //Send a simple string
            Log.Add("Test simple log");
            //Send an error log
            Log.Level(LurgLevel.Error).Add("Test error log");
            //Send added properties
            Log.Level().AddProperty("test1", "test1").AddProperty("test2", LurgLevel.Fatal).AddProperty(test).Add("Test Adding Properties");
            //Output the enabled log types
            Log.Level(LurgLevel.Information).Add("Configured Logs: {LogCount}, Enabled Logs: {EnabledCount}", Logging.Config.LogType.Count, Logging.EnabledLogs.Count);
            Log.Level(LurgLevel.Information).Add("Configured Log List:");
            foreach (LogType logType in Logging.Config.LogType)
            {
                Log.Level(LurgLevel.Information).Add(" - {LogType}", logType);
            }

            //Add a correlation Id
            Log.Level(LurgLevel.Debug, "TestCorro").Add("Enabled Log List:");
            foreach (LogType logType in Logging.EnabledLogs)
            {
                Log.Level(LurgLevel.Information).Add(" - {LogType}", logType);
            }

            //Output any failure reasons
            foreach (KeyValuePair<LogType, FailureReason> logFailure in Logging.LogFailures)
            {
                Log.Level(LurgLevel.Error).Add("Failure - {LogType}: {FailureReason}", logFailure.Key, logFailure.Value);
            }

            Logging.Close();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
