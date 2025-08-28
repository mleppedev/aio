# 50 Preguntas y Respuestas de Alto Nivel Alineadas a los Requerimientos

> Respuestas concisas, ejecutivas y accionables. Enfocadas en: C#, .NET, arquitectura backend/microservicios, Blazor, REST, CI/CD, seguridad, observabilidad, resiliencia, escalabilidad y gobierno técnico.

1. **¿Cómo definirías los bounded contexts principales y sus límites de integración para evitar acoplamientos cíclicos?**  
   Divido por capacidades de negocio: Identidad, Enrolamiento, Matching, Gestión de Credenciales, Auditoría, Facturación. Uso contratos explícitos (OpenAPI/JSON Schema) y eventos de dominio (solo datos mínimos). Evito dependencias cruzadas directas aplicando anti-corruption layers donde un contexto necesita transformar modelos. Reviso diagramas de dependencias en cada PR y aplico validación estática (por ejemplo, reglas de namespaces) para impedir referencias prohibidas.

2. **¿Qué criterios usas para decidir microservicio vs módulo interno en un monolito modular .NET?**  
   Evalúo: tasa de cambio independiente, criticidad de escalado diferencial, límites de datos, necesidad de aislamiento de fallos y cumplimiento. Si <2 criterios fuertes, inicio como módulo (monolito modular) para reducir complejidad operacional. Promuevo extraer cuando: despliegues desacoplados bloquean velocidad o patrones de carga divergen claramente.

3. **¿Cómo versionas APIs REST minimizando fricción?**  
   Estrategia semántica por recurso (v1, v2) en URL solo cuando hay breaking changes. Para evoluciones no-breaking uso extensibilidad hacia adelante: campos opcionales, HATEOAS mínima o links, encabezado custom X-Experimental para pruebas. Mantengo máximo 2 versiones activas y un calendario de deprecación comunicado en doc portal interno.

4. **¿Cómo orquestas flujos distribuidos (enrolamiento + verificación) garantizando consistencia eventual?**  
   Coreografía basada en eventos para pasos independientes; saga orquestada (coordinador) cuando se requieren compensaciones ordenadas. Outbox pattern asegura publicación atómica. Idempotencia con claves naturales (userId + biometricType + hashTemplate). Timeouts + estados explícitos (Pending, Validated, FailedCompensation).

5. **¿Cuándo migrar de Blazor Server a WASM o híbrido?**  
   Métricas disparadoras: latencia de ida y vuelta > umbral (p.ej. p95 > 250ms), costo de conexiones persistentes alto, necesidad de offline, escalado horizontal del servidor por carga de UI no-core. Migración gradual: componentes críticos puros WASM compartiendo librerías .NET Standard.

6. **¿Cómo estructuras la solución .NET para aislación sin perder agilidad?**  
   Capas: Domain (POCOs, invariantes), Application (use cases, CQRS handlers), Infrastructure (EF Core, external adapters), API (controllers minimal). Enforced via proyectos separados y referencias unidireccionales. Análisis de dependencias automatizado y tests de arquitectura (NetArchTest).

7. **¿Qué combinación de resiliencia aplicas ante picos de latencia en matching biométrico?**  
   Circuit breaker (fallos consecutivos), timeout agresivo por operación, retry exponencial con jitter solo para fallos transitorios (no validation), bulkhead para aislar pool de conexiones, fallback a cola diferida si excede SLA de respuesta interactiva. Observabilidad con métricas: rate de apertura de breaker y latencia p95/p99.

8. **¿Cuándo usar gRPC además de REST?**  
   Lo adopto para: baja latencia entre servicios internos, streaming bidireccional (ej. progresos de procesado), contratos fuertemente tipados. Mantengo REST para consumo externo y simplicidad. Documento criterios y evito duplicación: un servicio no expone ambos protocolos para el mismo caso de uso sin necesidad real.

9. **¿Cómo aseguras determinismo y auditabilidad en pipelines biométricos?**  
   Versionado de modelos/algoritmos, hashing de plantillas antes/después, registro estructurado (traceId + dominio + versión), firma de eventos críticos, inmutabilidad de logs en almacenamiento append-only (WORM). Reproducibilidad: contenedores inmutables y datasets controlados con checksums.

10. **¿Cómo defines SLOs iniciales y error budgets?**  
    Identifico User Journey clave (verificación en < 2s p95, disponibilidad 99.9%, FAR < X%). Error budget = 0.1% tiempo no disponible. Si se agota antes de ciclo trimestral, congelación de features y foco en fiabilidad. Dashboards con SLI reales (no métricas sintéticas).

11. **¿Cómo minimizas tamaño y exposición de DTOs?**  
    Separación de modelos interno vs API, filtrado server-side selectivo, uso de `ProblemDetails` estándar para errores. Evito sobrecarga: no anidar colecciones innecesarias, paginación keyset, hipermedia ligera.

