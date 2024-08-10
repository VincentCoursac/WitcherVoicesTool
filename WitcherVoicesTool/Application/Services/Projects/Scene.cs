using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using WitcherVoicesTool.Application.Panels;
using WitcherVoicesTool.Utils;
using static VoicesToolProgram;
namespace WitcherVoicesTool.Application.Services;

public class SceneHeader
{
    public string Id = "";
    public string Name = "";
    public string ParentFolderId = "";
}

public class LineBase
{
    public string Id = "/";
    public string Character = "";
    public string LineText = "";
    
    [JsonIgnore] public bool bEditingText = false;
    
    public readonly List<LineAudioEntry> AudioEntries = new List<LineAudioEntry>();
    public readonly List<AudioSequenceEntry> Sequence = new List<AudioSequenceEntry>();
    
    [JsonIgnore] private WaveOut? WaveOut;

    public ISampleProvider? BuildProvider()
    {
        WaveFormat? Format = null;
        List<ISampleProvider> Providers = new List<ISampleProvider>();
                
        foreach (var Entry in Sequence)
        {
            if (Entry.Type == AudioSequenceEntryType.Audio)
            {
                if (Entry.Audio != null)
                {
                    Format = Entry.Audio.BuildSampleProvider()?.WaveFormat;
                    break;
                }
            }
            
            if (Entry.Type == AudioSequenceEntryType.Template)
            {
                if (Entry.Template != null)
                {
                    Format = Entry.Template.BuildProvider()?.WaveFormat;
                    break;
                }
            }
        }

        if (Format == null)
        {
            return null;
        }
                
        foreach (var Entry in Sequence)
        {
            if (Entry.Type == AudioSequenceEntryType.Audio)
            {
                if (Entry.Audio != null)
                {
                    ISampleProvider? BuiltProvider = Entry.Audio.BuildSampleProvider();
                    if (BuiltProvider != null)
                    {
                        Providers.Add(BuiltProvider);
                    }
                }
            }
            else if (Entry.Type == AudioSequenceEntryType.Silence)
            {
                Providers.Add(new SilenceProvider (Format).ToSampleProvider().Take(TimeSpan.FromSeconds(Entry.Duration)));
            }
            else if (Entry.Type == AudioSequenceEntryType.Template)
            {
                if (Entry.Template != null)
                {
                    ISampleProvider? BuiltProvider = Entry.Template.BuildProvider();
                    if (BuiltProvider != null)
                    {
                        Providers.Add(BuiltProvider);
                    }
                }
            }
        }
                
        return new ConcatenatingSampleProvider(Providers.ToArray());
    }
    
    public void Play()
    {
        ISampleProvider? Provider = BuildProvider();

        if (Provider != null)
        {
            WaveOut = new WaveOut();
            WaveOut.Init(BuildProvider());
            WaveOut.Play();
        }
    }
    
    public void OnPostLoad()
    {
        foreach (LineAudioEntry AudioEntry in AudioEntries)
        {
            AudioEntry.OnPostLoad();
        }

        foreach (AudioSequenceEntry Entry in Sequence)
        {
            Entry.OnPostLoad(this);
        }
    }
}

public class SceneLine : LineBase
{
    
}
public class Scene : ISavable
{
    public SceneHeader Header;

    public List<SceneLine> Lines = new List<SceneLine>();
    
    [JsonIgnore]
    public Project ParentProject;

    [JsonIgnore]
    private string AbsoluteSceneFolder;
    
    [JsonIgnore]
    private string RelativeSceneFolder;
    
    [JsonIgnore] 
    private WaveOut? WaveOut;
    
    public Scene(SceneHeader InHeader, string InRelativeSceneFolder, string InAbsoluteSceneFolder, Project InParentProject)
    {
        Header = InHeader;
        RelativeSceneFolder = InRelativeSceneFolder;
        AbsoluteSceneFolder = InAbsoluteSceneFolder;
        ParentProject = InParentProject;
    }

