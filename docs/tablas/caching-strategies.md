# Caching Strategies for .NET

**Guía completa de estrategias de caché aplicadas al desarrollo .NET con patrones, implementaciones y optimizaciones.**
Este documento cubre desde caché local hasta distribuido con Redis, incluyendo invalidación y patrones avanzados.
Fundamental para optimizar rendimiento, reducir latencia y escalar aplicaciones de alto tráfico.

## Cache Patterns Overview

**Resumen de patrones de caché más utilizados con sus características y casos de uso en aplicaciones .NET.**
Esta tabla proporciona una guía rápida para elegir el patrón apropiado según los requisitos de consistencia y rendimiento.
Esencial para optimizar la estrategia de caché según el contexto específico de la aplicación.

| **Patrón**        | **Descripción**                       | **Consistencia** | **Rendimiento** | **Caso de Uso**                 |
| ----------------- | ------------------------------------- | ---------------- | --------------- | ------------------------------- |
| **Cache-Aside**   | Aplicación maneja caché manualmente   | Eventual         | Alto            | Datos leídos frecuentemente     |
| **Write-Through** | Escribe en caché y BD simultáneamente | Fuerte           | Medio           | Datos críticos                  |
| **Write-Behind**  | Escribe en caché, BD asincrónicamente | Eventual         | Muy Alto        | Alto volumen de escrituras      |
| **Write-Around**  | Escribe solo en BD, omite caché       | Eventual         | Variable        | Datos escritos una vez          |
| **Refresh-Ahead** | Actualiza caché antes de expiración   | Eventual         | Alto            | Datos con expiración predecible |

## Cache-Aside Pattern

**Implementación del patrón Cache-Aside donde la aplicación maneja explícitamente el caché.**
Este patrón proporciona control total sobre cuándo y cómo se cachean los datos.
Ideal para datos de solo lectura o actualizados infrecuentemente.

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<ProductService> _logger;

    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
        SlidingExpiration = TimeSpan.FromMinutes(5),
        Priority = CacheItemPriority.High
    };

    public async Task<Product> GetProductAsync(int productId)
    {
        var cacheKey = $"product:{productId}";

        // 1. Try L1 Cache (Memory)
        if (_cache.TryGetValue(cacheKey, out Product cachedProduct))
        {
            _logger.LogDebug("Product {ProductId} found in L1 cache", productId);
            return cachedProduct;
        }

        // 2. Try L2 Cache (Distributed)
        var distributedValue = await _distributedCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(distributedValue))
        {
            var product = JsonSerializer.Deserialize<Product>(distributedValue);

            // Populate L1 cache
            _cache.Set(cacheKey, product, _cacheOptions);

            _logger.LogDebug("Product {ProductId} found in L2 cache", productId);
            return product;
        }

        // 3. Load from database
        var dbProduct = await _repository.GetByIdAsync(productId);
        if (dbProduct != null)
        {
            // Store in both cache levels
            var serializedProduct = JsonSerializer.Serialize(dbProduct);
            var distributedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };

            await _distributedCache.SetStringAsync(cacheKey, serializedProduct, distributedOptions);
            _cache.Set(cacheKey, dbProduct, _cacheOptions);

            _logger.LogDebug("Product {ProductId} loaded from database and cached", productId);
        }

        return dbProduct;
    }

    public async Task UpdateProductAsync(Product product)
    {
        // Update database first
        await _repository.UpdateAsync(product);

        // Invalidate cache entries
        var cacheKey = $"product:{product.Id}";
        _cache.Remove(cacheKey);
        await _distributedCache.RemoveAsync(cacheKey);

        // Optionally warm up cache immediately
        _cache.Set(cacheKey, product, _cacheOptions);
        var serializedProduct = JsonSerializer.Serialize(product);
        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        await _distributedCache.SetStringAsync(cacheKey, serializedProduct, distributedOptions);

        _logger.LogInformation("Product {ProductId} updated and cache refreshed", product.Id);
    }
}
```

## Write-Through Pattern

**Implementación del patrón Write-Through donde las escrituras van simultáneamente a caché y base de datos.**
Este patrón garantiza consistencia fuerte entre caché y almacenamiento persistente.
Ideal para datos críticos donde la consistencia es más importante que la latencia de escritura.

```csharp
public class UserProfileService
{
    private readonly IUserRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserProfileService> _logger;

