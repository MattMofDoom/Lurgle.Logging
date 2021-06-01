using Lurgle.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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
            //Add masked properties for test
            Logging.Config.LogMaskPolicy = MaskPolicy.MaskWithString;
            Logging.AddCommonProperty("TestCommonMask", "mask1234");
            Log.Level().AddProperty("Mechagodzilla", "Godzilla").AddProperty("password", "godzilla").Add("Testing masking properties, send complaints to {Email:l}", "mechagodzilla@monster.rargh");
            //Switch masked properties to use MaskPolicy.MaskLettersAndNumbers
            Logging.Config.LogMaskPolicy = MaskPolicy.MaskLettersAndNumbers;
            Logging.AddCommonProperty("TestCommonMask2", "mask1234");
            Log.Level().AddProperty("Mechagodzilla", "Godzilla123").AddProperty("password", "godzilla123").Add("Testing masking properties, send complaints to {Email:l}", "mechagodzilla123@monster.rargh");
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
.UseStartup<Startup>();
        }
    }
}