    public bool Save()
    {
        Directory.CreateDirectory(AbsoluteSceneFolder);

        string Json = JsonConvert.SerializeObject(this, Formatting.Indented);

        try
        {
            File.WriteAllText(GetSceneFilePath(), Json);

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return false;
        }
    }

    public ISampleProvider? BuildPlayListProvider()
    {
        WaveFormat? Format = null;
        List<ISampleProvider> Providers = new List<ISampleProvider>();
            
        foreach (var Line in Lines)
        {
            Format = Line.BuildProvider()?.WaveFormat;
            break;
        }

        if (Format == null)
        {
            return null;
        }
        
        foreach (var Line in Lines)
        {
            ISampleProvider? AudioProvider = Line.BuildProvider();
            if (AudioProvider != null)
            {
                Providers.Add(AudioProvider);
            }

            if (Line != Lines.Last())
            {
                Providers.Add(new SilenceProvider (Format).ToSampleProvider().Take(TimeSpan.FromSeconds(1.5f)));
            }
        }
            
        return new ConcatenatingSampleProvider(Providers.ToArray());
    }

    public void Play()
    {
        WaveOut = new WaveOut();
        WaveOut.Init(BuildPlayListProvider());
        WaveOut.Play();
    }

    public void ExportLine(SceneLine Line)
    {
        ExportLineInternal(Line);
        ExportLineInternal(Line, "16k", 16000);
    }

    private void ExportLineInternal(SceneLine Line, string Subfolder = "", int OutRate = -1)
    {
        int LineIndex = Lines.FindIndex(l => l.Id == Line.Id);
        if (!Macros.ENSURE(LineIndex != -1))
        {
            return;
        }

        string Filename = RelativeSceneFolder.Replace("/", "_");
        Filename += $"{FileUtils.GetSafeFilename(Header.Name)}_{FileUtils.GetSafeFilename(Line.Character)}_{(LineIndex + 1).ToString().PadLeft(2, '0')}.wav";
        string BaseFolder = GetExportedAudiosFolder();
        if (Subfolder.Length > 0)
        {
            BaseFolder = Path.Combine(BaseFolder, Subfolder);
        }
        Directory.CreateDirectory(BaseFolder);
        string AudioPath = Path.Combine(BaseFolder, Filename.ToLower());
        ISampleProvider? Provider = Line.BuildProvider();
        
        if (Provider == null)
        {
            return;
        }

        if (OutRate != -1)
        {
            var OutFormat = new WaveFormat(OutRate, Provider.WaveFormat.Channels);
            var Resampler = new WdlResamplingSampleProvider(Provider, OutRate);
            
            WaveFileWriter.CreateWaveFile16(AudioPath, Resampler);
        }
        else
        {
            WaveFileWriter.CreateWaveFile16(AudioPath, Provider);
        }
        
        
    }

    string GetSceneFilePath()
    {
        return Path.Combine(AbsoluteSceneFolder, FileUtils.GetSafeFilename(Header.Name) + ".vtscene");
    }

    string GetExportedAudiosFolder()
    {
        return Path.Combine(AbsoluteSceneFolder, "Waves");
    }

    public void OnPostLoad(string InRelativeSceneFolder, string InAbsoluteSceneFolder, Project InParentProject)
    {
        RelativeSceneFolder = InRelativeSceneFolder;
        AbsoluteSceneFolder = InAbsoluteSceneFolder;
        ParentProject = InParentProject;

        foreach (SceneLine Line in Lines)
        {
            Line.OnPostLoad();
        }
    }
}

public class SceneFolder
{
    public string Id = "";
    public string Name = "";
    public string SafeName = "";
    public string? ParentFolderId = null;

    public readonly List<SceneFolder> ChildrenFolders = new List<SceneFolder>();
    public List<SceneHeader> ScenesHeaders = new List<SceneHeader>();
}


