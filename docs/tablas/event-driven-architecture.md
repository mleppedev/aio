# Event-Driven Architecture

**Arquitectura orientada a eventos en .NET con patrones de messaging, Event Sourcing y CQRS.**
Este documento cubre implementación de message brokers, event handlers, sagas y arquitectura hexagonal.
Fundamental para construir sistemas distribuidos resilientes y desacoplados con alta escalabilidad.

## Event-Driven Patterns

**Patrones fundamentales de arquitectura orientada a eventos en aplicaciones .NET.**
Esta tabla compara diferentes patrones de messaging con sus características y casos de uso.
Esencial para diseñar sistemas reactivos que respondan eficientemente a cambios de estado.

| **Patrón**         | **Propósito**                          | **Casos de Uso**            | **Ventajas**               | **Desafíos**             |
| ------------------ | -------------------------------------- | --------------------------- | -------------------------- | ------------------------ |
| **Event Sourcing** | Persistir eventos como source of truth | Auditoría, temporal queries | Historial completo, replay | Complejidad, snapshots   |
| **CQRS**           | Separar reads y writes                 | Alta lectura vs escritura   | Optimización independiente | Eventual consistency     |
| **Saga Pattern**   | Transacciones distribuidas             | Workflows complejos         | Resilience, compensation   | Orchestration complexity |
| **Pub/Sub**        | Comunicación asíncrona                 | Notificaciones, integration | Desacoplamiento            | Message ordering         |
| **Event Store**    | Base de datos de eventos               | Event sourcing storage      | Atomic writes, projections | Query complexity         |
| **Outbox Pattern** | Transactional messaging                | Garantías de entrega        | Exactly-once delivery      | Implementation overhead  |

## Event Sourcing Implementation

**Implementación completa de Event Sourcing con aggregate roots, eventos y projections.**
Esta sección demuestra store de eventos, snapshots y reconstrucción de aggregates.
Fundamental para sistemas que requieren auditoría completa y capacidad de replay.

