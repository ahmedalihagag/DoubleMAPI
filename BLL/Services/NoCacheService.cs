namespace BLL.Services;

using BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// ✅ No-op cache service used when Redis is unavailable or disabled
/// Implements the Null Object pattern to gracefully handle missing Redis
/// </summary>
public class NoCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key) where T : class => Task.FromResult<T?>(null);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class => Task.CompletedTask;

    public Task RemoveAsync(params string[] keys) => Task.CompletedTask;
    public Task RemoveAsync(string key) => Task.CompletedTask;

    public Task ClearAllAsync() => Task.CompletedTask;

    public Task<bool> ExistsAsync(string key) => Task.FromResult(false);
}
