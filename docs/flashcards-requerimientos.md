# Flashcards Cotidianas sobre los Requerimientos

Formato: Pregunta breve → Respuesta ultra concisa (memory hook). Usa para repasos rápidos. 50 ítems agrupados.

## 1. C# / .NET Fundamentos (8)

1. ¿`async/await` para qué? → Liberar hilo mientras se espera I/O (mejor escalabilidad).
2. ¿`IEnumerable` vs `IQueryable`? → `IEnumerable` evalúa en memoria; `IQueryable` traduce a consulta (deferida).
3. ¿Por qué `record`? → Inmutabilidad + value equality clara en DTOs/config.
4. ¿Cuándo usar `Span<T>`? → Procesar buffers en caliente evitando allocations.
5. ¿Inyección de dependencias clave? → Separar contratos, facilitar test y reemplazo runtime.
6. ¿Configuración tipada? → `Options pattern` para validar y centralizar settings.
7. ¿Por qué `CancellationToken`? → Cortar trabajo inútil liberando recursos.
8. ¿Pooling de conexiones? → Reusa conexiones y evita overhead establecimiento.

## 2. APIs REST / Contratos (7)

9. ¿Versionar cuándo? → Sólo ante breaking change real.
10. ¿Validación dónde? → Entrada: modelo + reglas negocio antes de procesar.
11. ¿Idempotencia POST crítica? → Reintentos seguros (Idempotency-Key).
12. ¿Paginación recomendada? → Keyset para grandes volúmenes (estable y rápida).
13. ¿Errores estándar? → `ProblemDetails` uniforme facilita cliente.
14. ¿Campos nuevos cómo? → Agregar opcionales sin romper clientes.
15. ¿OpenAPI sirve para? → Contrato único: doc, mocks, tests y generación clientes.

## 3. Microservicios / Integración (8)

16. ¿Por qué dividir? → Desplegar y escalar de forma independiente.
17. ¿Cuándo NO dividir? → Dominio pequeño + equipo reducido + cambios siempre conjuntos.
18. ¿Comunicación sincrónica riesgo? → Cascada de latencias/fallos.
19. ¿Evento vs llamada directa? → Evento: desacople temporal y múltiples consumidores.
20. ¿Outbox para? → Publicar evento atómico con transacción de datos.
21. ¿Saga cuándo? → Flujo distribuido con pasos que requieren compensación.
22. ¿Gateway rol básico? → Entrada unificada: auth, routing, limits.
23. ¿Evitar chatty services? → Agregar datos necesarios en el primer servicio / caching.

## 4. Blazor (5)

24. ¿Blazor Server ventaja? → Render rápido inicial y acceso directo a servidor.
25. ¿Blazor WASM ventaja? → Menos ida/vuelta, offline parcial.
26. ¿Compartir lógica? → Librerías .NET estándar para validar en cliente y servidor.
27. ¿State management básico? → `Scoped` services / `Context` + persistir lo necesario.
28. ¿Evitar render excesivo? → `ShouldRender` + fragmentar componentes.

## 5. CI/CD & Calidad (8)

29. ¿Pipeline mínimo? → Compilar, test unitario, análisis estático, empaquetar, desplegar.
30. ¿Feature branch por qué? → Aíslas cambios y facilitas revisión.
31. ¿PR pequeño? → Menos riesgo, revisión rápida.
32. ¿Tests de contrato aportan? → Detectar ruptura de APIs temprano.
33. ¿Canary release valor? → Exposición limitada + rollback fácil.
34. ¿Infra-as-code? → Repetible, auditable y versionado.
35. ¿Automatizar lint/format? → Quitar ruido de revisión humana.
36. ¿Métrica DORA clave inicial? → Lead Time para acelerar entrega.

## 6. Seguridad / Compliance (7)

37. ¿Principio base? → Menor privilegio siempre.
38. ¿Cifrado tránsito? → HTTPS/mTLS impide escucha y MITM.
39. ¿Cifrado reposo? → Protege datos si medio se expone.
40. ¿Rotar claves? → Limita ventana de compromiso.
41. ¿Logs que cuidar? → No incluir datos crudos biométricos / secretos.
42. ¿Threat modeling cuándo? → Al iniciar feature sensible y en cambios grandes.
43. ¿MFA operaciones críticas? → Reduce riesgo de abuso interno.

## 7. Observabilidad & Operabilidad (7)

44. ¿Tres pilares? → Logs, métricas, trazas.
45. ¿SLO vs SLA? → SLO interno; SLA compromiso externo.
46. ¿Error budget? → Margen para fallar antes de frenar features.
47. ¿Tracing útil? → Ver latencia por salto y aislar cuellos.
48. ¿Dashboards minimal? → Latencia p95, errores %, saturación, throughput.
49. ¿Health check liveness? → Proceso vivo (no bloqueado).
50. ¿Readiness check? → Está listo para recibir tráfico (dependencias OK).

---

## Uso sugerido

- 5–10 minutos: repasa una categoría.
- Alterna: pregunta → intenta responder → verifica.
- Marca (✔) las dominadas y enfoca las que fallas 2 veces.

## Siguientes opcionales

Pide si quieres: versión tipo Anki (Q/A separado), ampliación de respuestas clave, top 20 más probables, inglés paralelo o modo examen (oculto respuestas).

---

## Top 20 Más Probables (Prioriza memorizar primero)

1. ¿`async/await` para qué? → Escalabilidad liberando hilos en I/O.
2. ¿Versionar cuándo? → Sólo ante breaking change real.
3. ¿Idempotencia POST crítica? → Reintentos seguros sin duplicar.
4. ¿Paginación recomendada? → Keyset para grandes volúmenes.
5. ¿Evento vs llamada directa? → Evento = desacople + múltiples consumidores.
6. ¿Outbox para? → Garantizar publicación atómica de eventos.
7. ¿Saga cuándo? → Flujo distribuido con compensaciones.
8. ¿Blazor Server ventaja? → Menor latencia inicial y acceso directo a servidor.
9. ¿Pipeline mínimo? → Build, tests, análisis, package, deploy.
10. ¿Tests de contrato aportan? → Detectar rupturas de API temprano.
11. ¿Canary release valor? → Riesgo controlado + rollback rápido.
12. ¿Infra-as-code? → Repetibilidad y trazabilidad de infra.
13. ¿Principio base seguridad? → Menor privilegio.
14. ¿Cifrado tránsito? → Protección contra escucha/MITM.
15. ¿Threat modeling cuándo? → Inicio feature sensible y grandes cambios.
16. ¿Tres pilares observabilidad? → Logs, métricas, trazas.
17. ¿SLO vs SLA? → SLO interno, SLA compromiso externo.
18. ¿Error budget? → Margen antes de frenar features.
19. ¿Health check liveness? → Ver que el proceso no está colgado.
20. ¿Readiness check? → Confirmar dependencias listas antes de tráfico.

Tips de repaso:

- Técnica de bloqueo: tapa la respuesta y verbalízala.
- Espaciado: 3 pasadas hoy (mañana, tarde, noche), luego 1 al día.
- Señaliza (✔) cuando respondas 3 veces seguidas sin dudar.