```csharp
// Base event and aggregate infrastructure
public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public int Version { get; set; }
}

public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _uncommittedEvents = new();

    public Guid Id { get; protected set; }
    public int Version { get; protected set; }

    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected void ApplyEvent(DomainEvent @event)
    {
        @event.Version = Version + 1;
        Apply(@event);
        _uncommittedEvents.Add(@event);
        Version++;
    }

    public void LoadFromHistory(IEnumerable<DomainEvent> events)
    {
        foreach (var @event in events)
        {
            Apply(@event);
            Version = @event.Version;
        }
    }

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    protected abstract void Apply(DomainEvent @event);
}

// Order aggregate with event sourcing
public class Order : AggregateRoot
{
    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public decimal TotalAmount { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public Order() { } // For reconstruction

    public Order(Guid orderId, string customerId, List<OrderItem> items)
    {
        ApplyEvent(new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            Items = items,
            TotalAmount = items.Sum(i => i.Price * i.Quantity)
        });
    }

    public void ConfirmPayment(string paymentId, decimal paidAmount)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm payment for order in {Status} status");

        if (paidAmount != TotalAmount)
            throw new InvalidOperationException($"Payment amount {paidAmount} does not match order total {TotalAmount}");

        ApplyEvent(new OrderPaymentConfirmedEvent
        {
            OrderId = Id,
            PaymentId = paymentId,
            PaidAmount = paidAmount
        });
    }

    public void Ship(string trackingNumber, string carrier)
    {
        if (Status != OrderStatus.PaymentConfirmed)
            throw new InvalidOperationException($"Cannot ship order in {Status} status");

        ApplyEvent(new OrderShippedEvent
        {
            OrderId = Id,
            TrackingNumber = trackingNumber,
            Carrier = carrier,
            ShippedAt = DateTime.UtcNow
        });
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver order in {Status} status");

        ApplyEvent(new OrderDeliveredEvent
        {
            OrderId = Id,
            DeliveredAt = DateTime.UtcNow
        });
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered order");

        ApplyEvent(new OrderCancelledEvent
        {
            OrderId = Id,
            CancellationReason = reason,
            CancelledAt = DateTime.UtcNow
        });
    }

    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent created:
                Id = created.OrderId;
                CustomerId = created.CustomerId;
                Items = created.Items;
                TotalAmount = created.TotalAmount;
                Status = OrderStatus.Pending;
                break;

            case OrderPaymentConfirmedEvent paymentConfirmed:
                Status = OrderStatus.PaymentConfirmed;
                break;

            case OrderShippedEvent shipped:
                Status = OrderStatus.Shipped;
                ShippedAt = shipped.ShippedAt;
                break;

            case OrderDeliveredEvent delivered:
                Status = OrderStatus.Delivered;
                DeliveredAt = delivered.DeliveredAt;
                break;

            case OrderCancelledEvent cancelled:
                Status = OrderStatus.Cancelled;
                break;
        }
    }
}

// Domain events
public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class OrderPaymentConfirmedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
}

public class OrderShippedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
}

public class OrderDeliveredEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public DateTime DeliveredAt { get; set; }
}

public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}

// Event store implementation
public interface IEventStore
{
    Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId, int fromVersion = 0);
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, int expectedVersion);
    Task<T?> GetAggregateAsync<T>(Guid aggregateId) where T : AggregateRoot, new();
    Task SaveAggregateAsync<T>(T aggregate) where T : AggregateRoot;
}

public class SqlEventStore : IEventStore
{
    private readonly string _connectionString;
    private readonly IEventSerializer _serializer;
    private readonly ILogger<SqlEventStore> _logger;

    public SqlEventStore(string connectionString, IEventSerializer serializer, ILogger<SqlEventStore> logger)
    {
        _connectionString = connectionString;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId, int fromVersion = 0)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT EventType, EventData, Version, OccurredAt
            FROM Events
            WHERE AggregateId = @AggregateId AND Version > @FromVersion
            ORDER BY Version";

        var eventData = await connection.QueryAsync(sql, new { AggregateId = aggregateId, FromVersion = fromVersion });

        var events = new List<DomainEvent>();
        foreach (var row in eventData)
        {
            var @event = _serializer.Deserialize(row.EventType, row.EventData);
            @event.Version = row.Version;
            events.Add(@event);
        }

        _logger.LogDebug("Retrieved {EventCount} events for aggregate {AggregateId} from version {FromVersion}",
            events.Count, aggregateId, fromVersion);

        return events;
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, int expectedVersion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check concurrency
            const string versionCheckSql = "SELECT ISNULL(MAX(Version), 0) FROM Events WHERE AggregateId = @AggregateId";
            var currentVersion = await connection.QuerySingleAsync<int>(versionCheckSql,
                new { AggregateId = aggregateId }, transaction);

            if (currentVersion != expectedVersion)
            {
                throw new ConcurrencyException(
                    $"Expected version {expectedVersion} but found {currentVersion} for aggregate {aggregateId}");
            }

            // Save events
            const string insertSql = @"
                INSERT INTO Events (Id, AggregateId, EventType, EventData, Version, OccurredAt)
                VALUES (@Id, @AggregateId, @EventType, @EventData, @Version, @OccurredAt)";

            foreach (var @event in events)
            {
                var eventData = _serializer.Serialize(@event);

                await connection.ExecuteAsync(insertSql, new
                {
                    @event.Id,
                    AggregateId = aggregateId,
                    @event.EventType,
                    EventData = eventData,
                    @event.Version,
                    @event.OccurredAt
                }, transaction);
            }

            await transaction.CommitAsync();

            _logger.LogDebug("Saved {EventCount} events for aggregate {AggregateId} at version {Version}",
                events.Count(), aggregateId, events.Max(e => e.Version));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<T?> GetAggregateAsync<T>(Guid aggregateId) where T : AggregateRoot, new()
    {
        var events = await GetEventsAsync(aggregateId);

        if (!events.Any())
            return null;

        var aggregate = new T();
        aggregate.LoadFromHistory(events);

        return aggregate;
    }

    public async Task SaveAggregateAsync<T>(T aggregate) where T : AggregateRoot
    {
        var events = aggregate.UncommittedEvents;

        if (!events.Any())
            return;

        await SaveEventsAsync(aggregate.Id, events, aggregate.Version - events.Count);
        aggregate.MarkEventsAsCommitted();
    }
}

// Event serializer
public interface IEventSerializer
{
    string Serialize(DomainEvent @event);
    DomainEvent Deserialize(string eventType, string eventData);
}

public class JsonEventSerializer : IEventSerializer
{
    private readonly Dictionary<string, Type> _eventTypes;
    private readonly JsonSerializerOptions _options;

    public JsonEventSerializer()
    {
        _eventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(DomainEvent)))
            .ToDictionary(t => t.Name, t => t);

        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public string Serialize(DomainEvent @event)
    {
        return JsonSerializer.Serialize(@event, @event.GetType(), _options);
    }

    public DomainEvent Deserialize(string eventType, string eventData)
    {
        if (!_eventTypes.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"Unknown event type: {eventType}");

        return (DomainEvent)JsonSerializer.Deserialize(eventData, type, _options)!;
    }
}

// Supporting types
public enum OrderStatus
{
    Pending,
    PaymentConfirmed,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
```

