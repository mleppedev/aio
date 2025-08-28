# Q&A Entrevista Técnica (Biometrika – Líder de Desarrollo Fullstack)

> 50 preguntas con respuestas modelo. Personaliza con ejemplos reales (métricas, tecnologías exactas). Mantén respuestas 60–120 segundos máximo.

---

### 1. Arquitectura: ¿Cómo estructurarías una plataforma de verificación de identidad basada en microservicios?

Respuesta: Separaría dominios: enrolamiento, verificación, firma, gestión de plantillas biométricas, notificaciones y auditoría. API Gateway central (rate limiting / auth), servicio de orquestación para flujos compuestos, almacenamiento cifrado de plantillas, event bus (verificaciones.resultado) para desacoplar post-procesos. Observabilidad transversal (tracing + métricas). Seguridad: mTLS interno, JWT/OIDC en borde, políticas de acceso por rol. Escalabilidad horizontal para verificación y matching.

### 2. ¿Cuándo NO usarías microservicios?

Respuesta: Cuando el dominio aún es volátil, el equipo es pequeño, no hay necesidad de escalado independiente, el costo operativo superaría beneficios y no existen capacidades de observabilidad ni automatización maduras. Empezaría con un módulo bien separado (monolito modular) y evolucionaría incrementalmente.

### 3. Estrategia de versionado de APIs REST.

Respuesta: Versionado mayor en la ruta (/v1 /v2) sólo para breaking changes. Cambios menores / compatibles documentados. Uso de encabezados de depreciación y ventanas de coexistencia. Tests contract (consumidor) para detectar rupturas antes de deploy.

### 4. ¿Cómo asegurar idempotencia en operaciones de verificación?

Respuesta: Genero Idempotency-Key (hash de payload+documento) en cabecera; almaceno estado de procesamiento en tabla con TTL. Si llega repetido devuelvo resultado previo. Para publicación de eventos uso outbox + transacción local evitando duplicados.

### 5. Patrones de resiliencia aplicables.

Respuesta: Circuit Breaker (fallos externos), Retry exponencial con jitter (timeouts transitorios), Bulkhead (aislar pools), Timeout controlado, Fallback degradado (respuesta pending). Implementación con Polly centralizada vía delegating handlers.

### 6. Métricas clave iniciales.

Respuesta: Latencia P50/P95/P99 por endpoint crítico, tasa de errores (5xx / validación), throughput verificaciones/min, tiempo promedio enrolamiento, MTTR, éxito vs fallo de matching, cola pendiente, uso CPU/memoria y tiempo de ciclo CI→Prod.

### 7. ¿Cómo diseñar almacenamiento de plantillas biométricas seguro?

Respuesta: Plantilla cifrada con AES-256 (clave manejada por KMS), metadata mínima (no datos personales innecesarios), hash integridad (HMAC), segmentación por cliente, control de acceso estricto (RBAC + logs inmutables). Backup cifrado y rotación de llaves programada.

### 8. Minimización de datos personales.

Respuesta: Guardar sólo atributos indispensables, aplicar tokenización o pseudonimización cuando sea posible, establecer políticas de retención + borrado seguro, acceso bajo justificación, auditoría cada acceso.

### 9. Estrategia para latencia baja en verificación.

Respuesta: Caching de plantillas calientes, pre-carga de modelos, pool conexiones reutilizable, compresión selectiva, reducir saltos de red (colocalizar servicios), asíncrono para integraciones lentas, análisis profiling para hotspots (allocations / GC). Métrica objetivo P95 <500ms.

### 10. ¿Cómo manejarías picos de tráfico repentinos?

Respuesta: Autoescalado basado en QPS y longitud de cola, circuit breaker preventivo hacia dependencias lentas, priorización (cola diferenciada), graceful degradation (respuestas 202 + procesamiento async), warm pools. Pruebas de carga periódicas.

### 11. Diferencia Blazor Server vs WASM y elección.

