# Contexto y Propósito

## ¿Qué es?
La concurrencia en .NET es la capacidad de ejecutar múltiples tareas de forma paralela o asíncrona para aprovechar recursos multi-core y evitar bloqueos en operaciones de I/O. Se implementa con Threads, Tasks, async/await, Parallel, ThreadPool, Channels y colecciones thread-safe.

## ¿Por qué?
Porque el rendimiento de aplicaciones modernas depende de manejar miles de requests concurrentes o procesar grandes volúmenes de datos en paralelo. En mi experiencia, elegir mal entre concurrencia y paralelismo llevó a bloqueos y sobrecarga, mientras que aplicar patrones correctos habilitó arquitecturas resilientes y de alto throughput.

## ¿Para qué?
- **Escalar APIs y microservicios** sin bloquear threads en operaciones de red o BD.  
- **Procesar datos intensivos en CPU** con paralelismo de datos (Parallel.ForEach, PLINQ).  
- **Coordinar flujos producer-consumer** con Channels y BlockingCollection.  
- **Proteger recursos compartidos** con primitivas de sincronización como SemaphoreSlim o ReaderWriterLock.  

## Valor agregado desde la experiencia
- Aplicar **async/await con CancellationToken** evitó fugas de recursos en integraciones externas.  
- Usar **Parallel.ForEach** procesó grandes volúmenes de datos municipales en segundos en vez de minutos.  
- Implementar **BlockingCollection** simplificó pipelines producer-consumer en servicios de logística.  
- **Channels con backpressure** permitieron manejar streams IoT sin pérdida de datos ni sobrecarga en memoria.  

# Concurrency in .NET

**Guía completa de programación concurrente en .NET con Task, async/await, Parallel y collections thread-safe.**
Este documento cubre desde conceptos básicos hasta patrones avanzados como Producer-Consumer y Actor Model.
Fundamental para construir aplicaciones de alto rendimiento que aprovechen eficientemente recursos multi-core.

## Concurrency Fundamentals

**Conceptos fundamentales de concurrencia en .NET con sus características y casos de uso.**
Esta tabla compara diferentes enfoques de concurrencia con sus ventajas y limitaciones.
Esencial para elegir la estrategia correcta según el tipo de operación y requisitos de rendimiento.

| **Concepto**    | **Descripción**                | **Cuándo Usar**                       | **Ventajas**               | **Limitaciones**         |
| --------------- | ------------------------------ | ------------------------------------- | -------------------------- | ------------------------ |
| **Thread**      | Hilo de ejecución del OS       | Control granular, blocking operations | Control total              | Overhead, complejidad    |
| **Task**        | Abstracción sobre threads      | Operaciones asíncronas                | Fácil manejo, composición  | Overhead mínimo          |
| **async/await** | Programación asíncrona         | I/O bound operations                  | Escalabilidad, legibilidad | No mejora CPU-bound      |
| **Parallel**    | Paralelismo de datos           | CPU-bound operations                  | Uso multi-core             | Overhead de particionado |
| **ThreadPool**  | Pool de threads reutilizables  | Muchas tareas cortas                  | Eficiencia de threads      | Limitado por pool size   |
| **Channels**    | Comunicación producer-consumer | Pipelines de datos                    | Type-safe, backpressure    | Complejidad adicional    |

## Async/Await Best Practices

**Mejores prácticas para programación asíncrona en .NET con ejemplos de implementación correcta.**
Esta sección demuestra patrones comunes y errores a evitar en código asíncrono.
Fundamental para escribir código asíncrono eficiente y libre de deadlocks.

