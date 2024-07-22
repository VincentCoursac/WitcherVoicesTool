using System.Numerics;
using ImGuiNET;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Models;

namespace WitcherVoicesTool.Application.Panels;

class LineAudioEntry
{
    public int UniqueId;
    public VoiceLine Line;
    
    private VorbisWaveReader? Reader = null;
    private WaveOut? WaveOut;
    public readonly List<float> Samples = new List<float>();

    public float StartTime;
    public float EndTime;

    public float FadeInDuration = 0f;
    public float FadeOutDuration = 0f;

    public float TotalDuration;

    public string Name = "";

    public Vector4 Color = new Vector4(1, 1, 1, 1);

    public LineAudioEntry(int Id, VoiceLine InLine)
    {
        UniqueId = Id;
        Line = InLine;
        Name = $"{Line.LineId} - {Line.Character}";

        ImGui.ColorConvertHSVtoRGB(UniqueId / 16f, 0.6f, 0.6f, out Color.X, out Color.Y, out Color.Z);
        
        Reader = new VorbisWaveReader(VoiceAudioService.GetVoiceLineAudioPath(Line));
        var Buffer = new float[Reader.WaveFormat.SampleRate];
        
        int BytesRead;

        while ((BytesRead = Reader.Read(Buffer, 0, Buffer.Length)) > 0)
        {
            for (int i = 0; i < BytesRead; i++)
            {
                Samples.Add(Buffer[i] * 100);
            }
        }

        StartTime = 0f;
        EndTime = (float) Reader.TotalTime.TotalSeconds;

        TotalDuration = (float)Reader.TotalTime.TotalSeconds;

        /*WaveOut = new WaveOut();
        WaveOut?.Init(Reader);*/
    }

    public void Play()
    {
        WaveOut = new WaveOut();
        WaveOut.Init(BuildSampleProvider());
        WaveOut.Play();
    }

    public ISampleProvider? BuildSampleProvider()
    {
        Reader = new VorbisWaveReader(VoiceAudioService.GetVoiceLineAudioPath(Line));
        Reader.Position = 0;
        
        var TrimmedSampleProvider = new OffsetSampleProvider(Reader.ToSampleProvider())
        {
            SkipOver = TimeSpan.FromSeconds(StartTime),
            Take = TimeSpan.FromSeconds(EndTime - StartTime)
        };
        
   
        var FadeInSampleProvider = new FadeInOutSampleProvider(TrimmedSampleProvider);

        if (FadeInDuration > 0)
        {
            FadeInSampleProvider.BeginFadeIn(FadeInDuration * 1000);
        }

        var FadeOutSampleProvider = new DelayFadeOutSampleProvider(FadeInSampleProvider);
        
        if (FadeOutDuration > 0)
        {
            FadeOutSampleProvider.BeginFadeOut((EndTime - StartTime - FadeOutDuration) * 1000, FadeOutDuration * 1000);
        }

        return FadeOutSampleProvider;
    }

    public string FormattedName()
    {
        return $"[{UniqueId}] {Name}";
    }
}