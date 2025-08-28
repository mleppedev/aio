# Resilience Patterns for .NET

**Guía completa de patrones de resiliencia para construir aplicaciones .NET robustas y tolerantes a fallos.**
Este documento cubre desde circuit breakers hasta bulk heads, con implementaciones prácticas en C#.
Fundamental para sistemas distribuidos que requieren alta disponibilidad y recuperación automática ante fallos.

## Resilience Patterns Overview

**Resumen de patrones de resiliencia fundamentales con sus características y casos de uso.**
Esta tabla proporciona una guía rápida para elegir el patrón apropiado según el tipo de fallo y contexto.
Esencial para diseñar sistemas que puedan manejar fallos gracefully y mantener operatividad parcial.

| **Patrón**          | **Propósito**                        | **Cuándo Usar**        | **Beneficios**           | **Complejidad** |
| ------------------- | ------------------------------------ | ---------------------- | ------------------------ | --------------- |
| **Circuit Breaker** | Evitar llamadas a servicios fallidos | Alta tasa de fallos    | Fallo rápido, protección | Media           |
| **Retry**           | Reintentar operaciones fallidas      | Fallos transitorios    | Recuperación automática  | Baja            |
| **Timeout**         | Limitar tiempo de espera             | Llamadas lentas        | Evitar bloqueos          | Baja            |
| **Bulkhead**        | Aislar recursos críticos             | Proteger recursos      | Aislamiento de fallos    | Alta            |
| **Cache-aside**     | Datos disponibles offline            | Fallos de BD/API       | Disponibilidad           | Media           |
| **Rate Limiting**   | Controlar carga del sistema          | Picos de tráfico       | Protección de recursos   | Media           |
| **Fallback**        | Respuesta alternativa                | Servicio no disponible | Funcionalidad degradada  | Baja            |

## Circuit Breaker Pattern

**Implementación del patrón Circuit Breaker para proteger servicios externos de cascadas de fallos.**
Este patrón implementa estados (Closed, Open, Half-Open) para controlar automáticamente el acceso a servicios.
Fundamental para evitar que fallos en servicios externos afecten la estabilidad general del sistema.

```csharp
public class CircuitBreaker
{
    private readonly string _name;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _retryTimeout;
    private readonly ILogger<CircuitBreaker> _logger;

    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitBreakerState _state;
    private readonly object _lock = new object();

    public CircuitBreaker(
        string name,
        int failureThreshold = 5,
        TimeSpan timeout = default,
        TimeSpan retryTimeout = default,
        ILogger<CircuitBreaker> logger = null)
    {
        _name = name;
        _failureThreshold = failureThreshold;
        _timeout = timeout == default ? TimeSpan.FromSeconds(30) : timeout;
        _retryTimeout = retryTimeout == default ? TimeSpan.FromMinutes(1) : retryTimeout;
        _logger = logger;
        _state = CircuitBreakerState.Closed;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ValidateState();

        try
        {
            using var timeoutCts = new CancellationTokenSource(_timeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            var result = await operation().ConfigureAwait(false);
            OnSuccess();
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (OperationCanceledException)
        {
            OnFailure(new TimeoutException($"Operation timed out after {_timeout}"));
            throw new TimeoutException($"Circuit breaker '{_name}': Operation timed out");
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw new CircuitBreakerOpenException($"Circuit breaker '{_name}' is open", ex);
        }
    }

    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation().ConfigureAwait(false);
            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    private void ValidateState()
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitBreakerState.Open:
                    if (DateTime.UtcNow - _lastFailureTime > _retryTimeout)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _logger?.LogInformation("Circuit breaker '{Name}' entering Half-Open state", _name);
                    }
                    else
                    {
                        throw new CircuitBreakerOpenException($"Circuit breaker '{_name}' is open");
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    // Allow single request through
                    break;

                case CircuitBreakerState.Closed:
                    // Normal operation
                    break;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
                _logger?.LogInformation("Circuit breaker '{Name}' closed after successful request", _name);
            }
            else if (_state == CircuitBreakerState.Closed && _failureCount > 0)
            {
                _failureCount = 0;
                _logger?.LogDebug("Circuit breaker '{Name}' failure count reset", _name);
            }
        }
    }

    private void OnFailure(Exception exception)
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            _logger?.LogWarning(exception,
                "Circuit breaker '{Name}' failure #{FailureCount}", _name, _failureCount);

            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitBreakerState.Open;
                _logger?.LogError("Circuit breaker '{Name}' opened after {FailureCount} failures",
                    _name, _failureCount);
            }
            else if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Open;
                _logger?.LogError("Circuit breaker '{Name}' reopened after failure in Half-Open state", _name);
            }
        }
    }

    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    public CircuitBreakerMetrics GetMetrics()
    {
        lock (_lock)
        {
            return new CircuitBreakerMetrics
            {
                Name = _name,
                State = _state,
                FailureCount = _failureCount,
                LastFailureTime = _lastFailureTime,
                FailureThreshold = _failureThreshold
            };
        }
    }
}

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
    public CircuitBreakerOpenException(string message, Exception innerException) : base(message, innerException) { }
}

public class CircuitBreakerMetrics
{
    public string Name { get; set; }
    public CircuitBreakerState State { get; set; }
    public int FailureCount { get; set; }
    public DateTime LastFailureTime { get; set; }
    public int FailureThreshold { get; set; }
}
```

