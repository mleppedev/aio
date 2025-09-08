# Contexto y Propósito

## ¿Qué es?
Los principios SOLID son cinco guías de diseño orientado a objetos aplicables en .NET para escribir código mantenible y extensible:contentReference[oaicite:3]{index=3}. Incluyen SRP (Single Responsibility), OCP (Open/Closed), LSP (Liskov Substitution), ISP (Interface Segregation) y DIP (Dependency Inversion).

## ¿Por qué?
Porque proyectos grandes se vuelven inmanejables sin un diseño limpio. En mi experiencia, aplicar SOLID en banca y retail redujo deuda técnica y aceleró el onboarding de nuevos desarrolladores.

## ¿Para qué?
- **Mejorar mantenibilidad** separando responsabilidades.  
- **Facilitar extensibilidad** con código abierto a extensión pero cerrado a modificación.  
- **Asegurar polimorfismo confiable** evitando violaciones de sustitución.  
- **Reducir acoplamiento** con inversión de dependencias.  

## Valor agregado desde la experiencia
- Aplicar **SRP** en servicios de usuario redujo errores en validaciones y persistencia.  
- Con **OCP**, nuevos métodos de pago se agregaron sin tocar código existente.  
- **ISP** evitó interfaces “gordas” que generaban implementaciones innecesarias.  
- **DIP + IoC containers** mejoraron testabilidad y velocidad de despliegue en municipalidades.  

# SOLID Principles for .NET

**Guía completa de los principios SOLID aplicados específicamente al desarrollo .NET con ejemplos prácticos en C#.**
Este documento cubre cada principio con violaciones comunes, soluciones y patrones de implementación específicos para aplicaciones empresariales.
Fundamental para escribir código mantenible, extensible y que siga las mejores prácticas de ingeniería de software orientada a objetos.

## SOLID Overview

**Resumen de los cinco principios SOLID con su propósito fundamental y beneficios en el desarrollo .NET.**
Esta tabla proporciona una vista rápida de cada principio y su impacto en la arquitectura del software.
Esencial para entender cómo cada principio contribuye a crear código más limpio, testeable y mantenible.

| **Principio**             | **Acrónimo** | **Propósito**                                     | **Beneficio Principal**                |
| ------------------------- | ------------ | ------------------------------------------------- | -------------------------------------- |
| **Single Responsibility** | **S**        | Una clase, una responsabilidad                    | Cohesión alta, bajo acoplamiento       |
| **Open/Closed**           | **O**        | Abierto para extensión, cerrado para modificación | Código extensible sin romper existente |
| **Liskov Substitution**   | **L**        | Las subclases deben ser intercambiables           | Polimorfismo confiable                 |
| **Interface Segregation** | **I**        | Interfaces específicas, no gordas                 | Dependencias mínimas                   |
| **Dependency Inversion**  | **D**        | Depender de abstracciones, no implementaciones    | Desacoplamiento y testabilidad         |

## SOLID Benefits Summary

**Resumen de beneficios obtenidos al aplicar correctamente los principios SOLID en proyectos .NET.**
Esta tabla consolida las ventajas prácticas de seguir cada principio en el desarrollo empresarial.
Fundamental para justificar la inversión en arquitectura limpia y convencer a equipos de desarrollo.

| **Principio** | **Beneficios Técnicos**                          | **Beneficios de Negocio**              |
| ------------- | ------------------------------------------------ | -------------------------------------- |
| **SRP**       | Clases enfocadas, fácil debugging, alta cohesión | Desarrollo más rápido, menos bugs      |
| **OCP**       | Código extensible, sin regresiones               | Nuevas features sin riesgo             |
| **LSP**       | Polimorfismo confiable, menos casting            | Código predecible y robusto            |
| **ISP**       | Dependencias mínimas, interfaces limpias         | Testing más simple, menos acoplamiento |
| **DIP**       | Código testeable, arquitectura flexible          | Fácil mantenimiento, adaptabilidad     |

## Common SOLID Violations

**Violaciones comunes de los principios SOLID que se encuentran frecuentemente en proyectos .NET.**
Esta referencia ayuda a identificar y corregir anti-patrones antes de que se conviertan en deuda técnica.
Esencial para code reviews y refactoring de código legacy hacia arquitecturas más limpias.

