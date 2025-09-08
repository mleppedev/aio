# Contexto y Propósito

## ¿Qué es?
Los sistemas distribuidos son arquitecturas donde múltiples nodos colaboran para ofrecer disponibilidad, escalabilidad y tolerancia a fallos. En .NET se implementan con microservicios, colas de mensajes, event sourcing, CQRS y patrones como Saga. Los trade-offs se analizan con el teorema CAP (Consistency, Availability, Partition tolerance).

## ¿Por qué?
Porque hoy ninguna aplicación de escala significativa vive en un solo servidor. En mi experiencia, diseñar sin considerar consistencia eventual o resiliencia ante particiones llevó a sistemas frágiles. Adoptar patrones distribuidos correctos permitió garantizar continuidad en banca y servicios municipales.

## ¿Para qué?
- **Asegurar disponibilidad global** con replicación geográfica.  
- **Manejar transacciones distribuidas** con Saga (orchestration y choreography).  
- **Separar lecturas y escrituras** con CQRS y proyecciones.  
- **Aplicar resiliencia** con circuit breakers, retries y colas de eventos.  

## Valor agregado desde la experiencia
- **Event Sourcing** permitió trazabilidad total en auditorías financieras.  
- Con **Saga orchestration** implementamos procesos de pago + inventario + despacho sin inconsistencias.  
- **Consistencia eventual con colas** resolvió problemas de latencia en aplicaciones municipales de emergencia.  
- Aplicar **circuit breakers con Polly** evitó cascadas de fallos en microservicios bancarios.  

# Distributed Systems for .NET

**Guía completa de sistemas distribuidos aplicados al desarrollo .NET con patrones, teoremas y estrategias de implementación.**
Este documento cubre desde conceptos fundamentales como CAP theorem hasta implementaciones prácticas con microservicios y event sourcing.
Crítico para arquitectos y senior developers que diseñan sistemas escalables, resilientes y de alta disponibilidad.

## CAP Theorem & Trade-offs

**Teorema CAP aplicado a sistemas distribuidos con ejemplos de decisiones arquitectónicas en .NET.**
Esta tabla explica las limitaciones fundamentales y cómo elegir el trade-off correcto según el contexto empresarial.
Esencial para tomar decisiones informadas sobre consistencia, disponibilidad y tolerancia a particiones.

| **Propiedad**           | **Definición**                                            | **Ejemplo .NET**                  | **Cuándo Priorizar**                  |
| ----------------------- | --------------------------------------------------------- | --------------------------------- | ------------------------------------- |
| **Consistency**         | Todos los nodos ven los mismos datos simultáneamente      | SQL Server Always On              | Transacciones financieras             |
| **Availability**        | Sistema responde siempre (aunque sea con datos obsoletos) | Redis Cluster, MongoDB ReplicaSet | E-commerce, redes sociales            |
| **Partition Tolerance** | Sistema funciona a pesar de fallos de red                 | Cassandra, EventStore             | Sistemas geográficamente distribuidos |

### CAP Combinations

| **Combinación**                     | **Características**                       | **Tecnologías .NET**   | **Casos de Uso**               |
| ----------------------------------- | ----------------------------------------- | ---------------------- | ------------------------------ |
| **CP (Consistency + Partition)**    | Sacrifica disponibilidad por consistencia | SQL Server, PostgreSQL | Banking, inventarios críticos  |
| **AP (Availability + Partition)**   | Sacrifica consistencia por disponibilidad | Cosmos DB, DynamoDB    | Contenido web, analytics       |
| **CA (Consistency + Availability)** | Solo funciona sin particiones de red      | Sistemas monolíticos   | Aplicaciones single-datacenter |

## Consistency Patterns

**Patrones de consistencia en sistemas distribuidos con implementaciones específicas para aplicaciones .NET.**
Esta sección cubre desde consistencia fuerte hasta eventual con trade-offs de rendimiento y complejidad.
Fundamental para elegir el modelo de consistencia apropiado según los requisitos del negocio.

### Strong Consistency

```csharp
// Patrón: Distributed Transaction con 2PC
public class OrderProcessingService
{
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly ITransactionCoordinator _coordinator;

    public async Task<Result> ProcessOrderAsync(Order order)
    {
        using var transaction = await _coordinator.BeginTransactionAsync();

        try
        {
            // Fase 1: Prepare
            var paymentPrepared = await _paymentService.PrepareChargeAsync(
                transaction.Id, order.Total);
            var inventoryReserved = await _inventoryService.ReserveItemsAsync(
                transaction.Id, order.Items);

            if (!paymentPrepared.Success || !inventoryReserved.Success)
            {
                await transaction.AbortAsync();
                return Result.Failed("Preparation failed");
            }

            // Fase 2: Commit
            await _paymentService.CommitChargeAsync(transaction.Id);
            await _inventoryService.CommitReservationAsync(transaction.Id);
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.AbortAsync();
            return Result.Failed(ex.Message);
        }
    }
}
```

