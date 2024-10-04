#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WitcherVoicesTool.Application.Panels;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Utils;

using static VoicesToolProgram;

namespace WitcherVoicesTool.Application.Services;

public class ProjectService : Singleton<ProjectService>
{
    private readonly List<Project> RecentProjects = new List<Project>();
    
    private Project? CurrentProject;
    public bool CreateProject(Project NewProject)
    {
        NewProject.Id = Guid.NewGuid().ToString();
        NewProject.ProjectFileName = FileUtils.GetSafeFilename(NewProject.Name);
            
        CurrentProject = NewProject;
        if (CurrentProject.Save())
        {
            List<string> RecentProjectsPaths = ApplicationSettings.Get().ProjectsSettings.RecentProjects;
            RecentProjectsPaths.Add(NewProject.GetProjectFilePath());
            if (RecentProjectsPaths.Count > 10)
            {
                RecentProjectsPaths.RemoveAt(0);
            }
            ApplicationSettings.Save();
            ContentPanelManager.GetInstance().AddContentPanel(new ProjectPanel(CurrentProject));
            LoadRecentProjects();
            return true;
        }

        return false;
    }

    public void LoadProject(string ProjectPath)
    {
        List<string> RecentProjectsPaths = ApplicationSettings.Get().ProjectsSettings.RecentProjects;
        if (RecentProjectsPaths.Contains(ProjectPath))
        {
            return;
        }
        
        string ProjectJson = File.ReadAllText(ProjectPath);
        Project Project = JsonConvert.DeserializeObject<Project>(ProjectJson);
            
        if (Macros.ENSURE(Project != null))
        {
            Project.OnPostLoad();
            CurrentProject = Project;
            RecentProjectsPaths.Add(ProjectPath);
            RecentProjects.Add(CurrentProject);
            
            ApplicationSettings.Save();
            ContentPanelManager.GetInstance().AddContentPanel(new ProjectPanel(CurrentProject));
        }
    }
    
    public void LoadRecentProjects()
    {
        RecentProjects.Clear();

        List<string> RecentProjectsPaths = ApplicationSettings.Get().ProjectsSettings.RecentProjects;

        foreach (string ProjectPath in RecentProjectsPaths.ToList())
        {
            if (!File.Exists(ProjectPath))
            {
                Logger.Error($"Invalid recent project path '{ProjectPath}'");
                RecentProjectsPaths.Remove(ProjectPath);
                ApplicationSettings.Save();
                continue;
            }
            
            string ProjectJson = File.ReadAllText(ProjectPath);
            Project Project = JsonConvert.DeserializeObject<Project>(ProjectJson);
            
            if (Macros.ENSURE(Project != null))
            {
                Project.OnPostLoad();
                RecentProjects.Add(Project);
            }
        }
    }

    public List<Project> GetRecentProjects()
    {
        return RecentProjects;
    }

    public void OpenProject(Project Project)
    {
       ContentPanelManager.GetInstance().AddContentPanel(new ProjectPanel(Project));
    }
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.