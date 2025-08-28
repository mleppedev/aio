# Requerimientos Técnicos – Banco de Preguntas y Respuestas (50)

Requisitos foco: C#, .NET, Fullstack (énfasis Backend), Microservicios, CI/CD, Blazor, API REST.

Formato: Pregunta + Respuesta breve / estructurada. Adapta con ejemplos reales tuyos.

---

### 1. Diferencia entre .NET CLR y .NET Core / .NET (moderno)

Respuesta: CLR original era Windows-centric; .NET moderno (Core unificado) es multiplataforma, modular (NuGet), rendimiento mejorado (tiered JIT, R2R) y con un runtime unificado para server, cloud y WASM.

### 2. ¿Cuándo usarías struct en lugar de class en C#?

Respuesta: Para tipos pequeños, inmutables, de semántica de valor y que se usen intensivamente en colecciones evitando heap allocations (e.g. coordenadas, short-lived value objects). Evito structs grandes (>16 bytes) para no penalizar copias.

### 3. Explica async/await internamente.

Respuesta: Compila a state machine; await registra continuación y libera el thread; cuando Task completa se reanuda estado. Evita bloqueo de threads y mejora escalabilidad IO-bound.

### 4. Diferencia Task.Run vs async natural.

Respuesta: Task.Run fuerza trabajo a thread pool (CPU-bound). Async natural se apoya en IO completion ports. No envolver IO-based async en Task.Run innecesariamente.

### 5. Cómo diagnosticar una fuga de memoria en .NET.

Respuesta: Uso dotnet-counters / dotnet-gcdump / PerfView, analizo objetos retenidos (roots), busco eventos de largo ciclo (static caches, event handlers no removidos) y aplico profiling comparativo.

### 6. Principio SOLID aplicado a servicios.

Respuesta: Single Responsibility (servicio con una razón de cambio), Open/Closed (extensión vía interfaces), Liskov (sustitución polimórfica), Interface Segregation (contratos pequeños) y Dependency Inversion (depender de abstracciones) → reduce acoplamiento.

### 7. Patrón Repository ¿lo usarías siempre?

Respuesta: Sólo si aporta abstracción útil o testabilidad. En muchos escenarios modernos EF Core DbContext ya actúa como UoW + repositorio; duplicar capas añade fricción.

### 8. Diferencia entre AddSingleton, AddScoped, AddTransient.

Respuesta: Singleton: una instancia app; Scoped: una por request (o scope manual); Transient: nueva cada resolución. Elegir según estado y costo de creación.

### 9. Manejo de configuración por ambiente.

Respuesta: appsettings.{Environment}.json + environment variables + user secrets (dev) + inyección IOptionsSnapshot/IOptionsMonitor y nunca valores sensibles en repositorio.

### 10. ¿Qué es un middleware en ASP.NET Core?

Respuesta: Componente pipeline HTTP que procesa request/response y puede delegar al siguiente. Ideal para cross-cutting (logging, auth, rate limiting).

### 11. Estrategia para versionar una API REST.

Respuesta: v1, v2 en ruta para breaking changes; headers para deprecation; mantener compatibilidad temporal; contrato documentado OpenAPI.

### 12. Criterios para paginación eficiente.

Respuesta: Use limit + keyset (WHERE > lastId) en grandes volúmenes; evito OFFSET profundo; exponer token de continuación.

### 13. Idempotencia en POST crítico.

Respuesta: Idempotency-Key + registro persistente estado; si repetido devuelvo resultado previo. Evita duplicados en reintentos cliente/red.

### 14. Status codes y su uso.

Respuesta: 200 OK, 201 Created (Location header), 202 Accepted async, 400 invalid, 401 auth, 403 forbidden, 404 not found, 409 conflict, 422 validation domain, 500 server error.

### 15. Seguridad básica de API.

Respuesta: HTTPS obligatorio, OIDC/OAuth2 para auth, scopes/roles para authZ, rate limiting, logging estructurado, input validation, secret management.

### 16. CORS ¿por qué y cuándo?

Respuesta: Controla orígenes permitidos en navegadores. Configurar dominios específicos y métodos necesarios; evitar \* en producción.

### 17. Diseño de DTO vs Domain Model.

Respuesta: DTO simplifica interfaz (evita sobreexponer dominio), reduce overposting y versiona la superficie pública. Domain model guarda invariantes.

### 18. Ventajas de microservicios.

Respuesta: Despliegue independiente, escalado granular, aislamiento de fallos, alineación a dominio. Costo: complejidad operativa, observabilidad, consistencia distribuida.

### 19. Cuándo NO usar microservicios.

Respuesta: Dominio volátil, equipo pequeño, falta de automatización, bajo volumen o complejidad. Prefiero monolito modular inicial.

### 20. Comunicación síncrona vs asíncrona.

Respuesta: Síncrona (HTTP) simple pero acopla latencia; asíncrona (mensajería) desacopla y mejora resiliencia. Usar eventos para hechos y comandos para acciones dirigidas.

### 21. Patrones de resiliencia en microservicios.

Respuesta: Retry con backoff + jitter, Circuit Breaker, Bulkhead isolation, Timeout, Fallback, Cache, Rate limiting.

### 22. Consistencia eventual manejo.

