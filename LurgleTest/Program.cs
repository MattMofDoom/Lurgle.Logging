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
                {"TestDictKey", FailureReason.LogTestFailed},
                {"TestClass", new Test()}
            };

            //Add a start log
            Log.Level().Add("{AppName:l} v{AppVersion:l} starting ...");
            Log.Level().Add("Seq Proxy: {Proxy}", Logging.Config.LogSeqUseProxy);
            Log.Add("Simple information log");
            Log.Add(LurgLevel.Debug, "Simple debug log");
            Log.Add("Log with {Properties:l}", args: "Properties");
            Log.Information("Information event");
            Log.Information("Information event with {Properties:l}", args: "Properties");
            Log.Verbose("Verbose event");
            Log.Verbose("Verbose event with {Properties:l}", args: "Properties");
            Log.Debug("Debug event");
            Log.Debug("Debug event with {Properties:l}", args: "Properties");
            Log.Warning("Warning event");
            Log.Warning("Warning event with {Properties:l}", args: "Properties");
            Log.Error("Error event");
            Log.Error("Error event with {Properties:l}", args: "Properties");
            Log.Fatal("Fatal event");
            Log.Fatal("Fatal event with {Properties:l}", args: "Properties");
            Log.AddProperty("TestClass1", new Test()).Add("Logging a test class without destructuring: {TestClass1}");
            Log.AddProperty("TestClass2", new Test(), true).Add("Logging a test class with destructuring: {TestClass2}");
            Log.AddProperty("Barry", "Barry").Warning("Warning event with {Barry:l}");
            Log.Error(new ArgumentOutOfRangeException(nameof(test)), "Exception: {Message:l}", args: "Error Message");
            Log.AddProperty(LurgLevel.Error, "Barry", "Barry").Add("Log an {Error:l}", "Error");
            Log.AddProperty(LurgLevel.Debug, "Barry", "Barry").Add("Just pass the log template with {Barry:l}");
            Log.AddProperty(new ArgumentOutOfRangeException(nameof(test)), "Barry", "Barry")
                .Add("Pass an exception with {Barry:l}");
            Log.AddProperty(new ArgumentOutOfRangeException(nameof(test)), "Mechagodzilla", "Barry")
                .Add("Pass an exception with a masked property: {Mechagodzilla:l}");
            Log.Information("Logging a using a masked property: {Mechagodzilla:l}", args: "Test");
            Log.AddProperty(test, true).AddProperty("Barry", "Barry").Add(
                "{Barry:l} wants to pass a dictionary that results in the TestDictKey property having {TestDictKey} and TestClass having the destructured {TestClass}");
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
            Logging.Init();
            Log.Level().AddProperty("Mechagodzilla", "Godzilla123").AddProperty("password", "godzilla123").Add(
                "Testing masking properties, send complaints to {Email:l}", "mechagodzilla123@monster.rargh");
            Log.Level().AddProperty("Test", new Test(), true).Add("Test passing a destructured class with alternate mask policy {Test}");
            Logging.AddCommonProperty("TestCommonMask2", "1234mask", update: true);
            Log.Level().Add("Test updating a common property");
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