## Retry Pattern with Exponential Backoff

**Implementación del patrón Retry con backoff exponencial y jitter para operaciones fallidas.**
Esta implementación incluye diferentes estrategias de backoff y manejo inteligente de excepciones.
Ideal para manejar fallos transitorios en servicios externos y operaciones de red.

```csharp
public class RetryPolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    private readonly double _backoffMultiplier;
    private readonly bool _useJitter;
    private readonly Func<Exception, bool> _retryCondition;
    private readonly ILogger<RetryPolicy> _logger;
    private readonly Random _random = new Random();

    public RetryPolicy(
        int maxAttempts = 3,
        TimeSpan baseDelay = default,
        TimeSpan maxDelay = default,
        double backoffMultiplier = 2.0,
        bool useJitter = true,
        Func<Exception, bool> retryCondition = null,
        ILogger<RetryPolicy> logger = null)
    {
        _maxAttempts = maxAttempts;
        _baseDelay = baseDelay == default ? TimeSpan.FromSeconds(1) : baseDelay;
        _maxDelay = maxDelay == default ? TimeSpan.FromMinutes(1) : maxDelay;
        _backoffMultiplier = backoffMultiplier;
        _useJitter = useJitter;
        _retryCondition = retryCondition ?? DefaultRetryCondition;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string operationName = null,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        Exception lastException = null;

        while (attempt < _maxAttempts)
        {
            try
            {
                attempt++;

                if (attempt > 1)
                {
                    _logger?.LogDebug("Retry attempt {Attempt}/{MaxAttempts} for operation '{OperationName}'",
                        attempt, _maxAttempts, operationName ?? "Unknown");
                }

                var result = await operation().ConfigureAwait(false);

                if (attempt > 1)
                {
                    _logger?.LogInformation("Operation '{OperationName}' succeeded on attempt {Attempt}",
                        operationName ?? "Unknown", attempt);
                }

                return result;
            }
            catch (Exception ex) when (attempt < _maxAttempts && _retryCondition(ex))
            {
                lastException = ex;
                var delay = CalculateDelay(attempt);

                _logger?.LogWarning(ex,
                    "Operation '{OperationName}' failed on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms",
                    operationName ?? "Unknown", attempt, _maxAttempts, delay.TotalMilliseconds);

                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogInformation("Retry cancelled for operation '{OperationName}'",
                        operationName ?? "Unknown");
                    throw;
                }
            }
        }

        _logger?.LogError(lastException,
            "Operation '{OperationName}' failed after {MaxAttempts} attempts",
            operationName ?? "Unknown", _maxAttempts);

        throw new RetryExhaustedException(
            $"Operation failed after {_maxAttempts} attempts", lastException);
    }

    public async Task ExecuteAsync(
        Func<Task> operation,
        string operationName = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation().ConfigureAwait(false);
            return true;
        }, operationName, cancellationToken).ConfigureAwait(false);
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        // Exponential backoff: baseDelay * (backoffMultiplier ^ (attempt - 1))
        var exponentialDelay = TimeSpan.FromMilliseconds(
            _baseDelay.TotalMilliseconds * Math.Pow(_backoffMultiplier, attempt - 1));

        // Cap at max delay
        var delay = exponentialDelay > _maxDelay ? _maxDelay : exponentialDelay;

        if (_useJitter)
        {
            // Add jitter: ±25% random variation
            var jitterMultiplier = 0.75 + (_random.NextDouble() * 0.5); // 0.75 to 1.25
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitterMultiplier);
        }

        return delay;
    }

    private static bool DefaultRetryCondition(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => true,
            TaskCanceledException => true,
            SocketException => true,
            TimeoutException => true,
            SqlException sqlEx => sqlEx.IsTransient,
            _ => false
        };
    }

    public static RetryPolicy CreateForHttp()
    {
        return new RetryPolicy(
            maxAttempts: 3,
            baseDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromSeconds(30),
            retryCondition: ex => ex is HttpRequestException or TaskCanceledException);
    }

    public static RetryPolicy CreateForDatabase()
    {
        return new RetryPolicy(
            maxAttempts: 5,
            baseDelay: TimeSpan.FromMilliseconds(500),
            maxDelay: TimeSpan.FromSeconds(10),
            retryCondition: ex => ex is SqlException sqlEx && sqlEx.IsTransient);
    }
}

public class RetryExhaustedException : Exception
{
    public RetryExhaustedException(string message) : base(message) { }
    public RetryExhaustedException(string message, Exception innerException) : base(message, innerException) { }
}

// Extension method for SqlException
public static class SqlExceptionExtensions
{
    private static readonly HashSet<int> TransientErrorNumbers = new HashSet<int>
    {
        -2,    // Timeout
        20,    // Connection timeout
        64,    // Connection lost
        233,   // Connection broken
        10928, // Resource limit
        10929, // Resource limit
        40197, // Service busy
        40501, // Service busy
        40613  // Database unavailable
    };

    public static bool IsTransient(this SqlException ex)
    {
        return TransientErrorNumbers.Contains(ex.Number);
    }
}
```

