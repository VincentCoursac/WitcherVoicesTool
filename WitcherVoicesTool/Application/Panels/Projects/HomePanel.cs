using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Application.Settings;

namespace WitcherVoicesTool.Application.Panels;

public class HomePanel : SingletonPanel<HomePanel>
{
    private readonly PopupInvokator CreateProjectPopup = new("Create Project");
    private Project? NewProject;
    public HomePanel()
    {
        bCanBeClosed = false;
        PanelName = "Home";
    }
    
    protected override void Initialize()
    {
        ProjectService.GetInstance().LoadRecentProjects();
        ApplicationSettings.Get().TemplateSettings.OnPostLoad();
    }

    protected override void DrawContent(float DeltaTime)
    {
        if (Widgets.ColoredButton("Create Project", Theming.Yellow))
        {
            NewProject = new Project();
            CreateProjectPopup.RequestPopup();
        }
        
        ImGui.SameLine();
        
        if (Widgets.ColoredButton("Open Project", Theming.Blue))
        {
            TryOpenProject();
        }
        
        ImGui.SameLine();

        if (ImGui.Button("Manage Templates"))
        {
            ContentPanelManager.GetInstance().AddContentPanel(new TemplatesPanel());
        }
        
        ImGui.SeparatorText("Recent projects");
        
        if (ProjectService.GetInstance().GetRecentProjects().Count > 0)
        {
            foreach (Project Project in ProjectService.GetInstance().GetRecentProjects())
            {
                ImGui.Text(Project.Name);
                ImGui.SameLine();
                if (ImGui.Button($"Open###{Project.Id}"))
                {
                    ProjectService.GetInstance().OpenProject(Project);
                }
            }
        }
        else
        {
            ImGui.TextColored(new Vector4(1,1,1,0.6f), "No recent projects...");
        }
    }

    protected override void DrawUnframed(float DeltaTime)
    {
        CreateProjectPopup.TryOpenIfNeeded();
        
        if (Widgets.ModalPopup(CreateProjectPopup.GetPopupName()))
        {
            ManageCreateProjectPopup();
            ImGui.EndPopup();
        }
    }

    void ManageCreateProjectPopup()
    {
        NewProject ??= new Project();
            
        ImGui.Text("Create your Project");
        ImGui.Separator();
        ImGui.Dummy(new Vector2(350,10));
        
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name   ");
        ImGui.SameLine();
        ImGui.PushItemWidth(-1);
        ImGui.InputTextWithHint("##ProjectName", "My Witcher 3 Mod", ref NewProject.Name, 64);
        ImGui.PopItemWidth();
        
        ImGui.Dummy(new Vector2(0, 5));
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Location");
        ImGui.SameLine();

        if (ImGui.Button("Choose..."))
        {
            using FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
   
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                NewProject.FolderPath = FolderDialog.SelectedPath;
            }
        }

        bool bIsDirectoryValid = false;
        
        if (Directory.Exists(NewProject.FolderPath))
        {
            bool bIsFolderEmpty = !Directory.EnumerateFileSystemEntries(NewProject.FolderPath).Any();
            
            if (bIsFolderEmpty)
            {
                ImGui.TextColored(new Vector4(0.2f,1,0.2f,1), NewProject.FolderPath);
                bIsDirectoryValid = true;
            }
            else
            {
                ImGui.TextColored(new Vector4(1,0.2f,0.2f,1), $"{NewProject.FolderPath} is not empty");
            }
        }
        else
        {
            ImGui.TextColored(new Vector4(1,0.2f,0.2f,1), "No path selected");
        }
        
        ImGui.Dummy(new Vector2(0, 15));
        
        if (Widgets.ColoredButton("Create", Theming.Yellow, !bIsDirectoryValid || NewProject.Name.Length == 0))
        {
            if (ProjectService.GetInstance().CreateProject(NewProject))
            {
                ImGui.CloseCurrentPopup();
            }
        }
        ImGui.SameLine(0 ,5);
        if (ImGui.Button("Cancel"))
        {
            ImGui.CloseCurrentPopup();
        }
    }

    void TryOpenProject()
    {
        using OpenFileDialog FileDialog = new OpenFileDialog();
        FileDialog.Filter = "Voice Tool Project (*.vtproject)|*.vtproject";
        FileDialog.RestoreDirectory = true;

        if (FileDialog.ShowDialog() == DialogResult.OK)
        {
            ProjectService.GetInstance().LoadProject(FileDialog.FileName);
        }
    }
}