Respuesta: Server mantiene circuito SignalR (baja carga inicial, dependencia latencia). WASM ejecuta cliente (mayor payload inicial, offline parcial). Para panel interno con control de acceso y cambios rápidos: Blazor Server. Para experiencia pública sin dependencia constante: WASM.

### 12. Gestión de estado en Blazor.

Respuesta: Inyectar state containers scoped, preservar estado crítico en local storage seguro (encriptado si sensible), invalidar en logout, evitar singletons con data mutable compartida no thread-safe.

### 13. Estrategia CI/CD.

Respuesta: Pipeline multi-stage: build → tests (unit + contract) → análisis estático → seguridad (SAST/secret scan) → package → deploy canary → monitoreo métricas → promoción automática si umbrales OK. Infra como código versionada.

### 14. ¿Cómo validarías cambios en performance pre-deploy?

Respuesta: Benchmarks con BenchmarkDotNet para funciones críticas, pruebas de carga (k6 / JMeter) en entorno staging representativo, comparación baseline vs nuevo (latencia y percentiles), gating si regresión > umbral.

### 15. Control de configuraciones sensibles.

Respuesta: Variables en vault (Azure Key Vault / HashiCorp), nunca en repositorio, rotación automática, acceso mediante políticas identity-managed, auditoría de cada lectura.

### 16. Detección de incidentes temprana.

Respuesta: Alertas basadas en SLO burn rate, dashboards golden signals, tracing sampling adaptativo ante errores, logs estructurados con correlation id y spike detection anomaly metrics.

### 17. Ejecución de migración monolito→microservicios sin gran downtime.

Respuesta: Strangler pattern: extraer endpoints por contexto, anti-corruption layer, routing progresivo, monitoreo comparativo (shadow traffic), desactivar componentes monolito gradualmente.

### 18. ¿Cómo aplicarías Domain-Driven Design?

Respuesta: Identificar bounded contexts (verificación, enrolamiento, firma, auditoría), definir ubiquitous language con negocio, mapear agregados (Persona, Verificación), usar eventos de dominio para integraciones y claridad transaccional.

### 19. Gestión de deuda técnica.

Respuesta: Inventario visible (matriz impacto/riesgo), porcentajes sprint reservados (ej 15%), quick wins priorizados en función de riesgo operativo y costo futuro, métricas (sonar issues, coverage) para justificar.

### 20. Estrategia de logging eficiente.

Respuesta: Logging estructurado (JSON) con nivel adecuado (Info vs Debug). Correlation ID propagado. Reducción de ruido mediante filtros. Retención y envío a sink central (ELK / OpenSearch). PII masked.

### 21. ¿Cómo implementas tracing distribuido?

Respuesta: OpenTelemetry SDK en cada servicio, export a collector → backend (Jaeger / Tempo / App Insights). Inyección de traceparent header desde gateway. Span para operaciones externas y DB. Métricas derivadas de spans.

### 22. Prevención de regresiones funcionales.

Respuesta: Battery tests: unitarios críticos, tests contract, smoke end-to-end, feature flags para rollout gradual, monitoreo post-deploy comparativo (error budget impacto).

### 23. Manejo de concurrencia en .NET.

Respuesta: Preferir async/await, evitar locks innecesarios, usar canales (System.Threading.Channels) para productor/consumidor, políticas de retry idempotentes, cuidado con contención en singletons.

### 24. Optimización de GC.

Respuesta: Evitar allocations en loops calientes, usar pooling (ArrayPool<T>), structs readonly cuando aplica, Span<T> / Memory<T>, reducción de boxing. Medir con dotnet-counters y PerfView antes y después.

### 25. Estrategia de pruebas.

Respuesta: Pirámide: Unit (rápidos, 70%), Contract tests (interfaces externas), Integration (DB real minimizados), End-to-End smoke. Incluyo tests de resiliencia (simulación fallos) y performance básicos.

### 26. Manejo de secretos en CI.

Respuesta: Usar service connections y variables secure del vault, no exponer en logs (masking), rotación programada, separar credenciales por ambiente (principle least privilege).

### 27. Control de acceso interno entre microservicios.

