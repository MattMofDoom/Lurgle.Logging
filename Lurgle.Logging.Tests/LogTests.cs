using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Lurgle.Logging.Tests
{
    public class LogTests
    {
        private static readonly Dictionary<int, string> ThreadList = new Dictionary<int, string>();

        public LogTests()
        {
            Logging.SetConfig(new LoggingConfig(appName: "TestMaster Prime",
                logType: new List<LogType> {LogType.Console}, enableCorrelationCache: true, correlationCacheExpiry:2));
            Logging.Init();
        }

        private static void CreateLog(LurgLevel level = LurgLevel.Information, string correlationId = null)
        {
            Log.Level(level, correlationId).Add("Test");
        }

        private void LogRunner()
        {
            var watch = new Stopwatch();
            watch.Start();
            CreateLog();
            ThreadList.Add(Thread.CurrentThread.ManagedThreadId, Logging.Cache.Get(Thread.CurrentThread.ManagedThreadId));
            for (var i = 1; i < 1001; i++)
            {
                CreateLog();
                Thread.Sleep(25);
            }
            watch.Stop();
        }

        /// <summary>
        /// Test that a single thread's correlation id is added to the cache
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
            Assert.True(Logging.Cache.Contains(threadId));
        }

        /// <summary>
        /// Test that threads maintain their correlation id throughout a run, and then expire from the cache
        /// </summary>
        [Fact]
        public void LogThreadCacheTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, enableCorrelationCache: true));
            Logging.Init();
            var count = Logging.Cache.Count;
            for (var i = 1; i < 21; i++)
            {
                new Thread(LogRunner).Start();
                count++;
            }

            Thread.Sleep(15000);
            Assert.True(Logging.Cache.Count == count);
            foreach (var thread in ThreadList)
            {
                var correlationId = Logging.Cache.Get(thread.Key);
                Assert.True(correlationId == thread.Value);
            }
            Thread.Sleep(15000);
            Assert.True(Logging.Cache.Count == count);
            foreach (var thread in ThreadList)
            {
                var correlationId = Logging.Cache.Get(thread.Key);
                Assert.True(correlationId == thread.Value);
            }
            Thread.Sleep(10000);
            watch.Stop();
            Assert.True(Logging.Cache.Count < count);

        }

        /// <summary>
        /// Test that a static correlation id can be maintained
        /// </summary>
        [Fact]
        public void StaticCorrelationTest()
        {
            Logging.Close();
            Logging.SetConfig(new LoggingConfig(Logging.Config, enableCorrelationCache: false));
            CreateLog(LurgLevel.Error, "Barry");
            Assert.True(Logging.CorrelationId == "Barry");
            CreateLog();
            Assert.True(Logging.CorrelationId == "Barry");
        }
    }
}