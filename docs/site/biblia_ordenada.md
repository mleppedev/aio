<!-- Generado automáticamente: versión ordenada inteligente a partir de biblia.md -->

# Biblia Técnica Ordenada

> Compendio reorganizado para estudio rápido de entrevista. Incluye índice, agrupación temática y respuestas concisas.

## Índice

1. [Introducción](#introducción)
2. [Fundamentos HTTP & REST](#fundamentos-http--rest)
   1. [REST Stateless](#rest-stateless)
   2. [Métodos HTTP](#métodos-http)
   3. [Códigos de Estado](#códigos-de-estado)
   4. [URI: Definición](#uri-definición)
   5. [Buenas Prácticas URIs REST](#buenas-prácticas-uris-rest)
3. [Comparativas Arquitectura](#comparativas-arquitectura)
   1. [REST vs SOAP](#rest-vs-soap)
   2. [REST vs AJAX](#rest-vs-ajax)
   3. [API vs Microservicio](#api-vs-microservicio)
4. [Oferta Laboral Biometrika](#oferta-laboral-biometrika)
   1. [Empresa](#empresa)
   2. [Funciones del Cargo](#funciones-del-cargo)
   3. [Requerimientos y Skills](#requerimientos-y-skills)
   4. [Deseables](#deseables)
5. [Stack & Integración Tecnológica](#stack--integración-tecnológica)
   1. [Vista de Capas](#vista-de-capas)
   2. [Ventajas Backend .NET](#ventajas-backend-net)
   3. [CI/CD Ideal](#cicd-ideal)
   4. [Must Know: Mensajería & Infra](#must-know-mensajería--infra)
   5. [Kafka + Redis Ventajas](#kafka--redis-ventajas)
   6. [Stack con Blazor / Alternativas](#stack-con-blazor--alternativas)
6. [Firma Electrónica en Chile](#firma-electrónica-en-chile)
7. [Perfil, Capacidades y Ejemplos](#perfil-capacidades-y-ejemplos)
   1. [Perfil Breve](#perfil-breve)
   2. [Bullets Estrategia de Producto](#bullets-estrategia-de-producto)
   3. [Ejemplos con Métricas](#ejemplos-con-métricas)
8. [Preguntas Rápidas Set 1 (General)](#preguntas-rápidas-set-1-general)
9. [Preguntas Rápidas Set 2 (.NET & Azure)](#preguntas-rápidas-set-2-net--azure)

---

## Introducción

Las próximas son preguntas típicas de entrevista respondidas de forma acotada desde la perspectiva de un developer senior (.NET / microservicios / Azure / biometría).

## Fundamentos HTTP & REST

### REST Stateless

**Pregunta:** ¿How are REST API stateless?  
**Respuesta:** El servidor no mantiene contexto de cliente; cada petición incluye toda la info necesaria (auth, parámetros, estado derivable) y se procesa de forma independiente.

### Métodos HTTP

GET (lectura, idempotente, seguro)  
POST (crear recurso / operación no idempotente)  
PUT (reemplazo total, idempotente)  
PATCH (actualización parcial)  
DELETE (elimina)  
HEAD (solo cabeceras)  
OPTIONS (capacidades)

### Códigos de Estado

1xx informativo (100 Continue)  
2xx éxito (200 OK, 201 Created, 204 No Content)  
3xx redirección (301, 302, 304)  
4xx error cliente (400, 401, 403, 404)  
5xx error servidor (500, 502, 503)

### URI Definición

URI identifica un recurso; URL = URI con localización/protocolo (https://api.example.com/users/1); URN = nombre sin localización (urn:isbn:...).

### Buenas Prácticas URIs REST

- Sustantivos plurales: `/users/123/orders/456`
- Jerarquía clara; sin verbos (usa métodos HTTP)
- minúsculas + guiones: `/user-profiles`
- Versionado: `/api/v1/...`
- Filtros / paginación en query: `?role=admin&page=2&limit=50`
- Evitar endpoints genéricos tipo `/getData`

## Comparativas Arquitectura

### REST vs SOAP

REST ligero (JSON común), flexible; SOAP rígido, solo XML, añade WS-\* (seguridad, transacciones). SOAP útil en banca/gobierno cuando se exigen contratos estrictos; REST preferido en APIs modernas y microservicios.

### REST vs AJAX

REST: estilo arquitectónico API.  
AJAX: técnica cliente para peticiones asíncronas sin recargar página.  
Se combinan: AJAX consume endpoints REST.

### API vs Microservicio

API = contrato/interfaz.  
Microservicio = unidad desplegable autónoma con lógica y datos.  
Relación: un microservicio suele exponerse mediante una API, pero no toda API implica microservicios.  
Cuadro: Definición / Alcance / Despliegue (API no se “despliega” por sí misma, microservicio sí) / Relación (API puerta, microservicio la sala detrás).

## Oferta Laboral Biometrika

### Empresa

Tecnológica chilena (desde 2005) enfocada en verificación de identidad biométrica, presencia regional (Perú, Uruguay). Cultura: innovación, honestidad, confianza, mejora continua.

### Funciones del Cargo

- Definir visión estratégica y roadmap de producto.
- Soporte pre y post venta técnico (reuniones, defensa propuestas, incidentes escalados).

### Requerimientos y Skills

Core: C#, .NET, Backend/Microservicios, CI/CD, Blazor, APIs REST, Inglés B2, liderazgo y comunicación transversal.

### Deseables

Vue.js, React.js, Biometría, Firma Electrónica, IA, Azure (o similar nube).

## Stack & Integración Tecnológica

### Vista de Capas

Backend .NET (microservicios) ⇄ API REST ⇄ Frontend (Blazor principal; React/Vue alternos) ⇄ Infra (Azure: AKS, Functions, ACR, Key Vault).  
Microservicios especializados para biometría / firma / IA; pipelines CI/CD automatizan build-test-deploy.

### Ventajas Backend .NET

Multiplataforma, alto rendimiento (benchmarks TechEmpower), modular, seguridad integrada (OAuth2/OIDC/JWT), ecosistema maduro, tooling productivo, soporte LTS y compatibilidad migratoria.

### CI/CD Ideal

- Ramas main/develop.
- Pipeline: restore → build → tests (unit + integración) → análisis estático (Sonar/CodeQL) → empaquetar imagen Docker (tag semántico).
- Despliegue: dev/staging auto, producción con aprobación.
- Infra as Code (Bicep/Terraform).
- Estrategias: blue/green, canary, rollback rápido.
- Observabilidad: App Insights / Prometheus+Grafana, logs centralizados.

### Must Know: Mensajería & Infra

RabbitMQ (colas, work queues), Kafka (event streaming log distribuido), Redis (cache, session store, pub/sub ligero), SQL Server/PostgreSQL (transaccional), Cosmos DB (NoSQL global), OAuth2/OIDC (authn/authz), Kubernetes/AKS (orquestación), Azure Functions (event-driven), CI/CD (GitHub Actions/Azure DevOps).

### Kafka + Redis Ventajas

Kafka: throughput alto, persistencia de eventos, reprocesamiento histórico.  
Redis: latencia microsegundos, cache & sesiones, simple broker.  
Combinados: Kafka para flujos/eventos masivos; Redis acelera lecturas críticas y reduce carga a bases relacionales.

### Stack con Blazor / Alternativas

Blazor unifica C# full stack (compartir modelos), reduce duplicidad lógica. React/Vue se usan cuando se prioriza ecosistema JS o UX avanzada. Azure + contenedores → escalabilidad + resiliencia.

## Firma Electrónica en Chile

Ley 19.799. Tipos: FEA (certificado acreditado) y FES (mecanismo identificador).  
Modelo: Microservicio de firma → recibe documento, genera hash, integra con proveedor (SOAP/REST), firma (XAdES/CAdES/PAdES), almacena firmado + logs auditoría (SQL/Cosmos).  
Seguridad: certificados en Key Vault, OAuth2/OIDC, logs, opciones serverless (Functions) para callbacks. Escalabilidad: desacople por colas (RabbitMQ/Kafka) para lotes masivos.

## Perfil, Capacidades y Ejemplos

### Perfil Breve

Define visión técnica alineada a negocio; arquitectura .NET + microservicios; implementa CI/CD robusto; coordina áreas (comercial/soporte); mentoría técnica; soporte preventa/postventa; foco en resiliencia y performance.

### Bullets Estrategia de Producto

- Arquitectura y roadmap técnico alineado al plan estratégico.
- Traducción de requerimientos comerciales a soluciones escalables.
- Prototipado para validar valor con stakeholders.
- Priorización de backlog con criterios negocio-impacto.
- Buenas prácticas (seguridad, mantenibilidad, observabilidad).
- Vigilancia tecnológica e innovación continua.
- Medición de impacto y reajuste iterativo.

### Ejemplos con Métricas (Simulados)

- Arquitectura microservicios redujo tiempo de despliegue 40%.
- Traducción de requerimientos → +25% adopción primer año.
- Prototipo biométrico: -60% tiempo validación identidad.
- CI/CD disminuyó incidentes prod 35% y aceleró entregas.
- Priorización colaborativa → +20% satisfacción interna.
- Integración IA automatizó 50% de procesos manuales.

## Preguntas Rápidas Set 1 (General)

1. **SOLID:** SRP, OCP, LSP, ISP, DIP.
2. **Interfaz vs Clase Abstracta:** Interfaz = contrato puro; Abstracta = contrato + lógica base compartida.
3. **Inyección de dependencias:** Pasar dependencias externas (constructor/prop) para desacoplar/testear.
4. **Task vs Thread:** Task abstrae scheduling sobre ThreadPool; Thread es primitivo OS.
5. **async/await:** Azúcar sintáctico para tareas asíncronas sin bloquear hilos.
6. **DTO:** Contenedor de datos sin lógica para transporte entre capas.
7. **Record C#:** Inmutabilidad y comparación estructural.
8. **Middleware:** Componente pipeline HTTP (logging, auth).
9. **Microservicio:** Servicio autónomo con lógica y almacenamiento propio.
10. **Ventaja Blazor:** C# lado cliente y reutilización de modelos.
11. **IEnumerable/ICollection/IList:** Enumeración / +operaciones colección / +índice.
12. **Value Object DDD:** Inmutable, definido por valor (Email).
13. **Swagger:** Documentación y prueba interactiva API.
14. **Transacción:** Unidad atómica ACID.
15. **SQL vs NoSQL:** Estructura y transacciones vs flexibilidad y escalado horizontal.
16. **REST Stateless:** Sin estado servidor entre requests.
17. **Idempotencia:** Mismo resultado repitiendo (PUT).
18. **JWT:** Token firmado con claims.
19. **CORS:** Política de orígenes permitidos.
20. **log.Error vs log.Fatal:** Error recuperable vs crítico terminal.
21. **Pipeline CI/CD:** Automación build-test-deploy continua.
22. **Unit vs Integration Test:** Aislado vs interacción real.
23. **TDD:** Test → Código mínimo → Refactor.
24. **Clean Architecture:** Separar dominio, casos de uso, interfaces, frameworks.
25. **Docker vs VM:** Contenedores ligeros vs virtualización completa.
26. **Health Check:** Endpoint estado servicio.
27. **Circuit Breaker:** Corta llamadas tras fallos repetidos.
28. **Monolito vs Microservicio:** Único despliegue vs múltiples servicios.
29. **KISS:** Mantener simple.
30. **Pull Request:** Revisión colaborativa antes de merge.

## Preguntas Rápidas Set 2 (.NET & Azure)

1. **.NET Core uso:** Multiplataforma, performante para microservicios.
2. **Framework vs Core:** Framework = Windows/monolítico; Core = modular multi-OS.
3. **Controller:** Clase endpoints HTTP.
4. **DI ASP.NET:** Contenedor nativo de servicios (SCOPES).
5. **Ciclo vida (Singleton/Scoped/Transient):** App / Request / Resolución.
6. **Hosted Service:** Trabajo background.
7. **gRPC vs REST:** Binario, rápido, contratos estrictos.
8. **Azure App Service:** PaaS para web/APIs.
9. **Azure Functions:** Serverless event-driven.
10. **AKS:** Kubernetes gestionado.
11. **API Management:** Gateway seguridad/throttling/analytics.
12. **Blob vs SQL:** Objetos vs relacional estructurado.
13. **Key Vault:** Secretos / llaves seguras.
14. **Service Bus:** Mensajería (colas/topics).
15. **Queue vs Topic:** 1:1 vs 1:N pub-sub.
16. **API Gateway:** Entrada única y políticas.
17. **Resiliencia:** Patrones retry, circuit, fallback.
18. **Azure Monitor:** Métricas y logs centralizados.
19. **Azure DevOps vs GitHub Actions:** Suite integral vs integrado al repo.
20. **Container Registry:** Almacén imágenes privadas.
21. **Escalado Horizontal/Vertical:** +instancias / +recursos.
22. **Polly Retry Policy:** Reintentos controlados.
23. **Health Checks ASP.NET:** Endpoint(s) estado dependencias.
24. **Circuit Breaker Polly:** Evita cascada de fallos.
25. **REST vs GraphQL:** Multiples endpoints y payload fijo vs un endpoint con consulta declarativa.
26. **SLA:** Garantía disponibilidad.
27. **Application Insights:** Telemetría y trazas.
28. **appsettings vs App Configuration:** Local por servicio vs centralizado dinámico.
29. **Bounded Context:** Límite semántico de dominio.
30. **Dapr:** Sidecar que abstrae pub/sub, state, bindings para microservicios .NET.

---

> Fin de la versión ordenada. Actualiza categorías si se agregan nuevas preguntas en el archivo original.
