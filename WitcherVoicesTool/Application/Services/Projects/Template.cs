using Newtonsoft.Json;

namespace WitcherVoicesTool.Application.Services;

public class Template : LineBase
{
    public string Name = "";
    
    [JsonIgnore] public bool bEditingName = false;
}