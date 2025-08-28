# Entrevista Técnica 23people – Biometrika (Líder de Desarrollo Fullstack)

> Documento de preparación. Fecha preparación: 19-08-2025. Rol: Líder de Desarrollo Fullstack (C# / .NET / Microservicios / Blazor / APIs / CI-CD). Basado en la oferta (`oferta-laboral.md`) y dossier (`biometrika.md`). Ajustar ejemplos con tu experiencia real.

---

## 1. Objetivo Probable de la Entrevista (con 23people)

23people buscará evidencias de:

- Dominio técnico transversal (.NET, arquitectura, prácticas modernas DevOps / DevSecOps).
- Capacidad de liderazgo técnico y comunicación clara con stakeholders no técnicos.
- Madurez en decisiones de diseño (trade-offs) y visión de producto.
- Experiencia gestionando performance, seguridad y resiliencia en producción.
- Pensamiento estructurado (cómo aterrizas problemas abiertos a soluciones concretas).

Formato típico: introducción breve → preguntas de background → deep-dive técnico → ejercicio (arquitectura o código) → preguntas del candidato.

---

## 2. Elevator Pitch Personal (Plantilla)

"Soy [AÑOS] años de experiencia en desarrollo backend y liderazgo técnico, especializado en .NET y arquitectura de microservicios. He liderado equipos de hasta [N] personas entregando productos con [Métricas clave: latencia, uptime, reducción de costos]. Me enfoco en construir plataformas seguras y escalables para flujos de identidad y transacciones críticas, aplicando prácticas CI/CD, observabilidad y seguridad desde el diseño. Busco aportar en Biometrika consolidando la plataforma de verificación e impulsando estándares técnicos y velocidad de entrega."

Personaliza con 2 métricas cuantificables (ej: "reduje el MTTR 40%" / "disminuí costos infra 25% aplicando caching").

---

## 3. Historia Profesional en Formato STAR/CAR (Esqueleto)

| Situación        | Tarea                            | Acción                                               | Resultado (cuantifica)                   |
| ---------------- | -------------------------------- | ---------------------------------------------------- | ---------------------------------------- |
| Monolito lento   | Migrar a microservicios críticos | Diseñé partición dominio + colas event-driven        | Latencia -55%, deploys semanales→diarios |
| Incidentes Sev1  | Mejorar resiliencia              | Implementé circuit breakers (Polly) + SLO dashboards | MTTR 4h→50min, fallos Sev1 -60%          |
| Lento onboarding | Optimizar pipeline QA            | Introduje pruebas contract + análisis estático       | Tiempo ciclo 10d→3d                      |

Prepara 3–4 historias fuertes.

---

## 4. Logros Cuantificables (Bullets de Impacto)

- Reduje [X] (latencia / costos / errores) en [Y]% mediante [acción técnica concreta].
- Implementé pipeline CI/CD (GitHub Actions / Azure DevOps) que pasó de 1 release / mes a [N] releases / semana.
- Aumenté cobertura pruebas de [A]% a [B]% reduciendo bugs en producción [C]%.
- Diseñé estrategia de caching (Redis + invalidación selectiva) que ahorró [$$ / % CPU].

Fórmula: Acción técnica + Métrica base → Resultado cuantificado + Horizonte temporal.

---

## 5. Mapeo Stack Oferta ↔ Tu Experiencia

| Oferta         | Qué Debes Demostrar                    | Frase Clave Preparada                                        |
| -------------- | -------------------------------------- | ------------------------------------------------------------ |
| C#, .NET       | Profundidad en async, DI, profiling    | "Profiling con dotTrace y optimización allocations"          |
| Microservicios | Bounded contexts, resiliencia, tracing | "Uso de Correlation IDs + OpenTelemetry"                     |
| Blazor         | Componentización, estado, prerender    | "Separé componentes reutilizables + validé rendimiento WASM" |
| API Rest       | Versionado, idempotencia, seguridad    | "Diseñé estrategia v1/v2 + ETags y rate limiting"            |
| CI/CD          | Pipelines multi stage, gates           | "Canary deploy + rollback automático por métricas"           |
| Seguridad      | OWASP, cifrado, secrets                | "Secret rotation automatizada + CSP headers"                 |

Completa con casos reales.

---

## 6. Preguntas Técnicas Probables y Enfoque de Respuesta

| Tema           | Pregunta Potencial                                                   | Cómo Responder (Framework)                                                             |
| -------------- | -------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Arquitectura   | ¿Cómo diseñarías un servicio de verificación de identidad escalable? | 1) Reqs F/NF 2) Componentes 3) Flujos 4) Datos 5) Resiliencia 6) Seguridad 7) Métricas |
| Microservicios | ¿Cómo manejas consistencia?                                          | Explicar eventual consistency, eventos, outbox pattern, idempotencia                   |
| Performance    | ¿Detectas y reduces latencia?                                        | APM + tracing → identificar hot spots → caching/item potent ops → benchmark            |
| Seguridad      | ¿Protección datos biométricos?                                       | Cifrado en reposo (AES-256), en tránsito (TLS1.2+), segregación, minimización datos    |
| Blazor         | Server vs WebAssembly trade-offs                                     | Latencia / estado en server, payload inicial, prerender, seguridad código              |
| CI/CD          | ¿Estrategia despliegue seguro?                                       | Branching, tests (unit, contract, integration), canary, observabilidad gating          |
| Observabilidad | ¿Qué métricas y logs defines primero?                                | Golden signals (latencia, tráfico, errores, saturación) + KPIs negocio                 |
| Failover       | Manejo de degradación                                                | Circuit breakers, bulkheads, retries exponenciales + jitter                            |
| Data           | ¿Versionas contratos?                                                | SemVer en endpoints + tests contract consumidores                                      |
| Liderazgo      | ¿Cómo alineas al equipo?                                             | Objetivos trimestrales + ADR + pair reviews                                            |

