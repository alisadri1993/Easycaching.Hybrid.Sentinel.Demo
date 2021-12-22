using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easycaching.Hybrid.Sentinel.Services
{
    public interface ICacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive);
        Task<string> GetCachedResponseAsync(string cacheKey);
    }
}
