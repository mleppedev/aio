# Contexto y Propósito

## ¿Qué es?
La observabilidad es la capacidad de entender el estado interno de un sistema a partir de sus logs, métricas y trazas. En .NET se implementa con OpenTelemetry, Application Insights, Prometheus, Serilog y Jaeger/Zipkin. Va más allá del monitoreo: busca correlacionar y diagnosticar.

## ¿Por qué?
Porque en sistemas distribuidos los errores no son obvios. En mi experiencia, carecer de observabilidad aumentó tiempos de resolución en banca, mientras que contar con métricas, logs estructurados y trazas distribuidas permitió aislar problemas en minutos en municipalidades y retail.

## ¿Para qué?
- **Correlacionar requests** en microservicios con trazas distribuidas.  
- **Detectar degradación de performance** con métricas de CPU, memoria y latencia.  
- **Asegurar SLA (Service Level Agreement)** mediante alertas proactivas.  
- **Auditar eventos críticos** con logging estructurado.  

## Valor agregado desde la experiencia
- Implementar **OpenTelemetry + Jaeger** permitió visualizar cuellos de botella en pipelines de pagos.  
- Con **Application Insights**, obtuvimos dashboards en tiempo real en proyectos municipales.  
- **Health checks automáticos** facilitaron despliegues resilientes en Kubernetes.  
- **Logs estructurados con Serilog** simplificaron auditorías bancarias y debugging de producción.  

# Observability in .NET

**Guía completa de observabilidad en aplicaciones .NET con telemetría, métricas, logging y tracing distribuido.**
Este documento cubre implementación de OpenTelemetry, Application Insights, structured logging y métricas de rendimiento.
Fundamental para monitoreo, debugging y optimización de aplicaciones distribuidas en producción.

## Observability Pillars

**Los tres pilares fundamentales de observabilidad en aplicaciones .NET modernas.**
Esta tabla compara logging, metrics y tracing con sus propósitos específicos y herramientas.
Esencial para implementar una estrategia completa de observabilidad y troubleshooting efectivo.

| **Pilar**                | **Propósito**                          | **Casos de Uso**                   | **Herramientas .NET**        | **Formato/Protocolo** |
| ------------------------ | -------------------------------------- | ---------------------------------- | ---------------------------- | --------------------- |
| **Logging**              | Eventos discretos del sistema          | Debugging, auditoría, errores      | Serilog, NLog, ILogger       | JSON, estructurado    |
| **Metrics**              | Mediciones numéricas agregadas         | Performance, SLA, alertas          | .NET Metrics API, Prometheus | Prometheus, OTLP      |
| **Tracing**              | Flujo de requests a través del sistema | Request flow, latencia distribuida | Activity, OpenTelemetry      | Jaeger, Zipkin, OTLP  |
| **Profiles**             | Análisis de rendimiento detallado      | CPU, memoria, hotspaths            | dotTrace, PerfView           | Flame graphs          |
| **Health Checks**        | Estado de componentes                  | Uptime, readiness, liveness        | ASP.NET Health Checks        | HTTP endpoints        |
| **Synthetic Monitoring** | Monitoreo proactivo                    | User experience, availability      | Application Insights         | HTTP, Browser         |

## OpenTelemetry Configuration

**Configuración completa de OpenTelemetry para observabilidad distribuida en .NET.**
Esta sección demuestra setup de tracing, metrics y logging con diferentes exporters.
Fundamental para correlacionar datos a través de servicios distribuidos y microservicios.

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Diagnostics;
using System.Diagnostics.Metrics;

public class OpenTelemetryConfiguration
{
    private static readonly ActivitySource ActivitySource = new("MyApp.Service");
    private static readonly Meter Meter = new("MyApp.Service.Metrics");

