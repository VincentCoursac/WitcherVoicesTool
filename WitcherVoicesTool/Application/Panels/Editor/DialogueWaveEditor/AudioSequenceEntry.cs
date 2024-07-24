using Newtonsoft.Json;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Application.Settings;

namespace WitcherVoicesTool.Application.Panels;

public enum AudioSequenceEntryType
{
    Audio,
    Silence,
    Template
}

public class AudioSequenceEntry
{
    [JsonIgnore]
    public LineAudioEntry? Audio;

    [JsonIgnore] 
    public Template? Template;
    
    public AudioSequenceEntryType Type;
    public string AudioId = "";
    public string TemplateId = "";
    
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
    
    public static AudioSequenceEntry FromTemplate(Template InTemplate)
    {
        AudioSequenceEntry Entry = new AudioSequenceEntry
        {
            Type = AudioSequenceEntryType.Template,
            Template = InTemplate,
            TemplateId = InTemplate.Id
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
            case AudioSequenceEntryType.Template:
                return $"[{Template?.Name}]";
        }

        return "";
    }

    public void OnPostLoad(LineBase Line)
    {
        if (Type == AudioSequenceEntryType.Audio)
        {
            foreach (var AudioEntry in Line.AudioEntries)
            {
                if (AudioEntry.Id == AudioId)
                {
                    Audio = AudioEntry;
                    break;
                }
            }
        }
        
        else if (Type == AudioSequenceEntryType.Template)
        {
            Template = ApplicationSettings.Get().TemplateSettings.FindTemplateById(TemplateId);
        }
    }
}