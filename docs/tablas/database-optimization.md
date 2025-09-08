# Contexto y Propósito

## ¿Qué es?
La optimización de bases de datos consiste en aplicar estrategias de indexación, consultas, particionado y monitoreo para garantizar que los sistemas manejen grandes volúmenes de datos con la menor latencia posible. En .NET se apoya en Entity Framework (EF) Core, consultas SQL optimizadas y configuraciones de pooling para maximizar rendimiento.

## ¿Por qué?
Porque la mayoría de cuellos de botella en aplicaciones empresariales no están en el código, sino en la base de datos. Una query mal diseñada puede tumbar un sistema entero. En mi experiencia en banca, retail y municipalidades, invertir en índices adecuados, query splitting y caching redujo costos de infraestructura y mejoró la experiencia de usuario.

## ¿Para qué?
- **Acelerar consultas críticas** con índices clusterizados, no-clustered y columnstore.  
- **Reducir latencia en ORMs** aplicando técnicas de proyección, compiled queries y `AsNoTracking`.  
- **Escalar horizontalmente** mediante particionado (sharding, geográfico, temporal).  
- **Monitorear y prevenir** degradación de rendimiento con logging y health checks.  

## Valor agregado desde la experiencia
- Usar **columnstore indexes** permitió procesar reportes analíticos en segundos en retail.  
- Con **query splitting** en EF Core evitamos explosiones cartesianas en joins complejos.  
- Estrategias de **sharding horizontal** permitieron manejar millones de registros en sistemas municipales.  
- Configurar **pooling de conexiones** en Azure SQL redujo errores de timeout en picos bancarios.  

# Database Optimization for .NET

**Guía completa de optimización de bases de datos para aplicaciones .NET de alto rendimiento.**
Este documento cubre desde índices y consultas hasta particionado y monitoreo avanzado.
Fundamental para escalar aplicaciones que manejan grandes volúmenes de datos con latencia mínima.

## Database Indexing Strategies

**Estrategias de indexación para optimizar consultas y rendimiento de bases de datos.**
Esta tabla compara tipos de índices con sus características y casos de uso específicos.
Esencial para diseñar esquemas de base de datos que escalen eficientemente.

| **Tipo de Índice** | **Estructura**              | **Uso Óptimo**                    | **Ventajas**                    | **Desventajas**                  |
| ------------------ | --------------------------- | --------------------------------- | ------------------------------- | -------------------------------- |
| **Clustered**      | B-Tree ordenado físicamente | Primary keys, búsquedas por rango | Acceso rápido secuencial        | Solo uno por tabla               |
| **Non-Clustered**  | B-Tree con punteros         | WHERE clauses frecuentes          | Múltiples por tabla             | Overhead adicional               |
| **Composite**      | Múltiples columnas          | Consultas con varios criterios    | Optimiza queries complejas      | Orden de columnas crítico        |
| **Covering**       | Incluye datos adicionales   | SELECT específicos                | Evita key lookups               | Mayor espacio requerido          |
| **Filtered**       | Subset de datos             | Valores específicos (IS NOT NULL) | Menor tamaño, mejor rendimiento | Limitado a condiciones estáticas |
| **Columnstore**    | Almacenamiento columnar     | Análisis, agregaciones            | Excelente compresión            | No óptimo para OLTP              |

## Query Optimization with Entity Framework

**Técnicas avanzadas de optimización para Entity Framework Core.**
Esta sección demuestra patrones para minimizar queries N+1, optimizar proyecciones y usar lazy loading efectivamente.
Fundamental para aplicaciones que requieren alto rendimiento con ORMs.