Respuesta: mTLS + identidad por servicio (cert), autorización basada en claims (scope) validada en gateway o sidecar. Minimizar tokens de larga duración.

### 28. Estrategia antifraude básica.

Respuesta: Correlación de patrones (frecuencia intentos, IP geolocalizada, device fingerprint), reglas + modelo ML (anomaly score), flag manual review, almacenamiento de features anónimas.

### 29. Reducción de cold starts / warmup.

Respuesta: Pre-warming contenedores (startup probes + triggers), reducir reflection heavy code, inicializar caches críticos al arrancar, aplicar Ready/Liveness probes correctos.

### 30. Gestión de dependencias externas lentas.

Respuesta: Timeout + retry con backoff, circuito abierto con fallback (cola diferida), caching de resultados si posible, monitor latencia y error rate dedicado.

### 31. ¿Cómo justificarías uso de eventos vs colas directas?

Respuesta: Eventos promueven desac acoplamiento (publicar hecho, múltiples consumidores), escalan nuevos casos sin cambiar productor. Colas directas acoplan semántica consumidor único. Uso ambos según semántica (evento vs comando).

### 32. Seguridad en firmas electrónicas.

Respuesta: Integridad (hash documento antes y después), sello de tiempo confiable, certificados válidos (OCSP/CRL check), almacenamiento seguro claves privadas (HSM/KMS), logs no repudiables.

### 33. ¿Cómo auditarías accesos a datos biométricos?

Respuesta: Log append-only con: quién, cuándo, motivo, hash plantilla; firma criptográfica lote (Merkle / hash chain) para detectar alteraciones. Revisión periódica.

### 34. Mitigación de ataques de fuerza bruta.

Respuesta: Rate limiting por IP / usuario, captcha adaptativo, bloqueo progresivo, monitoreo comportamiento y alerta anómala.

### 35. Estrategia de rollback seguro.

Respuesta: Deploy canary + monitoreo; si KPI de error supera umbral, automatizar rollback (última imagen estable). DB changes backwards-compatible (expand/contract). Feature flags para apagado inmediato.

### 36. Cómo evalúas un change request urgente.

Respuesta: Confirmo severidad/impacto, reviso riesgo técnico, plan test mínimo, comunicación stakeholders, despliegue controlado y post-mortem corto si era incidente.

### 37. Liderazgo técnico efectivo.

Respuesta: Claridad en objetivos, decisiones documentadas (ADR), code reviews colaborativas, mentorship, métricas de flujo (lead time, WIP) para ajustes.

### 38. Gestión de conflicto técnico en equipo.

Respuesta: Centrar debate en criterios (performance, mantenibilidad, costo), prototipo pequeño comparativo, decisión documentada y retro posterior.

### 39. Qué priorizas primero al llegar al rol.

Respuesta: Mapa arquitectura actual, health baseline (latencia / errores), vulnerabilidades obvias, pipeline CI/CD, quick wins de confiabilidad y backlog de riesgos.

### 40. Integración AI responsable.

Respuesta: Limitar exposición de PII, usar abstracciones (gateway AI), logging redactado, evaluación de sesgos, fallback en caso de timeout modelo, monitoreo de drift.

### 41. Cómo asegurar consistencia eventual aceptable.

Respuesta: Definir SLA de propagación, emitir eventos con versionado, compensación (sagas) para fallos parciales, idempotencia en consumidores, auditoría de reconciliación periódica.

### 42. Manejo de colas saturadas.

Respuesta: Backpressure (limitar productores), escalado consumidores, priorización (colas separadas), dead-letter queue con análisis y reintentos controlados.

### 43. Gestión de secretos rotos.

Respuesta: Revocación inmediata credencial, rotación masiva, invalidar tokens derivados, análisis alcance, monitoreo intensivo post-rotación, lecciones aprendidas.

### 44. Métrica para evaluar mejora en CI/CD.

Respuesta: Lead time (commit→prod), deployment frequency, change fail rate, MTTR. Meta: reducir lead time y mantener change fail rate bajo (<15%).

### 45. Diferencia entre autenticación y autorización.

