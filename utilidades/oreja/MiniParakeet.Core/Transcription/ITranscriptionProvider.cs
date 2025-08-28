using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Transcription;

/// <summary>
/// Abstracción de un proveedor de transcripción incremental (streaming o pseudo-streaming).
/// </summary>
public interface ITranscriptionProvider : IAsyncDisposable
{
    /// <summary>
    /// Evento disparado cada vez que hay actualización de texto (parcial o final).
    /// </summary>
    event EventHandler<TranscriptionUpdateEventArgs>? TranscriptionUpdated;

    /// <summary>
    /// Inicializa un stream de transcripción (reinicia estado interno).
    /// </summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Envía un chunk de audio PCM (16-bit, sample rate conocido) para procesar.
    /// </summary>
    Task PushChunkAsync(byte[] pcmChunk, CancellationToken ct = default);

    /// <summary>
    /// Señala que no habrá más audio; realiza flush final y cierra.
    /// </summary>
    Task CompleteAsync(CancellationToken ct = default);
}