## Bulkhead Pattern

**Implementación del patrón Bulkhead para aislar recursos críticos y evitar fallos en cascada.**
Este patrón utiliza pools de recursos separados para diferentes tipos de operaciones.
Fundamental para sistemas que necesitan garantizar que fallos en una funcionalidad no afecten otras.

```csharp
public class BulkheadResourcePool<T> : IDisposable where T : class
{
    private readonly string _name;
    private readonly Func<T> _resourceFactory;
    private readonly Action<T> _resourceDisposer;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<T> _resources;
    private readonly ILogger<BulkheadResourcePool<T>> _logger;
    private readonly int _maxSize;
    private volatile bool _disposed;

    public BulkheadResourcePool(
        string name,
        int maxSize,
        Func<T> resourceFactory,
        Action<T> resourceDisposer = null,
        ILogger<BulkheadResourcePool<T>> logger = null)
    {
        _name = name;
        _maxSize = maxSize;
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        _resourceDisposer = resourceDisposer;
        _logger = logger;

        _semaphore = new SemaphoreSlim(maxSize, maxSize);
        _resources = new ConcurrentQueue<T>();

        _logger?.LogInformation("Bulkhead pool '{Name}' created with max size {MaxSize}", name, maxSize);
    }

    public async Task<BulkheadResource<T>> AcquireAsync(
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BulkheadResourcePool<T>));

        var actualTimeout = timeout == default ? TimeSpan.FromSeconds(30) : timeout;

        using var timeoutCts = new CancellationTokenSource(actualTimeout);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        try
        {
            await _semaphore.WaitAsync(combinedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            _logger?.LogWarning("Bulkhead pool '{Name}' acquisition timed out after {Timeout}",
                _name, actualTimeout);
            throw new BulkheadTimeoutException($"Failed to acquire resource from bulkhead '{_name}' within {actualTimeout}");
        }

        T resource = null;
        try
        {
            // Try to get existing resource
            if (!_resources.TryDequeue(out resource))
            {
                // Create new resource
                resource = _resourceFactory();
                _logger?.LogDebug("Created new resource for bulkhead pool '{Name}'", _name);
            }
            else
            {
                _logger?.LogDebug("Reused existing resource from bulkhead pool '{Name}'", _name);
            }

            return new BulkheadResource<T>(resource, this);
        }
        catch
        {
            // Release semaphore if resource creation failed
            _semaphore.Release();
            _resourceDisposer?.Invoke(resource);
            throw;
        }
    }

    internal void Release(T resource)
    {
        if (_disposed)
        {
            _resourceDisposer?.Invoke(resource);
            return;
        }

        try
        {
            // Return resource to pool for reuse
            _resources.Enqueue(resource);
            _logger?.LogDebug("Resource returned to bulkhead pool '{Name}'", _name);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public BulkheadMetrics GetMetrics()
    {
        return new BulkheadMetrics
        {
            Name = _name,
            MaxSize = _maxSize,
            AvailableSlots = _semaphore.CurrentCount,
            QueuedResources = _resources.Count,
            ActiveResources = _maxSize - _semaphore.CurrentCount - _resources.Count
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Dispose all queued resources
        while (_resources.TryDequeue(out var resource))
        {
            try
            {
                _resourceDisposer?.Invoke(resource);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing resource in bulkhead pool '{Name}'", _name);
            }
        }

        _semaphore?.Dispose();
        _logger?.LogInformation("Bulkhead pool '{Name}' disposed", _name);
    }
}

public class BulkheadResource<T> : IDisposable where T : class
{
    private readonly BulkheadResourcePool<T> _pool;
    private readonly T _resource;
    private bool _disposed;

    internal BulkheadResource(T resource, BulkheadResourcePool<T> pool)
    {
        _resource = resource;
        _pool = pool;
    }

    public T Resource
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BulkheadResource<T>));
            return _resource;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _pool.Release(_resource);
    }
}

public class BulkheadTimeoutException : Exception
{
    public BulkheadTimeoutException(string message) : base(message) { }
    public BulkheadTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}

public class BulkheadMetrics
{
    public string Name { get; set; }
    public int MaxSize { get; set; }
    public int AvailableSlots { get; set; }
    public int QueuedResources { get; set; }
    public int ActiveResources { get; set; }
}

// Service implementation using bulkhead
public class ResilientHttpService
{
    private readonly BulkheadResourcePool<HttpClient> _httpPool;
    private readonly BulkheadResourcePool<SqlConnection> _dbPool;
    private readonly CircuitBreaker _externalApiCircuitBreaker;
    private readonly RetryPolicy _retryPolicy;
    private readonly ILogger<ResilientHttpService> _logger;

    public ResilientHttpService(ILogger<ResilientHttpService> logger)
    {
        _logger = logger;

        // HTTP client bulkhead
        _httpPool = new BulkheadResourcePool<HttpClient>(
            "ExternalApi",
            maxSize: 10,
            resourceFactory: () => new HttpClient { Timeout = TimeSpan.FromSeconds(30) },
            resourceDisposer: client => client.Dispose());

        // Database connection bulkhead
        _dbPool = new BulkheadResourcePool<SqlConnection>(
            "Database",
            maxSize: 20,
            resourceFactory: () => new SqlConnection("connection-string"),
            resourceDisposer: conn => conn.Dispose());

        // Circuit breaker for external API
        _externalApiCircuitBreaker = new CircuitBreaker(
            "ExternalApi",
            failureThreshold: 5,
            timeout: TimeSpan.FromSeconds(30),
            retryTimeout: TimeSpan.FromMinutes(1));

        // Retry policy
        _retryPolicy = RetryPolicy.CreateForHttp();
    }

    public async Task<string> CallExternalApiAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using var httpResource = await _httpPool.AcquireAsync(cancellationToken: cancellationToken);

        return await _externalApiCircuitBreaker.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpResource.Resource.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }, $"GET {endpoint}", cancellationToken);
        }, cancellationToken);
    }
}
```