    // Configure OpenTelemetry in Program.cs
    public static void ConfigureOpenTelemetry(WebApplicationBuilder builder)
    {
        var serviceName = "MyService";
        var serviceVersion = "1.0.0";

        // Configure resource
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion)
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("environment", builder.Environment.EnvironmentName),
                new KeyValuePair<string, object>("team", "backend"),
                new KeyValuePair<string, object>("component", "api")
            });

        // Configure Tracing
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(ActivitySource.Name)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = EnrichWithHttpRequest;
                        options.EnrichWithHttpResponse = EnrichWithHttpResponse;
                        options.Filter = (httpContext) =>
                        {
                            // Filter out health check endpoints
                            return !httpContext.Request.Path.Value?.Contains("/health") == true;
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = EnrichWithHttpRequestMessage;
                        options.EnrichWithHttpResponseMessage = EnrichWithHttpResponseMessage;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                        options.EnrichWithIDbCommand = EnrichWithDbCommand;
                    })
                    .AddRedisInstrumentation()
                    .AddConsoleExporter()
                    .AddJaegerExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317");
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            // Configure Metrics
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(Meter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            });

        // Configure Logging with OpenTelemetry
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.AddConsoleExporter();
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri("http://localhost:4318/v1/logs");
                otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });
    }

    // Custom enrichment methods
    private static void EnrichWithHttpRequest(Activity activity, HttpRequest httpRequest)
    {
        activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
        activity.SetTag("http.request.content_type", httpRequest.ContentType);
        activity.SetTag("http.request.content_length", httpRequest.ContentLength);

        if (httpRequest.Headers.ContainsKey("X-Request-ID"))
        {
            activity.SetTag("http.request.id", httpRequest.Headers["X-Request-ID"].ToString());
        }
    }

    private static void EnrichWithHttpResponse(Activity activity, HttpResponse httpResponse)
    {
        activity.SetTag("http.response.content_type", httpResponse.ContentType);
        activity.SetTag("http.response.content_length", httpResponse.ContentLength);
    }

    private static void EnrichWithHttpRequestMessage(Activity activity, HttpRequestMessage httpRequestMessage)
    {
        activity.SetTag("http.client.request.header.authorization",
            httpRequestMessage.Headers.Authorization?.Scheme == "Bearer" ? "Bearer [REDACTED]" : "None");
    }

    private static void EnrichWithHttpResponseMessage(Activity activity, HttpResponseMessage httpResponseMessage)
    {
        activity.SetTag("http.client.response.size", httpResponseMessage.Content?.Headers?.ContentLength);
    }

    private static void EnrichWithDbCommand(Activity activity, IDbCommand command)
    {
        activity.SetTag("db.rows_affected", "unknown"); // This would be set after execution
        activity.SetTag("db.command.parameters.count", command.Parameters?.Count);
    }
}

// Service with comprehensive instrumentation
public class ObservableUserService
{
    private static readonly ActivitySource ActivitySource = new("UserService");
    private static readonly Meter Meter = new("UserService.Metrics");

    private readonly IUserRepository _userRepository;
    private readonly ILogger<ObservableUserService> _logger;

    // Custom metrics
    private readonly Counter<int> _userCreatedCounter;
    private readonly Histogram<double> _userServiceDuration;
    private readonly UpDownCounter<int> _activeUsersGauge;

    public ObservableUserService(
        IUserRepository userRepository,
        ILogger<ObservableUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;

        // Initialize metrics
        _userCreatedCounter = Meter.CreateCounter<int>(
            "user_created_total",
            description: "Total number of users created");

        _userServiceDuration = Meter.CreateHistogram<double>(
            "user_service_duration_seconds",
            description: "Duration of user service operations");

        _activeUsersGauge = Meter.CreateUpDownCounter<int>(
            "active_users_current",
            description: "Current number of active users");
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("UserService.CreateUser");
        activity?.SetTag("user.email", request.Email);
        activity?.SetTag("user.role", request.Role);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Creating user with email {Email}", request.Email);

            // Add structured logging context
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Operation"] = "CreateUser",
                ["UserEmail"] = request.Email,
                ["CorrelationId"] = Activity.Current?.Id ?? Guid.NewGuid().ToString()
            });

            // Validate request
            if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
            {
                var exception = new UserAlreadyExistsException($"User with email {request.Email} already exists");
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity?.RecordException(exception);

                _logger.LogWarning("Attempted to create duplicate user {Email}", request.Email);
                throw exception;
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user, cancellationToken);

            // Record metrics
            _userCreatedCounter.Add(1, new KeyValuePair<string, object?>("role", request.Role));
            _activeUsersGauge.Add(1);

            // Add tags to activity
            activity?.SetTag("user.id", user.Id.ToString());
            activity?.SetTag("operation.success", true);

            _logger.LogInformation("Successfully created user {UserId} with email {Email}",
                user.Id, request.Email);

            return user;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);

            _logger.LogError(ex, "Failed to create user with email {Email}", request.Email);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _userServiceDuration.Record(stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("operation", "create_user"));
        }
    }

    public async Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("UserService.GetUser");
        activity?.SetTag("user.id", userId.ToString());

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Retrieving user {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                activity?.SetTag("user.found", false);
                _logger.LogInformation("User {UserId} not found", userId);
            }
            else
            {
                activity?.SetTag("user.found", true);
                activity?.SetTag("user.email", user.Email);
                activity?.SetTag("user.role", user.Role);

                _logger.LogDebug("Successfully retrieved user {UserId}", userId);
            }

            return user;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);

            _logger.LogError(ex, "Failed to retrieve user {UserId}", userId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _userServiceDuration.Record(stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("operation", "get_user"));
        }
    }
}

