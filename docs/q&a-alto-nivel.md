# Q&A Alto Nivel – Líder de Desarrollo (.NET / Identidad / Biometría)

> 50 preguntas estratégicas y de liderazgo técnico para preparar conversación con management, reclutadores y stakeholders. Sin respuestas escritas para fomentar formulación propia. Añade métricas y casos reales.

---

### 1. ¿Cómo articularías la visión técnica de la plataforma de verificación de identidad para los próximos 12 meses?

Respuesta: Consolidar una **plataforma modular** y **observable**: (a) estabilizar **servicios core** (verificación, enrolamiento, firma) con **SLOs** definidos; (b) introducir **tracing distribuido** y **métricas accionables**; (c) reducir **latencia P95 <500ms** en endpoints críticos; (d) elevar **cobertura de tests** y **seguridad** (secret scanning, cifrado homogéneo) y (e) habilitar **escalado** predecible para crecimiento regional. Objetivo: acelerar **onboarding** y reducir **fraude**.

### 2. ¿Qué criterios usarías para decidir entre evolucionar un módulo existente vs crear un nuevo microservicio?

Respuesta: Analizo: **cohesión**, **frecuencia de despliegues**, **requisitos de escalado**, **bounded context** claro, tamaño equipo y **costo operativo**. Separo si reduce dependencias y acelera **ciclos de entrega** sin inflar complejidad; de lo contrario, **refactor modular**.

### 3. ¿Cómo equilibras velocidad de entrega con requisitos de cumplimiento y seguridad en datos biométricos?

Respuesta: Defino un **security & compliance baseline** automatizado (lint seguridad, **SAST**, **secret scan**, políticas **IaC**) y **feature flags** para liberar valor rápido sin sacrificar controles. Seguridad parte del **Definition of Done**.

### 4. ¿Cuál sería tu marco para priorizar deuda técnica frente a features comerciales urgentes?

Respuesta: **Matriz impacto vs costo** (riesgo operativo, performance, seguridad). Deuda que amenaza **SLOs** o eleva **MTTR** priorizada. Reservo **10–20% capacidad** y cada ítem ligado a métrica (ej: reducir **build time 30%**).

### 5. ¿Cómo comunicarías a negocio el valor de invertir en observabilidad avanzada?

Respuesta: Enfatizo **MTTR menor** (menos horas incidentes, menos **SLA penalties**) y **tracing** → decisiones de producto basadas en datos. Comparo **baseline** vs proyección de reducción de severidad.

### 6. ¿Qué indicadores iniciales definirías como SLOs críticos de la plataforma?

Respuesta: **Disponibilidad (≥99.9%)**, **latencia P95**, **tasa éxito verificación**, **error rate <1%**, tiempo enrolamiento medio, **MTTR**. Versionados trimestralmente.

### 7. ¿Cómo evaluarías la madurez actual del **pipeline CI/CD** y qué roadmap plantearías?

Respuesta: **Auditoría**: tiempo ciclo, automatización pruebas, **gates seguridad**, frecuencia despliegues, rollback time. Roadmap: (1) **tests contract** + trazabilidad artefactos; (2) **canary** + métricas gating; (3) **infraestructura declarativa**; (4) **seguridad continua**.

### 8. ¿Qué estrategia seguirías para reducir el riesgo de dependencia de un proveedor externo clave?

Respuesta: **Abstracción** vía adaptador, métricas de **latencia** y **error**, **fallback** degradado, acuerdos **SLA** claros, evaluación segundo proveedor / modo **batch** alternativo.

### 9. ¿Cómo establecerías un modelo de gobierno de arquitectura sin burocracia excesiva?

Respuesta: **Principios ligeros**, **ADRs** cortos, comité técnico quincenal (excepciones), métricas de **salud arquitectónica** y revisión **post-mortem**.

### 10. ¿Qué principios arquitectónicos propondrías formalizar (y por qué)?

Respuesta: **Dominios claros**, **seguridad by default**, **observabilidad obligatoria**, **contracts versionados**, **backward compatibility** temporal, **idempotencia**.

### 11. ¿Cómo abordarías una migración gradual hacia mayor aislamiento **multi-tenant**?

Respuesta: Catalogar **tenants** y cargas, introducir **tenant resolver**, separar datos sensibles (schema/DB), aplicar **cifrado por tenant** y monitorear **contención** recursos.

### 12. ¿Qué estrategia usarías para contener crecimiento de **complejidad accidental** en el código base?

Respuesta: **Revisiones arquitectónicas** ligeras, **linting** dependencias, métricas de **acoplamiento** / código muerto, **refactors iterativos**, límites tamaño por servicio.

### 13. ¿Cómo decidir cuándo introducir **GraphQL**, **gRPC** u otra interfaz?