### Eventual Consistency

```csharp
// Patrón: Event Sourcing con Eventual Consistency
public class EventDrivenOrderService
{
    private readonly IEventStore _eventStore;
    private readonly IEventBus _eventBus;

    public async Task ProcessOrderAsync(Order order)
    {
        // Persister evento inmediatamente
        var orderPlaced = new OrderPlacedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items,
            Total = order.Total,
            Timestamp = DateTime.UtcNow
        };

        await _eventStore.AppendAsync(order.Id, orderPlaced);

        // Publicar evento para procesamiento asíncrono
        await _eventBus.PublishAsync(orderPlaced);
    }
}

// Handler que maneja consistencia eventual
public class OrderPlacedEventHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IEventBus _eventBus;

    public async Task HandleAsync(OrderPlacedEvent @event)
    {
        try
        {
            // Procesar pago asincrónicamente
            var paymentResult = await _paymentService.ChargeAsync(
                @event.CustomerId, @event.Total);

            if (paymentResult.Success)
            {
                await _eventBus.PublishAsync(new PaymentProcessedEvent
                {
                    OrderId = @event.OrderId,
                    TransactionId = paymentResult.TransactionId
                });
            }
            else
            {
                await _eventBus.PublishAsync(new PaymentFailedEvent
                {
                    OrderId = @event.OrderId,
                    Reason = paymentResult.Error
                });
            }
        }
        catch (Exception ex)
        {
            // Implementar retry con backoff exponencial
            await _eventBus.PublishAsync(new PaymentRetryRequiredEvent
            {
                OrderId = @event.OrderId,
                AttemptNumber = 1,
                NextRetryAt = DateTime.UtcNow.AddMinutes(1)
            });
        }
    }
}
```

## Saga Pattern Implementation

**Implementación del patrón Saga para manejar transacciones distribuidas de larga duración.**
Esta sección muestra tanto choreography como orchestration saga con manejo de compensación.
Esencial para procesos de negocio complejos que abarcan múltiples servicios distribuidos.

### Orchestration Saga

```csharp
// Orchestrador central que coordina la saga
public class OrderProcessingSaga
{
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;
    private readonly ISagaStateStore _stateStore;

    public async Task ProcessAsync(OrderSagaState state)
    {
        try
        {
            switch (state.CurrentStep)
            {
                case SagaStep.PaymentProcessing:
                    await ProcessPaymentAsync(state);
                    break;

                case SagaStep.InventoryReservation:
                    await ReserveInventoryAsync(state);
                    break;

                case SagaStep.ShippingArrangement:
                    await ArrangeShippingAsync(state);
                    break;

                case SagaStep.Completed:
                    await CompleteSagaAsync(state);
                    break;
            }
        }
        catch (Exception ex)
        {
            await CompensateAsync(state, ex);
        }
    }

    private async Task ProcessPaymentAsync(OrderSagaState state)
    {
        var result = await _paymentService.ChargeAsync(
            state.Order.CustomerId, state.Order.Total);

        if (result.Success)
        {
            state.PaymentTransactionId = result.TransactionId;
            state.CurrentStep = SagaStep.InventoryReservation;
            await _stateStore.UpdateAsync(state);

            // Continuar con el siguiente paso
            await ProcessAsync(state);
        }
        else
        {
            throw new SagaException($"Payment failed: {result.Error}");
        }
    }

    private async Task CompensateAsync(OrderSagaState state, Exception error)
    {
        // Ejecutar compensaciones en orden inverso
        if (!string.IsNullOrEmpty(state.ShippingId))
        {
            await _shippingService.CancelShippingAsync(state.ShippingId);
        }

        if (state.InventoryReserved)
        {
            await _inventoryService.ReleaseReservationAsync(state.Order.Id);
        }

        if (!string.IsNullOrEmpty(state.PaymentTransactionId))
        {
            await _paymentService.RefundAsync(state.PaymentTransactionId);
        }

        state.Status = SagaStatus.Failed;
        state.ErrorMessage = error.Message;
        await _stateStore.UpdateAsync(state);
    }
}
```

### Choreography Saga

