using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WitcherVoicesTool.Application.Settings;

namespace WitcherVoicesTool.Flow.Tools;

using static VoicesToolProgram;

public class LipsyncConverterTask : FlowTask
{
    private string FolderPath;

    private int TotalWaveFiles = 0;
    private int ConvertedWaveFiles = 0;
    
    public LipsyncConverterTask(string InFolderPath)
    {
        FolderPath = InFolderPath;
    }
    protected override bool Execute()
    {
        if (!Directory.Exists(FolderPath))
        {
            return false;
        }

        string OutputFolderDir = Path.Combine(FolderPath, "16k");

        if (!Directory.Exists(OutputFolderDir))
        {
            Directory.CreateDirectory(OutputFolderDir);
        }
        
        string[] WavFiles = Directory.GetFiles(FolderPath, "*.wav");
        TotalWaveFiles = WavFiles.Length;

        foreach (var WavFilePath in WavFiles)
        {
            try
            {
                string OutputFile = Path.Combine(OutputFolderDir, Path.GetFileName(WavFilePath));
                using var Reader = new AudioFileReader(WavFilePath);

                float MaxWaveDuration = ApplicationSettings.Get().ToolsSettings.LipsyncConverterMaxAudioDuration.Get();

                if (MaxWaveDuration > 0 && Reader.TotalTime.TotalSeconds <= MaxWaveDuration)
                {
                    ISampleProvider SourceProvider = Reader;

                    if (Reader.WaveFormat.Channels == 2)
                    {
                        var MonoConverter = new StereoToMonoSampleProvider(SourceProvider);
                        MonoConverter.LeftVolume = 0.5f; 
                        MonoConverter.RightVolume = 0.5f;

                        SourceProvider = MonoConverter;
                    }
                    
                    var Resampler = new WdlResamplingSampleProvider(SourceProvider, 16000);
                    WaveFileWriter.CreateWaveFile16(OutputFile, Resampler);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Could not convert wave file '{WavFilePath}'");
                Logger.Error(e);
            }
            
            ++ConvertedWaveFiles;
        }
        
        return true;
    }
    
    public override float GetEstimatedCost()
    {
        return TotalWaveFiles;
    }

    protected override float GetOngoingProgress()
    {
        return ConvertedWaveFiles;
    }
}