    public async Task UpdateUserProfileAsync(UserProfile profile)
    {
        var cacheKey = $"user:profile:{profile.UserId}";

        using var activity = Activity.StartActivity("UpdateUserProfile");
        activity?.SetTag("user.id", profile.UserId);

        try
        {
            // Transactional approach - both operations must succeed
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                // 1. Update database within transaction
                await _repository.UpdateAsync(profile);

                // 2. Update cache
                var serializedProfile = JsonSerializer.Serialize(profile);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                };

                await _cache.SetStringAsync(cacheKey, serializedProfile, cacheOptions);

                // 3. Commit transaction only if both operations succeeded
                await transaction.CommitAsync();

                _logger.LogInformation("User profile {UserId} updated successfully", profile.UserId);

                // Publish event for other components
                await PublishUserProfileUpdatedEventAsync(profile);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile {UserId}", profile.UserId);

            // Ensure cache consistency by removing potentially stale data
            await _cache.RemoveAsync(cacheKey);
            throw;
        }
    }

    public async Task<UserProfile> GetUserProfileAsync(string userId)
    {
        var cacheKey = $"user:profile:{userId}";

        // Try cache first
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            return JsonSerializer.Deserialize<UserProfile>(cachedValue);
        }

        // Load from database and cache
        var profile = await _repository.GetByUserIdAsync(userId);
        if (profile != null)
        {
            var serializedProfile = JsonSerializer.Serialize(profile);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(cacheKey, serializedProfile, cacheOptions);
        }

        return profile;
    }
}
```

## Write-Behind (Write-Back) Pattern

**Implementación del patrón Write-Behind para máximo rendimiento con escrituras asíncronas.**
Este patrón optimiza la latencia escribiendo primero al caché y persistiendo asincrónicamente.
Ideal para aplicaciones de alto volumen donde la latencia de escritura es crítica.

```csharp
public class HighVolumeUserActivityService
{
    private readonly IMemoryCache _cache;
    private readonly IUserActivityRepository _repository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HighVolumeUserActivityService> _logger;

    private readonly Timer _flushTimer;
    private readonly ConcurrentDictionary<string, UserActivityBatch> _pendingWrites;

