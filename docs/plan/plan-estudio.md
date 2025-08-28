# 🧠 Plan de Estudio Intensivo – Senior Software Developer .NET (3 días)

## Día 1 – Fundamentos sólidos de .NET Core y C#

### ✅ Objetivo:

Reforzar C#, ASP.NET Core, Entity Framework y patrones SOLID.

### 📚 Teoría

- C# avanzado: LINQ, async/await, SOLID, delegates, interfaces vs clases abstractas.
- ASP.NET Core: ciclo de vida, routing, controllers, middleware, dependency injection.
- Entity Framework Core: migrations, relaciones, queries con LINQ, Fluent API.

### 💻 Práctica

- Crear API CRUD de productos (.NET 8 + EF Core + SQLite).
- Paginación y búsqueda.

### ✅ Checklist

- [ ] Proyecto funcional con API REST
- [ ] DBContext y migraciones con EF Core
- [ ] SOLID aplicado en servicios
- [ ] Uso correcto de inyección de dependencias

### 💬 Preguntas clave

- ¿Qué es el principio de inversión de dependencias?
- ¿Cómo funciona el ciclo de vida de una request en ASP.NET Core?
- ¿Cuándo usar interfaz vs clase abstracta?

---

## Día 2 – Arquitectura, servicios distribuidos, testing y CI/CD

### ✅ Objetivo:

Aplicar Clean Architecture, testing, autenticación y prácticas modernas.

### 📚 Teoría

- Clean Architecture y separación de capas.
- Repository, Unit of Work, CQRS, Mediator.
- Autenticación con JWT.
- Testing con xUnit y Moq.
- CI/CD con GitHub Actions.

### 💻 Práctica

- Refactor del API usando Clean Architecture.
- Autenticación con JWT.
- Tests unitarios y mocking.
- Pipeline de CI/CD simple.

### ✅ Checklist

- [ ] Separación Domain/Application/Infra/API
- [ ] Auth con JWT funcionando
- [ ] Tests para servicios y controladores
- [ ] Pipeline de build/test

### 💬 Preguntas clave

- ¿Cómo testear un endpoint protegido?
- ¿Qué es CQRS y cómo lo aplicas?
- ¿Por qué usar patrón Repository?

---

## Día 3 – Escalabilidad, rendimiento, sistemas distribuidos

### ✅ Objetivo:

Mostrar seniority técnico en rendimiento, resiliencia, mensajería y debugging.

### 📚 Teoría

- Redis caching, Polly (retry, circuit breaker).
- RabbitMQ, Kafka: cuándo y cómo.
- Background Services, Hosted Services.
- Serilog, MiniProfiler, dotnet-trace.

### 💻 Práctica

- Redis para caching en endpoint.
- Simulación de worker background.
- Logs estructurados y profiling.

### ✅ Checklist

- [ ] Redis funcionando como cache
- [ ] Background service funcionando
- [ ] Logs y métricas integrados
- [ ] Análisis de rendimiento básico

### 💬 Preguntas clave

- ¿Cómo escalarías un sistema a 10K RPS?
- ¿Cómo implementas resiliencia en microservicios?
- ¿Qué herramientas usas para debugging en producción?

---

## Sección adicional 1 – Testing, TDD y mocking

### 📚 Teoría

- Testing unitario vs integración.
- xUnit, Moq, TDD.

### 💻 Ejemplo

```csharp
[Fact]
public void GetAll_ReturnsProducts() {
    var mock = new Mock<IRepo>();
    mock.Setup(r => r.GetAll()).Returns(GetSample());
    var service = new ProductService(mock.Object);
    Assert.NotEmpty(service.GetAll());
}
```

### Preguntas:

- ¿Cómo testearías una lógica que depende de EF Core?
- ¿Qué beneficios ves en el TDD?

---

## Sección adicional 2 – Algoritmos y lógica

### 📚 Temas clave

- Arrays, diccionarios, sets, árboles, grafos.
- Sorting, búsqueda binaria, recursión.
- LeetCode-style problems.

### Preguntas:

- ¿Cómo invertirías una lista enlazada?
- ¿Cómo encontrarías el primer carácter no repetido?

### Práctica sugerida:

- LeetCode: ["Two Sum", "Valid Parentheses"]
- HackerRank: ["30 Days of Code"]

---

## Sección adicional 3 – Refactorización y lectura de código legado

### 📚 Teoría

- Técnicas de refactor seguro.
- Code smells.
- Aplicar principios SOLID a código heredado.

### Preguntas:

- ¿Cómo dividirías una clase de 1000 líneas?
- ¿Cómo reduces la duplicación en un sistema viejo?

---

## Sección adicional 4 – Clases abstractas vs interfaces

### 📚 Teoría

- Interfaces: contrato puro.
- Abstractas: lógica compartida + métodos abstractos.

### 💻 Ejemplo

```csharp
public interface IWorker { void Work(); }
public abstract class BaseWorker {
    public void Log() => Console.WriteLine("Log");
    public abstract void Work();
}
```

### Preguntas:

- ¿Puedes tener múltiples interfaces? ¿Y clases abstractas?
- ¿Cómo decides cuál usar?

---

## Sección adicional 5 – Inyección de dependencias avanzada

### 📚 Teoría

- Lifetimes: Transient, Scoped, Singleton.
- IoC containers, testabilidad.

### 💻 Ejemplo

```csharp
services.AddScoped<IService, MyService>();
services.AddSingleton<ILogger, ConsoleLogger>();
```

### Preguntas:

- ¿Qué pasa si inyectas un Scoped en un Singleton?
- ¿Cómo testearías un servicio inyectado con dependencias?

---

## Preguntas estratégicas de alto nivel (soft & liderazgo)

- Describe una arquitectura distribuida que hayas implementado.
- ¿Qué técnicas usas para evitar errores en producción?
- ¿Cómo lideras code reviews?
- ¿Cómo asegurarías que un sistema cumpla con su SLA?
- ¿Cuál fue tu mayor desafío técnico como líder de equipo?

---

## Recursos extra

- [Clean Architecture Template – Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture)
- [Microsoft Learn .NET Path](https://learn.microsoft.com/en-us/training/paths/build-web-api-aspnet-core/)
- [LeetCode – C# Problems](https://leetcode.com/problemset/?language=cpp)
- [Awesome .NET Core](https://github.com/thangchung/awesome-dotnet-core)