Respuesta: Agregación múltiple / evitar **overfetching**: GraphQL. Comunicación interna alto rendimiento / **streaming**: gRPC. Exposición pública: **REST**.

### 14. ¿Qué framework de métricas usarías para alinear mejoras técnicas con objetivos de negocio?

Respuesta: **North Star** (onboarding) + métricas salud (**DORA** + **SLOs**). Cada iniciativa con métrica clara.

### 15. ¿Cómo organizarías los equipos alrededor de dominios de negocio en lugar de capas técnicas?

Respuesta: Equipos **verticales** (Verificación, Enrolamiento, Firma) con **ownership end-to-end** y un **platform enablement** compartido.

### 16. ¿Qué plan propondrías para elevar el nivel de seguridad (roadmap 6 meses)?

Respuesta: Mes 1 **baseline** (scans). Mes 2–3 **hardening secretos** + **mTLS**. Mes 3–4 **threat modeling** + **logging inmutable**. Mes 5–6 automatizar revisiones / **tabletop exercises**.

17. ¿Cómo manejarías la presión de un cliente grande que pide un bypass de controles de seguridad?  
    Respuesta: Explico **riesgo cuantificado** (impacto legal / reputación), ofrezco **alternativa segura** y documento. No se cede en **controles clave**.

18. ¿Cuál es tu enfoque para evaluar y seleccionar herramientas en el ecosistema .NET?  
    Respuesta: Criterios: **alineación estándares**, **costo aprendizaje**, **comunidad/soporte**, **performance**, **extensibilidad**. **Piloto** pequeño + **KPI comparativo**.

19. ¿Cómo diseñarías un plan de resiliencia frente a caídas de una fuente externa de identidad?  
    Respuesta: **Timeouts** + **circuit breaker**, **fallback** cola offline, **reintentos espaciados**, **caché** verificaciones recientes, **dashboard** específico.

20. ¿Cómo definirías criterios de aceptación no funcionales estándar para nuevos servicios?  
    Respuesta: **Checklist**: latencia objetivo, **error budget**, **logging/tracing**, tests mínimos, **health endpoints**, política secretos, modelo permisos.

21. ¿Qué mecanismos de retroalimentación continua instalarías entre soporte y desarrollo?  
    Respuesta: **Canal estructurado** (incidentes categorizados), **revisión semanal**, **feedback loop** en backlog, métricas de **tickets reabiertos**.

22. ¿Cómo fomentarías la adopción de prácticas de testing en un equipo con madurez desigual?  
    Respuesta: **Pairing** en tests críticos, **ejemplos vivos**, métricas sin castigo, **refactors guiados por tests**, **coaching** puntual.

23. ¿Qué formato usarías para comunicar decisiones arquitectónicas a stakeholders no técnicos?  
    Respuesta: Documento **1 página**: problema, opción elegida, alternativas descartadas, impacto métricas negocio y **timeline**.

24. ¿Cómo alinearías el roadmap técnico con ciclos comerciales (ventas/licenciamiento)?  
    Respuesta: **Planificación trimestral**; dependencias técnicas priorizadas si desbloquean ventas; **feature gates** para demos anticipadas.

25. ¿Qué indicadores te harían decir que un microservicio debe fusionarse o eliminarse?  
    Respuesta: **Baja tasa despliegues**, **dependencia circular**, **latencia inter-servicios** alta, **código mínimo** con **costo operativo** alto.

26. ¿Cómo abordarías la reducción del MTTR sin inflar el gasto operativo?  
    Respuesta: Mejorar **detección** (alertas SLO), **runbooks** claros, **post-mortems** rápidos, optimizar tooling existente.

27. ¿Qué estrategia de talento aplicarías para mantener y atraer perfiles senior .NET?  
    Respuesta: **Trayectorias técnicas** claras, tiempo para **mejora interna**, **transparencia**, **retos técnicos** relevantes.

28. ¿Cómo evaluarías objetivamente a un candidato técnico para tu equipo?  
    Respuesta: **Ejercicio estructurado** (arquitectura + código), criterios (claridad, **trade-offs**, tests), **panel diverso**, **scorecards**.

29. ¿Cómo detectarías señales tempranas de fatiga o burnout en el equipo?  
    Respuesta: Cambios en **throughput**, aumento **bugs triviales**, baja participación, feedback 1:1 → ajustar **WIP** y prioridades.

30. ¿Qué criterios usarías para institucionalizar feature flags sin deuda eterna?  
    Respuesta: Flag con **owner**, **fecha expiración**, **propósito** y retiro automático en pipeline.