// Custom middleware for correlation tracking
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        // Add to current activity
        Activity.Current?.SetTag("correlation.id", correlationId);

        // Add to logging context
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        await _next(context);
    }
}

// Models
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string message) : base(message) { }
}

public interface IUserRepository
{
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
    Task CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
```

## Application Insights Integration

**Integración avanzada con Azure Application Insights para observabilidad completa.**
Esta sección demuestra configuración de telemetría personalizada, dependency tracking y performance counters.
Fundamental para monitoreo en Azure con analytics queries y alertas automáticas.

```csharp
public class ApplicationInsightsConfiguration
{
    public static void ConfigureApplicationInsights(WebApplicationBuilder builder)
    {
        // Add Application Insights
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
            options.EnableAdaptiveSampling = true;
            options.EnableQuickPulseMetricStream = true;
            options.EnablePerformanceCounterCollectionModule = true;
            options.EnableDependencyTrackingTelemetryModule = true;
            options.EnableEventCounterCollectionModule = true;
        });

        // Add custom telemetry processors
        builder.Services.AddSingleton<ITelemetryProcessor, CustomTelemetryProcessor>();
        builder.Services.AddSingleton<ITelemetryProcessor, SensitiveDataFilterProcessor>();

        // Add custom telemetry initializers
        builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
        builder.Services.AddSingleton<ITelemetryInitializer, UserContextInitializer>();

        // Configure sampling
        builder.Services.Configure<TelemetryConfiguration>(config =>
        {
            config.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                .UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5, excludedTypes: "Event")
                .Use(next => new CustomTelemetryProcessor(next))
                .Build();
        });
    }
}

// Custom telemetry processor for filtering sensitive data
public class SensitiveDataFilterProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;
    private readonly ILogger<SensitiveDataFilterProcessor> _logger;

    private static readonly string[] SensitiveProperties =
    {
        "password", "token", "secret", "key", "authorization", "ssn", "creditcard"
    };

    public SensitiveDataFilterProcessor(ITelemetryProcessor next, ILogger<SensitiveDataFilterProcessor> logger)
    {
        _next = next;
        _logger = logger;
    }

    public void Process(ITelemetry item)
    {
        switch (item)
        {
            case RequestTelemetry request:
                FilterSensitiveData(request.Properties);
                FilterSensitiveData(request.Context.GlobalProperties);
                break;

            case DependencyTelemetry dependency:
                FilterSensitiveData(dependency.Properties);
                FilterSensitiveUrl(dependency);
                break;

            case TraceTelemetry trace:
                FilterSensitiveData(trace.Properties);
                FilterSensitiveMessage(trace);
                break;

            case ExceptionTelemetry exception:
                FilterSensitiveData(exception.Properties);
                FilterSensitiveException(exception);
                break;
        }

        _next.Process(item);
    }

    private void FilterSensitiveData(IDictionary<string, string> properties)
    {
        var keysToFilter = properties.Keys
            .Where(key => SensitiveProperties.Any(sensitive =>
                key.Contains(sensitive, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var key in keysToFilter)
        {
            properties[key] = "[REDACTED]";
        }
    }

    private void FilterSensitiveUrl(DependencyTelemetry dependency)
    {
        if (!string.IsNullOrEmpty(dependency.Data))
        {
            // Remove query parameters that might contain sensitive data
            var uri = new Uri(dependency.Data);
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var baseUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
                dependency.Data = $"{baseUrl}?[QUERY_REDACTED]";
            }
        }
    }

    private void FilterSensitiveMessage(TraceTelemetry trace)
    {
        if (!string.IsNullOrEmpty(trace.Message))
        {
            foreach (var sensitive in SensitiveProperties)
            {
                if (trace.Message.Contains(sensitive, StringComparison.OrdinalIgnoreCase))
                {
                    trace.Message = "[SENSITIVE_DATA_FILTERED]";
                    break;
                }
            }
        }
    }

    private void FilterSensitiveException(ExceptionTelemetry exception)
    {
        if (exception.Exception != null)
        {
            // Filter sensitive data from exception messages
            var message = exception.Exception.Message;
            foreach (var sensitive in SensitiveProperties)
            {
                if (message.Contains(sensitive, StringComparison.OrdinalIgnoreCase))
                {
                    exception.Properties["original_message"] = "[FILTERED]";
                    break;
                }
            }
        }
    }
}

