using System.Numerics;
using ImGuiNET;

namespace WitcherVoicesTool.Application.Panels;

public sealed class ContentPanelManager
{
    private ContentPanelManager() { }

    private List<ContentPanel> Panels = new List<ContentPanel>();

    private bool bShowDemoWindow = false; //TODO: Move this
    private bool bShowMenuBar = true;

    public void AddContentPanel(ContentPanel Panel)
    {
        Panels.Add(Panel);
    }

    public void SetShowMenu(bool bShowMenu)
    {
        bShowMenuBar = bShowMenu;
    }
    
    public void Draw(float DeltaTime)
    {
        ImGuiViewportPtr Viewport = ImGui.GetMainViewport(); 
        ImGui.SetNextWindowPos(Viewport.WorkPos);
        ImGui.SetNextWindowSize(Viewport.WorkSize);
        ImGui.SetNextWindowViewport(Viewport.ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);

        ImGuiWindowFlags Flags = ImGuiWindowFlags.NoDocking;
        Flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        Flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

        if (bShowMenuBar)
        {
            Flags |= ImGuiWindowFlags.MenuBar;
        }

        ImGuiDockNodeFlags DockspaceFlags = ImGuiDockNodeFlags.PassthruCentralNode;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGui.Begin("FullscreenDock", Flags);

        ImGui.PopStyleVar(3);

        uint DockspaceId = ImGui.GetID("FullscreenDock");
        ImGui.DockSpace(DockspaceId, new Vector2(0.0f, 0.0f), DockspaceFlags);

        DrawMenu();


        ImGui.End();
 
        if (bShowDemoWindow)
        {
            ImGui.ShowDemoWindow();
        }
        
        for (int i = Panels.Count - 1; i >= 0; --i)
        {
            ImGui.PushID(i);
            ContentPanel Panel = Panels[i];
            Panel.Draw(i, DeltaTime);
            ImGui.PopID();
        }
    }

    void DrawMenu()
    {
        if (!bShowMenuBar)
        {
            return;
        }
        
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Debug"))
            {
                ImGui.MenuItem("Show Demo Window", "", ref bShowDemoWindow);
                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }
    }
    
    private static ContentPanelManager? Instance;
    
    public static ContentPanelManager? GetInstance()
    {
        return Instance ??= new ContentPanelManager();
    }
}