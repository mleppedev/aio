using System;

namespace MiniParakeet.Core.LocalWhisper;

/// <summary>
/// Resample lineal simple (downsample) de 48k (o 44.1k) a 16k mono para whisper.
/// NOTA: Implementación rápida; para calidad mejor usar lib especializada.
/// </summary>
public static class PcmResampler
{
    public static float[] ToMono16k(short[] interleaved, int sourceSampleRate, int channels)
    {
        if (channels <= 0) throw new ArgumentOutOfRangeException(nameof(channels));
        double ratio = 16000.0 / sourceSampleRate;
        int frames = interleaved.Length / channels;
        int targetFrames = (int)Math.Floor(frames * ratio);
        var mono = new float[targetFrames];
        for (int i = 0; i < targetFrames; i++)
        {
            double srcPos = i / ratio;
            int i0 = (int)srcPos;
            double frac = srcPos - i0;
            if (i0 >= frames - 1) i0 = frames - 2;
            int i1 = i0 + 1;
            // Mix channels average
            float s0 = 0, s1 = 0;
            for (int c = 0; c < channels; c++)
            {
                s0 += interleaved[i0 * channels + c];
                s1 += interleaved[i1 * channels + c];
            }
            s0 /= channels; s1 /= channels;
            var sample = (1 - frac) * s0 + frac * s1;
            mono[i] = (float)(sample / short.MaxValue);
        }
        return mono;
    }
}