// Custom telemetry initializer
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context != null)
        {
            // Add custom dimensions
            telemetry.Context.GlobalProperties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            telemetry.Context.GlobalProperties["MachineName"] = Environment.MachineName;
            telemetry.Context.GlobalProperties["AssemblyVersion"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

            // Add user context
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                telemetry.Context.User.Id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                telemetry.Context.User.AuthenticatedUserId = context.User.FindFirst(ClaimTypes.Email)?.Value;
            }

            // Add session context
            if (context.Session.IsAvailable)
            {
                telemetry.Context.Session.Id = context.Session.Id;
            }

            // Add correlation context
            if (context.Request.Headers.ContainsKey("X-Correlation-ID"))
            {
                telemetry.Context.Operation.Id = context.Request.Headers["X-Correlation-ID"];
            }
        }
    }
}

// Service with Application Insights instrumentation
public class InstrumentedOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<InstrumentedOrderService> _logger;

    public InstrumentedOrderService(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        TelemetryClient telemetryClient,
        ILogger<InstrumentedOrderService> logger)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public async Task<OrderResult> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationId = Guid.NewGuid().ToString();

        try
        {
            // Track custom event
            _telemetryClient.TrackEvent("OrderProcessingStarted", new Dictionary<string, string>
            {
                ["OrderId"] = request.OrderId,
                ["CustomerId"] = request.CustomerId,
                ["ItemCount"] = request.Items.Count.ToString(),
                ["OperationId"] = operationId
            });

            _logger.LogInformation("Processing order {OrderId} for customer {CustomerId} with {ItemCount} items",
                request.OrderId, request.CustomerId, request.Items.Count);

            // Validate order
            var validationResult = await ValidateOrderAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _telemetryClient.TrackEvent("OrderValidationFailed", new Dictionary<string, string>
                {
                    ["OrderId"] = request.OrderId,
                    ["ValidationErrors"] = string.Join(", ", validationResult.Errors),
                    ["OperationId"] = operationId
                });

                return new OrderResult { Success = false, Errors = validationResult.Errors };
            }

            // Calculate totals
            var totalAmount = request.Items.Sum(item => item.Price * item.Quantity);

            // Track custom metric
            _telemetryClient.TrackMetric("OrderValue", totalAmount, new Dictionary<string, string>
            {
                ["Currency"] = "USD",
                ["CustomerSegment"] = GetCustomerSegment(request.CustomerId)
            });

            // Process payment
            var paymentResult = await _paymentService.ProcessPaymentAsync(new PaymentRequest
            {
                OrderId = request.OrderId,
                CustomerId = request.CustomerId,
                Amount = totalAmount
            }, cancellationToken);

            if (!paymentResult.Success)
            {
                _telemetryClient.TrackEvent("PaymentFailed", new Dictionary<string, string>
                {
                    ["OrderId"] = request.OrderId,
                    ["PaymentError"] = paymentResult.ErrorMessage,
                    ["Amount"] = totalAmount.ToString("C"),
                    ["OperationId"] = operationId
                });

                return new OrderResult
                {
                    Success = false,
                    Errors = new[] { $"Payment failed: {paymentResult.ErrorMessage}" }
                };
            }

            // Save order
            var order = new Order
            {
                Id = request.OrderId,
                CustomerId = request.CustomerId,
                Items = request.Items,
                TotalAmount = totalAmount,
                PaymentId = paymentResult.PaymentId,
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository.SaveAsync(order, cancellationToken);

            // Track success metrics
            stopwatch.Stop();
            _telemetryClient.TrackMetric("OrderProcessingDuration", stopwatch.ElapsedMilliseconds);
            _telemetryClient.TrackMetric("OrdersProcessedTotal", 1);

            _telemetryClient.TrackEvent("OrderProcessingCompleted", new Dictionary<string, string>
            {
                ["OrderId"] = request.OrderId,
                ["PaymentId"] = paymentResult.PaymentId,
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds.ToString(),
                ["OperationId"] = operationId
            });

            _logger.LogInformation("Successfully processed order {OrderId} in {ElapsedMs}ms",
                request.OrderId, stopwatch.ElapsedMilliseconds);

            return new OrderResult
            {
                Success = true,
                OrderId = request.OrderId,
                PaymentId = paymentResult.PaymentId
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Track exception with custom properties
            var properties = new Dictionary<string, string>
            {
                ["OrderId"] = request.OrderId,
                ["CustomerId"] = request.CustomerId,
                ["OperationId"] = operationId,
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds.ToString()
            };

            _telemetryClient.TrackException(ex, properties);
            _telemetryClient.TrackEvent("OrderProcessingFailed", properties);

            _logger.LogError(ex, "Failed to process order {OrderId}", request.OrderId);
            throw;
        }
    }

    private async Task<ValidationResult> ValidateOrderAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Track dependency
        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>("OrderValidation");
        operation.Telemetry.Type = "Validation";

        try
        {
            if (string.IsNullOrEmpty(request.CustomerId))
                errors.Add("Customer ID is required");

            if (!request.Items.Any())
                errors.Add("Order must contain at least one item");

            if (request.Items.Any(item => item.Price <= 0))
                errors.Add("All items must have positive prices");

            // Simulate async validation
            await Task.Delay(50, cancellationToken);

            operation.Telemetry.Success = errors.Count == 0;
            return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
        }
        catch (Exception ex)
        {
            operation.Telemetry.Success = false;
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    private string GetCustomerSegment(string customerId)
    {
        // Simplified customer segmentation logic
        return customerId.GetHashCode() % 3 switch
        {
            0 => "Premium",
            1 => "Standard",
            _ => "Basic"
        };
    }
}