```csharp
// Saga basada en eventos sin orchestrador central
public class PaymentProcessedEventHandler : IEventHandler<PaymentProcessedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IEventBus _eventBus;

    public async Task HandleAsync(PaymentProcessedEvent @event)
    {
        try
        {
            await _inventoryService.ReserveItemsAsync(@event.OrderId, @event.Items);

            await _eventBus.PublishAsync(new InventoryReservedEvent
            {
                OrderId = @event.OrderId,
                Items = @event.Items,
                ReservationId = Guid.NewGuid()
            });
        }
        catch (Exception ex)
        {
            // Publicar evento de compensación
            await _eventBus.PublishAsync(new PaymentRefundRequiredEvent
            {
                OrderId = @event.OrderId,
                TransactionId = @event.TransactionId,
                Reason = "Inventory reservation failed"
            });
        }
    }
}
```

## Event Sourcing & CQRS

**Implementación de Event Sourcing y CQRS para sistemas distribuidos escalables.**
Esta sección cubre el almacenamiento de eventos, proyecciones y separación de comandos y consultas.
Fundamental para sistemas que requieren auditabilía completa y escalabilidad independiente de lectura/escritura.

### Event Store Implementation

```csharp
public interface IEventStore
{
    Task AppendAsync<T>(string aggregateId, IEnumerable<T> events) where T : IEvent;
    Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId, int fromVersion = 0);
    Task<T> GetAggregateAsync<T>(string aggregateId) where T : AggregateRoot, new();
}

public class SqlEventStore : IEventStore
{
    private readonly string _connectionString;
    private readonly IEventSerializer _serializer;

    public async Task AppendAsync<T>(string aggregateId, IEnumerable<T> events) where T : IEvent
    {
        using var connection = new SqlConnection(_connectionString);
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var @event in events)
            {
                var eventData = new EventData
                {
                    AggregateId = aggregateId,
                    EventType = @event.GetType().Name,
                    EventData = _serializer.Serialize(@event),
                    Version = @event.Version,
                    Timestamp = DateTime.UtcNow
                };

                await InsertEventAsync(connection, transaction, eventData);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<T> GetAggregateAsync<T>(string aggregateId) where T : AggregateRoot, new()
    {
        var events = await GetEventsAsync(aggregateId);
        var aggregate = new T();
        aggregate.LoadFromHistory(events);
        return aggregate;
    }
}
```

### CQRS with Projections

```csharp
// Command Side - Write Model
public class OrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IEventStore _eventStore;

    public async Task<Result> HandleAsync(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerId, command.Items);

        // Validaciones de negocio
        if (!order.IsValid())
            return Result.Failed("Invalid order");

        // Guardar eventos
        await _eventStore.AppendAsync(order.Id, order.UncommittedEvents);

        return Result.Success();
    }
}

// Query Side - Read Model
public class OrderProjectionHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IOrderReadRepository _readRepository;

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        var orderView = new OrderView
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            Items = @event.Items.Select(i => new OrderItemView
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList(),
            Total = @event.Total,
            Status = "Created",
            CreatedAt = @event.Timestamp
        };

        await _readRepository.InsertAsync(orderView);
    }
}

// Separate read repository optimized for queries
public class OrderReadRepository : IOrderReadRepository
{
    private readonly string _readConnectionString;

    public async Task<OrderView> GetByIdAsync(string orderId)
    {
        // Optimized for reading with denormalized data
        using var connection = new SqlConnection(_readConnectionString);
        return await connection.QuerySingleOrDefaultAsync<OrderView>(
            "SELECT * FROM OrderViews WHERE Id = @Id", new { Id = orderId });
    }

    public async Task<IEnumerable<OrderView>> GetByCustomerAsync(string customerId)
    {
        using var connection = new SqlConnection(_readConnectionString);
        return await connection.QueryAsync<OrderView>(
            "SELECT * FROM OrderViews WHERE CustomerId = @CustomerId ORDER BY CreatedAt DESC",
            new { CustomerId = customerId });
    }
}
```

## Distributed Communication Patterns

**Patrones de comunicación entre servicios distribuidos con implementaciones en .NET.**
Esta sección cubre comunicación síncrona y asíncrona con patrones de resilencia.
Esencial para diseñar interacciones confiables entre microservicios en producción.

### Synchronous Communication

```csharp
// Circuit Breaker Pattern con Polly
public class PaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public PaymentServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {timespan}");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker closed");
                });
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.PostAsJsonAsync("/payments", request);
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
                return result;
            }

            return PaymentResult.Failed($"HTTP {response.StatusCode}");
        }
        catch (CircuitBreakerOpenException)
        {
            return PaymentResult.Failed("Payment service temporarily unavailable");
        }
        catch (Exception ex)
        {
            return PaymentResult.Failed($"Payment failed: {ex.Message}");
        }
    }
}
```

### Asynchronous Communication

