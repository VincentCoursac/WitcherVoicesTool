using System.Numerics;
using ImGuiNET;

namespace WitcherVoicesTool.Application.Panels;

public abstract class ContentPanel
{
    const float MENU_HEIGHT = 20;
    
    protected string PanelName = "Panel";
    protected bool bFullscreen = false;
    protected bool bOnlyUnframed = false;

    private bool bInitialized = false;
    private bool bWaitingShutdown = false;

    public void ReceiveInit()
    {
        Initialize();
        bInitialized = true;
    }

    public void ReceiveShutdown()
    {
        Shutdown();
    }
    
    public void Draw(int PanelId, float DeltaTime, bool bMenuOpened)
    {
        if (!bOnlyUnframed)
        {
            ImGuiWindowFlags Flags = ImGuiWindowFlags.None;

            if (bFullscreen)
            {
                ImGuiViewportPtr Viewport = ImGui.GetMainViewport();

                if (bMenuOpened)
                {
                    ImGui.SetNextWindowPos(Viewport.WorkPos + new Vector2(0, MENU_HEIGHT));
                    ImGui.SetNextWindowSize(Viewport.WorkSize - new Vector2(0, MENU_HEIGHT));
                }
                else
                {
                    ImGui.SetNextWindowPos(Viewport.WorkPos); 
                    ImGui.SetNextWindowSize(Viewport.WorkSize);
                }
            
                Flags |= ImGuiWindowFlags.NoDocking;
                Flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
                Flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
            }
            
            if (ImGui.Begin($"{PanelName}##{PanelId}", Flags))
            {
                DrawContent(DeltaTime);
            
                ImGui.End();
            }
        }

        DrawUnframed(DeltaTime);
    }

    public bool ShouldInitialize()
    {
        return !bInitialized;
    }

    public bool ShouldShutdown()
    {
        return bWaitingShutdown;
    }

    protected virtual void Initialize() {}
    protected virtual void Shutdown() {}
    
    protected virtual void DrawContent(float DeltaTime) {}
    protected virtual void DrawUnframed(float DeltaTime) {}

    protected void Close()
    {
        bWaitingShutdown = true;
    }

    protected void ReplacePanelBy(ContentPanel NewPanel)
    {
        bWaitingShutdown = true;
        ContentPanelManager.GetInstance().AddContentPanel(NewPanel);
    }

    public string GetPanelName()
    {
        return PanelName;
    }
}