    public HighVolumeUserActivityService(
        IMemoryCache cache,
        IUserActivityRepository repository,
        IServiceScopeFactory scopeFactory,
        ILogger<HighVolumeUserActivityService> logger)
    {
        _cache = cache;
        _repository = repository;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pendingWrites = new ConcurrentDictionary<string, UserActivityBatch>();

        // Flush to database every 5 seconds
        _flushTimer = new Timer(FlushPendingWrites, null,
            TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public async Task RecordUserActivityAsync(UserActivity activity)
    {
        var cacheKey = $"activity:{activity.UserId}:{activity.SessionId}";

        // 1. Update cache immediately (fast response)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, activity, cacheOptions);

        // 2. Queue for asynchronous database write
        var batchKey = $"{activity.UserId}:{DateTime.UtcNow:yyyy-MM-dd-HH}";
        _pendingWrites.AddOrUpdate(batchKey,
            new UserActivityBatch { Activities = { activity } },
            (key, existing) =>
            {
                existing.Activities.Add(activity);
                existing.LastModified = DateTime.UtcNow;
                return existing;
            });

        _logger.LogDebug("User activity recorded for user {UserId}", activity.UserId);
    }

    private async void FlushPendingWrites(object state)
    {
        if (_pendingWrites.IsEmpty) return;

        var itemsToFlush = _pendingWrites.ToArray();
        var flushedKeys = new List<string>();

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserActivityRepository>();

        try
        {
            foreach (var (key, batch) in itemsToFlush)
            {
                // Only flush batches older than 1 second to allow batching
                if (DateTime.UtcNow - batch.LastModified < TimeSpan.FromSeconds(1))
                    continue;

                await repository.BulkInsertAsync(batch.Activities);
                flushedKeys.Add(key);

                _logger.LogDebug("Flushed {Count} activities for batch {BatchKey}",
                    batch.Activities.Count, key);
            }

            // Remove flushed items
            foreach (var key in flushedKeys)
            {
                _pendingWrites.TryRemove(key, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing pending writes to database");
            // Keep items in queue for retry
        }
    }

    public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, DateTime date)
    {
        var cacheKey = $"activities:{userId}:{date:yyyy-MM-dd}";

        // Try cache first
        if (_cache.TryGetValue(cacheKey, out List<UserActivity> cachedActivities))
        {
            return cachedActivities;
        }

        // Load from database
        var activities = await _repository.GetByUserAndDateAsync(userId, date);

        // Cache the results
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        _cache.Set(cacheKey, activities, cacheOptions);

        return activities;
    }

    // Ensure data is flushed on shutdown
    public async Task FlushAllPendingAsync()
    {
        _flushTimer?.Dispose();

        if (!_pendingWrites.IsEmpty)
        {
            FlushPendingWrites(null);
            await Task.Delay(1000); // Give time for final flush
        }
    }
}
```

## Cache Invalidation Strategies

**Estrategias de invalidación de caché para mantener consistencia de datos.**
Esta sección cubre diferentes enfoques desde invalidación manual hasta automática basada en eventos.
Fundamental para evitar datos obsoletos y mantener la confiabilidad del sistema.

### Time-based Invalidation

```csharp
public class TimeBasedCacheService
{
    private readonly IDistributedCache _cache;

    public async Task SetWithTtlAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var options = new DistributedCacheEntryOptions();

        if (ttl.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = ttl.Value;
        }
        else
        {
            // Default TTL strategy based on data type
            options.AbsoluteExpirationRelativeToNow = GetDefaultTtl(typeof(T));
        }

        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options);
    }

    private TimeSpan GetDefaultTtl(Type dataType)
    {
        return dataType.Name switch
        {
            nameof(User) => TimeSpan.FromHours(1),          // User data changes occasionally
            nameof(Product) => TimeSpan.FromMinutes(30),     // Product data changes more frequently
            nameof(Inventory) => TimeSpan.FromMinutes(5),    // Inventory changes rapidly
            nameof(PriceList) => TimeSpan.FromMinutes(15),   // Prices change moderately
            _ => TimeSpan.FromMinutes(10)                    // Default fallback
        };
    }
}
```

### Event-based Invalidation

```csharp
public class EventBasedCacheInvalidator
{
    private readonly IDistributedCache _cache;
    private readonly IServiceBus _serviceBus;
    private readonly ILogger<EventBasedCacheInvalidator> _logger;

    public EventBasedCacheInvalidator(
        IDistributedCache cache,
        IServiceBus serviceBus,
        ILogger<EventBasedCacheInvalidator> logger)
    {
        _cache = cache;
        _serviceBus = serviceBus;
        _logger = logger;

        // Subscribe to relevant domain events
        _serviceBus.Subscribe<ProductUpdatedEvent>(InvalidateProductCacheAsync);
        _serviceBus.Subscribe<InventoryChangedEvent>(InvalidateInventoryCacheAsync);
        _serviceBus.Subscribe<UserProfileUpdatedEvent>(InvalidateUserCacheAsync);
    }

    private async Task InvalidateProductCacheAsync(ProductUpdatedEvent eventData)
    {
        var keysToInvalidate = new[]
        {
            $"product:{eventData.ProductId}",
            $"product:details:{eventData.ProductId}",
            $"category:products:{eventData.CategoryId}",
            "products:featured",
            "products:popular"
        };

        await InvalidateKeysAsync(keysToInvalidate);
        _logger.LogInformation("Invalidated product cache for product {ProductId}", eventData.ProductId);
    }

