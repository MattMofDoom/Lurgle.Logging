using System;
using System.Collections.Generic;
using Lurgle.Logging;

// ReSharper disable UnusedMember.Global

namespace LurgleTest
{
    public class Test
    {
        public string Name => "Test";
        public LogType LogType => LogType.All;
        public string Mechagodzilla => "Rargh123";
    }

    internal static class Program
    {
        private static void Main()
        {
            Logging.AddCommonProperty("TestCommonProperty", "Common property for all log events");

            //Populate a small dictionary for testing per-event properties
            var test = new Dictionary<string, object>
            {
                { "TestDictKey", FailureReason.LogTestFailed },
                { "TestClass", new Test() }
            };

            //Add a start log
            Log.Error().SetTimestamp(DateTime.Today).AddProperty("TestProperty", "Oh hi there!")
                .Add("Test {Testing}", "Message");
            Log.Level().Add("{AppName:l} v{AppVersion:l} starting ...");
            Log.Add("Simple information log");
            Log.Add(LurgLevel.Debug, "Simple debug log");
            Log.Information().Add("Information event");
            Log.Information().Add("Information event with {Properties:l}", "Properties");
            Log.Verbose().Add("Verbose event");
            Log.Verbose().Add("Verbose event with {Properties:l}", "Properties");
            Log.Debug().Add("Debug event");
            Log.Debug().Add("Debug event with {Properties:l}", "Properties");
            Log.Warning().Add("Warning event");
            Log.Warning().Add("Warning event with {Properties:l}", "Properties");
            Log.Error().Add("Error event");
            Log.Error().Add("Error event with {Properties:l}", "Properties");
            Log.Fatal().Add("Fatal event");
            Log.Fatal().Add("Fatal event with {Properties:l}", "Properties");
            Log.AddProperty("Barry", "Barry").Warning("Warning event with {Barry:l}");
            Log.Error(new ArgumentOutOfRangeException(nameof(test))).Add("Exception: {Message:l}", "Error Message");
            Log.AddProperty(LurgLevel.Error, "Barry", "Barry").Add("Log an {Error:l}", "Error");
            Log.AddProperty(LurgLevel.Debug, "Barry", "Barry").Add("Just pass the log template with {Barry:l}");
            Log.AddProperty(new ArgumentOutOfRangeException(nameof(test)), "Barry", "Barry")
                .Add("Pass an exception with {Barry:l}");
            Log.AddProperty(test).AddProperty("Barry", "Barry").Add(
                "{Barry:l} wants to pass a dictionary that results in the TestDictKey property having {TestDictKey}");
            Log.Level().Warning("Override the event level and specify params like {Test:l}", "Test");

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
            //Switch masked properties to use MaskPolicy.MaskLettersAndNumbers, allow init event to be logged
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, logWriteInit: true,
                logMaskPolicy: MaskPolicy.MaskLettersAndNumbers));
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
            // ReSharper disable once UseDeconstruction
            foreach (var logFailure in Logging.LogFailures)
                Log.Level(LurgLevel.Error)
                    .Add("Failure - {LogType}: {FailureReason}", logFailure.Key, logFailure.Value);

            Logging.Close();
        }
    }
}