```csharp
public class OptimizedProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OptimizedProductRepository> _logger;

    public OptimizedProductRepository(
        ApplicationDbContext context,
        ILogger<OptimizedProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Optimized query with explicit loading and projection
    public async Task<PagedResult<ProductSummaryDto>> GetProductsOptimizedAsync(
        ProductFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking() // Read-only optimization
            .Where(p => !p.IsDeleted);

        // Apply filters efficiently
        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(p => p.Category.Name == filter.Category);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            // Use full-text search if available, otherwise LIKE
            query = query.Where(p =>
                EF.Functions.Contains(p.Name, filter.SearchTerm) ||
                EF.Functions.Contains(p.Description, filter.SearchTerm));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // Get total count before pagination (optimized)
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        var sortedQuery = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "price" => filter.SortDescending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "created" => filter.SortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Id) // Default stable sort
        };

        // Project to DTO to reduce data transfer
        var products = await sortedQuery
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Price = p.Price,
                CategoryName = p.Category.Name,
                BrandName = p.Brand.Name,
                InStock = p.StockQuantity > 0,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary).Url,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductSummaryDto>
        {
            Items = products,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    // Optimized query with explicit Include for related data
    public async Task<ProductDetailDto> GetProductWithDetailsAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images.Where(i => !i.IsDeleted))
            .Include(p => p.Specifications)
            .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
            .Include(p => p.Tags)
            .AsSplitQuery() // Avoid cartesian explosion
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);

        if (product == null)
            return null;

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            Category = new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            },
            Brand = new BrandDto
            {
                Id = product.Brand.Id,
                Name = product.Brand.Name,
                LogoUrl = product.Brand.LogoUrl
            },
            Images = product.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder
            }).OrderBy(i => i.SortOrder).ToList(),
            Specifications = product.Specifications.ToDictionary(s => s.Name, s => s.Value),
            Tags = product.Tags.Select(t => t.Name).ToList(),
            Reviews = product.Reviews.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                UserName = r.User.FirstName,
                CreatedAt = r.CreatedAt
            }).OrderByDescending(r => r.CreatedAt).ToList(),
            StockQuantity = product.StockQuantity,
            InStock = product.StockQuantity > 0,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    // Bulk operations for better performance
    public async Task<int> BulkUpdatePricesAsync(
        Dictionary<int, decimal> productPrices,
        CancellationToken cancellationToken = default)
    {
        var productIds = productPrices.Keys.ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            if (productPrices.TryGetValue(product.Id, out var newPrice))
            {
                product.Price = newPrice;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await _context.SaveChangesAsync(cancellationToken);
    }

    // Raw SQL for complex aggregations
    public async Task<List<CategorySalesDto>> GetCategorySalesStatsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                c.Id as CategoryId,
                c.Name as CategoryName,
                COUNT(DISTINCT oi.OrderId) as OrderCount,
                SUM(oi.Quantity) as TotalQuantity,
                SUM(oi.Price * oi.Quantity) as TotalRevenue,
                AVG(oi.Price * oi.Quantity) as AverageOrderValue
            FROM Categories c
            INNER JOIN Products p ON c.Id = p.CategoryId
            INNER JOIN OrderItems oi ON p.Id = oi.ProductId
            INNER JOIN Orders o ON oi.OrderId = o.Id
            WHERE o.OrderDate >= @fromDate
                AND o.OrderDate <= @toDate
                AND o.Status = 'Completed'
            GROUP BY c.Id, c.Name
            ORDER BY TotalRevenue DESC";

        return await _context.Database
            .SqlQueryRaw<CategorySalesDto>(sql,
                new SqlParameter("@fromDate", fromDate),
                new SqlParameter("@toDate", toDate))
            .ToListAsync(cancellationToken);
    }

    // Compiled queries for frequently used operations
    private static readonly Func<ApplicationDbContext, int, Task<Product>> GetProductByIdCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
            context.Products
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id && !p.IsDeleted));

    public async Task<Product> GetProductByIdCompiledAsync(int id)
    {
        return await GetProductByIdCompiled(_context, id);
    }
}
```

## Connection Pooling and Configuration

**Configuración avanzada de connection pooling para máximo rendimiento.**
Esta implementación optimiza conexiones de base de datos para aplicaciones de alta concurrencia.
Fundamental para minimizar latencia y maximizar throughput en aplicaciones empresariales.