12. **¿Monorepo o multirepo y por qué?**  
    Monorepo para consistencia de versiones compartidas (Contracts, Packages comunes) + pipeline unificada incremental. Multirepo solo cuando políticas de seguridad/compliance requieren aislamiento o ritmos de release radicalmente distintos. Uso matrices de build para evitar builds completos innecesarios.

13. **¿Qué métricas CI/CD vigilas y acción ante Change Failure Rate alto?**  
    Lead Time, Deployment Frequency, Change Failure Rate, MTTR. Si CFR > umbral (p.ej. 15%): root cause sessions, endurecer gates (tests contractuales, smoke en canary), mejorar observabilidad pre-producción (tracing en staging), coaching revisiones.

14. **¿Cómo integras pruebas de contrato sin frenar el flujo?**  
    Consumidores definen pactos; productor ejecuta verificación en pipeline. Stage dedicado contract-verify rápido (<1 min). Solo bloquea merge si ruptura verdaderamente breaking. Versionado incremental evita cascadas.

15. **¿EF Core, Dapper o mezcla?**  
    EF Core para CRUD rico, tracking y migraciones. Dapper para lecturas de alto rendimiento o agregaciones complejas read-only. Abstracción interna (puertos) evita fuga de detalles. Métricas comparativas antes de micro-optimizar.

16. **¿Cómo garantizas idempotencia en enrolamiento?**  
    Idempotency-Key en headers + almacenamiento de requests procesadas (hash payload). Transacción incluye verificación de existencia previa. Operaciones generan mismo resultado sin duplicados; respuestas cacheadas breve tiempo para reintentos inmediatos.

17. **¿Estrategia de cache para verificación?**  
    Multi-capa: in-memory (hot small sets), Redis distribuido para resultados recientes y metadata; TTL corto adaptativo (basado en tasa de acceso). Invalidation dirigida por eventos (revocación, actualización de plantilla). Cache stampede mitigado con locking y jitter.

18. **¿Cifrado en tránsito y reposo con rotación?**  
    TLS 1.3 obligatorio, mTLS interno servicios críticos. Reposo: AES-256 con claves en HSM/KMS; rotación automática programada + reencriptación paulatina (lazy rewrap). Secret scanning en pipeline.

19. **¿Introducción de feature flags sin deuda?**  
    Flags tipadas (booleans, rollout %, user targeting) centralizadas. Cada flag con owner, expiración definida. Auditoría de uso, tarea de limpieza mensual automática. No lógica de negocio permanente detrás de un flag.

20. **¿Secuencia de migración a microservicios?**  
    Identifico seams claros (bounded contexts), extraigo primero lo con alta tasa de cambio + bajo acoplamiento de datos. Strangler Fig: redirect incremental de endpoints. Métricas de impacto (deploy frequency, defect density) validan avance.

21. **¿Segmentación API Gateway vs servicios?**  
    Gateway: auth, rate limiting, routing, agregación ligera, observabilidad cross-cutting. Servicios: lógica de dominio, validaciones profundas. Evito lógica empresarial en el gateway.

22. **¿Análisis de incidente de latencia?**  
    Recopilo spans p95/p99 (tracing), correlaciono con métricas infra (CPU, GC, IO). Árbol crítico, identificar hop más costoso. Verifico cambios recientes (deploy, flag). Hipótesis: regresión de código, dependencia externa, contención de recursos.

23. **¿Contener cascading failures en matching service?**  
    Circuit breakers por dependencia, bulkheads aislando pools, request shedding (rechazo temprano), colas buffer temporal, degradación graciosa (responder estado pendiente).

24. **¿Auditoría sin degradar throughput?**  
    Logging asíncrono estructurado, batch + compresión, canal dedicado (e.g. Serilog sink + outbox). Sampling inteligente para eventos no críticos, full log solo para operaciones sensibles.

25. **¿Modelo de permisos para operaciones sensibles?**  
    RBAC + ABAC: roles base (Admin, Auditor, Operator) + atributos (región, nivel sensibilidad). Acciones críticas requieren step-up auth (MFA) y registro firmado. Principle of least privilege revisado trimestralmente.

26. **¿Evolución segura de respuestas REST sin romper Blazor?**  
    UI consume ViewModels internos mappeados desde DTOs. Tests de snapshot contractuales. Solo agrego campos nuevos opcionales. Feature toggles para campos experimentales.

27. **¿Threat modeling y cadencia?**  
    STRIDE por feature sensible; revisión inicial + re-evaluación en cambios mayores o cada release trimestral. Tooling automatiza checklist (auth, input, storage, transport, logging).

28. **¿Health checks profundos?**  
    Liveness: proceso vivo, GC saludable. Readiness: conectividad DB, latencia dependencia < umbral, cola no saturada. Endpoint separado /healthz subdividido /ready /live con caché corta para evitar DoS involuntario.

29. **¿Retry policy sin amplificar congestión?**  
    Exponential backoff + jitter, máximo intentos bajos (2-3). No retries en timeouts de cliente > SLA. Integrado con circuit breaker para cortar cascadas.