Respuesta: Autenticación: verificar identidad (quién eres). Autorización: qué puede hacer. Implemento OIDC para auth, RBAC/ABAC para autorización granular.

### 46. Hardening de contenedores.

Respuesta: Imágenes mínimas (distroless), escaneo vulnerabilidades automático, usuario no root, sólo puertos necesarios, firmas de imágenes (cosign), políticas runtime (seccomp/AppArmor).

### 47. Estrategia para pruebas de carga continua.

Respuesta: Escenarios representativos automatizados en pipeline nocturno, comparación contra baseline, thresholds definidos; si regresión se marca tarea.

### 48. Cómo manejar dependencia legacy sin contrato claro.

Respuesta: Crear wrapper/anti-corruption layer, definir contrato explícito, tests simulando comportamientos actuales, plan de aislar para futura sustitución.

### 49. Uso de feature flags.

Respuesta: Separar despliegue de release funcional, permitir A/B, rollback instantáneo sin redeploy, gradual rollout. Gestión centralizada y limpieza flags expiradas.

### 50. KPI que mostrarías al negocio.

Respuesta: Tiempo onboarding usuario, tasa verificaciones exitosas, tiempo promedio firma, incidentes críticos/mes, cycle time de features, ahorro costos infra por optimizaciones.

### 51. Cómo decidir entre gRPC y REST en tu ecosistema.

Respuesta: REST para interoperabilidad amplia y simplicidad pública; gRPC entre servicios internos de alto volumen por menor overhead y streaming bidireccional. Métricas / tracing estandarizados; fallback REST si cliente no soporta HTTP/2.

### 52. Estrategia para migrar endpoints síncronos a async.

Respuesta: Identificar bloqueos I/O (DB, HTTP), introducir métodos async incrementalmente empezando por capas de infraestructura, ajustar controladores, propagar CancellationToken, medir uso threads vs throughput.

### 53. Mitigación N+1 queries.

Respuesta: Proyección selectiva (SELECT columnas necesarias), incluir relaciones via explicit include, caching agregado, batch queries, o mover a CQRS read model optimizado.

### 54. Validación de entrada robusta.

Respuesta: FluentValidation / DataAnnotations, reglas centralizadas, short-circuit, devolver 400 estructurado, sanitización, limitar tamaños (payload, arrays) para proteger recursos.

### 55. Estrategia para multicliente (multi-tenant).

Respuesta: Aislamiento por base de datos (clientes grandes) y schema/shared para pequeños; separación lógica de claves, cifrado por tenant, limit quotas, auditoría segregada.

### 56. Control de versiones de esquemas DB sin downtime.

Respuesta: Expand/contract: agregar columnas nuevas, backfill, actualizar código para usar ambas, limpiar columnas obsoletas luego. Uso de migraciones automatizadas en pipeline y bandera de compatibilidad.

### 57. Prevención de deadlocks en SQL.

Respuesta: Acceso tablas en orden consistente, transacciones cortas, índices adecuados, aislamiento apropiado (READ COMMITTED SNAPSHOT), monitoreo y tuning queries.

### 58. Estrategia de índices.

Respuesta: Índices cubrientes para queries críticas, evitar sobre-indexación, monitoreo de fragmentación, revisar planes ejecución y eliminar índices no usados periódicamente.

### 59. Uso de caché distribuido seguro.

Respuesta: Redis con TLS, autenticación, expiración acorde (TTL), invalidación selectiva por keys derivadas, fallback a fuente si fallo caché, métrica de hit ratio.

### 60. Diferencia entre escalado vertical y horizontal.

Respuesta: Vertical añade recursos a una instancia (rápido pero límite físico). Horizontal agrega instancias (resiliencia y escalabilidad). Prefiero horizontal en microservicios stateless.

### 61. ¿Qué es un sidecar y cuándo usarlo?

Respuesta: Proceso auxiliar en el mismo pod contenedor que provee funcionalidad transversal (mTLS, logging, proxy). Útil en service mesh para abstraer concerns no funcionales.

### 62. Gestión de secretos en desarrollo local.