Respuesta: Eventos de dominio + outbox, idempotencia consumidores, compensaciones (sagas), reconciliación periódica / auditoría.

### 23. Observabilidad mínima.

Respuesta: Logs estructurados (correlationId), métricas (latencia, tasa error, throughput), tracing distribuido, health checks.

### 24. Estrategia de despliegue blue/green vs canary.

Respuesta: Blue/Green para cambios grandes y rollback inmediato; Canary para liberación gradual y validación métrica en producción.

### 25. Métricas DORA utilidad.

Respuesta: Deployment frequency, lead time, change fail rate, MTTR → correlacionan con desempeño organizacional y estabilidad.

### 26. Pipeline CI/CD ideal (etapas).

Respuesta: Build → Unit Tests → Static Analysis/SAST → Tests contract → Package → Deploy staging → Smoke tests → Canary prod → Promoción automática.

### 27. Infra as Code beneficios.

Respuesta: Reproducibilidad, versionado, revisión PR, consistencia entornos, rollback rápido. Herramientas: Terraform, Bicep.

### 28. Estrategia de gestión de secretos.

Respuesta: Vault/KMS, rotación, mínimos privilegios, no en código, auditoría accesos, escaneo leaks.

### 29. Control de dependencias NuGet.

Respuesta: Versiones fijadas, auditoría vulnerabilidades (Dependabot, dotnet list package --vulnerable), eliminación dependencias innecesarias.

### 30. Minimizar tiempo de build.

Respuesta: Caché de dependencias, incremental builds, dividir soluciones, analizar hot paths de compilación, evitar proyectos innecesarios.

### 31. Blazor Server vs WebAssembly.

Respuesta: Server: menor payload inicial, depende de conexión persistente (latencia). WASM: carga inicial mayor, más offline/edge. Elegir según perfil usuario y requisitos UX.

### 32. Componentización en Blazor.

Respuesta: Componentes pequeños reutilizables, parámetros tipados, StateHasChanged controlado, evitar lógica pesada en UI (delegar a servicios).

### 33. Manejo de estado en Blazor.

Respuesta: Scoped services + contenedores de estado, localStorage seguro (si aplica), invalidar al cerrar sesión, evitar singletons mutables.

### 34. Protección de formularios.

Respuesta: Validaciones lado cliente + servidor, antiforgery token, límites de tamaño, sanitización entradas, feedback claro errores.

### 35. Integración API desde Blazor.

Respuesta: HttpClient inyectado (IHttpClientFactory), políticas resiliencia (Polly), JSON serializer configurado, manejo de reintentos y cancelación.

### 36. Optimización rendimiento Blazor.

Respuesta: Pre-render, compresión Brotli, lazy load assemblies, virtualization listas grandes, reducción renders innecesarios.

### 37. Fullstack: integración front-back.

Respuesta: Contratos OpenAPI compartidos, generación de clientes, versionado coordinado, pruebas contract para evitar roturas.

### 38. Autenticación y autorización.

Respuesta: OIDC para autenticación, JWT tokens, roles / claims para autorización, policy-based, refresh tokens seguros.

### 39. Manejo de errores global.

Respuesta: Middleware captura, mapping a códigos HTTP consistentes, log estructurado, respuesta estándar con traceId.

### 40. Caching estratégico.

Respuesta: In-memory para datos rápidos, distribuido (Redis) para escalado, consideraciones de invalidación, TTL basado en volatilidad.

### 41. Compresión y performance HTTP.

Respuesta: Habilitar gzip/brotli, HTTP/2, minimizar payloads (compresión JSON, campos necesarios), usar ETags.

### 42. Serialización eficiente.

Respuesta: System.Text.Json (source-gen), evitar reflection costosa, predefinir opciones (IgnoreNull, naming policy), medir con benchmarks.

### 43. Prevención de N+1 en EF Core.

Respuesta: Include selectivo, proyección (Select), AsSplitQuery donde convenga, logging de queries lentas, profiling.

### 44. Transacciones distribuidas alternativa.

Respuesta: Evitar 2PC; usar sagas y compensaciones, garantizar idempotencia y consistencia eventual.

### 45. Estrategia de migraciones de base de datos.

Respuesta: Migraciones pequeñas reversibles, expand/contract pattern, scripts en control de versiones, validación en staging.

### 46. Gestión de colas (mensajería).

Respuesta: Definir DLQ, reintentos con backoff, idempotencia consumidores, métricas de lag y procesamiento.

### 47. Feature Flags disciplina.

Respuesta: Metadatos (owner, expiración), cleanup regular, separar deploy de release, auditoría uso.

### 48. Testing pirámide aplicada.

Respuesta: Amplia base unit tests, layer de contract/integration reducida y selectiva, smoke E2E, tests performance/resiliencia esenciales.

### 49. Métricas rendimiento servicio crítico.

Respuesta: Latencia P50/P95, throughput, error rate, saturación (CPU/mem), tiempo cold start, percentiles colas.

### 50. Indicadores de salud operativa.

Respuesta: SLO cumplidos, backlog de bugs críticos bajo, lead time estable, MTTR dentro objetivo, baja tasa cambios fallidos.

---

¿Necesitas una versión traducida o en formato flashcards?