## Rate Limiting Pattern

**Implementación de patrones de rate limiting para proteger servicios de sobrecarga.**
Esta sección incluye algoritmos Token Bucket, Sliding Window y Fixed Window.
Fundamental para controlar la carga del sistema y prevenir ataques de denegación de servicio.

```csharp
public interface IRateLimiter
{
    Task<bool> IsAllowedAsync(string key, CancellationToken cancellationToken = default);
    Task<RateLimitResult> CheckAsync(string key, CancellationToken cancellationToken = default);
}

public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly int _capacity;
    private readonly int _tokensPerSecond;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenBucketRateLimiter> _logger;

    public TokenBucketRateLimiter(
        int capacity,
        int tokensPerSecond,
        IMemoryCache cache,
        ILogger<TokenBucketRateLimiter> logger = null)
    {
        _capacity = capacity;
        _tokensPerSecond = tokensPerSecond;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string key, CancellationToken cancellationToken = default)
    {
        var result = await CheckAsync(key, cancellationToken);
        return result.IsAllowed;
    }

    public Task<RateLimitResult> CheckAsync(string key, CancellationToken cancellationToken = default)
    {
        var bucketKey = $"token_bucket:{key}";
        var now = DateTimeOffset.UtcNow;

        var bucket = _cache.GetOrCreate(bucketKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return new TokenBucket
            {
                Tokens = _capacity,
                LastRefill = now
            };
        });

        lock (bucket)
        {
            // Refill tokens based on elapsed time
            var elapsed = now - bucket.LastRefill;
            var tokensToAdd = (int)(elapsed.TotalSeconds * _tokensPerSecond);

            if (tokensToAdd > 0)
            {
                bucket.Tokens = Math.Min(_capacity, bucket.Tokens + tokensToAdd);
                bucket.LastRefill = now;
            }

            if (bucket.Tokens > 0)
            {
                bucket.Tokens--;
                return Task.FromResult(new RateLimitResult
                {
                    IsAllowed = true,
                    RemainingTokens = bucket.Tokens,
                    RetryAfter = null
                });
            }
            else
            {
                // Calculate retry after
                var timeToNextToken = TimeSpan.FromSeconds(1.0 / _tokensPerSecond);

                _logger?.LogWarning("Rate limit exceeded for key '{Key}'", key);

                return Task.FromResult(new RateLimitResult
                {
                    IsAllowed = false,
                    RemainingTokens = 0,
                    RetryAfter = timeToNextToken
                });
            }
        }
    }

    private class TokenBucket
    {
        public int Tokens { get; set; }
        public DateTimeOffset LastRefill { get; set; }
    }
}

public class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SlidingWindowRateLimiter> _logger;

    public SlidingWindowRateLimiter(
        int maxRequests,
        TimeSpan window,
        IMemoryCache cache,
        ILogger<SlidingWindowRateLimiter> logger = null)
    {
        _maxRequests = maxRequests;
        _window = window;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string key, CancellationToken cancellationToken = default)
    {
        var result = await CheckAsync(key, cancellationToken);
        return result.IsAllowed;
    }

    public Task<RateLimitResult> CheckAsync(string key, CancellationToken cancellationToken = default)
    {
        var windowKey = $"sliding_window:{key}";
        var now = DateTimeOffset.UtcNow;
        var windowStart = now - _window;

        var requests = _cache.GetOrCreate(windowKey, entry =>
        {
            entry.SlidingExpiration = _window.Add(TimeSpan.FromMinutes(1));
            return new List<DateTimeOffset>();
        });

        lock (requests)
        {
            // Remove requests outside the window
            requests.RemoveAll(timestamp => timestamp < windowStart);

            if (requests.Count < _maxRequests)
            {
                requests.Add(now);

                return Task.FromResult(new RateLimitResult
                {
                    IsAllowed = true,
                    RemainingTokens = _maxRequests - requests.Count,
                    RetryAfter = null
                });
            }
            else
            {
                // Calculate when the oldest request will expire
                var oldestRequest = requests.Min();
                var retryAfter = oldestRequest.Add(_window) - now;

                _logger?.LogWarning("Rate limit exceeded for key '{Key}'. {RequestCount}/{MaxRequests} requests in window",
                    key, requests.Count, _maxRequests);

                return Task.FromResult(new RateLimitResult
                {
                    IsAllowed = false,
                    RemainingTokens = 0,
                    RetryAfter = retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.FromSeconds(1)
                });
            }
        }
    }
}

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RemainingTokens { get; set; }
    public TimeSpan? RetryAfter { get; set; }
}

// Middleware for ASP.NET Core
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimiter rateLimiter,
        RateLimitOptions options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimiter = rateLimiter;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = _options.KeyGenerator(context);
        var result = await _rateLimiter.CheckAsync(key);

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.RemainingTokens.ToString();

        if (!result.IsAllowed)
        {
            if (result.RetryAfter.HasValue)
            {
                context.Response.Headers["Retry-After"] = ((int)result.RetryAfter.Value.TotalSeconds).ToString();
            }

            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        await _next(context);
    }
}

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public Func<HttpContext, string> KeyGenerator { get; set; } = context =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
```

