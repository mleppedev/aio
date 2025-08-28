# Testing Strategies

## Testing Pyramid for .NET

| **Nivel**             | **Proporción** | **Velocidad**       | **Costo** | **Confiabilidad** | **Herramientas**                |
| --------------------- | -------------- | ------------------- | --------- | ----------------- | ------------------------------- |
| **Unit Tests**        | 70%            | Muy rápida (ms)     | Bajo      | Media             | xUnit, NUnit, MSTest            |
| **Integration Tests** | 20%            | Moderada (segundos) | Medio     | Alta              | TestHost, WebApplicationFactory |
| **End-to-End Tests**  | 10%            | Lenta (minutos)     | Alto      | Muy alta          | Selenium, Playwright            |

## Unit Testing Frameworks Comparison

| **Framework** | **Sintaxis**    | **Assertions**      | **Parallel**      | **Ecosystem**        |
| ------------- | --------------- | ------------------- | ----------------- | -------------------- |
| **xUnit**     | Attribute-based | `Assert.Equal()`    | Sí (por defecto)  | Moderno, extensible  |
| **NUnit**     | Attribute-based | `Assert.That()`     | Sí (configurable) | Maduro, feature-rich |
| **MSTest**    | Attribute-based | `Assert.AreEqual()` | Sí                | Integrado con VS     |

## Mocking Libraries

| **Library**         | **Sintaxis** | **Performance** | **Features**      | **Learning Curve** |
| ------------------- | ------------ | --------------- | ----------------- | ------------------ |
| **Moq**             | Lambda-based | Buena           | Completo          | Moderada           |
| **NSubstitute**     | Fluent       | Muy buena       | Simple y poderoso | Fácil              |
| **FakeItEasy**      | Fluent       | Buena           | Muy expresivo     | Fácil              |
| **Microsoft Fakes** | Generated    | Excelente       | Shimming completo | Compleja           |

## Test Arrangement Patterns

| **Patrón**                   | **Estructura**         | **Beneficio**    | **Ejemplo**                             |
| ---------------------------- | ---------------------- | ---------------- | --------------------------------------- |
| **AAA (Arrange-Act-Assert)** | 3 secciones claras     | Legibilidad      | Standard en .NET                        |
| **Given-When-Then**          | BDD style              | Lenguaje natural | SpecFlow, comportamiento                |
| **Object Mother**            | Factory para test data | Reutilización    | `UserMother.ValidUser()`                |
| **Test Data Builder**        | Fluent construction    | Flexibilidad     | `new UserBuilder().WithEmail().Build()` |

## Integration Testing Approaches

| **Tipo**                  | **Scope**            | **Implementación** | **Trade-offs**                       |
| ------------------------- | -------------------- | ------------------ | ------------------------------------ |
| **In-Memory Database**    | Repository layer     | EF Core InMemory   | Rápido, pero diferente de producción |
| **Test Containers**       | Infraestructura real | Docker containers  | Realista, pero más lento             |
| **Test Database**         | Full database        | Dedicated test DB  | Realista, requiere cleanup           |
| **WebApplicationFactory** | API completa         | ASP.NET Test Host  | Full stack, good isolation           |

## Test Data Management

| **Estrategia**              | **Pros**           | **Contras**                               | **Cuándo usar**      |
| --------------------------- | ------------------ | ----------------------------------------- | -------------------- |
| **Fresh Database per Test** | Aislamiento total  | Muy lento                                 | Tests críticos       |
| **Transaction Rollback**    | Rápido, limpio     | Limitaciones con distributed transactions | Tests de repository  |
| **Database Seeding**        | Datos consistentes | Acoplamiento                              | Tests de integración |
| **Test Data Builders**      | Flexibilidad       | Setup overhead                            | Datos complejos      |

## Assertion Libraries

```csharp
// xUnit básico
Assert.Equal(expected, actual);
Assert.True(condition);
Assert.Throws<ArgumentException>(() => method());

// FluentAssertions
result.Should().NotBeNull();
result.Should().BeOfType<User>();
result.Should().Match<User>(u => u.Email.Contains("@"));
collection.Should().HaveCount(3).And.OnlyContain(x => x.IsActive);

// Shouldly
result.ShouldNotBeNull();
result.ShouldBeOfType<User>();
result.Email.ShouldContain("@");
```

## Test Doubles Types

| **Tipo**  | **Propósito**           | **Implementación**        | **Ejemplo**                             |
| --------- | ----------------------- | ------------------------- | --------------------------------------- |
| **Dummy** | Llenar parámetros       | Objeto sin comportamiento | `new User()`                            |
| **Stub**  | Respuestas predefinidas | Hard-coded returns        | `userRepo.GetById(1) returns user1`     |
| **Mock**  | Verificar interacciones | Verifica calls            | `Verify(x => x.Save(It.IsAny<User>()))` |
| **Spy**   | Capturar calls          | Partial mocking           | `CallBase = true` en Moq                |
| **Fake**  | Implementación ligera   | Working implementation    | In-memory repository                    |

## Testing Async Code

