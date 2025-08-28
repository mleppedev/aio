using System;
using System.Threading;
using System.Threading.Channels;

namespace MiniParakeet.Core.Audio;

public interface IAudioCapturer : IAsyncDisposable
{
    /// <summary>
    /// Inicia la captura y entrega buffers PCM crudos (16-bit little endian) a través del canal proporcionado.
    /// </summary>
    /// <param name="channel">Canal de salida de buffers.</param>
    /// <param name="ct">Token cancelación.</param>
    void Start(ChannelWriter<byte[]> channel, CancellationToken ct = default);

    /// <summary>
    /// Pausa la captura (no cierra recursos).
    /// </summary>
    void Pause();

    /// <summary>
    /// Reanuda tras una pausa.
    /// </summary>
    void Resume();
}