Practica verbalmente con tiempo (2–3 min por respuesta compleja).

---

## 7. Ejercicio de Diseño: Servicio de Verificación (Plantilla)

1. Requerimientos Funcionales: enrolar usuario, capturar biometría, verificar contra fuentes externas, responder score.
2. No Funcionales: P95 < 500ms (cache hits), disponibilidad ≥99.9%, auditabilidad, cumplimiento legal.
3. Componentes: API Gateway → Auth → Orquestador → Adaptadores (Registro Civil, base biométrica) → Motor de Matching → Storage (Plantillas cifradas) → Event Bus (emite eventos de verificación) → Observabilidad.
4. Datos: Tabla Personas, Plantillas (blob cifrado, metadata hash), Logs auditoría WORM.
5. Resiliencia: Retry con backoff + circuit breakers (Polly), cola outbox para integraciones externas.
6. Seguridad: Mutual TLS entre servicios, Rotación keys KMS, segregación RBAC.
7. Escalabilidad: Stateless services autoscale métricas CPU + QPS; caché caliente de plantillas más usadas.
8. Métricas: Latencia P50/P95, tasa fallos externos, throughput match/min, tiempo enrolamiento.
9. Riesgos: falsos positivos/negativos (calibración thresholds), picos de tráfico (pre-calentamiento), dependencia externa lenta (bulkhead + fallback degraded response).

Ten un diagrama mental (capas + flujos). Si piden, dibuja por bloques.

---

## 8. Fragmento OpenAPI (Ejemplo Resumido)

```yaml
paths:
	/verificaciones:
		post:
			summary: Inicia verificación identidad
			requestBody:
				required: true
				content:
					application/json:
						schema:
							type: object
							required: [documento, biometria]
							properties:
								documento: { type: string }
								biometria: { type: string, description: "Template base64" }
			responses:
				'202': { description: Aceptada (proceso async) }
				'400': { description: Datos inválidos }
				'401': { description: No autorizado }
	/verificaciones/{id}:
		get:
			summary: Estado verificación
			responses:
				'200': { description: OK }
				'404': { description: No encontrada }
```

Usa 202 para procesos async y callback / polling.

---

## 9. Patrones Clave en .NET que Debes Nombrar Naturalmente

- Dependency Injection (con IServiceCollection) y configuración por ambiente.
- Async/Await correcto (sin .Result / .Wait). Cancelación con CancellationToken.
- Resiliencia: Polly (Retry + CircuitBreaker + Timeout + Fallback).
- Minimal APIs vs Controllers (trade-offs simplicidad vs organización).
- Logging estructurado (Serilog / ILogger) + CorrelationId middleware.
- DTO vs Domain Models y AutoMapper (moderación).
- Source Generators (posible optimización) y Span<T> para performance puntual.

---

## 10. Algoritmos / Data Structures que Pueden Aparecer (Breve)

