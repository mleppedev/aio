# Parámetros de Audio (Decisiones Iniciales)

- **SampleRate**: 48000 Hz (mejor soporte voz + alineación con WASAPI loopback común).
- **Channels**: 2 (stereo capturado; mixdown a mono pendiente en etapa normalización futura).
- **BitsPerSample**: 16 (compatibilidad amplia y suficiente para transcripción).
- **Chunk objetivo**: 320 ms (equilibrio entre latencia percibida y coste overhead requests; tests usan 100 ms para simplificar).
- **Buffer interno capturador**: ~100 ms bloques simulados (en implementación real se adaptará al tamaño que WASAPI devuelva).

Pendiente: Implementación real de interop WASAPI y mixdown a mono (subtarea 1.3 expandida en fase posterior de optimización si se requiere mayor precisión de chunking real).