```csharp
public class DatabaseConfiguration
{
    public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Connection resilience
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);

                // Query optimization
                sqlOptions.CommandTimeout(30);

                // Performance features
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // EF Core optimizations
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(false); // Disable in production
            options.EnableDetailedErrors(false); // Disable in production

            // Connection pooling configuration
            options.EnableServiceProviderCaching();
            options.EnableSensitiveDataLogging(false);
        }, ServiceLifetime.Scoped);

        // Configure connection pool size
        services.Configure<SqlServerDbContextOptionsBuilder>(options =>
        {
            // Adjust based on expected concurrent users
            options.MaxPoolSize = 100;
            options.MinPoolSize = 5;
        });
    }
}

// Advanced connection management
public class DatabaseConnectionManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseConnectionManager> _logger;

    public string GetOptimizedConnectionString(string baseConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            // Connection pooling settings
            Pooling = true,
            MaxPoolSize = 100,
            MinPoolSize = 5,
            ConnectionTimeout = 30,
            CommandTimeout = 300,

            // Performance optimizations
            MultipleActiveResultSets = true,
            AsynchronousProcessing = true,

            // Security settings
            Encrypt = true,
            TrustServerCertificate = false,

            // Application settings
            ApplicationName = "MyApp",
            WorkstationID = Environment.MachineName
        };

        return builder.ConnectionString;
    }

    // Health check for database connections
    public async Task<bool> CheckDatabaseHealthAsync()
    {
        try
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT 1", connection);
            var result = await command.ExecuteScalarAsync();

            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return false;
        }
    }
}
```

## Advanced Indexing Strategies

**Estrategias avanzadas de indexación para consultas complejas.**
Esta sección cubre índices compuestos, filtrados y columnstore para diferentes patrones de acceso.
Esencial para optimizar consultas específicas y mejorar el rendimiento general de la base de datos.

### Composite Index Design

```sql
-- Optimal composite index for common query patterns
CREATE NONCLUSTERED INDEX IX_Products_Category_Price_InStock
ON Products (CategoryId, Price, InStock)
INCLUDE (Name, Description, Sku, StockQuantity, CreatedAt)
WHERE IsDeleted = 0;

-- Explanation:
-- 1. CategoryId first (high selectivity filter)
-- 2. Price second (range queries)
-- 3. InStock third (boolean filter)
-- 4. INCLUDE clause avoids key lookups
-- 5. Filtered WHERE clause excludes deleted records

-- Index for search functionality
CREATE NONCLUSTERED INDEX IX_Products_Search
ON Products (Name, Description)
INCLUDE (Id, Sku, Price, CategoryId)
WHERE IsDeleted = 0 AND IsActive = 1;

-- Full-text index for advanced search
CREATE FULLTEXT INDEX ON Products(Name, Description)
KEY INDEX PK_Products
WITH CHANGE_TRACKING AUTO;
```

### Columnstore Indexes for Analytics

```sql
-- Columnstore index for analytics workload
CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_OrderItems_Analytics
ON OrderItems (OrderId, ProductId, Quantity, Price, Discount, OrderDate);

-- Optimized for aggregation queries
-- Example usage:
SELECT
    p.CategoryId,
    YEAR(oi.OrderDate) as Year,
    MONTH(oi.OrderDate) as Month,
    SUM(oi.Quantity * oi.Price) as Revenue,
    COUNT(DISTINCT oi.OrderId) as OrderCount
FROM OrderItems oi
INNER JOIN Products p ON oi.ProductId = p.Id
WHERE oi.OrderDate >= '2023-01-01'
GROUP BY p.CategoryId, YEAR(oi.OrderDate), MONTH(oi.OrderDate);
```

## Query Performance Monitoring

**Técnicas de monitoreo y análisis de rendimiento de consultas.**
Esta sección proporciona herramientas para identificar y resolver problemas de rendimiento.
Fundamental para mantener el rendimiento óptimo en aplicaciones de producción.

### Query Execution Monitoring