    private async Task InvalidateInventoryCacheAsync(InventoryChangedEvent eventData)
    {
        var keysToInvalidate = new[]
        {
            $"inventory:{eventData.ProductId}",
            $"product:availability:{eventData.ProductId}",
            "products:instock"
        };

        await InvalidateKeysAsync(keysToInvalidate);
        _logger.LogInformation("Invalidated inventory cache for product {ProductId}", eventData.ProductId);
    }

    private async Task InvalidateKeysAsync(string[] keys)
    {
        var tasks = keys.Select(key => _cache.RemoveAsync(key));
        await Task.WhenAll(tasks);
    }
}
```

### Tag-based Invalidation

```csharp
public class TagBasedCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, HashSet<string>> _tagToKeys;
    private readonly ConcurrentDictionary<string, HashSet<string>> _keyToTags;

    public TagBasedCacheService(IMemoryCache cache)
    {
        _cache = cache;
        _tagToKeys = new ConcurrentDictionary<string, HashSet<string>>();
        _keyToTags = new ConcurrentDictionary<string, HashSet<string>>();
    }

    public void Set<T>(string key, T value, string[] tags, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }

        // Register callback to clean up tags when item expires
        options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
        {
            CleanupTagsForKey(evictedKey.ToString());
        });

        _cache.Set(key, value, options);

        // Associate key with tags
        _keyToTags.AddOrUpdate(key,
            new HashSet<string>(tags),
            (k, existing) => new HashSet<string>(tags));

        // Associate tags with key
        foreach (var tag in tags)
        {
            _tagToKeys.AddOrUpdate(tag,
                new HashSet<string> { key },
                (t, existing) =>
                {
                    existing.Add(key);
                    return existing;
                });
        }
    }

    public void InvalidateByTag(string tag)
    {
        if (_tagToKeys.TryGetValue(tag, out var keys))
        {
            foreach (var key in keys.ToArray())
            {
                _cache.Remove(key);
                CleanupTagsForKey(key);
            }

            _tagToKeys.TryRemove(tag, out _);
        }
    }

    private void CleanupTagsForKey(string key)
    {
        if (_keyToTags.TryRemove(key, out var tags))
        {
            foreach (var tag in tags)
            {
                if (_tagToKeys.TryGetValue(tag, out var keysForTag))
                {
                    keysForTag.Remove(key);
                    if (!keysForTag.Any())
                    {
                        _tagToKeys.TryRemove(tag, out _);
                    }
                }
            }
        }
    }
}

// Usage example
public class ProductCacheService
{
    private readonly TagBasedCacheService _cache;

    public void CacheProduct(Product product)
    {
        var tags = new[]
        {
            $"category:{product.CategoryId}",
            $"brand:{product.BrandId}",
            "products"
        };

        _cache.Set($"product:{product.Id}", product, tags, TimeSpan.FromHours(1));
    }

    public void InvalidateProductsByCategory(int categoryId)
    {
        _cache.InvalidateByTag($"category:{categoryId}");
    }
}
```

## Redis Advanced Patterns

**Patrones avanzados de Redis para caché distribuido de alto rendimiento.**
Esta sección cubre implementaciones específicas con Redis para escenarios empresariales complejos.
Esencial para maximizar el rendimiento y confiabilidad de sistemas distribuidos de gran escala.

### Redis Pub/Sub for Cache Invalidation

```csharp
public class RedisCacheInvalidationService
{
    private readonly IDatabase _database;
    private readonly ISubscriber _subscriber;
    private readonly IMemoryCache _localCache;
    private readonly ILogger<RedisCacheInvalidationService> _logger;

    public RedisCacheInvalidationService(
        IConnectionMultiplexer redis,
        IMemoryCache localCache,
        ILogger<RedisCacheInvalidationService> logger)
    {
        _database = redis.GetDatabase();
        _subscriber = redis.GetSubscriber();
        _localCache = localCache;
        _logger = logger;

        // Subscribe to invalidation events
        _subscriber.Subscribe("cache:invalidate", OnCacheInvalidationMessage);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        // Set in Redis
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiration);