```csharp
// Message Bus Implementation
public class ServiceBusMessagePublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusMessagePublisher> _logger;

    public async Task PublishAsync<T>(T message, string topic) where T : class
    {
        var sender = _client.CreateSender(topic);

        try
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message))
            {
                ContentType = "application/json",
                Subject = typeof(T).Name,
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Message published to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
            throw;
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }
}

// Message Handler with Dead Letter Queue
public class OrderEventProcessor : ServiceBusProcessor
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderEventProcessor> _logger;

    protected override async Task ProcessMessageAsync(ServiceBusReceivedMessage message)
    {
        try
        {
            var eventType = message.Subject;
            var eventData = message.Body.ToString();

            switch (eventType)
            {
                case nameof(PaymentProcessedEvent):
                    var paymentEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(eventData);
                    await _orderService.HandlePaymentProcessedAsync(paymentEvent);
                    break;

                default:
                    _logger.LogWarning("Unknown event type: {EventType}", eventType);
                    break;
            }

            await CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);

            if (message.DeliveryCount >= 3)
            {
                // Send to dead letter queue after 3 attempts
                await DeadLetterMessageAsync(message, "Max delivery count exceeded");
            }
            else
            {
                // Allow retry
                await AbandonMessageAsync(message);
            }
        }
    }
}
```

## Data Partitioning Strategies

**Estrategias de particionamiento de datos para escalar sistemas distribuidos.**
Esta sección cubre sharding horizontal, particionamiento vertical y estrategias híbridas.
Fundamental para manejar grandes volúmenes de datos distribuidos geográficamente.

### Horizontal Partitioning (Sharding)

| **Estrategia**      | **Ventajas**                           | **Desventajas**               | **Caso de Uso**            |
| ------------------- | -------------------------------------- | ----------------------------- | -------------------------- |
| **Range-based**     | Simple, consultas por rango eficientes | Hot spots, rebalanceo difícil | Datos temporales, logs     |
| **Hash-based**      | Distribución uniforme                  | Consultas por rango complejas | Datos de usuario, sesiones |
| **Directory-based** | Flexibilidad máxima                    | Punto único de falla          | Sistemas complejos         |
| **Composite**       | Combina ventajas                       | Mayor complejidad             | Aplicaciones empresariales |

### Implementation Example

```csharp
// Shard Router Implementation
public class ShardRouter
{
    private readonly Dictionary<string, IDataContext> _shards;
    private readonly IShardingStrategy _strategy;

    public ShardRouter(IShardingStrategy strategy, Dictionary<string, IDataContext> shards)
    {
        _strategy = strategy;
        _shards = shards;
    }

    public async Task<T> GetAsync<T>(string key) where T : class
    {
        var shardKey = _strategy.GetShardKey(key);
        var context = _shards[shardKey];
        return await context.GetAsync<T>(key);
    }

    public async Task SaveAsync<T>(string key, T entity) where T : class
    {
        var shardKey = _strategy.GetShardKey(key);
        var context = _shards[shardKey];
        await context.SaveAsync(key, entity);
    }
}

// Hash-based Sharding Strategy
public class HashShardingStrategy : IShardingStrategy
{
    private readonly string[] _shardKeys;

    public HashShardingStrategy(string[] shardKeys)
    {
        _shardKeys = shardKeys;
    }

    public string GetShardKey(string partitionKey)
    {
        var hash = partitionKey.GetHashCode();
        var index = Math.Abs(hash) % _shardKeys.Length;
        return _shardKeys[index];
    }
}
```

## Distributed System Challenges

**Desafíos comunes en sistemas distribuidos y sus soluciones con herramientas .NET.**
Esta tabla identifica problemas típicos y proporciona estrategias de mitigación probadas.
Esencial para anticipar y resolver problemas antes de que afecten la producción.

| **Desafío**            | **Problema**                       | **Solución .NET**                  | **Herramientas**          |
| ---------------------- | ---------------------------------- | ---------------------------------- | ------------------------- |
| **Network Latency**    | Comunicación lenta entre servicios | Connection pooling, async patterns | HttpClientFactory, gRPC   |
| **Split Brain**        | Múltiples líderes activos          | Consensus algorithms               | Service Fabric, Orleans   |
| **Clock Skew**         | Timestamps inconsistentes          | Vector clocks, logical timestamps  | NTP, Lamport timestamps   |
| **Cascade Failures**   | Un fallo causa fallos en cadena    | Circuit breakers, bulkheads        | Polly, rate limiting      |
| **Data Inconsistency** | Estados divergentes                | Eventual consistency, CRDT         | Event sourcing, Cosmos DB |
| **Service Discovery**  | Servicios no se encuentran         | Service mesh, registry             | Consul, Service Fabric    |
