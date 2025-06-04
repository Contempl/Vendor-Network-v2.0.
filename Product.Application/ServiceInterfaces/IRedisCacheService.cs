using Microsoft.Extensions.Caching.Distributed;

namespace Product.Application.ServiceInterfaces;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T? value, DistributedCacheEntryOptions? options = null);
    Task RemoveAsync(string key);
}