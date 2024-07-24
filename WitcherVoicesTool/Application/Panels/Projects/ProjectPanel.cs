using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Panels;

public class ProjectPanel : ContentPanel
{
    private readonly Project Project;

    private readonly PopupInvokator CreateScenePopup = new("Create Scene");
    private readonly PopupInvokator CreateSceneFolderPopup = new("Create Scene Folder");
    private SceneHeader? CreatingSceneHeader;
    private SceneFolder? CreatingSceneFolder;
    private SceneFolder? CreatingParentSceneFolder;
    
    public ProjectPanel(Project InProject)
    {
        Project = InProject;
        
        PanelName = $"{Project.Name} ###{Project.Id}";
    }

    protected override void DrawContent(float DeltaTime)
    {
        if (ImGui.Button("Create Folder"))
        {
            CreatingSceneFolder = new SceneFolder();
            CreatingParentSceneFolder = null;
            CreateSceneFolderPopup.RequestPopup();
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Create Scene"))
        {
            CreatingSceneHeader = new SceneHeader();
            CreatingParentSceneFolder = null;
            CreateScenePopup.RequestPopup();
        }
        
        ImGuiTableFlags TableFlags =  ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH  
                                    | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoBordersInBody;
        
        if (ImGui.BeginTable("Scenes", 4, TableFlags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, Theming.TEXT_BASE_WIDTH * 12.0f);
            ImGui.TableSetupColumn("Scenes", ImGuiTableColumnFlags.WidthFixed, Theming.TEXT_BASE_WIDTH * 12.0f);
            ImGui.TableSetupColumn("Lines", ImGuiTableColumnFlags.WidthFixed, Theming.TEXT_BASE_WIDTH * 12.0f);
            ImGui.TableHeadersRow();

            foreach(SceneFolder Folder in Project.RootFolder.ChildrenFolders)
            {
                DrawFolder(Folder);
            }
            
            foreach(SceneHeader Header in Project.RootFolder.ScenesHeaders)
            {
                DrawScene(Header);
            }
            
            ImGui.EndTable();
        }
    }

    void DrawFolder(SceneFolder Folder)
    {
        ImGui.PushID(Folder.Id);
        
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.SetNextItemAllowOverlap();
        bool bOpen = ImGui.TreeNodeEx(Folder.Name, ImGuiTreeNodeFlags.SpanAllColumns);
      
        ImGui.TableNextColumn();
        if (ImGui.SmallButton("+ F"))
        {
            CreatingSceneFolder = new SceneFolder();
            CreatingParentSceneFolder = Folder;
            CreateSceneFolderPopup.RequestPopup();
        }
        
        ImGui.SameLine();
        if (ImGui.SmallButton("+ S"))
        {
            CreatingSceneHeader = new SceneHeader();
            CreatingParentSceneFolder = Folder;
            CreateScenePopup.RequestPopup();
        }
        
        ImGui.TableNextColumn();
        ImGui.TextDisabled("0");
        ImGui.TableNextColumn();
        ImGui.TextDisabled("0");

        if (bOpen)
        {
            foreach (SceneFolder ChildFolder in Folder.ChildrenFolders)
            {
                DrawFolder(ChildFolder);
            }
            
            foreach (SceneHeader Header in Folder.ScenesHeaders)
            {
                DrawScene(Header);
            }
            
            ImGui.TreePop();
        }
        
        ImGui.PopID();
    }

    void DrawScene(SceneHeader Header)
    {
        ImGui.PushID(Header.Id);
        
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
      
        ImGui.SetNextItemAllowOverlap();
        ImGui.TreeNodeEx(Header.Name, ImGuiTreeNodeFlags.SpanAllColumns  | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
      
        ImGui.TableNextColumn();
        if (ImGui.SmallButton("Open"))
        {
            Project.OpenScene(Header);
        }
        ImGui.TableNextColumn();
        
        ImGui.TextDisabled("0");
        ImGui.TableNextColumn();
        ImGui.TextDisabled("0");
        
        ImGui.PopID();
    }

    protected override void DrawUnframed(float DeltaTime)
    {
        CreateScenePopup.TryOpenIfNeeded();
        CreateSceneFolderPopup.TryOpenIfNeeded();

        if (Widgets.ModalPopup(CreateScenePopup.GetPopupName()))
        {
            CreatingSceneHeader ??= new SceneHeader();
            
            ImGui.Dummy(new Vector2(350, 0));
            
            ImGui.Text("Path: ");
            ImGui.SameLine();
            ImGui.TextDisabled((CreatingParentSceneFolder != null ? Project.BuildFolderPath(CreatingParentSceneFolder) : "/" ) + $"{FileUtils.GetSafeFilename(CreatingSceneHeader.Name)}.vtscene");
            
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Scene Name   ");
            ImGui.SameLine();
            ImGui.PushItemWidth(-1);
            ImGui.InputTextWithHint("##SceneName", "Geralt meets Yarpen #1", ref CreatingSceneHeader.Name, 64);
            ImGui.PopItemWidth();
            
            ImGui.Dummy(new Vector2(0, 10));

            bool bOpenSceneAfterCreation = ApplicationSettings.Get().ProjectsSettings.bOpenSceneAfterCreation.Get();
            if (ImGui.Checkbox("Open scene after creation?", ref bOpenSceneAfterCreation))
            {
                ApplicationSettings.Get()?.ProjectsSettings.bOpenSceneAfterCreation.Set(bOpenSceneAfterCreation);
            }
            
            ImGui.Dummy(new Vector2(0, 5));
            
            if (Widgets.ColoredButton("Create", Theming.Yellow, CreatingSceneHeader.Name.Length == 0))
            {
                Scene? CreatedScene = Project.CreateScene(CreatingSceneHeader, CreatingParentSceneFolder);

                if (CreatedScene != null)
                {
                    ImGui.CloseCurrentPopup();

                    if (bOpenSceneAfterCreation)
                    {
                        ContentPanelManager.GetInstance().AddContentPanel(new ScenePanel(CreatedScene));
                    }
                }
            }
            
            ImGui.SameLine(0 ,5);
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
        
        if (Widgets.ModalPopup(CreateSceneFolderPopup.GetPopupName()))
        {
            CreatingSceneFolder ??= new SceneFolder();
            
            ImGui.Dummy(new Vector2(350, 0));

            ImGui.Text("Path: ");
            ImGui.SameLine();
            ImGui.TextDisabled((CreatingParentSceneFolder != null ? Project.BuildFolderPath(CreatingParentSceneFolder) : "/" ) + $"{FileUtils.GetSafeFilename(CreatingSceneFolder.Name)}/");
            
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Folder Name");
            ImGui.SameLine();
            ImGui.PushItemWidth(-1);
            ImGui.InputTextWithHint("##SceneFolder", "Quest #1", ref CreatingSceneFolder.Name, 64);
            ImGui.PopItemWidth();

            bool bFolderNameValid = Project.IsValidSceneFolderName(CreatingSceneFolder.Name, CreatingParentSceneFolder);

            if (!bFolderNameValid)
            {
                ImGui.TextColored(new Vector4(1,0,0,1), "Invalid folder name");
            }
            
            ImGui.Dummy(new Vector2(0, 15));
            
            if (Widgets.ColoredButton("Create", Theming.Yellow, !bFolderNameValid || CreatingSceneFolder.Name.Length == 0))
            {
                Project.CreateFolder(CreatingSceneFolder, CreatingParentSceneFolder);
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.SameLine(0 ,5);
            
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
    }
}