## Fallback Pattern

**Implementación del patrón Fallback para proporcionar respuestas alternativas cuando el servicio principal falla.**
Este patrón permite degradar gracefully la funcionalidad manteniendo operatividad básica.
Fundamental para mantener la experiencia del usuario incluso cuando servicios críticos no están disponibles.

```csharp
public class FallbackService<T>
{
    private readonly List<Func<CancellationToken, Task<T>>> _strategies;
    private readonly ILogger<FallbackService<T>> _logger;
    private readonly string _serviceName;

    public FallbackService(string serviceName, ILogger<FallbackService<T>> logger = null)
    {
        _serviceName = serviceName;
        _logger = logger;
        _strategies = new List<Func<CancellationToken, Task<T>>>();
    }

    public FallbackService<T> Primary(Func<CancellationToken, Task<T>> primaryStrategy)
    {
        _strategies.Insert(0, primaryStrategy);
        return this;
    }

    public FallbackService<T> Fallback(Func<CancellationToken, Task<T>> fallbackStrategy)
    {
        _strategies.Add(fallbackStrategy);
        return this;
    }

    public async Task<T> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_strategies.Any())
        {
            throw new InvalidOperationException("No strategies configured for fallback service");
        }

        Exception lastException = null;

        for (int i = 0; i < _strategies.Count; i++)
        {
            var strategy = _strategies[i];
            var strategyName = i == 0 ? "Primary" : $"Fallback-{i}";

            try
            {
                _logger?.LogDebug("Executing {StrategyName} strategy for service '{ServiceName}'",
                    strategyName, _serviceName);

                var result = await strategy(cancellationToken);

                if (i > 0)
                {
                    _logger?.LogInformation("Service '{ServiceName}' succeeded with {StrategyName} strategy",
                        _serviceName, strategyName);
                }

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;

                _logger?.LogWarning(ex, "Strategy {StrategyName} failed for service '{ServiceName}'",
                    strategyName, _serviceName);

                if (i == _strategies.Count - 1)
                {
                    _logger?.LogError("All fallback strategies exhausted for service '{ServiceName}'", _serviceName);
                }
            }
        }

        throw new FallbackExhaustedException(
            $"All fallback strategies failed for service '{_serviceName}'", lastException);
    }
}

public class FallbackExhaustedException : Exception
{
    public FallbackExhaustedException(string message) : base(message) { }
    public FallbackExhaustedException(string message, Exception innerException) : base(message, innerException) { }
}

// Usage example
public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public async Task<Product> GetProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var fallbackService = new FallbackService<Product>("GetProduct", _logger)
            .Primary(async ct =>
            {
                // Primary: External API
                var response = await _httpClient.GetAsync($"/api/products/{productId}", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Product>(json);
            })
            .Fallback(async ct =>
            {
                // Fallback 1: Cache
                var cacheKey = $"product:{productId}";
                if (_cache.TryGetValue(cacheKey, out Product cachedProduct))
                {
                    return cachedProduct;
                }
                throw new InvalidOperationException("Product not found in cache");
            })
            .Fallback(async ct =>
            {
                // Fallback 2: Local database
                return await _repository.GetByIdAsync(productId, ct);
            })
            .Fallback(async ct =>
            {
                // Fallback 3: Default/empty product
                return new Product
                {
                    Id = productId,
                    Name = "Product temporarily unavailable",
                    Description = "We're experiencing technical difficulties. Please try again later.",
                    Price = 0,
                    IsAvailable = false
                };
            });

        return await fallbackService.ExecuteAsync(cancellationToken);
    }
}
```

