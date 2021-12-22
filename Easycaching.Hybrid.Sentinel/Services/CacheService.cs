using EasyCaching.Core;
using System;
using System.Threading.Tasks;

namespace Easycaching.Hybrid.Sentinel.Services
{
    public class CacheService : ICacheService
    {
        private readonly IHybridCachingProvider _hybridCachingProvider;

        public CacheService(IHybridCachingProvider hybridCachingPtovider)
        {
            this._hybridCachingProvider = hybridCachingPtovider;
        }
        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if (response == null) return;
            await _hybridCachingProvider.SetAsync(cacheKey, response, timeToLive);
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var cachedResponse = await _hybridCachingProvider.GetAsync<string>(cacheKey);
            return string.IsNullOrWhiteSpace(cachedResponse.Value) ? null : cachedResponse.Value;
        }
    }
}
