using System.Collections.Generic;
using Lurgle.Logging;

namespace LurgleTest
{
    internal class Program
    {
        private static void Main()
        {
            Logging.AddCommonProperty("TestCommonProperty", "Common property for all log events");

            //Populate a small dictionary for testing per-event properties
            var test = new Dictionary<string, object>
            {
                {"TestDictKey", FailureReason.LogTestFailed}
            };

            //Add a start log
            Log.Level().Add("{AppName:l} v{AppVersion:l} starting ...");
            //Send a simple string
            Log.Add("Test simple log");
            //Send an error log
            Log.Level(LurgLevel.Error).Add("Test error log");
            //Send added properties
            Log.Level().AddProperty("test1", "test1").AddProperty("test2", LurgLevel.Fatal).AddProperty(test)
                .Add("Test Adding Properties");
            //Add masked properties for test
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, logMaskPolicy: MaskPolicy.MaskWithString));
            Logging.AddCommonProperty("TestCommonMask", "mask1234");
            Log.Level().AddProperty("Mechagodzilla", "Godzilla").AddProperty("password", "godzilla")
                .Add("Testing masking properties, send complaints to {Email:l}", "mechagodzilla@monster.rargh");
            //Switch masked properties to use MaskPolicy.MaskLettersAndNumbers
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, logMaskPolicy: MaskPolicy.MaskLettersAndNumbers));
            Logging.AddCommonProperty("TestCommonMask2", "mask1234");
            Log.Level().AddProperty("Mechagodzilla", "Godzilla123").AddProperty("password", "godzilla123").Add(
                "Testing masking properties, send complaints to {Email:l}", "mechagodzilla123@monster.rargh");
            //Output the enabled log types
            Log.Level().Add("Configured Logs: {LogCount}, Enabled Logs: {EnabledCount}", Logging.Config.LogType.Count,
                Logging.EnabledLogs.Count);
            Log.Level().Add("Configured Log List:");
            foreach (var logType in Logging.Config.LogType) Log.Level().Add(" - {LogType}", logType);

            //Set a new correlation Id
            Log.Level(LurgLevel.Debug, Logging.NewCorrelationId()).Add("Enabled Log List (Switch CorrelationId):");
            foreach (var logType in Logging.EnabledLogs) Log.Level().Add(" - {LogType}", logType);

            //Output any failure reasons
            foreach (var logFailure in Logging.LogFailures)
                Log.Level(LurgLevel.Error)
                    .Add("Failure - {LogType}: {FailureReason}", logFailure.Key, logFailure.Value);

            Logging.Close();
        }
    }
}