## Resilience Metrics and Monitoring

**Métricas y monitoreo para patrones de resiliencia en aplicaciones .NET.**
Esta tabla identifica KPIs críticos para evaluar la efectividad de patrones de resiliencia.
Esencial para optimizar umbrales y detectar problemas de resiliencia antes de que afecten usuarios.

| **Patrón**          | **Métricas Clave**                       | **Umbrales Saludables**  | **Alertas Críticas**              |
| ------------------- | ---------------------------------------- | ------------------------ | --------------------------------- |
| **Circuit Breaker** | Estado, tasa de fallos, tiempo abierto   | < 5% tiempo abierto      | Estado Open > 5 min               |
| **Retry**           | Intentos promedio, tasa éxito tras retry | < 2 intentos promedio    | > 50% operaciones requieren retry |
| **Rate Limiting**   | Requests bloqueados, latencia P99        | < 1% requests bloqueados | > 5% requests bloqueados          |
| **Bulkhead**        | Utilización pools, timeout rate          | < 80% utilización        | Pool exhausted > 1 min            |
| **Fallback**        | Activaciones fallback, latencia degraded | < 10% usan fallback      | > 25% operaciones usan fallback   |
| **Timeout**         | Operaciones timeout, latencia P95        | < 1% timeout             | > 5% operaciones timeout          |