```csharp
public class AsyncBestPractices
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsyncBestPractices> _logger;

    public AsyncBestPractices(HttpClient httpClient, ILogger<AsyncBestPractices> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // ✅ CORRECTO: ConfigureAwait(false) en bibliotecas
    public async Task<string> GetDataAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch data from {Url}", url);
            throw;
        }
    }

    // ✅ CORRECTO: Paralelismo con Task.WhenAll
    public async Task<List<ProductData>> GetMultipleProductsAsync(IEnumerable<int> productIds)
    {
        var tasks = productIds.Select(async id =>
        {
            try
            {
                var data = await GetDataAsync($"/api/products/{id}").ConfigureAwait(false);
                return JsonSerializer.Deserialize<ProductData>(data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get product {ProductId}", id);
                return null;
            }
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Where(r => r != null).ToList();
    }

    // ✅ CORRECTO: Timeout con CancellationToken
    public async Task<string> GetDataWithTimeoutAsync(string url, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Request to {url} timed out after {timeout}");
        }
    }

    // ✅ CORRECTO: Async enumerable para streaming
    public async IAsyncEnumerable<T> ProcessDataStreamAsync<T>(
        IAsyncEnumerable<string> dataStream,
        Func<string, T> processor,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in dataStream.WithCancellation(cancellationToken))
        {
            T processed;
            try
            {
                processed = processor(item);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process item: {Item}", item);
                continue;
            }

            yield return processed;
        }
    }

    // ✅ CORRECTO: Lazy async initialization
    private readonly Lazy<Task<ExpensiveResource>> _expensiveResource = new(InitializeResourceAsync);

    public Task<ExpensiveResource> GetResourceAsync() => _expensiveResource.Value;

    private static async Task<ExpensiveResource> InitializeResourceAsync()
    {
        // Simulate expensive initialization
        await Task.Delay(2000).ConfigureAwait(false);
        return new ExpensiveResource();
    }

    // ✅ CORRECTO: Exception handling en async void (solo para event handlers)
    public async void OnEventHandlerAsync(object sender, EventArgs e)
    {
        try
        {
            await ProcessEventAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in event handler");
            // En aplicaciones reales, considera usar un global exception handler
        }
    }

    private async Task ProcessEventAsync(EventArgs e)
    {
        // Event processing logic
        await Task.Delay(100).ConfigureAwait(false);
    }

    // ❌ INCORRECTO: Deadlock potential (commented for reference)
    /*
    public string GetDataSync(string url)
    {
        // ❌ NUNCA hagas esto - puede causar deadlock
        return GetDataAsync(url).Result;
    }
    */

    // ✅ CORRECTO: Sync over async cuando es necesario
    public string GetDataSyncSafe(string url)
    {
        return Task.Run(async () => await GetDataAsync(url)).GetAwaiter().GetResult();
    }
}

public class ProductData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ExpensiveResource
{
    public string ConnectionString { get; set; } = "expensive-connection";
}
```

## Parallel Programming Patterns

**Patrones de programación paralela para operaciones CPU-intensive en .NET.**
Esta sección demustra PLINQ, Parallel.ForEach y particionado de datos para máximo rendimiento.
Ideal para procesar grandes volúmenes de datos aprovechando todos los cores disponibles.

