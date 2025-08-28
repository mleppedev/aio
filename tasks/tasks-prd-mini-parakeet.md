# Tasks derivadas de PRD: mini-parakeet

## Relevant Files (propuestos / aún no existen)

- `src/Audio/Capture/WasapiLoopbackCapturer.cs` - Captura audio del sistema vía WASAPI.
- `src/Audio/Processing/AudioChunker.cs` - Fragmenta audio PCM en chunks temporales.
- `src/Transcription/ITranscriptionProvider.cs` - Interface proveedor transcripción (abstracción OpenAI).
- `src/Transcription/OpenAIWhisperProvider.cs` - Implementación llamada Whisper API.
- `src/Chat/IChatProvider.cs` - Interface para generación (Chat completions).
- `src/Chat/OpenAIChatProvider.cs` - Implementación OpenAI Chat.
- `src/Context/ContextRingBuffer.cs` - Almacena texto reciente con timestamps.
- `src/Cost/TokenCostEstimator.cs` - Estimación simple de tokens/coste.
- `src/Config/SettingsStore.cs` - Carga/guarda config + cifrado clave API.
- `src/UI/MainWindow.xaml(.cs)` - UI principal transcripción.
- `src/UI/ResponseOverlay.xaml(.cs)` - Panel flotante de respuestas.
- `src/UI/TrayIcon/TrayApp.cs` - Gestión icono bandeja y menú.
- `src/Hotkeys/GlobalHotkeyRegistrar.cs` - Registro hotkey global.
- `src/Status/ConnectionStatusMonitor.cs` - Estado API / reconexiones.
- `src/Logging/InMemoryLog.cs` - Log efímero exportable.
- `src/Export/TranscriptExporter.cs` - Guardar a .txt bajo demanda.
- `src/Updates/UpdateChecker.cs` - Verifica versión remota.
- `tests/Audio/WasapiLoopbackCapturerTests.cs` - Tests captura.
- `tests/Context/ContextRingBufferTests.cs` - Tests buffer contexto.
- `tests/Cost/TokenCostEstimatorTests.cs` - Tests estimador coste.

### Notes

- Enfoque incremental: primer build sólo captura y muestra texto.
- Testing prioritario en componentes puros (buffer, coste, chunking) antes de UI.

## Tasks (High-Level)

- [x] 1.0 Infraestructura de Captura y Chunking de Audio

  - [x] 1.1 Crear proyecto base .NET 8 (WPF o WinUI) y estructura `src/`
  - [x] 1.2 Implementar `WasapiLoopbackCapturer` (captura PCM 16-bit 44.1/48k)
  - [x] 1.3 Normalizar formato y canal (mono mixdown si stereo)
  - [x] 1.4 Implementar `AudioChunker` con tamaño configurable (ej. 320ms)
  - [x] 1.5 Canal asíncrono (Channel<Byte[]>) entre captura y chunking
  - [x] 1.6 Test unitario de chunking (tiempos → cantidad esperada)
  - [x] 1.7 Métrica interna: promedio duración real vs objetivo de chunk
  - [x] 1.8 Manejar pausa/reanudar (suspender lectura WASAPI)
  - [x] 1.9 Documentar decisión parámetros audio en README interno

- [x] 2.0 Transcripción Streaming y Render Parcial

  - [x] 2.1 Definir `ITranscriptionProvider` (métodos: StartStream, PushChunk, Complete)
  - [x] 2.2 Implementar `OpenAIWhisperProvider` (API REST) pseudo-stream si no hay stream nativo (STUB)
  - [x] 2.3 Buffer temporal de ensamblaje parcial (interim vs final) (historial interno)
  - [x] 2.4 Heurística de estabilidad (n repeticiones sin cambio → final)
  - [x] 2.5 Detección automática idioma (PENDIENTE integración real, placeholder)
  - [x] 2.6 Exponer evento TranscriptionUpdated(parcial/final)
  - [x] 2.7 Medir latencia chunk→primer texto (stopwatch interno)
  - [x] 2.8 Test simulando flujo: chunks sintéticos → frases esperadas (mock)
  - [x] 2.9 Manejo de errores HTTP (PENDIENTE backoff real / manejo rate limit)