Respuesta: Archivo .env cifrado o gestor (Doppler / Vault dev), nunca commiteado, inyección por variables entorno, rotación simulada y principios mínimos para evitar fuga.

### 63. Enfoque para testear código dependiente de tiempo.

Respuesta: Abstraer reloj (IClock), inyección en tests con tiempo fijo o virtual, usar frameworks de time travel, evitar DateTime.UtcNow directo.

### 64. Manejo de tareas largas.

Respuesta: Procesamiento async en background workers (HostedService), colas de trabajo, estado consultable vía endpoints, cancelación y reintentos configurables.

### 65. Qué aportarías a observabilidad existente.

Respuesta: Normalización naming métricas, correlación logs-traces-metrics, SLO dashboards con error budgets, alertas semánticas (negocio), runbooks claros.

### 66. Tratamiento de excepciones global.

Respuesta: Middleware capturador, mapping a códigos HTTP, log estructurado (stack + correlation id), ocultar detalles sensibles, métricas de tasa error.

### 67. Estrategia para introducir GraphQL (si pidieran).

Respuesta: Caso: agregación múltiple de vistas cliente. Gateway GraphQL encima de microservicios, control de profundidad y rate limiting, persisted queries para performance.

### 68. Control de calidad en PRs.

Respuesta: Plantilla PR (cambio, riesgo, rollback), validaciones automáticas (tests, lint, security scan), revisión par, check list de criterios Definition of Done.

### 69. Management de feature toggles acumulados.

Respuesta: Registrar metadata (owner, expiración), auditoría, limpieza en releases programadas, impedir despliegue si toggle expirado sigue activo.

### 70. Diseño para auditoría inmutable.

Respuesta: Almacén append-only (WORM), hashing encadenado, firma periódica, replicación, acceso sólo escritura por API controlada.

### 71. Métricas para pipeline de verificación de identidad.

Respuesta: Tiempo total verificación, match score distribución, tasa falsos positivos/negativos (si disponible), latencia por fuente externa, porcentaje reintentos.

### 72. Cómo reducir costo cloud sin degradar SLAs.

Respuesta: Rightsizing, autoscaling adecuado, caching resultados caros, apagar entornos fuera horario, observabilidad para detectar sobreprovisión, uso spot/ahorro reservas.

### 73. Estrategia para migrar a OpenTelemetry.

Respuesta: Inventario instrumentaciones, capa abstracción logging/tracing actual, introducir OTEL dual export, validar paridad, retirar librerías legacy progresivamente.

### 74. Gestión de claves criptográficas.

Respuesta: KMS/HSM central, rotación programada, separación de roles (uso vs administración), logging acceso, versionado de claves y rollback plan.

### 75. Minimizar impacto de GC en alta carga.

Respuesta: Configuración Server GC, reducir LOH allocations, object pooling, Span<T>, evitar string concatenations en loops (StringBuilder / interpolated handler).

### 76. Hardening de Kubernetes básico.

Respuesta: Namespaces separados, RBAC estricta, network policies, pod security standards, imágenes firmadas, escaneo continuo, resource limits.

### 77. Prevención de secret leakage en logs.

Respuesta: Sanitización central (middleware), listas de patrones, pruebas automáticas (scan) en pipeline, política no-log de ciertos headers.

### 78. Definición de SLIs y SLOs.

Respuesta: SLIs: latencia <X ms P95, error rate <Y%, disponibilidad. SLOs: objetivos medibles sobre ventana fija. Error budget gestiona frecuencia deploys.

### 79. Estrategia de Blue-Green vs Canary.

Respuesta: Blue-Green para cambios grandes con rollback instantáneo; canary para despliegue gradual basado en métricas. Selección según riesgo y capacidad monitoreo.

### 80. Generación de documentación técnica viva.

Respuesta: ADRs en repo, diagramas generados (Structurizr/PlantUML), sync automática de OpenAPI, lint de docs en pipeline, ownership explícito.

### 81. Control de saturación de thread pool.