| **Violación**                   | **Síntoma**                                        | **Principio Afectado** | **Solución Rápida**                |
| ------------------------------- | -------------------------------------------------- | ---------------------- | ---------------------------------- |
| **Clase God**                   | Clase con 500+ líneas, múltiples responsabilidades | SRP                    | Extraer clases por responsabilidad |
| **Switch/If gigante**           | Lógica condicional para tipos                      | OCP                    | Strategy pattern + Factory         |
| **Cast defensivo**              | `if (obj is ConcreteType)` frecuente               | LSP                    | Rediseñar jerarquía con interfaces |
| **Interface Fat**               | Interface con 10+ métodos no relacionados          | ISP                    | Dividir en interfaces específicas  |
| **new en constructor**          | Instanciación directa de dependencias              | DIP                    | Inyección de dependencias          |
| **Herencia para reutilización** | Herencia sin relación "es-un"                      | LSP                    | Composición sobre herencia         |
| **Singleton abusado**           | Singleton para evitar DI                           | DIP                    | Registro en contenedor DI          |

## Single Responsibility Principle (SRP)

**Principio de Responsabilidad Única: cada clase debe tener una sola razón para cambiar.**
Esta sección muestra violaciones típicas del SRP y cómo refactorizar hacia una responsabilidad única.
Crítico para mantener la cohesión alta y facilitar el mantenimiento de código en proyectos empresariales.

### ❌ Violación del SRP

```csharp
// MALO: UserService tiene múltiples responsabilidades
public class UserService
{
    public bool ValidateUser(User user)
    {
        // Validación de negocio
        if (string.IsNullOrEmpty(user.Email)) return false;
        if (user.Age < 18) return false;
        return true;
    }

    public void SaveUser(User user)
    {
        // Acceso a datos
        var connectionString = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
        using var connection = new SqlConnection(connectionString);
        // ... código de base de datos
    }

    public void SendWelcomeEmail(User user)
    {
        // Envío de emails
        var smtpClient = new SmtpClient("smtp.company.com");
        var message = new MailMessage("noreply@company.com", user.Email)
        {
            Subject = "Bienvenido",
            Body = $"Hola {user.Name}, bienvenido a nuestra plataforma"
        };
        smtpClient.Send(message);
    }
}
```

### ✅ Aplicando SRP Correctamente

```csharp
// BUENO: Cada clase tiene una sola responsabilidad

// Responsabilidad: Validación de reglas de negocio
public class UserValidator : IUserValidator
{
    public ValidationResult Validate(User user)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(user.Email))
            errors.Add("Email es requerido");

        if (user.Age < 18)
            errors.Add("Usuario debe ser mayor de edad");

        return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
    }
}

// Responsabilidad: Persistencia de datos
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task SaveAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        // ... implementación de guardado
    }
}

// Responsabilidad: Comunicaciones
public class EmailService : IEmailService
{
    private readonly IEmailConfiguration _config;

    public EmailService(IEmailConfiguration config)
    {
        _config = config;
    }

    public async Task SendWelcomeEmailAsync(User user)
    {
        var message = new EmailMessage
        {
            To = user.Email,
            Subject = "Bienvenido",
            Body = GenerateWelcomeTemplate(user)
        };

        await SendAsync(message);
    }

    private string GenerateWelcomeTemplate(User user) =>
        $"Hola {user.Name}, bienvenido a nuestra plataforma";
}

// Orquestador que coordina las responsabilidades
public class UserRegistrationService
{
    private readonly IUserValidator _validator;
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public UserRegistrationService(
        IUserValidator validator,
        IUserRepository repository,
        IEmailService emailService)
    {
        _validator = validator;
        _repository = repository;
        _emailService = emailService;
    }

    public async Task<RegistrationResult> RegisterUserAsync(User user)
    {
        var validationResult = _validator.Validate(user);
        if (!validationResult.IsValid)
            return RegistrationResult.Failed(validationResult.Errors);

        await _repository.SaveAsync(user);
        await _emailService.SendWelcomeEmailAsync(user);

        return RegistrationResult.Success();
    }
}
```