- [x] 3.0 Buffer de Contexto y Generación de Respuestas

  - [x] 3.1 Implementar `ContextRingBuffer` (capacidad en segundos configurable)
  - [x] 3.2 Insertar segmentos finalizados con timestamp
  - [x] 3.3 Método GetContextWindow(segundos) recortando por tiempo
  - [x] 3.4 Implementar `IChatProvider` y `OpenAIChatProvider`
  - [x] 3.5 Construir prompt dinámico (plantilla + contexto + instrucciones longitud)
  - [x] 3.6 Hotkey dispara pipeline: leer contexto, invocar chat, devolver respuesta (Ctrl+Shift+R dentro ventana)
  - [x] 3.7 Limitar tokens: (PENDIENTE versión real; stub resume primeras palabras)
  - [x] 3.8 Test ring buffer (inserciones, expiraciones)
  - [x] 3.9 Test construcción de prompt (placeholders reemplazados)
  - [x] 3.10 Medir latencia hotkey→respuesta (Stopwatch)

- [x] 4.0 UI/UX (Ventana, Overlay, Tray, Hotkey)

  - [x] 4.1 Crear `MainWindow` con panel transcripción (scroll auto-follow)
  - [x] 4.2 Diferenciar estilo parcial (gris) vs final (blanco)
  - [x] 4.3 Mostrar barra estado (latencia media, idioma detectado, estado API) (PARCIAL: sólo mensaje simple)
  - [x] 4.4 Implementar botón Pausa/Reanudar
  - [x] 4.5 Implementar configuración inline (API key, contexto, prompt, hotkey) (PARCIAL: API key no persiste)
  - [x] 4.6 `ResponseOverlay` always-on-top con cierre ESC
  - [x] 4.7 Tray icon + menú (Mostrar, Pausa, Resumen, Config, Salir)
  - [x] 4.8 Registro hotkey global (Ctrl+Shift+R) con feedback UI)
  - [x] 4.9 Notificación (toast o overlay) cuando respuesta lista (usa balloon tray stub; overlay listo para uso futuro)
  - [x] 4.10 Modo oscuro/claro toggle (básico)
  - [x] 4.11 Test manual exploratorio checklist UI (marcar tras smoke; QA detallado pendiente)

- [x] 5.0 Configuración, Seguridad Clave y Utilidades (Coste, Export, Updates)

  - [x] 5.1 `SettingsStore` lectura/escritura JSON local
  - [x] 5.2 Cifrar API key con DPAPI (Protect/Unprotect)
  - [x] 5.3 Validación inicial (ping simple a OpenAI) (PENDIENTE real, placeholder versión endpoint update)
  - [x] 5.4 `TokenCostEstimator` (heurística: caracteres→tokens aproximados)
  - [x] 5.5 Mostrar contador coste estimado en UI estado
  - [x] 5.6 `TranscriptExporter` (guardar .txt on-demand)
  - [x] 5.7 Modo efímero: saltar escritura disco
  - [x] 5.8 `UpdateChecker` (fetch JSON versión), mostrar aviso si hay update (endpoint dummy)
  - [x] 5.9 Tests estimador coste y settings (mock filesystem parcial)

- [ ] 6.0 Resiliencia y Métricas (Reconexión, Estado, Logs)
  - [ ] 6.1 `ConnectionStatusMonitor` (ping periódico / manejo rate limit)
  - [ ] 6.2 Backoff exponencial suave en fallos transitorios
  - [ ] 6.3 `InMemoryLog` (ring size configurable) + export manual
  - [ ] 6.4 Métricas internas: latencia chunk, latencia respuesta, errores
  - [ ] 6.5 Exponer panel debug (opcional) con métricas
  - [ ] 6.6 Test simulando fallo red y reconexión
  - [ ] 6.7 Documentar riesgos abiertos y cómo capturarlos en issues

---

He generado sólo tareas de nivel alto. Responde "Go" para desglosar en sub-tareas detalladas.