31. ¿Cómo justificarías inversión en cifrado adicional (HSM/KMS) ante un CFO?  
    Respuesta: **Riesgo financiero evitado** (multas, pérdida clientes) vs costo; refuerzo **reputación** y **compliance**.

32. ¿Qué enfoque seguirías para establecer niveles de severidad y respuesta a incidentes?  
    Respuesta: **Matriz impacto vs alcance**, tiempos objetivo, roles **on-call** rotativos, **playbooks** documentados.

33. ¿Cómo definirías un plan de reducción de latencia P95 sin micro-optimizar prematuramente?  
    Respuesta: (1) **medir/segmentar** (2) **quick wins** (caché, N+1) (3) optimizaciones estructurales (4) **tuning basado en evidencia**.

34. ¿Qué métricas observarías para medir impacto de refactorizaciones grandes?  
    Respuesta: **Lead time**, defectos regresión, **cobertura**, latencia afectada, **MTTR**, **complejidad ciclomatica**.

35. ¿Cómo estructurarías un programa de mentorship técnico cruzado?  
    Respuesta: **Parejas rotativas**, objetivos formales, seguimiento bimensual y **demo aprendizajes**.

36. ¿Qué razonamiento usarías para introducir un **service mesh** y cuándo no hacerlo?  
    Respuesta: Necesidad **observabilidad/mTLS** uniforme + tráfico complejo; evitar si pocos servicios o **overhead** supera valor.

37. ¿Cómo afrontarías divergencia tecnológica (varios lenguajes) en un entorno .NET dominante?  
    Respuesta: **Tech radar**, criterios adopción, soporte sólo a runtimes con **ownership** claro, evitar fragmentación tooling.

38. ¿Qué lineamientos pondrías sobre cuándo crear una nueva base de datos vs compartir una existente?  
    Respuesta: **Aislamiento compliance**, perfiles carga distintos o contención rendimiento; si no, **schema** separado.

39. ¿Cómo organizarías un proceso ligero de Architecture Review?  
    Respuesta: **Template único** (problema, decisión, impacto), reunión corta semanal y aprobación **asíncrona**.

40. ¿Qué criterios guiarían la selección entre **SQL** y **NoSQL** para un nuevo módulo?  
    Respuesta: Patrones acceso, necesidad **transaccional**, volumen, latencia, flexibilidad esquema y coste operación.

41. ¿Cómo balancearías privacidad del usuario con analítica de producto accionable?  
    Respuesta: **Pseudonimización**, **agregación**, minimización acceso crudo y **consentimiento** explícito.

42. ¿Qué narrativa usarías para explicar a clientes finales el flujo de verificación biométrica y su seguridad?  
    Respuesta: Lenguaje claro: **capturamos**, **ciframos**, comparamos contra **fuentes autorizadas**, minimizamos datos, **controles auditados**.

43. ¿Cómo identificarías y atacarías el cuello de botella principal del flujo de enrolamiento?  
    Respuesta: **Tracing** paso a paso, ranking latencias, experimento **A/B** (caching/optimización), iterar hasta **P95 objetivo**.

44. ¿Qué planeación harías para soportar un pico estacional x5 en verificaciones?  
    Respuesta: **Pruebas carga** anticipadas, **autoscaling** revisado, límites protectores, **colas** + degradación controlada, monitoreo reforzado.

45. ¿Cómo gestionarías expectativas de stakeholders frente a una reescritura parcial de módulo crítico?  
    Respuesta: **Roadmap incremental**, métricas riesgo, entregables intermedios, **comunicación transparente**.

46. ¿Qué criterios aplicarías para medir efectividad de un líder técnico dentro del equipo?  
    Respuesta: **Calidad decisiones**, reducción bloqueos, **mentoring** visible, métricas flujo, baja incidencia defectos críticos.

47. ¿Cómo introducirías pairing y mob programming sin afectar plazos?  
    Respuesta: **Piloto** tareas complejas, **timeboxing**, medir **defectos reducidos** y conocimiento compartido.

48. ¿Qué estrategia de versionado interno adoptarías para librerías compartidas?  
    Respuesta: **SemVer**, **changelog** automatizado, **pruebas contract**, política **deprecation** clara.

49. ¿Cómo evaluarías el ROI de una inversión en mayor automatización de seguridad?  
    Respuesta: Comparo **tiempo manual** + **riesgo fallo** vs **costo implementación** y **ahorro proyectado**.

50. ¿Qué legado técnico te gustaría dejar tras 12–18 meses en el rol?  
    Respuesta: **Plataforma verificación** con **SLOs consistentes**, **pipeline maduro**, cultura de **decisiones documentadas** y **seguridad integrada** sin fricción.

---

Si deseas versión con respuestas modelo o priorización (top 10 más probables), pídelo.