## Open/Closed Principle (OCP)

**Principio Abierto/Cerrado: las entidades de software deben estar abiertas para extensión pero cerradas para modificación.**
Esta sección demuestra cómo usar interfaces y polimorfismo para evitar modificar código existente.
Fundamental para crear sistemas extensibles que no requieren cambios en código ya probado y desplegado.

### ❌ Violación del OCP

```csharp
// MALO: Cada nuevo método de pago requiere modificar PaymentProcessor
public class PaymentProcessor
{
    public void ProcessPayment(decimal amount, PaymentType type)
    {
        switch (type)
        {
            case PaymentType.CreditCard:
                // Lógica de tarjeta de crédito
                Console.WriteLine($"Procesando {amount} con tarjeta de crédito");
                break;

            case PaymentType.PayPal:
                // Lógica de PayPal
                Console.WriteLine($"Procesando {amount} con PayPal");
                break;

            case PaymentType.Bitcoin:
                // ¡Nuevo caso! Hay que modificar el código existente
                Console.WriteLine($"Procesando {amount} con Bitcoin");
                break;

            default:
                throw new NotSupportedException($"Tipo de pago {type} no soportado");
        }
    }
}

public enum PaymentType
{
    CreditCard,
    PayPal,
    Bitcoin // Añadir nuevos tipos requiere cambios en múltiples lugares
}
```

### ✅ Aplicando OCP Correctamente

```csharp
// BUENO: Extensible sin modificar código existente

public interface IPaymentMethod
{
    string Name { get; }
    Task<PaymentResult> ProcessAsync(decimal amount, PaymentDetails details);
    bool CanProcess(PaymentDetails details);
}

// Implementaciones concretas (cerradas para modificación)
public class CreditCardPayment : IPaymentMethod
{
    public string Name => "Credit Card";

    public async Task<PaymentResult> ProcessAsync(decimal amount, PaymentDetails details)
    {
        // Validar número de tarjeta
        if (!IsValidCreditCard(details.CardNumber))
            return PaymentResult.Failed("Número de tarjeta inválido");

        // Procesar con gateway de tarjetas
        var result = await _creditCardGateway.ChargeAsync(amount, details);
        return result.IsSuccess
            ? PaymentResult.Success(result.TransactionId)
            : PaymentResult.Failed(result.Error);
    }

    public bool CanProcess(PaymentDetails details) =>
        !string.IsNullOrEmpty(details.CardNumber);

    private bool IsValidCreditCard(string cardNumber) =>
        // Algoritmo de Luhn
        cardNumber?.Length >= 13 && cardNumber.Length <= 19;
}

public class PayPalPayment : IPaymentMethod
{
    public string Name => "PayPal";

    public async Task<PaymentResult> ProcessAsync(decimal amount, PaymentDetails details)
    {
        // Redirigir a PayPal
        var paypalRequest = new PayPalRequest
        {
            Amount = amount,
            Currency = "USD",
            ReturnUrl = details.ReturnUrl,
            CancelUrl = details.CancelUrl
        };

        var result = await _paypalClient.CreatePaymentAsync(paypalRequest);
        return PaymentResult.Redirect(result.RedirectUrl);
    }

    public bool CanProcess(PaymentDetails details) =>
        !string.IsNullOrEmpty(details.Email);
}

// Nueva implementación SIN modificar código existente
public class BitcoinPayment : IPaymentMethod
{
    public string Name => "Bitcoin";

    public async Task<PaymentResult> ProcessAsync(decimal amount, PaymentDetails details)
    {
        var bitcoinAddress = await _bitcoinWallet.GenerateAddressAsync();
        var expectedAmount = await _exchangeService.ConvertToBitcoinAsync(amount);

        return PaymentResult.PendingConfirmation(bitcoinAddress, expectedAmount);
    }

    public bool CanProcess(PaymentDetails details) =>
        details.AcceptsCryptocurrency;
}

// Processor extensible (abierto para extensión)
public class PaymentProcessor
{
    private readonly IEnumerable<IPaymentMethod> _paymentMethods;

    public PaymentProcessor(IEnumerable<IPaymentMethod> paymentMethods)
    {
        _paymentMethods = paymentMethods;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentDetails details)
    {
        var method = _paymentMethods.FirstOrDefault(m => m.CanProcess(details));

        if (method == null)
            return PaymentResult.Failed("No hay método de pago disponible");

        return await method.ProcessAsync(amount, details);
    }

    public IEnumerable<string> GetAvailableMethods(PaymentDetails details) =>
        _paymentMethods.Where(m => m.CanProcess(details)).Select(m => m.Name);
}
```

