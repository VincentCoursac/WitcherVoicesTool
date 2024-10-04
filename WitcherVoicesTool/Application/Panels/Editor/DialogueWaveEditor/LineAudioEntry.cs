using System.Numerics;
using ImGuiNET;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Models;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Panels;

public class LineAudioEntry
{
    public string Id;
    public VoiceLine Line;
    
    [JsonIgnore]
    private VorbisWaveReader? Reader = null;
    
    [JsonIgnore]
    private WaveOut? WaveOut;
    
    [JsonIgnore]
    public readonly List<float> Samples = new List<float>();

    public float StartTime;
    public float EndTime;

    public float TotalDuration;

    public string Name = "";

    public Vector4 Color = new Vector4(1, 1, 1, 1);

    public LineAudioEntry()
    {
        Id = "";
        Line = new VoiceLine();
    }

    public LineAudioEntry(VoiceLine InLine)
    {
        Id = Guid.NewGuid().ToString();
        Line = InLine;
        Name = $"{Line.LineId} - {Line.Character}";

        ImGui.ColorConvertHSVtoRGB(MathUtils.RandomFloatInRange(0,16) / 16f, 0.6f, 0.6f, out Color.X, out Color.Y, out Color.Z);

        SetupSamplesAndDurations(true);
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
        
   
        /*var FadeProvider = new DelayFadeOutSampleProvider(TrimmedSampleProvider);

        if (FadeInDuration > 0)
        {
            FadeProvider.SetFadeIn(FadeInDuration * 1000);
        }
        
        if (FadeOutDuration > 0)
        {
            //FadeProvider.BeginFadeOut((EndTime - StartTime - FadeOutDuration) * 1000, FadeOutDuration * 1000);
            FadeProvider.SetFadeOut((EndTime - StartTime - FadeOutDuration) * 1000, FadeOutDuration * 1000);
        }*/

        return TrimmedSampleProvider;
    }

    public string FormattedName()
    {
        return $"[{Line.LineId}] {Name}";
    }

    public void OnPostLoad()
    {
        SetupSamplesAndDurations(false);
    }

    void SetupSamplesAndDurations(bool bFirstTime)
    {
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

        if (bFirstTime)
        {
            StartTime = 0f;
            EndTime = (float) Reader.TotalTime.TotalSeconds;
        }
        
        TotalDuration = (float)Reader.TotalTime.TotalSeconds;
    }
}