// Models for Application Insights example
public class OrderRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderResult
{
    public bool Success { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}
```

## Custom Metrics and Health Checks

**Implementación de métricas personalizadas y health checks para monitoreo de aplicaciones.**
Esta sección demuestra System.Diagnostics.Metrics, health checks y métricas de business logic.
Fundamental para alertas proactivas y SLA monitoring en aplicaciones de producción.

```csharp
public class CustomMetricsService
{
    private static readonly Meter Meter = new("MyApp.Business.Metrics");

    // Business metrics
    private readonly Counter<long> _ordersProcessedCounter;
    private readonly Histogram<double> _orderProcessingDuration;
    private readonly ObservableGauge<int> _activeConnectionsGauge;
    private readonly UpDownCounter<int> _inventoryLevel;

    private readonly IConnectionMonitor _connectionMonitor;
    private readonly ILogger<CustomMetricsService> _logger;

    public CustomMetricsService(IConnectionMonitor connectionMonitor, ILogger<CustomMetricsService> logger)
    {
        _connectionMonitor = connectionMonitor;
        _logger = logger;

        // Initialize business metrics
        _ordersProcessedCounter = Meter.CreateCounter<long>(
            name: "orders_processed_total",
            unit: "orders",
            description: "Total number of orders processed by the system");

        _orderProcessingDuration = Meter.CreateHistogram<double>(
            name: "order_processing_duration_seconds",
            unit: "s",
            description: "Time taken to process an order");

        _inventoryLevel = Meter.CreateUpDownCounter<int>(
            name: "inventory_level_current",
            unit: "items",
            description: "Current inventory level for products");

        // Observable gauge for real-time metrics
        _activeConnectionsGauge = Meter.CreateObservableGauge<int>(
            name: "active_connections_current",
            unit: "connections",
            description: "Current number of active database connections",
            observeValue: () => _connectionMonitor.GetActiveConnectionCount());

        // Register additional observable metrics
        Meter.CreateObservableGauge<long>(
            name: "memory_usage_bytes",
            unit: "bytes",
            description: "Current memory usage",
            observeValue: () => GC.GetTotalMemory(forceFullCollection: false));

        Meter.CreateObservableGauge<double>(
            name: "cpu_usage_percent",
            unit: "%",
            description: "Current CPU usage percentage",
            observeValue: GetCpuUsagePercentage);
    }

    public void RecordOrderProcessed(string orderType, string customerSegment, double processingTimeSeconds)
    {
        var tags = new TagList
        {
            ["order_type"] = orderType,
            ["customer_segment"] = customerSegment
        };

        _ordersProcessedCounter.Add(1, tags);
        _orderProcessingDuration.Record(processingTimeSeconds, tags);

        _logger.LogDebug("Recorded order processing metrics: type={OrderType}, segment={CustomerSegment}, duration={Duration}s",
            orderType, customerSegment, processingTimeSeconds);
    }

    public void UpdateInventoryLevel(string productId, int changeAmount)
    {
        var tags = new TagList
        {
            ["product_id"] = productId
        };

        _inventoryLevel.Add(changeAmount, tags);

        _logger.LogDebug("Updated inventory for product {ProductId}: change={Change}", productId, changeAmount);
    }