```csharp
public class ParallelProcessingService
{
    private readonly ILogger<ParallelProcessingService> _logger;
    private readonly ParallelOptions _defaultParallelOptions;

    public ParallelProcessingService(ILogger<ParallelProcessingService> logger)
    {
        _logger = logger;
        _defaultParallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            TaskScheduler = TaskScheduler.Default
        };
    }

    // Parallel processing with custom partitioner
    public async Task<List<ProcessedItem>> ProcessLargeDatasetAsync<T>(
        IEnumerable<T> data,
        Func<T, ProcessedItem> processor,
        CancellationToken cancellationToken = default)
    {
        var items = data.ToList();
        var results = new ConcurrentBag<ProcessedItem>();
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        // Use Partitioner for better load balancing
        var partitioner = Partitioner.Create(items, loadBalance: true);

        try
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(partitioner, parallelOptions, item =>
                {
                    try
                    {
                        var processed = processor(item);
                        results.Add(processed);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process item: {Item}", item);
                    }
                });
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Parallel processing was cancelled");
            throw;
        }

        return results.ToList();
    }

    // PLINQ example with aggregation
    public Task<ProcessingStatistics> AnalyzeDataAsync(
        IEnumerable<DataPoint> dataPoints,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var statistics = dataPoints
                .AsParallel()
                .WithCancellation(cancellationToken)
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Where(dp => dp.IsValid)
                .Select(dp => new
                {
                    Value = dp.Value,
                    Square = dp.Value * dp.Value,
                    IsPositive = dp.Value > 0
                })
                .Aggregate(
                    new ProcessingStatistics(),
                    (acc, item) =>
                    {
                        acc.Count++;
                        acc.Sum += item.Value;
                        acc.SumOfSquares += item.Square;
                        if (item.IsPositive) acc.PositiveCount++;
                        return acc;
                    },
                    (acc1, acc2) => new ProcessingStatistics
                    {
                        Count = acc1.Count + acc2.Count,
                        Sum = acc1.Sum + acc2.Sum,
                        SumOfSquares = acc1.SumOfSquares + acc2.SumOfSquares,
                        PositiveCount = acc1.PositiveCount + acc2.PositiveCount
                    },
                    acc =>
                    {
                        acc.Average = acc.Count > 0 ? acc.Sum / acc.Count : 0;
                        acc.Variance = acc.Count > 1
                            ? (acc.SumOfSquares - acc.Sum * acc.Sum / acc.Count) / (acc.Count - 1)
                            : 0;
                        return acc;
                    });

            _logger.LogInformation("Processed {Count} data points with {PositiveCount} positive values",
                statistics.Count, statistics.PositiveCount);

            return statistics;
        }, cancellationToken);
    }

    // CPU-intensive work with progress reporting
    public async Task<ProcessingResult> ProcessWithProgressAsync<T>(
        IList<T> items,
        Func<T, ProcessedItem> processor,
        IProgress<ProcessingProgress> progress = null,
        CancellationToken cancellationToken = default)
    {
        var totalItems = items.Count;
        var processedCount = 0;
        var results = new ConcurrentBag<ProcessedItem>();
        var errors = new ConcurrentBag<ProcessingError>();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, totalItems)
        };

        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            Parallel.For(0, totalItems, parallelOptions, i =>
            {
                try
                {
                    var item = items[i];
                    var processed = processor(item);
                    results.Add(processed);

                    var completed = Interlocked.Increment(ref processedCount);

                    // Report progress every 100 items or on completion
                    if (completed % 100 == 0 || completed == totalItems)
                    {
                        progress?.Report(new ProcessingProgress
                        {
                            Completed = completed,
                            Total = totalItems,
                            PercentComplete = (double)completed / totalItems * 100,
                            ItemsPerSecond = completed / sw.Elapsed.TotalSeconds
                        });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new ProcessingError
                    {
                        Index = i,
                        Item = items[i],
                        Exception = ex
                    });

                    _logger.LogWarning(ex, "Failed to process item at index {Index}", i);
                }
            });
        }, cancellationToken);

        sw.Stop();

        return new ProcessingResult
        {
            ProcessedItems = results.ToList(),
            Errors = errors.ToList(),
            TotalProcessed = processedCount,
            ElapsedTime = sw.Elapsed,
            ItemsPerSecond = processedCount / sw.Elapsed.TotalSeconds
        };
    }

    // Memory-efficient batch processing
    public async IAsyncEnumerable<ProcessedBatch<T>> ProcessInBatchesAsync<T>(
        IAsyncEnumerable<T> dataStream,
        Func<T, ProcessedItem> processor,
        int batchSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<T>(batchSize);

        await foreach (var item in dataStream.WithCancellation(cancellationToken))
        {
            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                var processedBatch = await ProcessBatchAsync(batch, processor, cancellationToken);
                yield return processedBatch;

                batch.Clear();
            }
        }

        // Process remaining items
        if (batch.Count > 0)
        {
            var processedBatch = await ProcessBatchAsync(batch, processor, cancellationToken);
            yield return processedBatch;
        }
    }

    private async Task<ProcessedBatch<T>> ProcessBatchAsync<T>(
        IList<T> batch,
        Func<T, ProcessedItem> processor,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var results = new ConcurrentBag<ProcessedItem>();

        await Task.Run(() =>
        {
            Parallel.ForEach(batch, _defaultParallelOptions, item =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var processed = processor(item);
                    results.Add(processed);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process batch item: {Item}", item);
                }
            });
        }, cancellationToken);

        sw.Stop();

        return new ProcessedBatch<T>
        {
            OriginalItems = batch,
            ProcessedItems = results.ToList(),
            ProcessingTime = sw.Elapsed
        };
    }
}

public class DataPoint
{
    public double Value { get; set; }
    public bool IsValid { get; set; }
}

public class ProcessedItem
{
    public string Id { get; set; }
    public string Result { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

public class ProcessingStatistics
{
    public int Count { get; set; }
    public double Sum { get; set; }
    public double SumOfSquares { get; set; }
    public int PositiveCount { get; set; }
    public double Average { get; set; }
    public double Variance { get; set; }
}

public class ProcessingProgress
{
    public int Completed { get; set; }
    public int Total { get; set; }
    public double PercentComplete { get; set; }
    public double ItemsPerSecond { get; set; }
}

public class ProcessingError
{
    public int Index { get; set; }
    public object Item { get; set; }
    public Exception Exception { get; set; }
}

public class ProcessingResult
{
    public List<ProcessedItem> ProcessedItems { get; set; }
    public List<ProcessingError> Errors { get; set; }
    public int TotalProcessed { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public double ItemsPerSecond { get; set; }
}

public class ProcessedBatch<T>
{
    public IList<T> OriginalItems { get; set; }
    public List<ProcessedItem> ProcessedItems { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}
```

