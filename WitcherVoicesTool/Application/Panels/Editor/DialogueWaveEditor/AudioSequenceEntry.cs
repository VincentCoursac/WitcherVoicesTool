using Newtonsoft.Json;
using WitcherVoicesTool.Application.Services;

namespace WitcherVoicesTool.Application.Panels;

public enum AudioSequenceEntryType
{
    Audio,
    Silence
}

public class AudioSequenceEntry
{
    [JsonIgnore]
    public LineAudioEntry? Audio;
    
    public AudioSequenceEntryType Type;
    public string AudioId = "";
    public float Duration = 0f;

    public static AudioSequenceEntry FromAudio(LineAudioEntry InAudio)
    {
        AudioSequenceEntry Entry = new AudioSequenceEntry
        {
            Type = AudioSequenceEntryType.Audio,
            Audio = InAudio,
            AudioId = InAudio.Id,
            Duration = InAudio.TotalDuration
        };
        return Entry;
    }
    
    public static AudioSequenceEntry FromSilence(float SilenceDuration)
    {
        AudioSequenceEntry Entry = new AudioSequenceEntry
        {
            Type = AudioSequenceEntryType.Silence,
            Duration = SilenceDuration
        };
        return Entry;
    }
    public string GetName()
    {
        switch (Type)
        {
            case AudioSequenceEntryType.Audio:
                return Audio?.Name ?? string.Empty;
            case AudioSequenceEntryType.Silence:
                return "[Silence]";
        }

        return "";
    }

    public void OnPostLoad(SceneLine SceneLine)
    {
        if (Type == AudioSequenceEntryType.Audio)
        {
            foreach (var AudioEntry in SceneLine.AudioEntries)
            {
                if (AudioEntry.Id == AudioId)
                {
                    Audio = AudioEntry;
                    break;
                }
            }
        }
    }
}