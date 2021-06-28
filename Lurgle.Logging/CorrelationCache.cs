using System;
using System.Linq;
using System.Runtime.Caching;

// ReSharper disable UnusedMember.Global


namespace Lurgle.Logging
{
    /// <summary>
    ///     Cache for correlation ids by thread
    /// </summary>
    public class CorrelationCache
    {
        private readonly MemoryCache _cache;
        private readonly CacheItemPolicy _policy;

        /// <summary>
        ///     Correlation cache containing correlation ids by thread
        /// </summary>
        /// <param name="expiration"></param>
        public CorrelationCache(int expiration)
        {
            _cache = new MemoryCache("CorrelationCache");
            _policy = new CacheItemPolicy
                {SlidingExpiration = TimeSpan.FromSeconds(expiration), Priority = CacheItemPriority.Default};
        }

        /// <summary>
        ///     Count of items in cache
        /// </summary>
        public int Count => _cache.Count();

        /// <summary>
        ///     Add a thread's correlation id to the cache
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="correlationId"></param>
        public void Add(int threadId, string correlationId)
        {
            if (!Contains(threadId))
                _cache.Set(threadId.ToString(), correlationId, _policy);
        }

        /// <summary>
        ///     Replace a thread's correlation id in the cache
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="correlationId"></param>
        public void Replace(int threadId, string correlationId)
        {
            if (Contains(threadId))
                _cache.Remove(threadId.ToString());
            _cache.Set(threadId.ToString(), correlationId, _policy);
        }

        /// <summary>
        ///     Remove a thread's correlation id from the cache
        /// </summary>
        /// <param name="threadId"></param>
        public void Remove(int threadId)
        {
            if (Contains(threadId))
                _cache.Remove(threadId.ToString());
        }

        /// <summary>
        ///     Get a thread's correlation id from the cache
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public string Get(int threadId)
        {
            return (string) _cache.Get(threadId.ToString());
        }

        /// <summary>
        ///     Return whether a given thread's correlation id is in the cache
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public bool Contains(int threadId)
        {
            return _cache.Get(threadId.ToString()) != null;
        }

        /// <summary>
        ///     Remove all threads from the cache
        /// </summary>
        public void Clear()
        {
            _cache.Trim(100);
        }
    }
}