## Thread-Safe Collections

**Uso avanzado de colecciones thread-safe en .NET para programación concurrente.**
Esta sección demuestra ConcurrentDictionary, ConcurrentQueue, BlockingCollection y sus patrones de uso.
Fundamental para compartir datos entre threads de manera segura y eficiente.

```csharp
public class ThreadSafeCollectionsDemo
{
    private readonly ILogger<ThreadSafeCollectionsDemo> _logger;

    // Thread-safe cache implementation using ConcurrentDictionary
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly Timer _cleanupTimer;

    public ThreadSafeCollectionsDemo(ILogger<ThreadSafeCollectionsDemo> logger)
    {
        _logger = logger;

        // Setup cache cleanup timer
        _cleanupTimer = new Timer(CleanupExpiredEntries, null,
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    // ConcurrentDictionary advanced patterns
    public async Task<T> GetOrAddAsync<T>(string key, Func<string, Task<T>> factory, TimeSpan? expiration = null)
    {
        var expiresAt = expiration.HasValue
            ? DateTime.UtcNow.Add(expiration.Value)
            : DateTime.UtcNow.AddHours(1);

        // Use GetOrAdd with lazy evaluation
        var entry = _cache.GetOrAdd(key, _ => new CacheEntry
        {
            ValueTask = new Lazy<Task<object>>(async () =>
            {
                var value = await factory(key);
                return (object)value;
            }),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });

        // Check expiration
        if (DateTime.UtcNow > entry.ExpiresAt)
        {
            _cache.TryRemove(key, out _);
            return await GetOrAddAsync(key, factory, expiration);
        }

        try
        {
            var value = await entry.ValueTask.Value;
            return (T)value;
        }
        catch (Exception)
        {
            // Remove failed entry
            _cache.TryRemove(key, out _);
            throw;
        }
    }

    // Producer-Consumer pattern with BlockingCollection
    public async Task<List<ProcessedItem>> ProducerConsumerPatternAsync<T>(
        IEnumerable<T> sourceData,
        Func<T, ProcessedItem> processor,
        int maxConcurrency = 4,
        CancellationToken cancellationToken = default)
    {
        using var dataQueue = new BlockingCollection<T>(boundedCapacity: 100);
        using var resultsQueue = new BlockingCollection<ProcessedItem>();

        var results = new List<ProcessedItem>();

        // Producer task
        var producerTask = Task.Run(() =>
        {
            try
            {
                foreach (var item in sourceData)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    dataQueue.Add(item, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Producer cancelled");
            }
            finally
            {
                dataQueue.CompleteAdding();
            }
        }, cancellationToken);

        // Consumer tasks
        var consumerTasks = Enumerable.Range(0, maxConcurrency)
            .Select(i => Task.Run(async () =>
            {
                try
                {
                    foreach (var item in dataQueue.GetConsumingEnumerable(cancellationToken))
                    {
                        try
                        {
                            var processed = processor(item);
                            resultsQueue.Add(processed, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Consumer {ConsumerId} failed to process item", i);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer {ConsumerId} cancelled", i);
                }
            }, cancellationToken))
            .ToArray();

        // Results collector task
        var collectorTask = Task.Run(() =>
        {
            try
            {
                foreach (var result in resultsQueue.GetConsumingEnumerable(cancellationToken))
                {
                    results.Add(result);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Results collector cancelled");
            }
        }, cancellationToken);

        // Wait for producer and all consumers to complete
        await Task.WhenAll(new[] { producerTask }.Concat(consumerTasks));
        resultsQueue.CompleteAdding();

        // Wait for results collection to complete
        await collectorTask;

        return results;
    }

    // Advanced concurrent queue operations
    public class MessageProcessor
    {
        private readonly ConcurrentQueue<Message> _messageQueue = new();
        private readonly SemaphoreSlim _processingLock = new(1, 1);
        private volatile bool _isProcessing;

        public async Task EnqueueMessageAsync(Message message)
        {
            _messageQueue.Enqueue(message);

            // Trigger processing if not already running
            if (!_isProcessing)
            {
                _ = Task.Run(ProcessMessagesAsync);
            }
        }

        private async Task ProcessMessagesAsync()
        {
            await _processingLock.WaitAsync();

            try
            {
                _isProcessing = true;

                while (_messageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        await ProcessSingleMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing
                        Console.WriteLine($"Failed to process message: {ex.Message}");
                    }
                }
            }
            finally
            {
                _isProcessing = false;
                _processingLock.Release();
            }
        }

        private async Task ProcessSingleMessageAsync(Message message)
        {
            // Simulate processing
            await Task.Delay(100);
            Console.WriteLine($"Processed message: {message.Id}");
        }
    }

    // Thread-safe lazy initialization
    public class LazyResourceManager<T> where T : class
    {
        private readonly ConcurrentDictionary<string, Lazy<Task<T>>> _resources = new();
        private readonly Func<string, Task<T>> _factory;

        public LazyResourceManager(Func<string, Task<T>> factory)
        {
            _factory = factory;
        }

        public Task<T> GetResourceAsync(string key)
        {
            var lazyResource = _resources.GetOrAdd(key, k => new Lazy<Task<T>>(() => _factory(k)));
            return lazyResource.Value;
        }

        public void RemoveResource(string key)
        {
            _resources.TryRemove(key, out _);
        }

        public void ClearAll()
        {
            _resources.Clear();
        }

        public int Count => _resources.Count;
    }

    private void CleanupExpiredEntries(object state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _cache
            .Where(kvp => now > kvp.Value.ExpiresAt)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

public class CacheEntry
{
    public Lazy<Task<object>> ValueTask { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

## Channels and Pipelines

**Implementación de Channels para comunicación thread-safe y pipelines de procesamiento.**
Esta sección demuestra patrones producer-consumer avanzados con backpressure y control de flujo.
Ideal para arquitecturas de streaming y procesamiento de datos en tiempo real.

```csharp
public class ChannelBasedPipeline
{
    private readonly ILogger<ChannelBasedPipeline> _logger;