30. **¿Observability-as-code vs configuración dinámica?**  
    Dashboards y alertas base versionadas (infra-as-code). Umbrales ajustables dinámicamente vía parámetros centralizados. Cambios críticos requieren PR revisado para trazabilidad.

31. **¿Guías de PR eficientes?**  
    Checklist corta: tests, seguridad, performance, arquitectura. Límites de tamaño (ideal < 400 LOC). Etiquetas automáticas. Revisión dual para cambios críticos; single para refactors internos seguros.

32. **¿Cuándo introducir un bus de eventos?**  
    Cuando existe necesidad de desacoplar productores/consumidores múltiples, picos irregulares, o secuencias eventual-consistency. No para simples request/response de baja latencia.

33. **¿Gobernanza de esquemas OpenAPI/JSON?**  
    Repositorio central de contratos, validación en pipeline (breaking change detection). SemVer + changelog automático. Tests de compatibilidad backward en consumidores clave.

34. **¿Justificación de pruebas de carga continuas?**  
    Detectan regresiones de performance temprano, protegen SLA sin esperar incidentes. Umbrales iniciales: p95 latencia, throughput mínimo, error rate <1%. Integración semanal y en lanzamientos mayores.

35. **¿Reducir cold start en contenedores .NET?**  
    Trimming, ReadyToRun, calentamiento previo (pre-warming), pool mínimo de instancias, optimización de carga de configuración y JIT.

36. **¿Compatibilidad Blazor ↔ REST ante cambios breaking?**  
    Adapter layer y pruebas end-to-end contractuales. Flags para rutas nuevas. Deprecación comunicada y telemetría de uso para saber cuándo remover.

37. **¿Política integral de secrets?**  
    Vault central, rotación automática programada, jamás en variables de build planas. Escaneo en PR (trufflehog/gitleaks). Registro de acceso, principle of least knowledge.

38. **¿Catálogo interno de APIs?**  
    Portal (e.g. Backstage) ingestando OpenAPI, owners, SLA, métricas de uso. Búsqueda semántica y score de madurez.

39. **¿Señales de sobre-ingeniería en microservicios?**  
    Número de servicios alto vs tamaño de equipo, frecuencia de cambios cruzando múltiples, latencia inter-servicio significativa, dificultad de trazabilidad end-to-end.

40. **¿Reducir tiempo de build creciente?**  
    Caché de dependencias, build incremental, partición de soluciones, análisis de hotspots (proyectos pesados), pipeline paralelizado y detección temprana de test lentos.

41. **¿Validación temprana de esquemas biométricos?**  
    JSON Schema o validadores binarios, pre-validación streaming, rejection early con métricas; versionado explícito en payload.

42. **¿Feedback loop prod → backlog técnico?**  
    Dashboards con tags de dominio, weekly triage de anomalías, issues creados automáticamente cuando error budget se erosiona > X%.

43. **¿Qué ADRs vale documentar?**  
    Elecciones de arquitectura fundamentales (protocolos, storage, particionamiento, seguridad central), no detalles triviales (nombres de tablas). Criterio: costo de revertir vs costo de documentar.

44. **¿Consistencia fuerte vs eventual?**  
    Datos de autenticación crítica y revocaciones: fuerte. Estadísticas, analítica, caches: eventual. Evaluación por impacto de lectura obsoleta y frecuencia de actualización.

45. **¿Resiliencia ante dependencia gubernamental externa?**  
    Circuit breaker, cola de requests diferidos, fallback a modo degradado (marcar verificación pendiente), SLA diferenciado y alertas específicas.

46. **¿Medir impacto de optimizaciones JSON?**  
    Benchmark suite (BenchmarkDotNet) con payloads reales, medir CPU, asignaciones, latencia p95. Comparar baseline antes/después y registrar en changelog de performance.

47. **¿Prevenir replay attacks en endpoints biométricos?**  
    Nonces + expiración corta, timestamps firmados, tokens de un solo uso ligados a cliente + canal TLS, detección de duplicados en ventana temporal.

48. **¿Blue/green o canary sin fricción?**  
    Infra as code + router dinámico (peso de tráfico). Canary auto-promote si métricas (latencia, error rate, saturación) dentro de banda tras ventana. Rollback atómico, feature flags aislados.

49. **¿Cuadro de mando técnico líder?**  
    Panel único: SLOs (latencia, uptime), DORA metrics, error budget, adopción de versiones API, seguridad (vulns abiertas), coste unitario por transacción.

50. **¿Ruta de madurez plataforma hacia escala internacional?**  
    Niveles: (1) Fundacional (monolito modular + observabilidad básica), (2) Servicios clave extraídos + SLOs, (3) Automatización seguridad + performance continua, (4) Multi-región activa-activa, (5) Optimización costo / compliance avanzada. Métricas gates por nivel.

---

Si quieres: puedo generar versión flashcards, top 10 foco, o traducción al inglés.
