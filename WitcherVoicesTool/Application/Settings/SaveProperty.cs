using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WitcherVoicesTool.Application.Settings;

public class SaveProperty<T>
{
    [JsonProperty]
    private T Value { get; set; }
    
    public SaveProperty(T InitialValue)
    {
        Value = InitialValue;
    }

    public T Get()
    {
        return Value;
    }

    public void Set(T NewValue)
    {
        Value = NewValue;
        ApplicationSettings.Save();
    }
}