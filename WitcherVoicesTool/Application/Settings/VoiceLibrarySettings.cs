using System.Text.Json.Serialization;

namespace WitcherVoicesTool.Application.Settings;

public class VoiceLibrarySettings
{
    public readonly SaveProperty<string?> VoicesListFileLocation = new SaveProperty<string?>("NONE");
    public readonly SaveProperty<string> VoicesAudioFilesLocation = new SaveProperty<string>("NONE");
}