### Implementation Example

```csharp
public class ResilienceMetricsCollector
{
    private readonly IMetricsLogger _metrics;
    private readonly ILogger<ResilienceMetricsCollector> _logger;

    public void RecordCircuitBreakerState(string name, CircuitBreakerState state)
    {
        _metrics.Gauge("circuit_breaker.state")
            .WithTag("name", name)
            .WithTag("state", state.ToString())
            .Set(state == CircuitBreakerState.Open ? 1 : 0);
    }

    public void RecordRetryAttempt(string operation, int attempt, bool succeeded)
    {
        _metrics.Counter("retry.attempts")
            .WithTag("operation", operation)
            .WithTag("attempt", attempt.ToString())
            .WithTag("succeeded", succeeded.ToString())
            .Increment();
    }

    public void RecordRateLimitResult(string key, bool allowed, int remaining)
    {
        _metrics.Counter("rate_limit.requests")
            .WithTag("key", key)
            .WithTag("allowed", allowed.ToString())
            .Increment();

        _metrics.Gauge("rate_limit.remaining")
            .WithTag("key", key)
            .Set(remaining);
    }

    public void RecordBulkheadUtilization(string poolName, BulkheadMetrics metrics)
    {
        var utilizationPercent = (double)metrics.ActiveResources / metrics.MaxSize * 100;

        _metrics.Gauge("bulkhead.utilization")
            .WithTag("pool", poolName)
            .Set(utilizationPercent);

        _metrics.Gauge("bulkhead.available_slots")
            .WithTag("pool", poolName)
            .Set(metrics.AvailableSlots);
    }

    public void RecordFallbackActivation(string service, string strategy, bool succeeded)
    {
        _metrics.Counter("fallback.activations")
            .WithTag("service", service)
            .WithTag("strategy", strategy)
            .WithTag("succeeded", succeeded.ToString())
            .Increment();
    }
}
```