    public ChannelBasedPipeline(ILogger<ChannelBasedPipeline> logger)
    {
        _logger = logger;
    }

    // Basic producer-consumer with bounded channel
    public async Task<List<ProcessedData>> ProcessDataPipelineAsync<T>(
        IAsyncEnumerable<T> dataSource,
        Func<T, Task<ProcessedData>> processor,
        int maxConcurrency = 4,
        int bufferSize = 100,
        CancellationToken cancellationToken = default)
    {
        // Create bounded channel with backpressure
        var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(bufferSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        });

        var results = new ConcurrentBag<ProcessedData>();

        // Producer: Feed data into channel
        var producerTask = FeedDataAsync(dataSource, channel.Writer, cancellationToken);

        // Consumers: Process data from channel
        var consumerTasks = Enumerable.Range(0, maxConcurrency)
            .Select(i => ConsumeDataAsync(channel.Reader, processor, results, i, cancellationToken))
            .ToArray();

        // Wait for all tasks to complete
        await Task.WhenAll(new[] { producerTask }.Concat(consumerTasks));

        return results.ToList();
    }

    private async Task FeedDataAsync<T>(
        IAsyncEnumerable<T> dataSource,
        ChannelWriter<T> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var item in dataSource.WithCancellation(cancellationToken))
            {
                await writer.WriteAsync(item, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Producer failed");
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task ConsumeDataAsync<T>(
        ChannelReader<T> reader,
        Func<T, Task<ProcessedData>> processor,
        ConcurrentBag<ProcessedData> results,
        int consumerId,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    var processed = await processor(item);
                    results.Add(processed);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Consumer {ConsumerId} failed to process item", consumerId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Consumer {ConsumerId} failed", consumerId);
        }
    }

    // Multi-stage pipeline with different processing stages
    public async Task<List<FinalResult>> MultiStagePipelineAsync<TInput>(
        IAsyncEnumerable<TInput> input,
        CancellationToken cancellationToken = default)
    {
        var stage1Channel = Channel.CreateBounded<Stage1Data>(100);
        var stage2Channel = Channel.CreateBounded<Stage2Data>(100);
        var stage3Channel = Channel.CreateBounded<Stage3Data>(100);

        var finalResults = new ConcurrentBag<FinalResult>();

        // Stage 1: Input processing
        var stage1Task = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in input.WithCancellation(cancellationToken))
                {
                    var stage1Result = await ProcessStage1Async(item);
                    await stage1Channel.Writer.WriteAsync(stage1Result, cancellationToken);
                }
            }
            finally
            {
                stage1Channel.Writer.Complete();
            }
        }, cancellationToken);

        // Stage 2: Intermediate processing (parallel)
        var stage2Tasks = Enumerable.Range(0, 3).Select(_ => Task.Run(async () =>
        {
            try
            {
                await foreach (var item in stage1Channel.Reader.ReadAllAsync(cancellationToken))
                {
                    var stage2Result = await ProcessStage2Async(item);
                    await stage2Channel.Writer.WriteAsync(stage2Result, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stage 2 processing failed");
            }
        }, cancellationToken)).ToArray();

        // Wait for stage 2 completion and close channel
        var stage2Completion = Task.Run(async () =>
        {
            await Task.WhenAll(stage2Tasks);
            stage2Channel.Writer.Complete();
        });

        // Stage 3: Final processing
        var stage3Task = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in stage2Channel.Reader.ReadAllAsync(cancellationToken))
                {
                    var stage3Result = await ProcessStage3Async(item);
                    await stage3Channel.Writer.WriteAsync(stage3Result, cancellationToken);
                }
            }
            finally
            {
                stage3Channel.Writer.Complete();
            }
        }, cancellationToken);

        // Results collector
        var collectorTask = Task.Run(async () =>
        {
            await foreach (var item in stage3Channel.Reader.ReadAllAsync(cancellationToken))
            {
                var finalResult = await CreateFinalResultAsync(item);
                finalResults.Add(finalResult);
            }
        }, cancellationToken);

        // Wait for pipeline completion
        await Task.WhenAll(stage1Task, stage2Completion, stage3Task, collectorTask);

        return finalResults.ToList();
    }

    // Fan-out/Fan-in pattern
    public async Task<List<AggregatedResult>> FanOutFanInAsync<T>(
        IAsyncEnumerable<T> input,
        int fanOutDegree = 4,
        CancellationToken cancellationToken = default)
    {
        var inputChannel = Channel.CreateBounded<T>(100);
        var outputChannel = Channel.CreateBounded<ProcessedData>(200);

        // Feed input data
        var inputTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in input.WithCancellation(cancellationToken))
                {
                    await inputChannel.Writer.WriteAsync(item, cancellationToken);
                }
            }
            finally
            {
                inputChannel.Writer.Complete();
            }
        }, cancellationToken);

        // Fan-out: Multiple processors
        var processorTasks = Enumerable.Range(0, fanOutDegree)
            .Select(i => Task.Run(async () =>
            {
                await foreach (var item in inputChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        var processed = await ProcessItemAsync(item);
                        await outputChannel.Writer.WriteAsync(processed, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Processor {ProcessorId} failed", i);
                    }
                }
            }, cancellationToken))
            .ToArray();

        // Close output channel when all processors complete
        var outputCloseTask = Task.Run(async () =>
        {
            await Task.WhenAll(processorTasks);
            outputChannel.Writer.Complete();
        });

        // Fan-in: Aggregate results
        var results = new List<AggregatedResult>();
        var currentBatch = new List<ProcessedData>();

        await foreach (var processed in outputChannel.Reader.ReadAllAsync(cancellationToken))
        {
            currentBatch.Add(processed);

            // Batch processing for aggregation
            if (currentBatch.Count >= 10)
            {
                var aggregated = AggregateResults(currentBatch);
                results.Add(aggregated);
                currentBatch.Clear();
            }
        }

        // Process remaining items
        if (currentBatch.Count > 0)
        {
            var aggregated = AggregateResults(currentBatch);
            results.Add(aggregated);
        }

        await Task.WhenAll(inputTask, outputCloseTask);

        return results;
    }

    private async Task<Stage1Data> ProcessStage1Async<T>(T input)
    {
        await Task.Delay(10); // Simulate processing
        return new Stage1Data { ProcessedInput = input?.ToString() };
    }

    private async Task<Stage2Data> ProcessStage2Async(Stage1Data input)
    {
        await Task.Delay(20); // Simulate processing
        return new Stage2Data { TransformedData = input.ProcessedInput?.ToUpper() };
    }

    private async Task<Stage3Data> ProcessStage3Async(Stage2Data input)
    {
        await Task.Delay(15); // Simulate processing
        return new Stage3Data { FinalData = $"FINAL_{input.TransformedData}" };
    }

    private async Task<FinalResult> CreateFinalResultAsync(Stage3Data input)
    {
        await Task.Delay(5); // Simulate processing
        return new FinalResult { Result = input.FinalData, Timestamp = DateTime.UtcNow };
    }

    private async Task<ProcessedData> ProcessItemAsync<T>(T item)
    {
        await Task.Delay(50); // Simulate processing
        return new ProcessedData { Value = item?.ToString(), ProcessedAt = DateTime.UtcNow };
    }

    private AggregatedResult AggregateResults(List<ProcessedData> batch)
    {
        return new AggregatedResult
        {
            Count = batch.Count,
            AggregatedValue = string.Join(",", batch.Select(x => x.Value)),
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class ProcessedData
{
    public string Value { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class Stage1Data
{
    public string ProcessedInput { get; set; }
}

public class Stage2Data
{
    public string TransformedData { get; set; }
}

public class Stage3Data
{
    public string FinalData { get; set; }
}

public class FinalResult
{
    public string Result { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AggregatedResult
{
    public int Count { get; set; }
    public string AggregatedValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Synchronization Primitives

**Primitivas de sincronización en .NET para coordinar threads y controlar acceso a recursos.**
Esta tabla compara diferentes mecanismos de sincronización con sus características y casos de uso.
Fundamental para manejar condiciones de carrera y coordinar acceso concurrente a recursos compartidos.

| **Primitiva**        | **Propósito**                    | **Rendimiento** | **Uso Típico**                | **Limitaciones**        |
| -------------------- | -------------------------------- | --------------- | ----------------------------- | ----------------------- |
| **lock (Monitor)**   | Exclusión mutua                  | Alto            | Secciones críticas cortas     | Solo dentro del proceso |
| **Mutex**            | Exclusión mutua cross-process    | Medio           | Sincronización entre procesos | Mayor overhead          |
| **Semaphore**        | Limitar acceso concurrente       | Alto            | Pool de recursos              | No ownership            |
| **ReaderWriterLock** | Múltiples readers, single writer | Variable        | Acceso a datos compartidos    | Potencial starvation    |
| **ManualResetEvent** | Señalización entre threads       | Alto            | Coordinación de threads       | Manual reset requerido  |
| **AutoResetEvent**   | Señalización one-shot            | Alto            | Producer-consumer simple      | Un thread por señal     |
| **Barrier**          | Sincronización de fases          | Alto            | Algoritmos paralelos          | Número fijo de threads  |
| **CountdownEvent**   | Esperar múltiples operaciones    | Alto            | Fork-join patterns            | Manual decrement        |

### Advanced Synchronization Examples

```csharp
public class AdvancedSynchronization
{
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);
    private readonly Dictionary<string, object> _cache = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly Barrier _barrier;

    public AdvancedSynchronization(int maxConcurrentOperations = 10)
    {
        _semaphore = new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations);
        _barrier = new Barrier(Environment.ProcessorCount);
    }

    // Reader-Writer lock example
    public T GetOrAddToCache<T>(string key, Func<T> factory)
    {
        // Try read first
        _rwLock.EnterReadLock();
        try
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                return (T)cached;
            }
        }
        finally
        {
            _rwLock.ExitReadLock();
        }

        // Upgrade to write lock
        _rwLock.EnterWriteLock();
        try
        {
            // Double-check after acquiring write lock
            if (_cache.TryGetValue(key, out var cached))
            {
                return (T)cached;
            }

            var value = factory();
            _cache[key] = value;
            return value;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    // Semaphore-controlled resource access
    public async Task<T> ExecuteWithLimitAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Barrier synchronization for parallel phases
    public async Task<List<PhaseResult>> ExecuteParallelPhasesAsync(
        IList<WorkItem> workItems,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<PhaseResult>();
        var partitioner = Partitioner.Create(workItems, loadBalance: true);

        await Task.Run(() =>
        {
            Parallel.ForEach(partitioner, new ParallelOptions
            {
                CancellationToken = cancellationToken
            }, workItem =>
            {
                var phaseResult = new PhaseResult { WorkItemId = workItem.Id };

                // Phase 1: Initial processing
                phaseResult.Phase1Result = ProcessPhase1(workItem);
                _barrier.SignalAndWait(cancellationToken);

                // Phase 2: Processing that depends on Phase 1 completion
                phaseResult.Phase2Result = ProcessPhase2(workItem, phaseResult.Phase1Result);
                _barrier.SignalAndWait(cancellationToken);

                // Phase 3: Final processing
                phaseResult.Phase3Result = ProcessPhase3(workItem, phaseResult.Phase2Result);

                results.Add(phaseResult);
            });
        }, cancellationToken);

        return results.ToList();
    }

    private string ProcessPhase1(WorkItem item) => $"Phase1_{item.Id}";
    private string ProcessPhase2(WorkItem item, string phase1) => $"Phase2_{phase1}";
    private string ProcessPhase3(WorkItem item, string phase2) => $"Phase3_{phase2}";
}

public class WorkItem
{
    public int Id { get; set; }
    public string Data { get; set; }
}

public class PhaseResult
{
    public int WorkItemId { get; set; }
    public string Phase1Result { get; set; }
    public string Phase2Result { get; set; }
    public string Phase3Result { get; set; }
}
```