- Hashing / compare (para fingerprints de plantillas / deduplicación).
- Rate limiting (token bucket / leaky bucket) conceptual.
- Búsqueda eficiente (índices, caching LRU en capa aplicación).
- Cálculo percentiles (histogramas) para monitoreo.
- Idempotencia: claves compuestas (Idempotency-Key + hash payload).

Prepárate para escribir una función limpia asincrónica y testable.

---

## 11. Seguridad y Cumplimiento – Puntos a Mencionar

- OWASP Top 10 (inyección, auth rota, exposición datos sensibles).
- Cifrado at-rest (AES-256) y in-transit (TLS 1.2+), hashing Argon2 / PBKDF2 para credenciales.
- Segregación ambientes (dev/test/prod), principle of least privilege.
- Rotación periódica de secretos (Vault / KMS).
- Audit logging inmutable (append-only).
- Data minimization + retención definida.
- Firma electrónica: sellado de tiempo, cadena de certificados, no repudio.

---

## 12. Observabilidad / Métricas

Golden Signals: latencia, tráfico, errores, saturación.
KPIs negocio: verificaciones/min, tasa éxito, tiempo onboarding.
Prácticas: tracing distribuido (OpenTelemetry), métricas custom (prometheus counters + histograms), dashboards SLO.
Alertas basadas en burn rate (SLO error budget).

---

## 13. Liderazgo / Gestión

- Alinear backlog con objetivos trimestrales (OKRs / roadmaps).
- Code reviews enfocadas en: legibilidad, riesgos, seguridad, test coverage.
- Mentoring: pairing rotativo + tech talks cortas.
- Gestión de deuda técnica con registro visible (matriz impacto vs esfuerzo).

---

## 14. Preguntas Inteligentes para el Entrevistador

1. ¿Cómo miden actualmente el éxito técnico (SLOs, MTTR, throughput)?
2. ¿Estado de la migración / modernización (si aplica) y principales bloques?
3. ¿Qué retos de escalado o seguridad han sido más difíciles el último año?
4. ¿Cómo priorizan features vs deuda técnica?
5. ¿Qué herramientas tienen para observabilidad y qué falta?
6. ¿Mecanismos de retroalimentación entre soporte / comercial y desarrollo?

---

## 15. Red Flags a Detectar

- Falta total de métricas / monitoreo.
- Deploys manuales y poco frecuentes.
- Ausencia de controles de seguridad básicos (secretos en código, sin revisión).
- Ninguna definición de niveles de severidad en incidentes.
- Tensión constante negocio vs calidad sin mediación.

---

## 16. Plan 30-60-90 (Borrador a Mencionar si Preguntan)

| Día   | Enfoque                   | Entregables                                                 |
| ----- | ------------------------- | ----------------------------------------------------------- |
| 0–30  | Comprensión / Auditoría   | Mapa servicios, métricas base, quick wins seguridad         |
| 30–60 | Estabilización / Mejora   | Implementar 1 pipeline CI/CD mejorado + tracing distribuido |
| 60–90 | Escalamiento / Estándares | Roadmap arquitectura, guías coding, objetivos SLO definidos |

---

## 17. Checklist Pre-Entrevista

- [ ] Elevator pitch practicado (<60 seg)
- [ ] 3 historias STAR con métricas
- [ ] Ejercicio de arquitectura ensayado (voz en 8–9 pasos)
- [ ] Repaso patrones resiliencia (Polly) y async/await
- [ ] Argumentos seguridad y protección datos biométricos
- [ ] Preguntas inteligentes preparadas
- [ ] Ambiente tranquilo, notas impresas / visibles

---

## 18. Mini Guion para Ejercicio de Arquitectura (Prompt Mental)

1. Confirmo alcance → 2. Enumero requisitos F/NF → 3. Propongo componentes → 4. Profundizo datos → 5. Resiliencia → 6. Seguridad → 7. Escalado → 8. Observabilidad → 9. Trade-offs → 10. Cierre (KPIs).

---

## 19. Estrategia si Piden Live Coding (C#)

1. Replantea el problema brevemente.
2. Define inputs/outputs y casos borde (null, vacío, volumen grande, concurrencia).
3. Escribe tests mentales / comenta casos antes de código.
4. Implementa primero versión clara, luego micro-optimizas (solo si sobra tiempo).
5. Explica complejidad Big-O y posibles mejoras.

---

## 20. Nota Final

Mantén respuestas estructuradas pero conversacionales. Evita sobre-ingeniería verbal: cierra cada respuesta con impacto o métrica. Verifica supuestos antes de profundizar ("Asumiendo que... si no, lo abordaría así").

Éxito.
