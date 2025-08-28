# Glosario Técnico – Biometrika / Entrevista Líder de Desarrollo

> Referencia rápida de términos mencionados en documentos de preparación (oferta, dossier, Q&A). Agrupado por categorías. Fecha: 19-08-2025.

---

## Arquitectura / Patrones

**Microservicio:** Servicio pequeño autónomo alineado a un bounded context que se despliega independientemente.
**Monolito Modular:** Monolito con separación lógica interna clara; punto de partida antes de dividir en microservicios.
**Bounded Context (DDD):** Límite explícito de un modelo de dominio con lenguaje ubicuo consistente.
**Strangler Pattern:** Migración incremental rodeando sistema legacy y reemplazándolo por partes.
**CQRS:** Separación de comandos (escrituras) y queries (lecturas) para optimizar escalado y modelos distintos.
**Event-Driven Architecture:** Estilo donde eventos (hechos pasados) disparan reacciones desacopladas.
**Saga:** Coordinación distribuida de transacciones mediante pasos y compensaciones.
**Outbox Pattern:** Persistir evento en tabla outbox dentro de la misma transacción antes de publicarlo para evitar pérdida/doble envío.
**Anti-Corruption Layer:** Capa de traducción entre un sistema limpio y uno legacy para aislar complejidad.
**Service Mesh:** Capa de red que agrega funcionalidades (mTLS, retries, observabilidad) vía sidecars.

## Comunicación / APIs

**API Gateway:** Punto de entrada unificado que aplica auth, rate limiting, routing y observabilidad.
**REST:** Estilo arquitectónico basado en recursos, verbos HTTP y representación estándar (JSON, etc.).
**gRPC:** Framework RPC de alto rendimiento sobre HTTP/2 con contratos protobuf.
**Idempotencia:** Propiedad de una operación que puede ejecutarse múltiples veces con el mismo resultado.
**Rate Limiting:** Control de número de solicitudes permitidas en un intervalo.
**Backpressure:** Estrategia para frenar productores cuando consumidores no alcanzan demanda.
**Contract Test:** Prueba que valida que proveedor y consumidor de un API mantienen un contrato estable.
**OpenAPI (Swagger):** Especificación descriptiva para definir interfaces REST.

## Seguridad / Cumplimiento

**Autenticación (AuthN):** Verificación de identidad de un sujeto.
**Autorización (AuthZ):** Determinar qué acciones puede ejecutar un sujeto autenticado.
**OIDC (OpenID Connect):** Capa de identidad sobre OAuth2 para auth federada.
**RBAC:** Control de acceso basado en roles predefinidos.
**ABAC:** Control de acceso basado en atributos (usuario, recurso, contexto).
**mTLS:** TLS mutuo donde cliente y servidor presentan certificados.
**HSM:** Hardware Security Module para custodia de claves criptográficas.
**KMS:** Key Management Service (gestión y rotación de claves en cloud).
**PII:** Información Personal Identificable.
**Data Minimization:** Principio de recolectar sólo datos estrictamente necesarios.
**Encryption at Rest/In Transit:** Cifrado de datos almacenados / en tránsito.
**Firma Electrónica Avanzada:** Firma con identidad verificada, control exclusivo y detecta alteraciones (Ley 19.799 Chile).
**No Repudio:** Garantía de que autor no puede negar la firma/acción.
**Zero Trust:** Modelo que asume ninguna red como confiable por defecto.

## Biometría / Identidad

**Enrolamiento:** Proceso inicial de captura de datos biométricos de un usuario.
**Verificación 1:1:** Comparación de una muestra biométrica contra una plantilla declarada (¿eres quien dices ser?).
**Identificación 1:N:** Búsqueda de una muestra contra un conjunto grande de plantillas.
**Template Biométrico:** Representación compacta de características biométricas (no imagen en bruto).
**Liveness Detection:** Validación de que la muestra proviene de un sujeto vivo (anti spoofing).
**False Acceptance Rate (FAR):** Probabilidad de aceptar a un impostor.
**False Rejection Rate (FRR):** Probabilidad de rechazar a un usuario legítimo.
**Threshold:** Umbral de score que decide aceptación o rechazo.
**Score Matching:** Valor numérico que representa similitud entre muestras.

## Observabilidad / Operaciones

