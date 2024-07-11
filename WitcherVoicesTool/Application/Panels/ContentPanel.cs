using ImGuiNET;

namespace WitcherVoicesTool.Application.Panels;

public abstract class ContentPanel
{
    protected string PanelName = "Panel";
    
    public void Draw(int PanelId, float DeltaTime)
    {
        if (ImGui.Begin(  $"{PanelName}##{PanelId}"))
        {
            DrawContent(DeltaTime);
            
            ImGui.End();
        }
    }
    
    protected abstract void DrawContent(float DeltaTime);

}