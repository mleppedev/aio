# PRD: Mini Parakeet (Asistente de Transcripción y Respuesta en Tiempo Real)

## 1. Introducción / Overview

Un asistente ligero de escritorio (Windows) que **captura el audio del sistema en tiempo real**, lo **transcribe con Whisper API de OpenAI** y permite **generar respuestas bajo demanda** usando modelos de chat OpenAI. Es para **uso personal**, sin requisitos de privacidad corporativa estrictos, orientado a acelerar comprensión / resumen instantáneo de contenido que suena en los parlantes (reuniones, videos, podcasts). Fase inicial: foco en latencia ultra baja (<500 ms visible para fragmentos parciales) y simplicidad.

## 2. Objetivos (Goals)

1. Mostrar transcripción parcial en menos de 500 ms promedio desde emisión de audio (chunk inicial).
2. Alcanzar ≥90% precisión aceptada subjetivamente en Español/Inglés (uso real mixto).
3. Permitir disparar una respuesta generada (prompt configurable) con un hotkey (<1 s desde invocación a envío request).
4. Consumir < 250 MB RAM y < 10% CPU media en reposo (captura + streaming) en un equipo estándar.
5. Instalable como binario único o setup simple (<5 minutos para poner en marcha).

## 3. User Stories

- Como usuario personal, quiero ver subtítulos en vivo de lo que escucho para retener mejor información.
- Como usuario, quiero presionar una tecla y obtener un resumen breve de los últimos N segundos.
- Como usuario, quiero cambiar el prompt base de generación para adaptar el estilo de respuesta.
- Como usuario, quiero pausar/resumir la captura rápidamente sin cerrar la app.
- Como usuario, quiero que no se guarden transcripciones si así lo configuro (modo efímero).

## 4. Requerimientos Funcionales (FR)

FR-1: La aplicación debe capturar el audio de salida del sistema (parlantes) en Windows.  
FR-2: Debe fragmentar el audio en chunks (ej. 500 ms o umbral configurable) y enviarlos para transcripción streaming.  
FR-3: Debe mostrar texto parcial (interim) y actualizarlo con la versión final estabilizada.  
FR-4: Debe soportar dos idiomas iniciales: español e inglés, con detección dinámica (auto).  
FR-5: Debe proveer un hotkey global para generar una respuesta/resumen (ej. Ctrl+Shift+R).  
FR-6: Debe enviar al modelo de chat un prompt base + contexto de últimos X segundos/minutos (configurable).  
FR-7: Debe presentar la respuesta en un panel flotante superpuesto (always-on-top).  
FR-8: Debe permitir cambiar el prompt base en una pequeña UI editable.  
FR-9: Debe ofrecer modo efímero (no persistir transcripciones en disco).  
FR-10: Debe permitir opcionalmente guardar el transcript actual como archivo .txt manualmente.  
FR-11: Debe tener botón de pausa/reanudar captura.  
FR-12: Debe indicar estado de conexión a la API (OK / error / rate limit).  
FR-13: Debe manejar reconexiones automáticas ante fallos transitorios de red.  
FR-14: Debe permitir ajustar ventana de contexto para resumen (por defecto 120 s).  
FR-15: Debe mostrar contador aproximado de coste (tokens) de la sesión actual.  
FR-16: Debe permitir configurar clave API de OpenAI de forma segura en local (archivo cifrado simple o secure storage Windows).  
FR-17: Debe iniciar minimizado (opcional) en bandeja del sistema.  
FR-18: Debe mostrar notificación breve cuando una respuesta esté lista.  
FR-19: Debe ofrecer actualización automática simple (check versión).  
FR-20: Debe tener log en memoria para debug con export manual en caso de error.

## 5. Non-Goals (Fuera de Alcance Fase 1)

- Transcripción del micrófono (solo system audio).
- Síntesis de voz (TTS) de respuestas.
- Modo multiventana / multiusuario.
- Persistencia continua / indexación semántica.
- Integraciones con Zoom/Teams directas.
- Traducción en vivo.
- Redacción o anonimización automática de PII.
- Procesamiento multi-parlante (diarización avanzada).
- Soporte macOS/Linux en F1.

## 6. Consideraciones de Diseño (UI/UX)