        // Set in local cache
        var localOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };
        _localCache.Set(key, value, localOptions);
    }

    public async Task<T> GetAsync<T>(string key) where T : class
    {
        // Try local cache first (L1)
        if (_localCache.TryGetValue(key, out T localValue))
        {
            return localValue;
        }

        // Try Redis cache (L2)
        var redisValue = await _database.StringGetAsync(key);
        if (redisValue.HasValue)
        {
            var value = JsonSerializer.Deserialize<T>(redisValue);

            // Populate local cache
            var localOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            _localCache.Set(key, value, localOptions);

            return value;
        }

        return null;
    }

    public async Task InvalidateAsync(string key)
    {
        // Remove from Redis
        await _database.KeyDeleteAsync(key);

        // Remove from local cache
        _localCache.Remove(key);

        // Notify other instances
        await _subscriber.PublishAsync("cache:invalidate", key);
    }

    private void OnCacheInvalidationMessage(RedisChannel channel, RedisValue message)
    {
        var key = message.ToString();
        _localCache.Remove(key);
        _logger.LogDebug("Invalidated local cache key: {Key}", key);
    }
}
```

### Redis Lua Scripts for Atomic Operations

```csharp
public class RedisAtomicOperations
{
    private readonly IDatabase _database;

    // Lua script for atomic increment with expiration
    private const string IncrementWithExpirationScript = @"
        local key = KEYS[1]
        local increment = tonumber(ARGV[1])
        local expiration = tonumber(ARGV[2])

        local current = redis.call('GET', key)
        if current == false then
            current = 0
        else
            current = tonumber(current)
        end

        local new_value = current + increment
        redis.call('SET', key, new_value, 'EX', expiration)
        return new_value
    ";

    // Lua script for conditional cache update
    private const string ConditionalUpdateScript = @"
        local key = KEYS[1]
        local expected_version = ARGV[1]
        local new_value = ARGV[2]
        local new_version = ARGV[3]
        local expiration = tonumber(ARGV[4])

        local version_key = key .. ':version'
        local current_version = redis.call('GET', version_key)

        if current_version == expected_version then
            redis.call('SET', key, new_value, 'EX', expiration)
            redis.call('SET', version_key, new_version, 'EX', expiration)
            return 1
        else
            return 0
        end
    ";

    public async Task<long> IncrementWithExpirationAsync(string key, long increment, TimeSpan expiration)
    {
        var result = await _database.ScriptEvaluateAsync(
            IncrementWithExpirationScript,
            keys: new RedisKey[] { key },
            values: new RedisValue[] { increment, (int)expiration.TotalSeconds });

        return (long)result;
    }

    public async Task<bool> ConditionalUpdateAsync<T>(
        string key,
        T newValue,
        string expectedVersion,
        TimeSpan expiration)
    {
        var newVersion = Guid.NewGuid().ToString();
        var json = JsonSerializer.Serialize(newValue);

        var result = await _database.ScriptEvaluateAsync(
            ConditionalUpdateScript,
            keys: new RedisKey[] { key },
            values: new RedisValue[] {
                expectedVersion,
                json,
                newVersion,
                (int)expiration.TotalSeconds
            });

        return (int)result == 1;
    }
}
```

## Performance Optimization

**Optimizaciones de rendimiento para sistemas de caché en aplicaciones .NET de alta escala.**
Esta sección cubre técnicas avanzadas para minimizar latencia y maximizar throughput.
Fundamental para sistemas que manejan millones de operaciones de caché por segundo.

### Connection Pooling and Multiplexing

```csharp
public class OptimizedRedisConfiguration
{
    public static IServiceCollection AddOptimizedRedis(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configuration = ConfigurationOptions.Parse(connectionString);

            // Optimize for high-throughput scenarios
            configuration.ConnectRetry = 3;
            configuration.ConnectTimeout = 5000;
            configuration.SyncTimeout = 1000;
            configuration.AsyncTimeout = 1000;
            configuration.KeepAlive = 60;
            configuration.ReconnectRetryPolicy = new ExponentialRetry(1000, 30000);

            // Connection pooling settings
            configuration.DefaultDatabase = 0;
            configuration.AbortOnConnectFail = false;

            // Enable multiplexing
            configuration.AllowAdmin = false;
            configuration.ChannelPrefix = "app:";

            return ConnectionMultiplexer.Connect(configuration);
        });

        services.AddSingleton<IDatabase>(provider =>
            provider.GetService<IConnectionMultiplexer>().GetDatabase());

        services.AddSingleton<ISubscriber>(provider =>
            provider.GetService<IConnectionMultiplexer>().GetSubscriber());

        return services;
    }
}
```

### Batch Operations

```csharp
public class BatchCacheOperations
{
    private readonly IDatabase _database;