```csharp
[Fact]
public async Task GetUserAsync_ReturnsUser_WhenUserExists()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "John" };
    _mockRepository.Setup(x => x.GetByIdAsync(userId))
                   .ReturnsAsync(expectedUser);

    // Act
    var result = await _userService.GetUserAsync(userId);

    // Assert
    result.Should().BeEquivalentTo(expectedUser);
}

[Fact]
public async Task CreateUserAsync_ThrowsException_WhenEmailExists()
{
    // Arrange
    _mockRepository.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                   .ReturnsAsync(true);

    // Act & Assert
    await Assert.ThrowsAsync<DuplicateEmailException>(
        () => _userService.CreateUserAsync("test@email.com"));
}
```

## Test Categories and Traits

```csharp
[Fact]
[Trait("Category", "Unit")]
public void CalculatePrice_ReturnsCorrectValue()
{
    // Unit test
}

[Fact]
[Trait("Category", "Integration")]
public async Task SaveUser_PersistsToDatabase()
{
    // Integration test
}

[Fact]
[Trait("Category", "Slow")]
public async Task ProcessLargeFile_CompletesSuccessfully()
{
    // Slow test
}

// Run specific categories
// dotnet test --filter "Category=Unit"
// dotnet test --filter "Category!=Slow"
```

## Performance Testing with NBomber

```csharp
var scenario = Scenario.Create("user_api_test", async context =>
{
    var userId = Random.Shared.Next(1, 1000);
    var response = await httpClient.GetAsync($"/api/users/{userId}");

    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(5))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();
```

## Test Configuration Management

| **Approach**              | **Configuration**             | **Pros**         | **Cons**            |
| ------------------------- | ----------------------------- | ---------------- | ------------------- |
| **appsettings.Test.json** | Separate config file          | Clear separation | Must maintain       |
| **Environment Variables** | `ASPNETCORE_ENVIRONMENT=Test` | CI/CD friendly   | Platform dependent  |
| **In-Code Configuration** | Override in test startup      | Full control     | Code duplication    |
| **Test Fixtures**         | Setup once per class          | Performance      | Shared state issues |

## Database Testing Strategies

```csharp
// TestContainers approach
public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private ApplicationDbContext _dbContext;

    public DatabaseIntegrationTests()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

## API Testing with WebApplicationFactory

```csharp
public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real dependencies with mocks
                services.RemoveAll<IUserRepository>();
                services.AddScoped(_ => Mock.Of<IUserRepository>());
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        // Act
        var response = await _client.GetAsync("/api/users/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<User>();
        user.Should().NotBeNull();
    }
}
```

## Test Coverage Analysis

| **Métrica**          | **Herramienta**    | **Target** | **Significado**   |
| -------------------- | ------------------ | ---------- | ----------------- |
| **Line Coverage**    | Coverlet, dotCover | > 80%      | Líneas ejecutadas |
| **Branch Coverage**  | Fine Code Coverage | > 70%      | Ramas de decisión |
| **Method Coverage**  | Visual Studio      | > 90%      | Métodos llamados  |
| **Mutation Testing** | Stryker.NET        | > 60%      | Calidad de tests  |

## Testing Best Practices

| **Principio**           | **Descripción**                                        | **Ejemplo**              | **Beneficio**      |
| ----------------------- | ------------------------------------------------------ | ------------------------ | ------------------ |
| **FIRST**               | Fast, Independent, Repeatable, Self-validating, Timely | Tests rápidos y aislados | Confiabilidad      |
| **Descriptive Names**   | `Should_ReturnNull_When_UserNotFound`                  | Nombres expresivos       | Documentación viva |
| **Single Assertion**    | Un assert por test (cuando sea posible)                | Fallas específicas       | Debugging fácil    |
| **Test Data Isolation** | Cada test sus propios datos                            | Sin dependencias         | Paralelización     |

## Testing Pyramid Implementation

```mermaid
pyramid TB
    subgraph "E2E Tests (10%)"
        A[Selenium/Playwright<br/>Full User Journeys<br/>Critical Business Flows]
    end

    subgraph "Integration Tests (20%)"
        B[WebApplicationFactory<br/>Repository Tests<br/>API Contract Tests<br/>Database Integration]
    end

    subgraph "Unit Tests (70%)"
        C[Business Logic<br/>Validation Rules<br/>Calculations<br/>Utility Methods]
    end

    style A fill:#ef4444
    style B fill:#f59e0b
    style C fill:#22c55e
```

## Test Execution Pipeline

| **Fase**       | **Tests**               | **Frecuencia** | **Trigger**    |
| -------------- | ----------------------- | -------------- | -------------- |
| **Pre-commit** | Unit tests              | Cada commit    | Git hook       |
| **CI Build**   | Unit + Integration      | Cada push      | Pipeline       |
| **Nightly**    | All tests + Performance | Diario         | Scheduler      |
| **Release**    | Full suite + Manual     | Pre-release    | Manual trigger |

## Common Testing Anti-Patterns

| **Anti-Pattern**         | **Problema**               | **Solución**                      |
| ------------------------ | -------------------------- | --------------------------------- |
| **Ice Cream Cone**       | Más E2E que unit tests     | Invertir pirámide                 |
| **Test Interdependence** | Tests dependen entre sí    | Aislamiento completo              |
| **Happy Path Only**      | Solo casos positivos       | Test edge cases                   |
| **Brittle Tests**        | Fallan por cambios menores | Test behavior, not implementation |
| **Slow Test Suite**      | Suite tarda mucho          | Optimizar, paralelizar            |