## CQRS with MediatR

**Implementación de CQRS usando MediatR para separar commands y queries.**
Esta sección demuestra handlers, pipelines de comportamiento y validación.
Fundamental para optimizar lecturas y escrituras independientemente con modelos especializados.

```csharp
// CQRS commands and queries
public record CreateOrderCommand(
    string CustomerId,
    List<OrderItem> Items) : IRequest<CreateOrderResponse>;

public record ConfirmOrderPaymentCommand(
    Guid OrderId,
    string PaymentId,
    decimal PaidAmount) : IRequest<bool>;

public record GetOrderQuery(Guid OrderId) : IRequest<OrderQueryResponse?>;

public record GetOrdersByCustomerQuery(
    string CustomerId,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<OrderSummary>>;

// Command handlers
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IEventStore _eventStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IEventStore eventStore,
        IEventPublisher eventPublisher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _eventStore = eventStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for customer {CustomerId} with {ItemCount} items",
            request.CustomerId, request.Items.Count);

        try
        {
            var orderId = Guid.NewGuid();
            var order = new Order(orderId, request.CustomerId, request.Items);

            await _eventStore.SaveAggregateAsync(order);

            // Publish events for external integrations
            foreach (var @event in order.UncommittedEvents)
            {
                await _eventPublisher.PublishAsync(@event, cancellationToken);
            }

            _logger.LogInformation("Successfully created order {OrderId} for customer {CustomerId}",
                orderId, request.CustomerId);

            return new CreateOrderResponse
            {
                OrderId = orderId,
                Status = OrderStatus.Pending,
                TotalAmount = order.TotalAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}

public class ConfirmOrderPaymentCommandHandler : IRequestHandler<ConfirmOrderPaymentCommand, bool>
{
    private readonly IEventStore _eventStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ConfirmOrderPaymentCommandHandler> _logger;

    public ConfirmOrderPaymentCommandHandler(
        IEventStore eventStore,
        IEventPublisher eventPublisher,
        ILogger<ConfirmOrderPaymentCommandHandler> logger)
    {
        _eventStore = eventStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirming payment for order {OrderId} with payment {PaymentId}",
            request.OrderId, request.PaymentId);

        try
        {
            var order = await _eventStore.GetAggregateAsync<Order>(request.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for payment confirmation", request.OrderId);
                return false;
            }

            order.ConfirmPayment(request.PaymentId, request.PaidAmount);

            await _eventStore.SaveAggregateAsync(order);

            // Publish events
            foreach (var @event in order.UncommittedEvents)
            {
                await _eventPublisher.PublishAsync(@event, cancellationToken);
            }

            _logger.LogInformation("Successfully confirmed payment for order {OrderId}", request.OrderId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm payment for order {OrderId}", request.OrderId);
            throw;
        }
    }
}

// Query handlers with read models
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderQueryResponse?>
{
    private readonly IOrderReadModelRepository _readModelRepository;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(
        IOrderReadModelRepository readModelRepository,
        ILogger<GetOrderQueryHandler> logger)
    {
        _readModelRepository = readModelRepository;
        _logger = logger;
    }

    public async Task<OrderQueryResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving order {OrderId}", request.OrderId);

        var order = await _readModelRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogInformation("Order {OrderId} not found", request.OrderId);
            return null;
        }

        return new OrderQueryResponse
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt
        };
    }
}

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, PagedResult<OrderSummary>>
{
    private readonly IOrderReadModelRepository _readModelRepository;
    private readonly ILogger<GetOrdersByCustomerQueryHandler> _logger;

    public GetOrdersByCustomerQueryHandler(
        IOrderReadModelRepository readModelRepository,
        ILogger<GetOrdersByCustomerQueryHandler> logger)
    {
        _readModelRepository = readModelRepository;
        _logger = logger;
    }

    public async Task<PagedResult<OrderSummary>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving orders for customer {CustomerId} (page {Page}, size {PageSize})",
            request.CustomerId, request.Page, request.PageSize);

        var (orders, totalCount) = await _readModelRepository.GetByCustomerAsync(
            request.CustomerId, request.Page, request.PageSize, cancellationToken);

        var orderSummaries = orders.Select(o => new OrderSummary
        {
            OrderId = o.Id,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt
        }).ToList();

        return new PagedResult<OrderSummary>
        {
            Items = orderSummaries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }
}

// Pipeline behaviors for cross-cutting concerns
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning("Validation failed for {RequestType}: {ValidationErrors}",
                typeof(TRequest).Name, string.Join(", ", failures.Select(f => f.ErrorMessage)));

            throw new ValidationException(failures);
        }

        return await next();
    }
}

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);

        try
        {
            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

// Response types
public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderQueryResponse
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class OrderItemResponse
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderSummary
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// Read model repository
public interface IOrderReadModelRepository
{
    Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<(List<OrderReadModel> Orders, int TotalCount)> GetByCustomerAsync(
        string customerId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public class OrderReadModel
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
```