```csharp
public class QueryPerformanceMonitor
{
    private readonly ILogger<QueryPerformanceMonitor> _logger;
    private readonly IMetricsCollector _metrics;

    public class QueryMetrics
    {
        public string QueryHash { get; set; }
        public string CommandText { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public int RowsAffected { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string ApplicationName { get; set; }
    }

    public async Task<T> ExecuteWithMonitoringAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        try
        {
            var result = await operation();
            stopwatch.Stop();

            // Record successful execution
            _metrics.RecordQueryExecution(operationName, stopwatch.Elapsed, true);

            // Log slow queries
            if (stopwatch.Elapsed.TotalMilliseconds > 1000) // 1 second threshold
            {
                _logger.LogWarning("Slow query detected: {OperationName} took {Duration}ms",
                    operationName, stopwatch.Elapsed.TotalMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record failed execution
            _metrics.RecordQueryExecution(operationName, stopwatch.Elapsed, false);

            _logger.LogError(ex, "Query execution failed: {OperationName} after {Duration}ms",
                operationName, stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}

// EF Core interceptor for automatic monitoring
public class QueryPerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<QueryPerformanceInterceptor> _logger;
    private readonly IConfiguration _configuration;

    private readonly int _slowQueryThresholdMs;

    public QueryPerformanceInterceptor(
        ILogger<QueryPerformanceInterceptor> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _slowQueryThresholdMs = configuration.GetValue<int>("Database:SlowQueryThresholdMs", 1000);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        eventData.Context.Items["QueryStartTime"] = stopwatch;

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context.Items.TryGetValue("QueryStartTime", out var startTimeObj) &&
            startTimeObj is Stopwatch stopwatch)
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > _slowQueryThresholdMs)
            {
                _logger.LogWarning("Slow query detected: {Duration}ms - {CommandText}",
                    stopwatch.ElapsedMilliseconds,
                    command.CommandText.Take(500)); // Truncate for logging
            }

            // Could also send to monitoring system here
            RecordQueryMetrics(command, stopwatch.Elapsed, eventData);
        }

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void RecordQueryMetrics(DbCommand command, TimeSpan duration, CommandExecutedEventData eventData)
    {
        // Implementation to record metrics
        // Could send to Application Insights, Prometheus, etc.
    }
}
```

## Database Partitioning Strategies

**Estrategias de particionado para manejar grandes volúmenes de datos.**
Esta tabla compara diferentes enfoques de particionado con sus beneficios y consideraciones.
Fundamental para aplicaciones que manejan terabytes de datos y requieren escalabilidad horizontal.

| **Tipo de Partición**     | **Criterio**               | **Beneficios**                | **Casos de Uso**                      | **Consideraciones**                |
| ------------------------- | -------------------------- | ----------------------------- | ------------------------------------- | ---------------------------------- |
| **Horizontal (Sharding)** | Por ID, fecha, región      | Escalabilidad, paralelización | Aplicaciones masivas                  | Complejidad de queries cross-shard |
| **Vertical**              | Por columnas/funcionalidad | Optimización de I/O           | Separar datos frecuentes/infrecuentes | Joins complejos                    |
| **Funcional**             | Por dominio de negocio     | Aislamiento, especialización  | Microservicios                        | Transacciones distribuidas         |
| **Temporal**              | Por fecha/tiempo           | Archivado automático          | Logs, auditoría                       | Gestión de ventanas                |
| **Geográfica**            | Por ubicación              | Latencia reducida             | Aplicaciones globales                 | Consistencia eventual              |

### Horizontal Partitioning Implementation

```csharp
public class ShardedOrderService
{
    private readonly IShardResolver _shardResolver;
    private readonly Dictionary<string, IOrderRepository> _shardRepositories;
    private readonly ILogger<ShardedOrderService> _logger;

    public ShardedOrderService(
        IShardResolver shardResolver,
        IEnumerable<IOrderRepository> repositories,
        ILogger<ShardedOrderService> logger)
    {
        _shardResolver = shardResolver;
        _shardRepositories = repositories.ToDictionary(r => r.ShardKey);
        _logger = logger;
    }

    public async Task<Order> GetOrderAsync(int orderId)
    {
        var shardKey = _shardResolver.ResolveShardForOrder(orderId);
        var repository = _shardRepositories[shardKey];

        return await repository.GetByIdAsync(orderId);
    }

    public async Task<List<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        // Orders may be distributed across multiple shards
        var shardKeys = _shardResolver.GetShardsForCustomer(customerId);
        var tasks = shardKeys.Select(async shardKey =>
        {
            var repository = _shardRepositories[shardKey];
            return await repository.GetByCustomerIdAsync(customerId);
        });

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(orders => orders).ToList();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        var shardKey = _shardResolver.ResolveShardForNewOrder(order);
        var repository = _shardRepositories[shardKey];

        order.ShardKey = shardKey; // Store shard info for future queries
        return await repository.CreateAsync(order);
    }

    // Cross-shard aggregation
    public async Task<OrderStatistics> GetOrderStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        var tasks = _shardRepositories.Values.Select(async repository =>
        {
            return await repository.GetStatisticsAsync(fromDate, toDate);
        });

        var shardStats = await Task.WhenAll(tasks);

        return new OrderStatistics
        {
            TotalOrders = shardStats.Sum(s => s.TotalOrders),
            TotalRevenue = shardStats.Sum(s => s.TotalRevenue),
            AverageOrderValue = shardStats.Sum(s => s.TotalRevenue) / shardStats.Sum(s => s.TotalOrders)
        };
    }
}

// Shard resolver implementation
public class OrderShardResolver : IShardResolver
{
    private readonly IConfiguration _configuration;

    public string ResolveShardForOrder(int orderId)
    {
        // Simple modulo-based sharding
        var shardCount = _configuration.GetValue<int>("Database:ShardCount", 4);
        var shardIndex = orderId % shardCount;
        return $"shard_{shardIndex}";
    }

    public string ResolveShardForNewOrder(Order order)
    {
        // Could use customer ID, date, or other criteria
        return ResolveShardForCustomer(order.CustomerId);
    }

    public string ResolveShardForCustomer(int customerId)
    {
        var shardCount = _configuration.GetValue<int>("Database:ShardCount", 4);
        var shardIndex = customerId % shardCount;
        return $"shard_{shardIndex}";
    }

    public List<string> GetShardsForCustomer(int customerId)
    {
        // For this implementation, customer orders are in one shard
        return new List<string> { ResolveShardForCustomer(customerId) };
    }

    public List<string> GetAllShards()
    {
        var shardCount = _configuration.GetValue<int>("Database:ShardCount", 4);
        return Enumerable.Range(0, shardCount)
            .Select(i => $"shard_{i}")
            .ToList();
    }
}
```