    public async Task<Dictionary<string, T>> GetBatchAsync<T>(IEnumerable<string> keys)
        where T : class
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var values = await _database.StringGetAsync(redisKeys);

        var results = new Dictionary<string, T>();

        for (int i = 0; i < redisKeys.Length; i++)
        {
            if (values[i].HasValue)
            {
                var value = JsonSerializer.Deserialize<T>(values[i]);
                results[redisKeys[i]] = value;
            }
        }

        return results;
    }

    public async Task SetBatchAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        var batch = _database.CreateBatch();
        var tasks = new List<Task>();

        foreach (var (key, value) in items)
        {
            var json = JsonSerializer.Serialize(value);
            tasks.Add(batch.StringSetAsync(key, json, expiration));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }
}
```

## Cache Monitoring and Metrics

**Métricas y monitoreo para sistemas de caché en producción.**
Esta tabla identifica KPIs críticos y herramientas de observabilidad para optimización continua.
Esencial para mantener rendimiento óptimo y detectar problemas antes de que afecten usuarios.

| **Métrica**         | **Descripción**                             | **Umbral Saludable**            | **Herramienta .NET**        |
| ------------------- | ------------------------------------------- | ------------------------------- | --------------------------- |
| **Hit Ratio**       | % de requests que encuentran datos en caché | > 85%                           | Custom counters             |
| **Miss Ratio**      | % de requests que no encuentran datos       | < 15%                           | Application Insights        |
| **Latency P95**     | Tiempo de respuesta percentil 95            | < 5ms local, < 20ms distribuido | BenchmarkDotNet             |
| **Memory Usage**    | Uso de memoria del caché                    | < 80% de límite                 | Performance counters        |
| **Eviction Rate**   | Frecuencia de expulsión de elementos        | Estable, sin picos              | Redis INFO                  |
| **Connection Pool** | Conexiones activas vs disponibles           | < 70% utilización               | StackExchange.Redis metrics |
| **Network I/O**     | Throughput de red para caché distribuido    | Sin saturación                  | System counters             |

### Implementation Example

```csharp
public class CacheMetrics
{
    private readonly IMetricsLogger _metrics;
    private readonly Timer _metricsTimer;

    public CacheMetrics(IMetricsLogger metrics)
    {
        _metrics = metrics;
        _metricsTimer = new Timer(RecordMetrics, null,
            TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public void RecordCacheHit(string cacheType, string operation)
    {
        _metrics.Counter("cache.hits")
            .WithTag("type", cacheType)
            .WithTag("operation", operation)
            .Increment();
    }

    public void RecordCacheMiss(string cacheType, string operation)
    {
        _metrics.Counter("cache.misses")
            .WithTag("type", cacheType)
            .WithTag("operation", operation)
            .Increment();
    }

    public void RecordLatency(string cacheType, TimeSpan duration)
    {
        _metrics.Histogram("cache.latency")
            .WithTag("type", cacheType)
            .Record(duration.TotalMilliseconds);
    }
}
```
