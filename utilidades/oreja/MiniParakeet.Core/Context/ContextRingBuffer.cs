using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniParakeet.Core.Context;

/// <summary>
/// Buffer circular basado en tiempo: conserva sólo entradas dentro de la ventana (capacidad segundos).
/// Pensado para almacenar segmentos finales de transcripción.
/// </summary>
public sealed class ContextRingBuffer
{
    private readonly object _sync = new();
    private readonly List<Entry> _entries = new();
    private readonly int _capacitySeconds;

    private record Entry(DateTime Utc, string Text);

    public ContextRingBuffer(int capacitySeconds)
    {
        if (capacitySeconds <= 0) throw new ArgumentOutOfRangeException(nameof(capacitySeconds));
        _capacitySeconds = capacitySeconds;
    }

    /// <summary>Agrega una nueva pieza finalizada.</summary>
    public void Add(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        lock (_sync)
        {
            _entries.Add(new Entry(DateTime.UtcNow, text.Trim()));
            TrimInternal();
        }
    }

    /// <summary>Devuelve concatenación de entradas dentro de los últimos N segundos.</summary>
    public string GetWindow(int seconds)
    {
        if (seconds <= 0) return string.Empty;
        var threshold = DateTime.UtcNow.AddSeconds(-seconds);
        lock (_sync)
        {
            return string.Join(" ", _entries.Where(e => e.Utc >= threshold).Select(e => e.Text));
        }
    }

    public int Count { get { lock (_sync) return _entries.Count; } }

    /// <summary>Limpia completamente el buffer.</summary>
    public void Clear()
    {
        lock (_sync) _entries.Clear();
    }

    private void TrimInternal()
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-_capacitySeconds);
        if (_entries.Count == 0) return;
        int firstValid = _entries.FindIndex(e => e.Utc >= cutoff);
        if (firstValid > 0)
            _entries.RemoveRange(0, firstValid);
    }
}