**Logging Estructurado:** Logs con pares clave-valor que facilitan búsqueda y agregación.
**Tracing Distribuido:** Seguimiento de una transacción a través de múltiples servicios mediante spans.
**Span:** Segmento individual de trabajo con timestamp y metadata.
**Metrics / SLIs:** Indicadores cuantificables de comportamiento (latencia, error rate, throughput).
**SLO:** Objetivo cuantitativo sobre un SLI (ej: latencia P95 < 500ms).
**Error Budget:** Margen de errores permitido antes de frenar cambios.
**MTTR:** Mean Time To Recovery (promedio para restaurar servicio).
**Canary Release:** Despliegue gradual a un subconjunto de tráfico.
**Blue-Green Deployment:** Dos entornos idénticos alternando tráfico para releases y rollbacks rápidos.
**Feature Flag:** Control runtime para activar/desactivar funcionalidad sin redeploy.
**Health Check (Liveness/Readiness):** Endpoints para indicar si proceso está vivo / listo para tráfico.

## Rendimiento / Escalabilidad

**Escalado Horizontal:** Agregar más instancias para aumentar capacidad.
**Escalado Vertical:** Aumentar recursos de una instancia existente.
**Caching:** Almacenamiento temporal para acelerar accesos repetidos.
**Keyset Pagination:** Paginación basada en cursor evitando OFFSET costoso.
**Hot Path:** Ruta de ejecución más frecuente/critica en términos de performance.
**Cold Start:** Latencia adicional inicial por creación o carga de contexto.
**Throughput:** Cantidad de operaciones completadas por unidad de tiempo.
**Backoff Exponencial:** Estrategia de reintentos con tiempos crecientes.

## Datos / Persistencia

**Event Sourcing:** Persistir eventos inmutables y reconstruir estado derivado.
**Read Model:** Proyección optimizada para consultas en CQRS.
**Snapshot:** Captura periódica de estado para acelerar reconstrucción.
**Index Covering:** Índice que contiene todas las columnas necesarias para una consulta.
**TTL (Time To Live):** Expiración automática de un registro tras cierto tiempo.
**Partitioning:** División de datos en subconjuntos gestionables (por rango, hash, etc.).

## DevSecOps / Pipeline

**CI/CD:** Integración Continua / Despliegue Continuo.
**Pipeline:** Flujo automatizado de build, test y deploy.
**SAST:** Static Application Security Testing (análisis de código fuente).
**DAST:** Dynamic Application Security Testing (análisis en ejecución).
**Secret Scanning:** Detección de credenciales en repositorios.
**Infrastructure as Code (IaC):** Definición declarativa de infraestructura versionable.
**Rollback:** Reversión a versión previa ante fallo.
**Drift Detection:** Detección de diferencias entre estado deseado (IaC) y real.

## .NET / C# Específico

**Async/Await:** Modelo asincrónico sin bloqueo de threads.
**Task:** Representación de operación asincrónica.
**CancellationToken:** Mecanismo para propagar cancelación cooperativa.
**Span<T>:** Tipo para manejar memoria contigua sin copiar.
**IAsyncEnumerable:** Enumeración asincrónica de secuencias.
**Dependency Injection (DI):** Inversión de control para desacoplar dependencias.
**Polly:** Librería .NET para políticas de resiliencia (retry, circuit breaker, bulkhead).
**Minimal APIs:** Estilo ligero de definir endpoints en .NET sin controllers completos.

## Criptografía / Firmas

**Hash Criptográfico:** Función unidireccional para integridad (SHA-256, etc.).
**HMAC:** Hash con clave para integridad y autenticidad.
**PKI:** Infraestructura de clave pública (certificados, CA, revocación).
**OCSP/CRL:** Protocolos para validar revocación de certificados.
**Time Stamping Authority (TSA):** Servicio que agrega sello de tiempo confiable a una firma.

## Gestión / Liderazgo

**ADR (Architecture Decision Record):** Documento corto que captura decisión arquitectónica y su contexto.
**OKR:** Objetivos y Resultados Clave para alineación estratégica.
**WIP (Work In Progress):** Trabajo simultáneo activo; limitarlo mejora flujo.
**Blameless Post-Mortem:** Análisis de incidentes sin culpar individuos para fomentar aprendizaje.

---

Si necesitas ampliar con ejemplos de código para un término, indícalo.
