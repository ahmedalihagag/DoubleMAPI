using BLL.Interfaces;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger _logger;
        private readonly IDatabase _database;

        public CacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = Log.ForContext<CacheService>();
            _database = _connectionMultiplexer.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return null;

                _logger.Debug("Retrieving cache key: {Key}", key);

                var value = await _database.StringGetAsync(key);

                if (!value.HasValue)
                {
                    _logger.Debug("Cache miss for key: {Key}", key);
                    return null;
                }

                var deserialized = JsonSerializer.Deserialize<T>(value.ToString());
                _logger.Debug("Cache hit for key: {Key}", key);

                return deserialized;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || value == null)
                    return;

                _logger.Debug("Setting cache key: {Key}, Expiration: {Expiration}", key, expiration);

                var serialized = JsonSerializer.Serialize(value);
                if (expiration.HasValue)
                {
                    await _database.StringSetAsync(key, serialized, new StackExchange.Redis.Expiration(expiration.Value));
                }
                else
                {
                    await _database.StringSetAsync(key, serialized);
                }

                _logger.Debug("Cache set successfully for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;

                _logger.Debug("Removing cache key: {Key}", key);
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(params string[] keys)
        {
            try
            {
                if (keys == null || keys.Length == 0)
                    return;

                var validKeys = keys.Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => (RedisKey)(object)k)
                    .ToArray();

                if (validKeys.Length == 0)
                    return;

                _logger.Debug("Removing {Count} cache keys", validKeys.Length);
                await _database.KeyDeleteAsync(validKeys);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error removing multiple cache keys");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return false;

                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking cache key existence: {Key}", key);
                return false;
            }
        }

        public async Task ClearAllAsync()
        {
            try
            {
                _logger.Warning("Clearing all cache");

                var endpoints = _connectionMultiplexer.GetEndPoints();

                foreach (var endpoint in endpoints)
                {
                    var server = _connectionMultiplexer.GetServer(endpoint);
                    await server.FlushDatabaseAsync();
                }

                _logger.Warning("All cache cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error clearing all cache");
            }
        }
    }
}
