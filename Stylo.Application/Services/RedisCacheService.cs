using Microsoft.Extensions.Caching.Distributed;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetStringAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _cache.SetStringAsync(key, value, options, cancellationToken);
        }

        public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _cache.GetStringAsync(key, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            return value is not null;
        }
    }
}