Respuesta: Monitoreo de eventos (dotnet-counters), minimizar operaciones bloqueantes, usar async verdadero, partitioning de trabajo intensivo en CPU.

### 82. Estrategia para compatibilidad hacia atrás en eventos.

Respuesta: Sólo agregar campos opcionales, no cambiar semántica, versionar canal si ruptura, consumidores tolerantes (ignore unknown), contrato Schemas (Schema Registry).

### 83. Gestión de colisiones en caching distribuido.

Respuesta: Namespacing por dominio/versión, hashes seguros en keys largas, política de invalidación jerárquica. Detección early de hot keys.

### 84. Minimizar latencia en acceso a base biométrica.

Respuesta: Caching de índices, compresión optimizada de plantillas, acceso paralelo segmentado, prefetch heurístico y almacenamiento en memoria para top N.

### 85. Procedimiento post-mortem.

Respuesta: Recolección timeline objetiva, análisis causa raíz (5 whys / fishbone), acciones correctivas SMART, no-blame, seguimiento y verificación cierre.

### 86. Uso de health checks avanzados.

Respuesta: Liveness (proceso vivo), readiness (dep listo), health detallado (DB, cola, dependencias externas), degradación escalonada y endpoints no expuestos públicamente.

### 87. Estrategia de cifrado de datos en tránsito interno.

Respuesta: mTLS entre pods/servicios mediante mesh (cert rotación automática), TLS termination solo en borde, políticas que impiden plaintext.

### 88. Manejo de ficheros grandes en API.

Respuesta: Streaming (IAsyncEnumerable / HttpResponse.BodyWriter), validación temprana tamaño, almacenamiento temporal cifrado, procesamiento asíncrono.

### 89. Estrategia de paginación eficiente.

Respuesta: Keyset pagination para grandes datasets (WHERE > lastId), evitar OFFSET costoso, incluir índices adecuados, proveer token de continuación.

### 90. Prevenir overfetching/underfetching sin GraphQL.

Respuesta: Endpoints específicos por caso uso, parámetros fields select, proyecciones DTO, caching agresivo de vistas agregadas.

### 91. Limpieza de recursos huérfanos.

Respuesta: Jobs programados (cron) para detectar registros sin referencia, metricar cantidad, soft delete→purge según política retención.

### 92. Estrategia de sandbox para integraciones externas.

Respuesta: Ambiente aislado con datos ficticios, contratos estables, simulación latencias/errores, test contract automatizados.

### 93. Minimizar lock contention.

Respuesta: Diseñar estructuras lock-free (ConcurrentDictionary), granularidad fina, uso de Interlocked para contadores, evitar locks anidados.

### 94. Gestión de migraciones fallidas.

Respuesta: Transacciones por paso, chequeo precondiciones, rollback script automático, monitoreo y alerta inmediata, feature flag para activar nuevas columnas.

### 95. Tratamiento de colas dead-letter.

Respuesta: Métricas de tasa DLQ, proceso de reintento manual/automático tras análisis causa, clasificación (bug, datos, transitorio) y acciones preventivas.

### 96. Trazabilidad de una operación de verificación end-to-end.

Respuesta: Correlation ID generado en borde propagado en headers, logging estructurado, spans por cada servicio, timeline en herramienta tracing.

### 97. Gestión de configuración dinámica.

Respuesta: Servicio central (config server) con watchers, recarga en caliente (IOptionsSnapshot), versionado y rollback rápido, auditoría de cambios.

### 98. Reducción de costo en almacenamiento logs.

Respuesta: Niveles ajustados (Error/Warn), retención escalonada (quente corto, frío comprimido), sampling logs de info redundante, estructuración para compresión eficaz.

### 99. Métricas de equipo que observarías.

Respuesta: Lead time, throughput (items completados), WIP, tasa retrabajo, satisfacción equipo, defectos post-release.

### 100. Señales de éxito en primeros 90 días.

Respuesta: Mapa arquitectura validado, SLOs definidos, pipeline mejorado, incidentes críticos reducidos, decisiones arquitectónicas documentadas, backlog de deuda priorizado.

---

Fin del documento.
