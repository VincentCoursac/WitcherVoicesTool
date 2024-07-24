using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Flow;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Panels.Loading;

public class LoadApplicationPanel : ContentPanel
{
    private readonly FlowTaskSequencer Sequencer = new FlowTaskSequencer();
    private bool bLoading = true;

    private float CurrentProgress = 0f;
    public LoadApplicationPanel()
    {
        bOnlyUnframed = true;
    }

    protected override void Initialize()
    {
        Sequencer.OnCompleted += OnCompletedCallback;
        Sequencer.Start();
       
        ImGui.OpenPopup("Loading");
    }
    
    protected override void Shutdown()
    {
        
    }

    protected override void DrawContent(float DeltaTime)
    {
        
    }

    protected override void DrawUnframed(float DeltaTime)
    {
        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        
        if (ImGui.BeginPopupModal("Loading", ref bLoading, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar))
        {
            if (Sequencer.IsInProgress())
            {
                ImGui.Text("Loading...");
                CurrentProgress = MathUtils.InterpTo(CurrentProgress, Sequencer.GetProgressRatio(), DeltaTime, 1f);
                ImGui.ProgressBar(CurrentProgress, new Vector2(0.0f, 0.0f));
            }
            else if (Sequencer.WasCanceled())
            {
                ImGui.TextColored(new Vector4(1,0,0,1), "Failed");
            }
            
            ImGui.EndPopup();
        }
    }

    void OnCompletedCallback(object? sender, EventArgs e)
    {
        ContentPanelManager.GetInstance().SetShowMenu(true);
        ReplacePanelBy(new BuildVoiceLibraryPanel());
    }
}