- Ventana principal minimal: área de transcripción (scroll autostick), barra de estado (latencia ms, estado API).
- Panel flotante: muestra respuesta generada; cierre con ESC.
- Colores modo claro/oscuro; tipografía monoespaciada para alinear tokens parciales.
- Indicador de chunk parcial (texto gris) y final (texto blanco).
- Icono en bandeja con menú contextual: Mostrar/Ocultar, Pausa, Resumen rápido, Configuración, Salir.
- Config simple: clave API, longitud contexto (s), idioma forzado (auto|es|en), prompt base, hotkey.

## 7. Consideraciones Técnicas

- Captura de audio Windows: WASAPI loopback (latencia baja).
- Pipeline: Captura PCM → normalización → buffer ring → fragmentación → envío a endpoint de transcripción (Whisper API streaming si disponible; si no, fallback a segment polling).
- Lenguaje probable: C# (.NET 8) + WPF o WinUI 3 (desktop) por rapidez y acceso nativo a WASAPI.
- Manejo de streaming: canal asíncrono (System.Threading.Channels) para desacoplar captura vs transmisión.
- Transcripción: Whisper API (OpenAI) con detección automática; si no soporta stream estable, pseudo-stream (chunk + parcial).
- Generación: Chat Completions (gpt-4o-mini o similar) con prompt: "Resume los últimos X segundos en ≤ Y palabras" configurable.
- Almacenamiento efímero: ring buffer de texto + timestamps para recortar contexto.
- Cifrado de clave API: DPAPI ProtectedData.LocalMachine o CurrentUser.
- Telemetría local (solo en memoria) para métricas (latencia media chunk, tokens usados).
- Actualizaciones: simple fetch JSON (versión) desde una URL (manual en F1 si hosting no definido).

## 8. Success Metrics

- Latencia media parcial < 500 ms (captura → pantalla).
- Respuesta generada < 1.5 s p95 (hotkey → texto final) para resúmenes cortos.
- Precisión percibida ≥ 90% en sesiones mixtas (feedback manual).
- Consumo CPU medio < 10% en equipo estándar (audio + UI + peticiones).
- Errores transcripción críticos (fallo total) < 2 por hora de uso.

## 9. Open Questions

1. ¿Se necesita fallback local (Whisper local) si la API no está disponible? (No definido).
2. ¿Se limitará tamaño máximo de contexto (memoria) para evitar costes? (Propuesta: máx 5 min).
3. ¿Formato / precisión del cálculo de coste (tokens estimados vs reales)?
4. ¿Se garantiza ofuscación o cifrado adicional de logs si se exportan?
5. ¿Canal futuro para atajos adicionales (resumen tipo bullets, acción QA)?

## 10. Riesgos y Mitigación

- Latencia > objetivo: Optimizar tamaño chunk (ej. 320ms), procesamiento asíncrono, batch envío.
- Costos escalando: añadir modo compresión contexto (resumen incremental) antes de enviar.
- Cambios API OpenAI: abstracción de proveedor (interface ITranscriptionProvider / IChatProvider).
- Captura fallida en algunos drivers: fallback a librería NAudio configuración manual.
- Saturación tokens: recorte adaptativo (trim más antiguo, summarization rolling).

## 11. Supuestos

- Usuario tiene conexión estable >=10 Mbps.
- Equipo con Windows 10+ y audio loopback habilitable.
- Whisper API soporta idioma mixto sin configuración extra.
- No se requiere empaquetado MSI avanzado en F1 (zip ejecutable suficiente).

## 12. MVP Checklist (Derivado)

- [ ] Captura WASAPI loopback básica.
- [ ] Fragmentación y envío a transcripción API.
- [ ] UI transcripción parcial/final.
- [ ] Buffer de contexto + hotkey resumen.
- [ ] Llamada chat y render panel respuesta.
- [ ] Configurable: API key, prompt, duración contexto, hotkey.
- [ ] Modo pausa / reanudar.
- [ ] Persistencia opcional manual (.txt).
- [ ] Estimación simple de coste tokens.
- [ ] Manejo de errores / reconexión.

---

Este documento está orientado a un desarrollador junior: cada FR es independiente y testeable. Actualizar tras resolver Open Questions.