## Liskov Substitution Principle (LSP)

**Principio de Sustitución de Liskov: las subclases deben ser intercambiables con sus clases base.**
Esta sección muestra violaciones del LSP y cómo diseñar jerarquías que mantienen el comportamiento esperado.
Esencial para crear polimorfismo confiable y evitar errores en tiempo de ejecución.

### ❌ Violación del LSP

```csharp
// MALO: Square viola LSP porque no se comporta como Rectangle
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }

    public int CalculateArea() => Width * Height;
}

public class Square : Rectangle
{
    public override int Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            base.Height = value; // ¡Modifica altura sin que el cliente lo sepa!
        }
    }

    public override int Height
    {
        get => base.Height;
        set
        {
            base.Height = value;
            base.Width = value; // ¡Modifica ancho sin que el cliente lo sepa!
        }
    }
}

// Este código falla con Square aunque funciona con Rectangle
public void TestRectangle(Rectangle rectangle)
{
    rectangle.Width = 5;
    rectangle.Height = 4;

    // Asume que width y height son independientes
    Assert.AreEqual(20, rectangle.CalculateArea()); // ¡Falla con Square!
}
```

### ✅ Aplicando LSP Correctamente

```csharp
// BUENO: Usar abstracciones que respeten el comportamiento

public interface IShape
{
    int CalculateArea();
    string GetDescription();
}

public class Rectangle : IShape
{
    public Rectangle(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public virtual int CalculateArea() => Width * Height;

    public virtual string GetDescription() => $"Rectangle {Width}x{Height}";
}

public class Square : IShape
{
    public Square(int side)
    {
        Side = side;
    }

    public int Side { get; }

    public int CalculateArea() => Side * Side;

    public string GetDescription() => $"Square {Side}x{Side}";
}

// Alternativa con factory pattern
public static class ShapeFactory
{
    public static IShape CreateRectangle(int width, int height)
    {
        if (width == height)
            return new Square(width);
        return new Rectangle(width, height);
    }
}

// Cliente que funciona con cualquier implementación de IShape
public class AreaCalculator
{
    public int CalculateTotalArea(IEnumerable<IShape> shapes) =>
        shapes.Sum(shape => shape.CalculateArea());

    public void PrintShapeInfo(IShape shape)
    {
        Console.WriteLine($"{shape.GetDescription()}: Area = {shape.CalculateArea()}");
    }
}
```

## Interface Segregation Principle (ISP)

**Principio de Segregación de Interfaces: los clientes no deben depender de interfaces que no usan.**
Esta sección demuestra cómo dividir interfaces grandes en interfaces más específicas y cohesivas.
Fundamental para minimizar dependencias y crear código más flexible y testeable.

### ❌ Violación del ISP

```csharp
// MALO: Interface monolítica que no todas las clases necesitan
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
    void Relax();
    void TakeMedicalLeave();
    void ReceivePayment();
}

// Robot no come ni duerme pero se ve forzado a implementar estos métodos
public class Robot : IWorker
{
    public void Work()
    {
        Console.WriteLine("Robot working...");
    }

    public void Eat()
    {
        throw new NotImplementedException("Robots don't eat"); // ¡Problema!
    }

    public void Sleep()
    {
        throw new NotImplementedException("Robots don't sleep"); // ¡Problema!
    }

    public void Relax()
    {
        throw new NotImplementedException("Robots don't relax"); // ¡Problema!
    }

    public void TakeMedicalLeave()
    {
        throw new NotImplementedException("Robots don't get sick"); // ¡Problema!
    }

    public void ReceivePayment()
    {
        // Los robots no reciben salario tradicional
        throw new NotImplementedException();
    }
}
```

### ✅ Aplicando ISP Correctamente