    private double GetCpuUsagePercentage()
    {
        try
        {
            // Simplified CPU usage calculation
            // In production, use PerformanceCounter or similar
            var process = Process.GetCurrentProcess();
            return (process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount) * 100;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get CPU usage");
            return 0;
        }
    }
}

// Health Checks Implementation
public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            // Built-in health checks
            .AddDbContextCheck<ApplicationDbContext>()
            .AddUrlGroup(new Uri("https://api.external-service.com/health"), "External API")
            .AddRedis(configuration.GetConnectionString("Redis"))
            .AddAzureServiceBusQueue(
                connectionString: configuration.GetConnectionString("ServiceBus"),
                queueName: "processing-queue")

            // Custom health checks
            .AddCheck<DatabaseHealthCheck>("database-detailed")
            .AddCheck<ExternalServiceHealthCheck>("external-service-detailed")
            .AddCheck<BusinessLogicHealthCheck>("business-logic")
            .AddCheck<CacheHealthCheck>("cache-performance");

        return services;
    }
}

// Custom Database Health Check
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Test basic connectivity
            await _context.Database.CanConnectAsync(cancellationToken);

            // Test query performance
            var userCount = await _context.Users.CountAsync(cancellationToken);

            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["user_count"] = userCount,
                ["connection_string"] = _context.Database.GetConnectionString()?.Split(';')[0] + ";[...]"
            };

            if (stopwatch.ElapsedMilliseconds > 5000) // 5 seconds threshold
            {
                _logger.LogWarning("Database health check slow: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return HealthCheckResult.Degraded("Database responding slowly", data: data);
            }

            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unavailable", ex);
        }
    }
}

// Business Logic Health Check
public class BusinessLogicHealthCheck : IHealthCheck
{
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<BusinessLogicHealthCheck> _logger;

    public BusinessLogicHealthCheck(
        IOrderService orderService,
        IInventoryService inventoryService,
        ILogger<BusinessLogicHealthCheck> logger)
    {
        _orderService = orderService;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        var data = new Dictionary<string, object>();

        try
        {
            // Check order processing capability
            var pendingOrders = await _orderService.GetPendingOrderCountAsync(cancellationToken);
            data["pending_orders"] = pendingOrders;

            if (pendingOrders > 1000)
            {
                issues.Add($"High number of pending orders: {pendingOrders}");
            }

            // Check inventory levels
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync(cancellationToken);
            data["low_stock_items"] = lowStockItems.Count;

            if (lowStockItems.Count > 50)
            {
                issues.Add($"Many items with low stock: {lowStockItems.Count}");
            }

            // Check processing rates
            var recentThroughput = await _orderService.GetRecentThroughputAsync(TimeSpan.FromMinutes(5), cancellationToken);
            data["recent_throughput_per_minute"] = recentThroughput;

            if (recentThroughput < 10) // Less than 10 orders per minute
            {
                issues.Add($"Low processing throughput: {recentThroughput} orders/min");
            }

            if (issues.Any())
            {
                return HealthCheckResult.Degraded(
                    $"Business logic issues detected: {string.Join(", ", issues)}",
                    data: data);
            }

            return HealthCheckResult.Healthy("Business logic is operating normally", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Business logic health check failed");
            return HealthCheckResult.Unhealthy("Business logic health check failed", ex, data);
        }
    }
}

// Cache Performance Health Check
public class CacheHealthCheck : IHealthCheck
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheHealthCheck> _logger;

