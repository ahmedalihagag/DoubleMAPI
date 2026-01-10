using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Get value from cache by key
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Set value in cache with expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Remove specific key from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Remove multiple keys from cache
        /// </summary>
        Task RemoveAsync(params string[] keys);

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Clear all cache (use with caution)
        /// </summary>
        Task ClearAllAsync();
    }
}
