using EDDY.IS.WidgetProvider.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Services
{
    public enum CacheStorage
    {
        Distributed = 1,
        Memory = 2
    }

    public class CacheService : ICacheService
    {
        private static IConfiguration _config;
        private static IMemoryCache _cache;
        private string _cachePrefix = "";
        private TimeSpan _defaultExpiration;
        private bool? _cacheEnabled;

        public CacheService(IConfiguration config, IMemoryCache cache)
        {
            _config = config;
            _cache = cache;
            _cachePrefix = _config.GetSection("RedisSettings")["CachePrefix"];
            _cacheEnabled = Convert.ToBoolean(_config.GetSection("RedisSettings")["CacheEnabled"]);
            var expiration = config.GetSection("RedisSettings")["CacheDuration"];

            if (!String.IsNullOrEmpty(expiration))
                _defaultExpiration = new TimeSpan(0, Convert.ToInt32(expiration), 0);
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = _config.GetConnectionString("RedisConnection");
            //_cachePrefix = _config.GetSection("RedisSettings")["RedisCacheKeyPrefix"];

            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        public string GetCacheItem(string redisKey)
        {
            if (_cacheEnabled.HasValue && _cacheEnabled.Value)
            {
                string cacheItem = null;

                IDatabase cache = null;

                cache = lazyConnection.Value.GetDatabase();

                cacheItem = cache.StringGet(_cachePrefix + "_" + redisKey);
                
                return cacheItem;
            }

            return "";
        }

        public bool RemoveCacheItem(string key)
        {
            throw new NotImplementedException();
        }

        public bool SetCacheItem(string key, string value)
        {
            return SetCacheItem(key, value, _defaultExpiration);
        }

        public bool SetCacheItem(string key, string value, TimeSpan expiration)
        {
            if (_cacheEnabled.HasValue && _cacheEnabled.Value)
            {
                IDatabase cache = null;
                cache = lazyConnection.Value.GetDatabase();

                return cache.StringSet(_cachePrefix + "_" + key, value, expiration);
            }

            return false;
        }

        public object GetCacheItem(string key, CacheStorage storageType = CacheStorage.Memory)
        {
            object returnValue = null;

            if (storageType == CacheStorage.Distributed)
            {
                throw new NotImplementedException();
            }
            else
            {
                returnValue = _cache.Get(key);
            }

            return returnValue;
        }

        public bool SetCacheItem(string key, object value, TimeSpan? expiration = null, bool indefiniteExpiration = false, CacheStorage storageType = CacheStorage.Memory)
        {
            if (storageType == CacheStorage.Distributed)
            {
                throw new NotImplementedException();
            }
            else
            {
                MemoryCacheEntryOptions opts = new MemoryCacheEntryOptions();

                if (indefiniteExpiration) opts.Priority = CacheItemPriority.NeverRemove;

                _cache.Set(key, value, opts);

                return true;
            }
        }
    }
}
