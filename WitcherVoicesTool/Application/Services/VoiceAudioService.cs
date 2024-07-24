using System.Diagnostics;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Models;
using WitcherVoicesTool.Utils;

using static VoicesToolProgram;

namespace WitcherVoicesTool.Application.Services;

public class VoiceAudioService : Singleton<VoiceAudioService>
{
    private VorbisWaveReader? CurrentReader = null;
    private WaveOut? CurrentWaveOut = null;
    private VoiceLine? CurrentlyPlayedVoiceLine = null;
    
    public void PlayVoiceLine(VoiceLine Line)
    {
        string AudioPath = GetVoiceLineAudioPath(Line);
        if (!File.Exists(AudioPath))
        {
            Logger.Error($"Can't find audio at '{AudioPath}'");
            return;
        }

        CleanAudioDevice();

        CurrentReader = new VorbisWaveReader(AudioPath);
        CurrentWaveOut = new WaveOut();
        CurrentWaveOut.Init(CurrentReader);
        CurrentWaveOut.Play();

        CurrentlyPlayedVoiceLine = Line;

        CurrentWaveOut.PlaybackStopped += (sender, args) =>
        {
            CurrentlyPlayedVoiceLine = null;
        };
    }

    public void LocateVoiceLineAudio(VoiceLine Line)
    {
        string filePath = GetVoiceLineAudioPath(Line);
        if (!File.Exists(filePath))
        {
            return;
        }

        // combine the arguments together
        // it doesn't matter if there is a space after ','
        string argument = "/select, \"" + filePath +"\"";

        System.Diagnostics.Process.Start("explorer.exe", argument);
    }

    public int GetCurrentlyPlayedVoiceLineId()
    {
        return CurrentlyPlayedVoiceLine?.Id ?? -1;
    }

    void CleanAudioDevice()
    {
        if (CurrentReader != null)
        {
            CurrentReader.Dispose();
            CurrentReader = null;
        }
        
        if (CurrentWaveOut != null)
        {
            CurrentWaveOut.Dispose();
            CurrentWaveOut = null;
        }
    }

    public static string GetVoiceLineAudioPath(VoiceLine Line)
    {
        return Path.Combine(ApplicationSettings.Get().VoiceLibrarySettings.VoicesAudioFilesLocation.Get(),
            Line.LineId + ".wav.ogg");
    }
}