```csharp
// BUENO: Interfaces específicas y cohesivas

public interface IWorkable
{
    Task WorkAsync();
    bool IsAvailable { get; }
}

public interface IEater
{
    void Eat(Food food);
    bool IsHungry { get; }
}

public interface ISleeper
{
    void Sleep(TimeSpan duration);
    bool IsTired { get; }
}

public interface IEmployee : IWorkable
{
    string EmployeeId { get; }
    void ReceivePayment(decimal amount);
}

public interface IHuman : IEater, ISleeper
{
    void Relax();
    void TakeMedicalLeave(int days);
}

// Implementaciones específicas que solo implementan lo que necesitan
public class Robot : IWorkable
{
    public bool IsAvailable { get; private set; } = true;

    public async Task WorkAsync()
    {
        IsAvailable = false;
        Console.WriteLine("Robot processing tasks...");
        await ProcessTasksAsync();
        IsAvailable = true;
    }

    private async Task ProcessTasksAsync()
    {
        // Lógica específica del robot
        await Task.Delay(1000);
    }
}

public class HumanEmployee : IEmployee, IHuman
{
    public string EmployeeId { get; }
    public bool IsAvailable { get; private set; } = true;
    public bool IsHungry { get; private set; }
    public bool IsTired { get; private set; }

    public HumanEmployee(string employeeId)
    {
        EmployeeId = employeeId;
    }

    public async Task WorkAsync()
    {
        if (IsTired)
        {
            Console.WriteLine("Employee is too tired to work efficiently");
            return;
        }

        IsAvailable = false;
        Console.WriteLine($"Employee {EmployeeId} working...");
        await Task.Delay(2000);
        IsTired = true;
        IsAvailable = true;
    }

    public void Eat(Food food)
    {
        Console.WriteLine($"Employee eating {food.Name}");
        IsHungry = false;
    }

    public void Sleep(TimeSpan duration)
    {
        Console.WriteLine($"Employee sleeping for {duration.Hours} hours");
        IsTired = false;
    }

    public void Relax()
    {
        Console.WriteLine("Employee relaxing...");
        IsTired = false;
    }

    public void TakeMedicalLeave(int days)
    {
        Console.WriteLine($"Employee taking {days} days medical leave");
        IsAvailable = false;
    }

    public void ReceivePayment(decimal amount)
    {
        Console.WriteLine($"Employee received payment: ${amount}");
    }
}

// Cliente que usa solo las interfaces que necesita
public class WorkManager
{
    public async Task AssignWorkAsync(IEnumerable<IWorkable> workers)
    {
        var availableWorkers = workers.Where(w => w.IsAvailable);

        foreach (var worker in availableWorkers)
        {
            await worker.WorkAsync();
        }
    }
}

public class PayrollManager
{
    public void ProcessPayroll(IEnumerable<IEmployee> employees)
    {
        foreach (var employee in employees)
        {
            var salary = CalculateSalary(employee.EmployeeId);
            employee.ReceivePayment(salary);
        }
    }

    private decimal CalculateSalary(string employeeId) => 5000m; // Simplificado
}
```

## Dependency Inversion Principle (DIP)

**Principio de Inversión de Dependencias: depender de abstracciones, no de implementaciones concretas.**
Esta sección muestra cómo usar inyección de dependencias para crear código desacoplado y testeable.
Crítico para crear arquitecturas flexibles que faciliten el testing y el mantenimiento.

### ❌ Violación del DIP

```csharp
// MALO: OrderService depende directamente de implementaciones concretas
public class OrderService
{
    private readonly EmailService _emailService;
    private readonly SqlServerRepository _repository;
    private readonly PayPalPaymentProcessor _paymentProcessor;

    public OrderService()
    {
        // Dependencias hard-coded - difícil de testear y cambiar
        _emailService = new EmailService();
        _repository = new SqlServerRepository();
        _paymentProcessor = new PayPalPaymentProcessor();
    }

    public void ProcessOrder(Order order)
    {
        // Lógica acoplada a implementaciones específicas
        _repository.Save(order);
        _paymentProcessor.ProcessPayment(order.Total);
        _emailService.SendOrderConfirmation(order);
    }
}

// Imposible cambiar a MongoDB o enviar SMS sin modificar OrderService
public class SqlServerRepository
{
    public void Save(Order order)
    {
        // Implementación específica de SQL Server
        using var connection = new SqlConnection("Server=...");
        // ...
    }
}
```

