// Code found here : https://gist.github.com/markheath/8fb396a5fe4bf117f361, Thanks @markheath & @pengowray

using NAudio.Wave;

/// <summary>
/// Sample Provider to allow fading in and out
/// </summary>
public class DelayFadeOutSampleProvider : ISampleProvider
{
        private readonly object lockObject = new object();
    private readonly ISampleProvider source;

    private int fadeInStart;
    private int fadeInSamples;
    private int fadeOutStart;
    private int fadeOutSamples;

    private int position;

    /// <summary>
    /// Creates a new FadeInOutSampleProvider
    /// </summary>
    /// <param name="source">The source stream with the audio to be faded in or out</param>
    /// <param name="initiallySilent">If true, we start faded out</param>
    public DelayFadeOutSampleProvider(ISampleProvider source) {
        this.source = source;
    }

    public void SetFadeIn(TimeSpan FadeInLength, TimeSpan FadeInStartPosition) {
        lock (lockObject) {
            fadeInStart = (int)(FadeInStartPosition.TotalSeconds * source.WaveFormat.SampleRate); ;
            fadeInSamples = (int)(FadeInLength.TotalSeconds * source.WaveFormat.SampleRate);
        }
    }

    public void SetFadeIn(TimeSpan FadeInLength) {
        lock (lockObject) {
            fadeInStart = 0;
            fadeInSamples = (int)(FadeInLength.TotalSeconds * source.WaveFormat.SampleRate);
        }
    }

    public void SetFadeIn(double fadeDurationInMilliseconds) {
        lock (lockObject) {
            fadeInStart = 0;
            fadeInSamples = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
        }
    }

    /// <summary>
    /// Requests that a fade-out begins (will start on the next call to Read)
    /// </summary>
    /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
    public void SetFadeOut(double fadeAfterMilliseconds, double fadeDurationInMilliseconds) {
        lock (lockObject) {
            fadeOutStart = (int)((fadeAfterMilliseconds * source.WaveFormat.SampleRate) / 1000);
            fadeOutSamples = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
        }
    }

    public void SetFadeOut(TimeSpan FadeOutStartPosition, TimeSpan FadeOutLength) {
        lock (lockObject) {
            fadeOutStart = (int)(FadeOutStartPosition.TotalSeconds * source.WaveFormat.SampleRate); ;
            fadeOutSamples = (int)(FadeOutLength.TotalSeconds * source.WaveFormat.SampleRate);
        }
    }

    public void FadeEnding(TimeSpan FadeOutLength, TimeSpan SourceLength) {
        lock (lockObject) {
            var wave = source as IWaveProvider;
            fadeOutStart = (int)((SourceLength - FadeOutLength).TotalSeconds * source.WaveFormat.SampleRate);
            fadeOutSamples = (int)(FadeOutLength.TotalSeconds * source.WaveFormat.SampleRate);
        }
    }

    /// <summary>
    /// Reads samples from this sample provider
    /// </summary>
    /// <param name="buffer">Buffer to read into</param>
    /// <param name="offset">Offset within buffer to write to</param>
    /// <param name="count">Number of samples desired</param>
    /// <returns>Number of samples read</returns>
    public int Read(float[] buffer, int offset, int count) {
        int sourceSamplesRead = source.Read(buffer, offset, count);

        lock (lockObject) {
            for (int i = offset; i < offset + sourceSamplesRead; i++) {
                int samplePos = position + i;

                if (fadeInSamples > 0) {
                    if (samplePos < fadeInStart) {
                        buffer[i] = 0;
                    } else if (samplePos >= fadeInStart && samplePos < (fadeInStart + fadeInSamples)) {
                        buffer[i] *= (samplePos - fadeInStart) / (float)fadeInSamples;
                    }
                }

                if (fadeOutSamples > 0) {
                    if (samplePos >= fadeOutStart && samplePos < (fadeOutStart + fadeOutSamples)) {
                        buffer[i] *= (1 - ((samplePos - fadeOutStart) / (float)fadeOutSamples));
                    } else if (samplePos >= fadeOutStart + fadeOutSamples) {
                        buffer[i] = 0;
                    }
                }
            }
            position += sourceSamplesRead;
        }
        return sourceSamplesRead;
    }

    /// <summary>
    /// WaveFormat of this SampleProvider
    /// </summary>
    public WaveFormat WaveFormat {
        get { return source.WaveFormat; }
    }
}