## Saga Pattern Implementation

**Implementación del patrón Saga para orquestar transacciones distribuidas.**
Esta sección demuestra compensation logic, workflow management y failure handling.
Fundamental para mantener consistencia en operaciones que abarcan múltiples servicios.

```csharp
// Saga orchestrator interface and implementation
public interface ISagaOrchestrator<T> where T : Saga
{
    Task StartAsync(T saga, CancellationToken cancellationToken = default);
    Task HandleAsync(Guid sagaId, DomainEvent @event, CancellationToken cancellationToken = default);
    Task CompensateAsync(Guid sagaId, string reason, CancellationToken cancellationToken = default);
}

public abstract class Saga
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SagaStatus Status { get; set; } = SagaStatus.NotStarted;
    public Dictionary<string, object> Data { get; set; } = new();
    public List<SagaStep> Steps { get; set; } = new();
    public List<string> CompletedSteps { get; set; } = new();
    public List<string> CompensatedSteps { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public abstract void DefineSteps();
    public abstract Task<bool> CanStartAsync();

    protected void AddStep(string name, Func<CancellationToken, Task> action, Func<CancellationToken, Task>? compensation = null)
    {
        Steps.Add(new SagaStep
        {
            Name = name,
            Action = action,
            Compensation = compensation
        });
    }

    public SagaStep? GetNextStep()
    {
        return Steps.FirstOrDefault(s => !CompletedSteps.Contains(s.Name));
    }

    public void MarkStepCompleted(string stepName)
    {
        if (!CompletedSteps.Contains(stepName))
        {
            CompletedSteps.Add(stepName);
        }
    }

    public void MarkStepCompensated(string stepName)
    {
        if (!CompensatedSteps.Contains(stepName))
        {
            CompensatedSteps.Add(stepName);
        }
    }

    public bool IsCompleted => CompletedSteps.Count == Steps.Count;

    public List<SagaStep> GetStepsToCompensate()
    {
        return Steps
            .Where(s => CompletedSteps.Contains(s.Name) &&
                       !CompensatedSteps.Contains(s.Name) &&
                       s.Compensation != null)
            .Reverse()
            .ToList();
    }
}

// Order processing saga
public class OrderProcessingSaga : Saga
{
    public string CustomerId => Data.GetValueOrDefault("CustomerId")?.ToString() ?? string.Empty;
    public Guid OrderId => Guid.Parse(Data.GetValueOrDefault("OrderId")?.ToString() ?? Guid.Empty.ToString());
    public decimal TotalAmount => decimal.Parse(Data.GetValueOrDefault("TotalAmount")?.ToString() ?? "0");
    public string PaymentId => Data.GetValueOrDefault("PaymentId")?.ToString() ?? string.Empty;
    public string ReservationId => Data.GetValueOrDefault("ReservationId")?.ToString() ?? string.Empty;

    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderProcessingSaga> _logger;

    public OrderProcessingSaga(
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IShippingService shippingService,
        INotificationService notificationService,
        ILogger<OrderProcessingSaga> logger)
    {
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _shippingService = shippingService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public override void DefineSteps()
    {
        AddStep("ReserveInventory", ReserveInventoryAsync, CompensateInventoryReservationAsync);
        AddStep("ProcessPayment", ProcessPaymentAsync, CompensatePaymentAsync);
        AddStep("CreateShipment", CreateShipmentAsync, CompensateShipmentAsync);
        AddStep("SendConfirmation", SendConfirmationAsync);
    }

    public override async Task<bool> CanStartAsync()
    {
        // Validate that order exists and is in correct state
        var orderExists = await ValidateOrderExistsAsync();
        var customerValid = await ValidateCustomerAsync();

        return orderExists && customerValid;
    }

    private async Task ReserveInventoryAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reserving inventory for order {OrderId}", OrderId);

        var reservationResult = await _inventoryService.ReserveAsync(OrderId, cancellationToken);

        if (!reservationResult.Success)
        {
            throw new SagaStepException($"Failed to reserve inventory: {reservationResult.ErrorMessage}");
        }

        Data["ReservationId"] = reservationResult.ReservationId;

        _logger.LogInformation("Successfully reserved inventory for order {OrderId} with reservation {ReservationId}",
            OrderId, reservationResult.ReservationId);
    }

    private async Task CompensateInventoryReservationAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(ReservationId))
            return;

        _logger.LogInformation("Compensating inventory reservation {ReservationId} for order {OrderId}",
            ReservationId, OrderId);

        await _inventoryService.CancelReservationAsync(ReservationId, cancellationToken);

        _logger.LogInformation("Successfully compensated inventory reservation {ReservationId}", ReservationId);
    }

    private async Task ProcessPaymentAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for order {OrderId} amount {TotalAmount}",
            OrderId, TotalAmount);

        var paymentResult = await _paymentService.ProcessPaymentAsync(new PaymentRequest
        {
            OrderId = OrderId.ToString(),
            CustomerId = CustomerId,
            Amount = TotalAmount
        }, cancellationToken);

        if (!paymentResult.Success)
        {
            throw new SagaStepException($"Payment failed: {paymentResult.ErrorMessage}");
        }

        Data["PaymentId"] = paymentResult.PaymentId;

        _logger.LogInformation("Successfully processed payment for order {OrderId} with payment {PaymentId}",
            OrderId, paymentResult.PaymentId);
    }

    private async Task CompensatePaymentAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(PaymentId))
            return;

        _logger.LogInformation("Compensating payment {PaymentId} for order {OrderId}", PaymentId, OrderId);

        await _paymentService.RefundAsync(PaymentId, TotalAmount, "Saga compensation", cancellationToken);

        _logger.LogInformation("Successfully compensated payment {PaymentId}", PaymentId);
    }

    private async Task CreateShipmentAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating shipment for order {OrderId}", OrderId);

        var shipmentResult = await _shippingService.CreateShipmentAsync(new CreateShipmentRequest
        {
            OrderId = OrderId,
            CustomerId = CustomerId
        }, cancellationToken);

        if (!shipmentResult.Success)
        {
            throw new SagaStepException($"Shipment creation failed: {shipmentResult.ErrorMessage}");
        }

        Data["ShipmentId"] = shipmentResult.ShipmentId;
        Data["TrackingNumber"] = shipmentResult.TrackingNumber;

        _logger.LogInformation("Successfully created shipment {ShipmentId} for order {OrderId}",
            shipmentResult.ShipmentId, OrderId);
    }

    private async Task CompensateShipmentAsync(CancellationToken cancellationToken)
    {
        var shipmentId = Data.GetValueOrDefault("ShipmentId")?.ToString();

        if (string.IsNullOrEmpty(shipmentId))
            return;

        _logger.LogInformation("Compensating shipment {ShipmentId} for order {OrderId}", shipmentId, OrderId);

        await _shippingService.CancelShipmentAsync(shipmentId, cancellationToken);

        _logger.LogInformation("Successfully compensated shipment {ShipmentId}", shipmentId);
    }

    private async Task SendConfirmationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending confirmation for order {OrderId}", OrderId);

        var trackingNumber = Data.GetValueOrDefault("TrackingNumber")?.ToString();

        await _notificationService.SendOrderConfirmationAsync(new OrderConfirmationRequest
        {
            OrderId = OrderId,
            CustomerId = CustomerId,
            PaymentId = PaymentId,
            TrackingNumber = trackingNumber
        }, cancellationToken);

        _logger.LogInformation("Successfully sent confirmation for order {OrderId}", OrderId);
    }

    private async Task<bool> ValidateOrderExistsAsync()
    {
        // Implementation would check if order exists in valid state
        await Task.Delay(10); // Simulate validation
        return true;
    }

    private async Task<bool> ValidateCustomerAsync()
    {
        // Implementation would validate customer exists and is active
        await Task.Delay(10); // Simulate validation
        return !string.IsNullOrEmpty(CustomerId);
    }
}

// Saga orchestrator implementation
public class SagaOrchestrator<T> : ISagaOrchestrator<T> where T : Saga
{
    private readonly ISagaRepository _sagaRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SagaOrchestrator<T>> _logger;

    public SagaOrchestrator(
        ISagaRepository sagaRepository,
        IServiceProvider serviceProvider,
        ILogger<SagaOrchestrator<T>> logger)
    {
        _sagaRepository = sagaRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(T saga, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting saga {SagaId} of type {SagaType}", saga.Id, typeof(T).Name);

        try
        {
            if (!await saga.CanStartAsync())
            {
                throw new InvalidOperationException("Saga cannot be started due to precondition failures");
            }

            saga.DefineSteps();
            saga.Status = SagaStatus.InProgress;

            await _sagaRepository.SaveAsync(saga, cancellationToken);

            await ExecuteNextStepAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start saga {SagaId}", saga.Id);
            saga.Status = SagaStatus.Failed;
            saga.FailureReason = ex.Message;
            await _sagaRepository.SaveAsync(saga, cancellationToken);
            throw;
        }
    }

    public async Task HandleAsync(Guid sagaId, DomainEvent @event, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetAsync<T>(sagaId, cancellationToken);

        if (saga == null)
        {
            _logger.LogWarning("Saga {SagaId} not found for event {EventType}", sagaId, @event.EventType);
            return;
        }

        if (saga.Status != SagaStatus.InProgress)
        {
            _logger.LogWarning("Received event {EventType} for saga {SagaId} in status {Status}",
                @event.EventType, sagaId, saga.Status);
            return;
        }

        try
        {
            // Process event and continue with next step
            await ExecuteNextStepAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle event {EventType} for saga {SagaId}", @event.EventType, sagaId);
            await CompensateAsync(sagaId, ex.Message, cancellationToken);
        }
    }

    public async Task CompensateAsync(Guid sagaId, string reason, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetAsync<T>(sagaId, cancellationToken);

        if (saga == null)
        {
            _logger.LogWarning("Saga {SagaId} not found for compensation", sagaId);
            return;
        }

        _logger.LogInformation("Starting compensation for saga {SagaId}: {Reason}", sagaId, reason);

        saga.Status = SagaStatus.Compensating;
        saga.FailureReason = reason;

        var stepsToCompensate = saga.GetStepsToCompensate();

        foreach (var step in stepsToCompensate)
        {
            try
            {
                _logger.LogInformation("Compensating step {StepName} for saga {SagaId}", step.Name, sagaId);

                if (step.Compensation != null)
                {
                    await step.Compensation(cancellationToken);
                    saga.MarkStepCompensated(step.Name);
                }

                _logger.LogInformation("Successfully compensated step {StepName} for saga {SagaId}",
                    step.Name, sagaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compensate step {StepName} for saga {SagaId}",
                    step.Name, sagaId);
                // Continue with other compensations
            }
        }

        saga.Status = SagaStatus.Compensated;
        saga.CompletedAt = DateTime.UtcNow;

        await _sagaRepository.SaveAsync(saga, cancellationToken);

        _logger.LogInformation("Completed compensation for saga {SagaId}", sagaId);
    }

    private async Task ExecuteNextStepAsync(T saga, CancellationToken cancellationToken)
    {
        var nextStep = saga.GetNextStep();

        if (nextStep == null)
        {
            // Saga completed successfully
            saga.Status = SagaStatus.Completed;
            saga.CompletedAt = DateTime.UtcNow;
            await _sagaRepository.SaveAsync(saga, cancellationToken);

            _logger.LogInformation("Saga {SagaId} completed successfully", saga.Id);
            return;
        }

        try
        {
            _logger.LogInformation("Executing step {StepName} for saga {SagaId}", nextStep.Name, saga.Id);

            await nextStep.Action(cancellationToken);
            saga.MarkStepCompleted(nextStep.Name);

            await _sagaRepository.SaveAsync(saga, cancellationToken);

            _logger.LogInformation("Successfully completed step {StepName} for saga {SagaId}",
                nextStep.Name, saga.Id);

            // Continue with next step
            await ExecuteNextStepAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Step {StepName} failed for saga {SagaId}", nextStep.Name, saga.Id);
            throw;
        }
    }
}

// Supporting types
public enum SagaStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed,
    Compensating,
    Compensated
}

public class SagaStep
{
    public string Name { get; set; } = string.Empty;
    public Func<CancellationToken, Task> Action { get; set; } = null!;
    public Func<CancellationToken, Task>? Compensation { get; set; }
}

public class SagaStepException : Exception
{
    public SagaStepException(string message) : base(message) { }
    public SagaStepException(string message, Exception innerException) : base(message, innerException) { }
}

// Repository and service interfaces
public interface ISagaRepository
{
    Task<T?> GetAsync<T>(Guid sagaId, CancellationToken cancellationToken = default) where T : Saga;
    Task SaveAsync<T>(T saga, CancellationToken cancellationToken = default) where T : Saga;
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent;
}

// Service interfaces for saga steps
public interface IInventoryService
{
    Task<InventoryReservationResult> ReserveAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task CancelReservationAsync(string reservationId, CancellationToken cancellationToken = default);
}

public interface IShippingService
{
    Task<CreateShipmentResult> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken cancellationToken = default);
    Task CancelShipmentAsync(string shipmentId, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendOrderConfirmationAsync(OrderConfirmationRequest request, CancellationToken cancellationToken = default);
}

// Result types
public class InventoryReservationResult
{
    public bool Success { get; set; }
    public string ReservationId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class CreateShipmentResult
{
    public bool Success { get; set; }
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class CreateShipmentRequest
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}

public class OrderConfirmationRequest
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
}
```