    public CacheHealthCheck(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<CacheHealthCheck> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            // Test memory cache
            var memoryCacheStopwatch = Stopwatch.StartNew();
            var testKey = $"health-check-{Guid.NewGuid()}";
            var testValue = "test-value";

            _memoryCache.Set(testKey, testValue, TimeSpan.FromSeconds(1));
            var retrievedValue = _memoryCache.Get<string>(testKey);

            memoryCacheStopwatch.Stop();
            data["memory_cache_response_time_ms"] = memoryCacheStopwatch.ElapsedMilliseconds;

            if (retrievedValue != testValue)
            {
                issues.Add("Memory cache read/write test failed");
            }

            // Test distributed cache
            var distributedCacheStopwatch = Stopwatch.StartNew();
            var distributedTestKey = $"health-check-distributed-{Guid.NewGuid()}";

            await _distributedCache.SetStringAsync(distributedTestKey, testValue, cancellationToken);
            var distributedRetrievedValue = await _distributedCache.GetStringAsync(distributedTestKey, cancellationToken);

            distributedCacheStopwatch.Stop();
            data["distributed_cache_response_time_ms"] = distributedCacheStopwatch.ElapsedMilliseconds;

            if (distributedRetrievedValue != testValue)
            {
                issues.Add("Distributed cache read/write test failed");
            }

            // Performance thresholds
            if (memoryCacheStopwatch.ElapsedMilliseconds > 100)
            {
                issues.Add($"Memory cache slow: {memoryCacheStopwatch.ElapsedMilliseconds}ms");
            }

            if (distributedCacheStopwatch.ElapsedMilliseconds > 1000)
            {
                issues.Add($"Distributed cache slow: {distributedCacheStopwatch.ElapsedMilliseconds}ms");
            }

            // Cleanup
            _memoryCache.Remove(testKey);
            await _distributedCache.RemoveAsync(distributedTestKey, cancellationToken);

            if (issues.Any())
            {
                return HealthCheckResult.Degraded(
                    $"Cache performance issues: {string.Join(", ", issues)}",
                    data: data);
            }

            return HealthCheckResult.Healthy("Cache systems are performing well", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache health check failed");
            return HealthCheckResult.Unhealthy("Cache health check failed", ex, data);
        }
    }
}

// Health Check UI Configuration
public static class HealthCheckUIConfiguration
{
    public static void ConfigureHealthCheckUI(WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // No checks, just confirms app is running
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Detailed health report
        app.MapHealthChecks("/health/detailed", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedHealthResponse
        });
    }

    private static async Task WriteDetailedHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            results = report.Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    status = kvp.Value.Status.ToString(),
                    duration = kvp.Value.Duration.TotalMilliseconds,
                    description = kvp.Value.Description,
                    data = kvp.Value.Data,
                    exception = kvp.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
}

// Supporting interfaces and classes
public interface IConnectionMonitor
{
    int GetActiveConnectionCount();
}

public interface IOrderService
{
    Task<int> GetPendingOrderCountAsync(CancellationToken cancellationToken = default);
    Task<double> GetRecentThroughputAsync(TimeSpan timeWindow, CancellationToken cancellationToken = default);
}

public interface IInventoryService
{
    Task<List<string>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
}

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}
```

## Structured Logging Best Practices

**Mejores prácticas para structured logging en .NET con Serilog y análisis de logs.**
Esta sección demuestra configuración avanzada de logging, enrichers y structured data.
Fundamental para debugging efectivo y analysis de comportamiento en aplicaciones distribuidas.

| **Aspecto**         | **Buena Práctica**           | **Ejemplo**                                          | **Beneficio**       |
| ------------------- | ---------------------------- | ---------------------------------------------------- | ------------------- |
| **Log Levels**      | Usar niveles apropiados      | Debug, Info, Warning, Error, Critical                | Filtrado efectivo   |
| **Structured Data** | Usar parámetros nombrados    | `Log.Information("User {UserId} logged in", userId)` | Queryable logs      |
| **Correlation IDs** | Tracear requests             | Correlation ID en todos los logs                     | Request tracking    |
| **Sensitive Data**  | Filtrar información sensible | Redactar passwords, tokens                           | Security compliance |
| **Performance**     | Async logging                | Usar async appenders                                 | No bloquear app     |
| **Context**         | Agregar contexto relevante   | User, session, operation info                        | Rich debugging info |

```csharp
// Comprehensive Serilog configuration
public static class SerilogConfiguration
{
    public static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.With<CorrelationIdEnricher>()
                .Enrich.With<UserContextEnricher>()
                .Filter.ByExcluding(IsHealthCheckLog)
                .Destructure.UsingAttributes()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.File(
                    new JsonFormatter(),
                    path: "logs/app-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.ApplicationInsights(
                    builder.Configuration.GetConnectionString("ApplicationInsights"),
                    TelemetryConverter.Traces)
                .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq"));
        });
    }

    private static bool IsHealthCheckLog(LogEvent logEvent)
    {
        return logEvent.Properties.ContainsKey("RequestPath") &&
               logEvent.Properties["RequestPath"].ToString().Contains("/health");
    }
}

// Custom enrichers
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = Activity.Current?.Id ??
                           HttpContext.Current?.Request?.Headers["X-Correlation-ID"]?.FirstOrDefault() ??
                           Guid.NewGuid().ToString();

        var property = propertyFactory.CreateProperty(CorrelationIdPropertyName, correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }
}

