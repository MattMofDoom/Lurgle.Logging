using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lurgle.Logging.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace Lurgle.Logging.Tests
{
    /// <summary>
    ///     Log unit tests
    /// </summary>
    public class LogTests
    {
        private static readonly Dictionary<int, string> ThreadList = new Dictionary<int, string>();
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        ///     Constructor for unit tests
        /// </summary>
        /// <param name="testOutputHelper"></param>
        public LogTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            Logging.SetConfig(new LoggingConfig(appName: "TestMaster Prime",
                logType: new List<LogType> {LogType.Console}, enableCorrelationCache: true, correlationCacheExpiry: 2));
            Logging.Init();
        }

        /// <summary>
        ///     Create a new ILevel using the passed parameters to simulate a log entry
        /// </summary>
        /// <param name="level"></param>
        /// <param name="correlationId"></param>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static ILevel CreateLog(LurgLevel level = LurgLevel.Information, string correlationId = null)
        {
            //We don't actually need to output to the console
            return Log.Level(level, correlationId);
        }

        /// <summary>
        ///     Run 1000 log entries with a 25ms delay
        /// </summary>
        private void LogRunner()
        {
            var watch = new Stopwatch();
            watch.Start();
            CreateLog();
            ThreadList.Add(Thread.CurrentThread.ManagedThreadId,
                Logging.Cache.Get(Thread.CurrentThread.ManagedThreadId));
            for (var i = 1; i < 1001; i++)
            {
                CreateLog();
                Thread.Sleep(25);
            }

            watch.Stop();
            _testOutputHelper.WriteLine("Thread {0} - Completed all thread runs in {1:N2} seconds",
                Thread.CurrentThread.ManagedThreadId, watch.ElapsedMilliseconds / 1000);
        }

        /// <summary>
        ///     Test that a single thread's correlation id is added to the cache
        /// </summary>
        [Fact]
        public void SingleLogCached()
        {
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, enableCorrelationCache: true));
            Logging.Init();
            Assert.True(Logging.Config.EnableCorrelationCache);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            CreateLog();
            _testOutputHelper.WriteLine("Thread {0} - Cache Count {1}, Correlation Id {2}", threadId,
                Logging.Cache.Count, Logging.Cache.Get(threadId));
            Assert.True(Logging.Cache.Contains(threadId));
        }

        /// <summary>
        ///     Test that threads maintain their correlation id throughout a run, and then expire from the cache
        /// </summary>
        [Fact]
        public void LogThreadCacheTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, enableCorrelationCache: true));
            Logging.Init();
            Logging.Cache.Clear();
            var count = Logging.Cache.Count;
            for (var i = 1; i < 21; i++)
            {
                new Thread(LogRunner).Start();
                count++;
            }

            _testOutputHelper.WriteLine("Total in Cache at {0:N2} seconds: {1}", watch.ElapsedMilliseconds / 1000,
                Logging.Cache.Count);
            Thread.Sleep(15000);
            _testOutputHelper.WriteLine("Total in Cache after {0:N2} seconds: {1}", watch.ElapsedMilliseconds / 1000,
                Logging.Cache.Count);
            Assert.True(Logging.Cache.Count == count);
            foreach (var thread in ThreadList)
            {
                var correlationId = Logging.Cache.Get(thread.Key);
                _testOutputHelper.WriteLine("Thread {0} ({1:N2} seconds) - Expect: {2}, Matched: {3}", thread.Key,
                    watch.ElapsedMilliseconds / 1000, thread.Value, thread.Value == correlationId);
                Assert.True(correlationId == thread.Value);
            }

            Thread.Sleep(15000);
            _testOutputHelper.WriteLine("Total in Cache after {0:N2} seconds: {1}", watch.ElapsedMilliseconds / 1000,
                Logging.Cache.Count);
            Assert.True(Logging.Cache.Count == count);
            foreach (var thread in ThreadList)
            {
                var correlationId = Logging.Cache.Get(thread.Key);
                _testOutputHelper.WriteLine("Thread {0} ({1:N2} seconds) - Expect: {2}, Matched: {3}", thread.Key,
                    watch.ElapsedMilliseconds / 1000, thread.Value, thread.Value == correlationId);
                Assert.True(correlationId == thread.Value);
            }

            Thread.Sleep(10000);
            watch.Stop();
            _testOutputHelper.WriteLine("Cache expiry count at {0:N2} seconds: {1}", watch.ElapsedMilliseconds / 1000,
                Logging.Cache.Count);
            Assert.True(Logging.Cache.Count < count);
        }

        /// <summary>
        ///     Test that a static correlation id can be maintained
        /// </summary>
        [Fact]
        public void StaticCorrelationTest()
        {
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, enableCorrelationCache: false));
            CreateLog(LurgLevel.Error, "Barry");
            _testOutputHelper.WriteLine("Correlation Id: {0}", Logging.CorrelationId);
            Assert.True(Logging.CorrelationId == "Barry");
            CreateLog();
            Assert.True(Logging.CorrelationId == "Barry");
        }
    }
}