### ✅ Aplicando DIP Correctamente

```csharp
// BUENO: Depender de abstracciones

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int orderId);
    Task SaveAsync(Order order);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
}

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentMethod method);
}

public interface INotificationService
{
    Task SendOrderConfirmationAsync(Order order);
    Task SendOrderUpdateAsync(Order order, string status);
}

// Implementaciones concretas intercambiables
public class SqlServerOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public SqlServerOrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task SaveAsync(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        // Implementación SQL Server específica
    }

    public async Task<Order> GetByIdAsync(int orderId)
    {
        // Implementación con Entity Framework
        using var context = new ApplicationDbContext();
        return await context.Orders.FindAsync(orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        using var context = new ApplicationDbContext();
        return await context.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }
}

public class MongoDbOrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public MongoDbOrderRepository(IMongoDatabase database)
    {
        _orders = database.GetCollection<Order>("orders");
    }

    public async Task SaveAsync(Order order)
    {
        await _orders.ReplaceOneAsync(
            o => o.Id == order.Id,
            order,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<Order> GetByIdAsync(int orderId)
    {
        return await _orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        return await _orders.Find(o => o.CustomerId == customerId).ToListAsync();
    }
}

public class EmailNotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ITemplateEngine _templateEngine;

    public EmailNotificationService(IEmailSender emailSender, ITemplateEngine templateEngine)
    {
        _emailSender = emailSender;
        _templateEngine = templateEngine;
    }

    public async Task SendOrderConfirmationAsync(Order order)
    {
        var template = await _templateEngine.GetTemplateAsync("OrderConfirmation");
        var body = await _templateEngine.RenderAsync(template, order);

        await _emailSender.SendAsync(
            to: order.Customer.Email,
            subject: $"Order Confirmation #{order.Id}",
            body: body);
    }

    public async Task SendOrderUpdateAsync(Order order, string status)
    {
        var template = await _templateEngine.GetTemplateAsync("OrderUpdate");
        var body = await _templateEngine.RenderAsync(template, new { order, status });

        await _emailSender.SendAsync(
            to: order.Customer.Email,
            subject: $"Order Update #{order.Id}",
            body: body);
    }
}

// Servicio desacoplado que acepta abstracciones
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IPaymentProcessor paymentProcessor,
        INotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _paymentProcessor = paymentProcessor;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<OrderResult> ProcessOrderAsync(Order order)
    {
        try
        {
            _logger.LogInformation("Processing order {OrderId}", order.Id);

            // Guardar orden
            await _repository.SaveAsync(order);

            // Procesar pago
            var paymentResult = await _paymentProcessor.ProcessPaymentAsync(
                order.Total,
                order.PaymentMethod);

            if (!paymentResult.IsSuccess)
            {
                _logger.LogWarning("Payment failed for order {OrderId}: {Error}",
                    order.Id, paymentResult.Error);
                return OrderResult.PaymentFailed(paymentResult.Error);
            }

            // Actualizar estado y notificar
            order.Status = OrderStatus.Paid;
            order.PaymentTransactionId = paymentResult.TransactionId;
            await _repository.SaveAsync(order);

            await _notificationService.SendOrderConfirmationAsync(order);

            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            return OrderResult.Success(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
            return OrderResult.Failed($"Error processing order: {ex.Message}");
        }
    }
}

// Configuración de dependencias en Startup.cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registro de dependencias - fácil cambiar implementaciones
        services.AddScoped<IOrderRepository, SqlServerOrderRepository>();
        // services.AddScoped<IOrderRepository, MongoDbOrderRepository>(); // Fácil cambio

        services.AddScoped<IPaymentProcessor, StripePaymentProcessor>();
        services.AddScoped<INotificationService, EmailNotificationService>();
        // services.AddScoped<INotificationService, SmsNotificationService>(); // Fácil agregar

        services.AddScoped<OrderService>();
    }
}
```