public class UserContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserEmail", userEmail));
            }
        }
    }
}

// Service with comprehensive structured logging
public class StructuredLoggingService
{
    private readonly ILogger<StructuredLoggingService> _logger;
    private readonly IUserRepository _userRepository;

    public StructuredLoggingService(
        ILogger<StructuredLoggingService> logger,
        IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<User>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var operationId = Guid.NewGuid();

        // Use structured logging with proper context
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "CreateUser",
            ["OperationId"] = operationId,
            ["UserEmail"] = request.Email,
            ["RequestedRole"] = request.Role
        });

        _logger.LogInformation("Starting user creation for {UserEmail} with role {Role}",
            request.Email, request.Role);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validate input
            var validationResult = ValidateCreateUserRequest(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("User creation failed validation for {UserEmail}: {ValidationErrors}",
                    request.Email, validationResult.Errors);

                return ServiceResult<User>.Failure(validationResult.Errors);
            }

            // Check for existing user
            var existingUser = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempted to create duplicate user {UserEmail} (existing user ID: {ExistingUserId})",
                    request.Email, existingUser.Id);

                return ServiceResult<User>.Failure(new[] { "User already exists" });
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user, cancellationToken);

            stopwatch.Stop();

            // Log success with metrics
            _logger.LogInformation("Successfully created user {UserId} with email {UserEmail} in {ElapsedMs}ms",
                user.Id, user.Email, stopwatch.ElapsedMilliseconds);

            // Log business metrics
            _logger.LogInformation("User creation metrics: {@UserMetrics}", new
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreationDurationMs = stopwatch.ElapsedMilliseconds,
                OperationId = operationId
            });

            return ServiceResult<User>.Success(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Structured exception logging
            _logger.LogError(ex, "User creation failed for {UserEmail} after {ElapsedMs}ms: {ErrorMessage}",
                request.Email, stopwatch.ElapsedMilliseconds, ex.Message);

            // Log additional context for troubleshooting
            _logger.LogError("User creation error context: {@ErrorContext}", new
            {
                RequestEmail = request.Email,
                RequestRole = request.Role,
                OperationId = operationId,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace?.Split('\n').Take(3) // First 3 lines of stack trace
            });

            throw;
        }
    }

    public async Task<ServiceResult<User>> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "UpdateUser",
            ["UserId"] = userId,
            ["UpdateFields"] = request.GetUpdatedFields()
        });

        _logger.LogInformation("Starting user update for {UserId} with fields: {UpdateFields}",
            userId, string.Join(", ", request.GetUpdatedFields()));

        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Attempted to update non-existent user {UserId}", userId);
                return ServiceResult<User>.Failure(new[] { "User not found" });
            }

            // Log before state for audit trail
            _logger.LogInformation("User {UserId} before update: {@UserBefore}", userId, new
            {
                user.Email,
                user.Role,
                user.LastModifiedAt
            });

            // Apply updates
            var hasChanges = false;

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                _logger.LogInformation("Updating email for user {UserId} from {OldEmail} to {NewEmail}",
                    userId, user.Email, request.Email);
                user.Email = request.Email;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(request.Role) && request.Role != user.Role)
            {
                _logger.LogInformation("Updating role for user {UserId} from {OldRole} to {NewRole}",
                    userId, user.Role, request.Role);
                user.Role = request.Role;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for user {UserId} update", userId);
                return ServiceResult<User>.Success(user);
            }

            user.LastModifiedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Log after state for audit trail
            _logger.LogInformation("User {UserId} after update: {@UserAfter}", userId, new
            {
                user.Email,
                user.Role,
                user.LastModifiedAt
            });

            _logger.LogInformation("Successfully updated user {UserId}", userId);

            return ServiceResult<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}: {ErrorMessage}", userId, ex.Message);
            throw;
        }
    }

    private ValidationResult ValidateCreateUserRequest(CreateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required");
        }
        else if (!IsValidEmail(request.Email))
        {
            errors.Add("Invalid email format");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            errors.Add("Role is required");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

// Helper classes for structured logging
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public static ServiceResult<T> Success(T data) => new() { Success = true, Data = data };
    public static ServiceResult<T> Failure(IEnumerable<string> errors) => new() { Success = false, Errors = errors };
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Role { get; set; }

    public IEnumerable<string> GetUpdatedFields()
    {
        var fields = new List<string>();
        if (!string.IsNullOrEmpty(Email)) fields.Add("Email");
        if (!string.IsNullOrEmpty(Role)) fields.Add("Role");
        return fields;
    }
}
```