### Temporal Partitioning for Time-Series Data

```sql
-- Partition function for monthly partitions
CREATE PARTITION FUNCTION PF_OrdersByMonth (DATETIME2)
AS RANGE RIGHT FOR VALUES (
    '2023-01-01', '2023-02-01', '2023-03-01', '2023-04-01',
    '2023-05-01', '2023-06-01', '2023-07-01', '2023-08-01',
    '2023-09-01', '2023-10-01', '2023-11-01', '2023-12-01',
    '2024-01-01'
);

-- Partition scheme
CREATE PARTITION SCHEME PS_OrdersByMonth
AS PARTITION PF_OrdersByMonth
TO (
    OrderData2023Q1, OrderData2023Q2, OrderData2023Q3,
    OrderData2023Q4, OrderData2024Q1, OrderData2024Q2,
    OrderData2024Q3, OrderData2024Q4, OrderDataFuture
);

-- Partitioned table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1),
    CustomerId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_Orders PRIMARY KEY (Id, OrderDate)
) ON PS_OrdersByMonth(OrderDate);

-- Automatic partition management
CREATE PROCEDURE sp_ManageOrderPartitions
AS
BEGIN
    DECLARE @NextMonth DATETIME2 = DATEADD(MONTH, 1, EOMONTH(GETDATE()));
    DECLARE @FileGroupName NVARCHAR(128) = 'OrderData' + FORMAT(@NextMonth, 'yyyyMM');

    -- Add new partition boundary
    ALTER PARTITION SCHEME PS_OrdersByMonth
    NEXT USED @FileGroupName;

    ALTER PARTITION FUNCTION PF_OrdersByMonth()
    SPLIT RANGE (@NextMonth);

    -- Archive old partitions (older than 2 years)
    DECLARE @ArchiveDate DATETIME2 = DATEADD(YEAR, -2, GETDATE());
    -- Implementation for archiving old partitions
END;
```

## Database Performance Metrics

**Métricas clave para monitorear el rendimiento de base de datos en producción.**
Esta tabla identifica KPIs críticos y umbrales saludables para optimización continua.
Esencial para detectar problemas de rendimiento antes de que afecten a los usuarios.

| **Métrica**             | **Descripción**              | **Umbral Saludable**           | **Herramienta de Monitoreo**      |
| ----------------------- | ---------------------------- | ------------------------------ | --------------------------------- |
| **Query Response Time** | Tiempo promedio de respuesta | < 100ms para queries simples   | SQL Server Profiler, App Insights |
| **CPU Utilization**     | Uso de CPU del servidor DB   | < 70% promedio                 | Performance Monitor               |
| **Memory Usage**        | Buffer pool hit ratio        | > 95% hit ratio                | sys.dm_os_performance_counters    |
| **Disk I/O**            | IOPS y latencia de disco     | < 20ms latencia promedio       | PerfMon, Azure Monitor            |
| **Blocking/Deadlocks**  | Bloqueos y deadlocks         | < 5 deadlocks/hora             | Extended Events                   |
| **Connection Pool**     | Conexiones activas vs máximo | < 80% utilización              | Connection string metrics         |
| **Index Fragmentation** | Fragmentación de índices     | < 30% fragmentación            | sys.dm_db_index_physical_stats    |
| **Wait Statistics**     | Tipos de esperas más comunes | Identificar cuellos de botella | sys.dm_os_wait_stats              |

