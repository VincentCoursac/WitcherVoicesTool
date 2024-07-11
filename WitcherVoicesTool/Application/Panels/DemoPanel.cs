using ImGuiNET;

namespace WitcherVoicesTool.Application.Panels;

public class DemoPanel : ContentPanel
{
    public DemoPanel(string Name = "Demo Panel")
    {
        PanelName = Name;
    }
    protected override void DrawContent(float DeltaTime)
    {
        ImGui.Text("Demo text");

        if (ImGui.Button("Add new demo panel"))
        {
            ContentPanelManager.GetInstance()?.AddContentPanel(new DemoPanel("Spawned Panel"));
        }
    }
}