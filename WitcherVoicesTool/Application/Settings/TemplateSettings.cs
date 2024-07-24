using Newtonsoft.Json;
using WitcherVoicesTool.Application.Services;

namespace WitcherVoicesTool.Application.Settings;

public class TemplateSettings : ISavable
{
    [JsonProperty]
    private readonly List<Template> Templates = new List<Template>();

    public void AddTemplate(Template NewTemplate)
    {
        NewTemplate.Id = Guid.NewGuid().ToString();
        Templates.Add(NewTemplate);
        Save();
    }

    public List<Template> GetTemplates()
    {
        return Templates;
    }

    public bool Save()
    {
        ApplicationSettings.Save();
        return true;
    }

    public void OnPostLoad()
    {
        foreach (var Template in Templates)
        {
            Template.OnPostLoad();
        }
    }

    public Template? FindTemplateById(string TemplateId)
    {
        foreach (var Template in Templates)
        {
            if (Template.Id.Equals(TemplateId))
            {
                return Template;
            }
        }

        return null;
    }
}