### Performance Monitoring Implementation

```csharp
public class DatabasePerformanceMonitor
{
    private readonly string _connectionString;
    private readonly ILogger<DatabasePerformanceMonitor> _logger;
    private readonly IMetricsLogger _metrics;

    public async Task<DatabaseHealthReport> GenerateHealthReportAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var report = new DatabaseHealthReport
        {
            Timestamp = DateTime.UtcNow,
            CpuUsage = await GetCpuUsageAsync(connection),
            MemoryUsage = await GetMemoryUsageAsync(connection),
            TopSlowQueries = await GetTopSlowQueriesAsync(connection),
            BlockingProcesses = await GetBlockingProcessesAsync(connection),
            IndexFragmentation = await GetIndexFragmentationAsync(connection),
            WaitStatistics = await GetWaitStatisticsAsync(connection)
        };

        // Alert on critical thresholds
        await CheckAndAlertAsync(report);

        return report;
    }

    private async Task<double> GetCpuUsageAsync(SqlConnection connection)
    {
        var query = @"
            SELECT TOP 1
                AVG(signal_wait_time_ms) * 100.0 / AVG(wait_time_ms) as cpu_usage
            FROM sys.dm_os_wait_stats
            WHERE wait_time_ms > 0
            AND wait_type NOT IN (
                'CLR_SEMAPHORE', 'LAZYWRITER_SLEEP', 'RESOURCE_QUEUE',
                'SLEEP_TASK', 'SLEEP_SYSTEMTASK', 'WAITFOR', 'HADR_FILESTREAM_IOMGR_IOCOMPLETION'
            )";

        using var command = new SqlCommand(query, connection);
        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? Convert.ToDouble(result) : 0;
    }

    private async Task<List<SlowQueryInfo>> GetTopSlowQueriesAsync(SqlConnection connection)
    {
        var query = @"
            SELECT TOP 10
                t.text as query_text,
                s.execution_count,
                s.total_elapsed_time / s.execution_count as avg_elapsed_time,
                s.total_logical_reads / s.execution_count as avg_logical_reads,
                s.total_physical_reads / s.execution_count as avg_physical_reads
            FROM sys.dm_exec_query_stats s
            CROSS APPLY sys.dm_exec_sql_text(s.sql_handle) t
            WHERE s.execution_count > 5
            ORDER BY s.total_elapsed_time / s.execution_count DESC";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        var slowQueries = new List<SlowQueryInfo>();
        while (await reader.ReadAsync())
        {
            slowQueries.Add(new SlowQueryInfo
            {
                QueryText = reader.GetString("query_text"),
                ExecutionCount = reader.GetInt32("execution_count"),
                AvgElapsedTime = reader.GetInt64("avg_elapsed_time"),
                AvgLogicalReads = reader.GetInt64("avg_logical_reads"),
                AvgPhysicalReads = reader.GetInt64("avg_physical_reads")
            });
        }

        return slowQueries;
    }

    private async Task CheckAndAlertAsync(DatabaseHealthReport report)
    {
        if (report.CpuUsage > 80)
        {
            _logger.LogWarning("High CPU usage detected: {CpuUsage}%", report.CpuUsage);
            _metrics.Counter("database.alerts").WithTag("type", "high_cpu").Increment();
        }

        if (report.MemoryUsage.BufferHitRatio < 95)
        {
            _logger.LogWarning("Low buffer hit ratio: {BufferHitRatio}%", report.MemoryUsage.BufferHitRatio);
            _metrics.Counter("database.alerts").WithTag("type", "low_buffer_hit").Increment();
        }

        var slowQueries = report.TopSlowQueries.Where(q => q.AvgElapsedTime > 1000000); // > 1 second
        if (slowQueries.Any())
        {
            _logger.LogWarning("Slow queries detected: {SlowQueryCount}", slowQueries.Count());
            _metrics.Counter("database.alerts").WithTag("type", "slow_queries").Increment();
        }
    }
}
```
