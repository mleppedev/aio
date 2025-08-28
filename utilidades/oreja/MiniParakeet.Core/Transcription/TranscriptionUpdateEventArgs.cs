using System;

namespace MiniParakeet.Core.Transcription;

public sealed class TranscriptionUpdateEventArgs : EventArgs
{
    public TranscriptionUpdateEventArgs(string text, bool isFinal, TimeSpan latency)
    {
        Text = text;
        IsFinal = isFinal;
        Latency = latency;
    }

    public string Text { get; }
    public bool IsFinal { get; }
    public TimeSpan Latency { get; }
}
