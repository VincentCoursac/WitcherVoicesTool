using Newtonsoft.Json;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Settings;

public class ApplicationSettings
{
    
    private static readonly string SettingsDirectory = "Settings";
    private static readonly string SettingsFilename = "Settings.json";

    public readonly VoiceLibrarySettings VoiceLibrarySettings = new VoiceLibrarySettings();
    public readonly ProjectsSettings ProjectsSettings = new ProjectsSettings();
    public readonly TemplateSettings TemplateSettings = new TemplateSettings();
    public readonly ToolsSettings ToolsSettings = new ToolsSettings();

    private static ApplicationSettings? Instance;

    public static void Load()
    {
        if (File.Exists(GetSettingsFilePath()))
        {
            string Json = File.ReadAllText(GetSettingsFilePath());
            Instance = JsonConvert.DeserializeObject<ApplicationSettings>(Json);
        }
        else
        {
            Instance = new ApplicationSettings();
            Save();
        }
    }

    public static void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        string Json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
        File.WriteAllText(GetSettingsFilePath(), Json);
    }

    private static string GetSettingsFilePath()
    {
        return Path.Combine(SettingsDirectory, SettingsFilename);
    }

    public static ApplicationSettings? Get()
    {
        return Instance;
    }
}