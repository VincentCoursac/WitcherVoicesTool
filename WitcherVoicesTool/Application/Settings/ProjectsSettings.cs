namespace WitcherVoicesTool.Application.Settings;

public class ProjectsSettings
{
    public readonly List<string> RecentProjects = new List<string>();
    public readonly SaveProperty<bool> bOpenSceneAfterCreation = new SaveProperty<bool>(true);
}