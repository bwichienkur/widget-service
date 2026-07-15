using EDDY.IS.WidgetProvider.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface ICacheService
    {
        string GetCacheItem(string key);
        bool SetCacheItem(string key, string value);
        bool RemoveCacheItem(string key);
        bool SetCacheItem(string key, string value, TimeSpan expiration);
        object GetCacheItem(string key, CacheStorage storageType = CacheStorage.Memory);
        bool SetCacheItem(string key, object value, TimeSpan? expiration = null, bool indefiniteExpiration = false, CacheStorage storageType = CacheStorage.Memory);
    }
}
