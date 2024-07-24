using Newtonsoft.Json;
using WitcherVoicesTool.Application.Panels;
using WitcherVoicesTool.Utils;
using static VoicesToolProgram;
namespace WitcherVoicesTool.Application.Services;

public class Project
{
    public string Id = "";
    public string Name = "";
    public string FolderPath = "";
    public string ProjectFileName = "";

    public readonly SceneFolder RootFolder = new SceneFolder{Id = "Root", SafeName = "./"};

    [JsonIgnore]
    private Dictionary<string, SceneFolder> FoldersMapping = new Dictionary<string, SceneFolder>();

    [JsonIgnore]
    private Dictionary<string, Scene> ScenesCache = new Dictionary<string, Scene>();
    
    public void OnPostLoad()
    {
        LoadSavedFolder(RootFolder);
    }

    void LoadSavedFolder(SceneFolder Folder)
    {
        FoldersMapping.TryAdd(Folder.Id, Folder);
        
        foreach (SceneFolder ChildFolder in Folder.ChildrenFolders)
        {
            LoadSavedFolder(ChildFolder);
        }
    }
    public void CreateFolder(SceneFolder Folder, SceneFolder? Parent)
    {
        Folder.Id = Guid.NewGuid().ToString();
        Folder.ParentFolderId = Parent?.Id;
        Folder.SafeName = FileUtils.GetSafeFilename(Folder.Name);

        //Directory.CreateDirectory(Folder.SafeName);

        if (Parent != null)
        {
            Parent.ChildrenFolders.Add(Folder);
        }
        else
        {
            RootFolder.ChildrenFolders.Add(Folder);
        }
        
        FoldersMapping.TryAdd(Folder.Id, Folder);
        
        Save();
    }

    public Scene? CreateScene(SceneHeader NewHeader, SceneFolder? Parent)
    {
        SceneFolder FinalParent = Parent ?? RootFolder;
        
        NewHeader.Id = Guid.NewGuid().ToString();
        NewHeader.ParentFolderId = FinalParent.Id;
        
        string RelativeSceneFolder = BuildFolderPath(FinalParent);
        string AbsoluteSceneFolder = Path.Combine(FolderPath, RelativeSceneFolder);
        
        Scene Scene = new Scene(NewHeader, RelativeSceneFolder, AbsoluteSceneFolder, this);
        FinalParent.ScenesHeaders.Add(NewHeader);

        if (Save() && Scene.Save())
        {
            return Scene;
        }

        return null;
    }
    
    public bool Save()
    {
        string Json = JsonConvert.SerializeObject(this, Formatting.Indented);

        try
        {
            File.WriteAllText(GetProjectFilePath(), Json);
                   
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return false;
        }
    }
    public string GetProjectFilePath()
    {
        return Path.Combine(FolderPath, ProjectFileName + ".vtproject");
    }

    public string BuildFolderPath(SceneFolder Folder)
    {
        string CurrentPath = Folder.SafeName + "/";

        if (Folder.ParentFolderId != null)
        {
           return BuildFolderPath(FoldersMapping[Folder.ParentFolderId]) + CurrentPath;
        }

        return CurrentPath;
    }

    public bool IsValidSceneFolderName(string FolderName, SceneFolder? ParentFolder)
    {
        string SafeFileName = FileUtils.GetSafeFilename(FolderName);
        
        foreach (SceneFolder Folder in ParentFolder?.ChildrenFolders ?? RootFolder.ChildrenFolders)
        {
            if (Folder.SafeName.Equals(SafeFileName))
            {
                return false;
            }
        }

        return true;
    }

    public SceneFolder? GetFolderById(string FolderId)
    {
        if (Macros.ENSURE(FoldersMapping.ContainsKey(FolderId)))
            return FoldersMapping[FolderId];
        return null;
    }

    public void OpenScene(SceneHeader Header)
    {
        Scene? LoadedScene = TryLoadScene(Header);
        if (LoadedScene != null)
        {
            ContentPanelManager.GetInstance().AddContentPanel(new ScenePanel(LoadedScene));
        }
    }

    Scene? TryLoadScene(SceneHeader Header)
    {
        if (ScenesCache.TryGetValue(Header.Id, out var FoundScene))
        {
            return FoundScene;
        }
        
        SceneFolder? Folder = GetFolderById(Header.ParentFolderId);

        if (Folder == null)
            return null;
        
        string RelativeSceneFolder = BuildFolderPath(Folder);
        string AbsoluteSceneFolder = Path.Combine(FolderPath, RelativeSceneFolder);
        string SceneFilePath = Path.Combine(AbsoluteSceneFolder, FileUtils.GetSafeFilename(Header.Name) + ".vtscene");

        if (File.Exists(SceneFilePath))
        {
            string SceneJson = File.ReadAllText(SceneFilePath);
            Scene Scene = JsonConvert.DeserializeObject<Scene>(SceneJson);
            
            if (Macros.ENSURE(Scene != null))
            {
                Scene?.OnPostLoad(RelativeSceneFolder, AbsoluteSceneFolder, this);
                ScenesCache.TryAdd(Header.Id, Scene);
                return Scene;
            }
        }

        return null;
    }
}