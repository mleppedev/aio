# Biometrika - Dossier para Candidato (Líder de Desarrollo Fullstack)

> Documento interno de apoyo al proceso de reclutamiento. Fuente primaria: sitio público https://www.biometrikalatam.com/ (consulta: 19-08-2025) y la oferta en `oferta-laboral.md`. Algunos puntos técnicos proyectan necesidades razonables del rol y deben validarse en entrevista.

---

## 1. Resumen Corporativo

Biometrika es una empresa tecnológica chilena (operación también en Perú y Uruguay) enfocada en soluciones de verificación de identidad, biometría y gestión de procesos asociados (firma electrónica, control de acceso, enrolamiento y flujos presenciales / digitales). Fundada en 2005, combina componentes biométricos, integraciones con organismos estatales (p.ej. Registro Civil) y plataformas multi‑canal (Web, Mobile, Cliente/Servidor).

**Propuesta de valor principal:** reducir fraude de identidad, acelerar onboarding y asegurar trazabilidad y cumplimiento normativo (Ley de Firma Electrónica #19.799, estándares de seguridad y auditoría).

---

## 2. Principales Soluciones / Productos (según sitio)

| Línea / Producto                                      | Enfoque                                                     | Relevancia para el rol técnico                                    |
| ----------------------------------------------------- | ----------------------------------------------------------- | ----------------------------------------------------------------- |
| BioPortal (Plataforma de Verificación y Enrolamiento) | Centraliza verificaciones biométricas y enrolamiento        | Orquestación de microservicios, APIs, performance y escalabilidad |
| Verificación de Identidad                             | Matching contra cédula / DNI / pasaporte y fuentes externas | Integraciones seguras, normalización de datos, latencia baja      |
| Documentos Electrónicos                               | Generación y complementación PDF con firmas                 | Servicios backend, plantillas, sellos de tiempo                   |
| Firma Electrónica (Simple, Avanzada y Biométrica)     | Cumplimiento legal chileno, multicanal                      | Criptografía aplicada, manejo de certificados, no repudio         |
| Control de Acceso (WAIS)                              | Gestión centralizada de accesos personas/vehículos          | Edge devices, eventos en tiempo real, auditoría                   |
| Control Casino                                        | Gestión alimentación corporativa                            | Módulos transaccionales y reporting                               |
| Control de Incendio (Cold Fire)                       | Supresión y monitoreo (línea complementaria)                | Integración IoT / telemetría potencial                            |
| Sistema de Gestión de Filas                           | Optimización de atención presencial                         | Tiempo real, colas distribuidas, UX omnicanal                     |
| Notario Virtual (empresa del grupo)                   | Certificación de identidad en línea                         | Validación cruzada, robustez legal                                |

---

## 3. Métricas / Tracción (publicadas en el sitio)

Se observan indicadores resumidos (interpretación a validar):

- 13+ años de experiencia (desde 2005)
- 800K+ personas enroladas biométricamente
- 2M+ verificaciones de identidad diarias (cifra mostrada como "2+Millón" en el sitio)

> Nota: Consolidar valores exactos y alcance temporal durante la entrevista (¿acumuladas vs. diarias? ¿Latam total?).

---

## 4. Stack Tecnológico (Implícito / Ofertado / Potencial)

**Declarado en oferta:** C#, .NET, Blazor, Microservicios, CI/CD, API Rest.

**Probables componentes (hipótesis razonables a validar):**

- .NET 6/7/8 para servicios core (rendimiento, soporte LTS)
- ASP.NET Core Web API + gRPC (posible en internos)
- Blazor Server / WebAssembly para frontales internos de operación
- Integraciones con entidades externas vía REST/SOAP y posibles colas (RabbitMQ, Azure Service Bus, Kafka)
- Autenticación: JWT / OAuth2 / OpenID Connect (Keycloak, IdentityServer u otro)
- Persistencia: SQL Server / PostgreSQL + caches (Redis) para sesiones y respuestas biométricas temporales
- Almacenamiento de plantillas biométricas: repos seguro cifrado (en disco / HSM / KMS cloud)
- CI/CD: Azure DevOps / GitHub Actions / Jenkins (pipeline multi‑ambiente, escaneos SAST/DAST)
- Infraestructura: Contenedores (Docker) orquestados (Kubernetes / AKS) o mezcla on‑prem vs. cloud híbrida
- Observabilidad: Prometheus / Grafana / App Insights / ELK

> Validar en la entrevista qué partes están productivas, cuáles en migración y prioridades de modernización.

---

## 5. Desafíos Técnicos del Rol

1. Escalabilidad de verificaciones biométricas de alta concurrencia.
2. Garantizar baja latencia y alta disponibilidad en flujos críticos (onboarding, firma, control de acceso).
3. Gobernanza de microservicios (versionado de APIs, contratos, backward compatibility).
4. Seguridad y cumplimiento (gestión de llaves, cifrado en tránsito y reposo, auditoría forense, protección de datos personales).
5. Optimización de costs vs. performance (estrategias de caching y colas para desacoplar picos de verificación).
6. Evolución UI/UX interna (Blazor) sin comprometer la velocidad de entrega.
7. Integración continua con pipelines robustos (tests automáticos, code quality gates, despliegues progresivos / blue-green o canary).

---

## 6. KPIs Potenciales para Evaluar el Éxito del Rol

- Tiempo de ciclo (commit → producción)
- Tasa de fallos en producción (MTTR / incidentes Sev1–Sev3)
- Latencia P95 en endpoints de verificación / firma
- Uptime (%) de servicios core (≥99.9% objetivo)
- Cobertura automática de pruebas (unitarias + contract tests)
- Reducción de costos infra por transacción (optimización recursos)

---

## 7. Cultura y Colaboración

Valores enunciados: innovación, honestidad, trabajo en equipo, confianza y mejora continua. El rol requiere interlocución transversal (comercial, soporte, administración) y soporte pre / post venta técnico (defensa de propuestas, análisis de incidentes escalados).

**Prácticas recomendadas a impulsar:**

- Documentación living (Architecture Decision Records, catálogos de servicios)
- Security by design y threat modeling temprano
- Observabilidad como parte del Definition of Done
- Revisión de código orientada a calidad y conocimiento compartido

---

## 8. Riesgos / Áreas a Profundizar en Entrevista

| Tema                           | Preguntas sugeridas                                        |
| ------------------------------ | ---------------------------------------------------------- |
| Microservicios                 | ¿Número actual? ¿Gateway / service mesh?                   |
| Biometría                      | ¿Algoritmos usados? ¿Propios vs. terceros (AFIS/face SDK)? |
| Almacenamiento datos sensibles | ¿Cifrado? ¿Rotación de llaves?                             |
| Cumplimiento legal             | ¿Procesos de auditoría? ¿Logs inmutables?                  |
| DevSecOps                      | ¿Herramientas SAST/DAST/Secret scanning integradas?        |
| Observabilidad                 | ¿Stack actual? ¿Cuadros de mando críticos?                 |
| Roadmap                        | Modernización prioritaria próximos 6–12 meses              |

---

## 9. Alineación con la Oferta (`oferta-laboral.md`)

- Énfasis en liderazgo técnico y visión de producto → encaja con la necesidad de coordinar roadmap de plataformas como BioPortal y Verificación de Identidad.
- Blazor y .NET sugieren intención de mantener un stack coherente full C# (front y back) para velocidad y consistencia.
- Microservicios y CI/CD → foco en madurez operativa, probablemente etapa de escalamiento / consolidación.

---

## 10. Pitch para Candidato

Rol ideal para alguien que quiera:

- Diseñar arquitectura de plataformas de identidad de alto impacto nacional / regional.
- Implementar prácticas modernas (microservicios, observabilidad, seguridad) donde hay base instalada y espacio de evolución.
- Liderar iniciativas de performance, resiliencia y cumplimiento regulatorio.
- Influir directamente en la estrategia de producto y en el ciclo comercial técnico.

---

## 11. Próximos Pasos Sugeridos (antes de entrevista)

1. Preparar ejemplos concretos de proyectos con: migración monolito→microservicios, hardening seguridad, y optimización TTM.
2. Refrescar conocimiento de: patrones de resiliencia (.NET Polly), IdentityServer u OpenID Connect, pipelines YAML en Azure DevOps / GitHub Actions.
3. Tener anécdotas sobre manejo de incidentes críticos (post-mortem, MTTR, blameless culture).
4. Revisar fundamentos de biometría (huella, rostro, liveness detection) para conversación de alto nivel.

---

## 12. Descargo

Los datos públicos pueden variar; validar en entrevistas cualquier cifra o supuesto técnico. Este documento no representa comunicación oficial de Biometrika.

---

¿Requieres una versión resumida (one-pager) o traducción al inglés? Indícalo y la genero.
