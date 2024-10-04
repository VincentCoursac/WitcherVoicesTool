using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Flow;
using WitcherVoicesTool.Flow.Tools;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Panels.Tools;

public class LipsyncConverterPanel : ContentPanel
{
    private bool bWorkInprogress = false;
    private readonly FlowTaskSequencer ConverterSequencer = new FlowTaskSequencer();
    private float CurrentConversionProgress;
    public LipsyncConverterPanel()
    {
        PanelName = "16k Converter";
        ConverterSequencer.OnCompleted += OnConversionCompleted;
    }

    private void OnConversionCompleted(object? sender, EventArgs e)
    {
        bWorkInprogress = false;
    }
    protected override void DrawContent(float DeltaTime)
    {
        ImGui.BeginDisabled(bWorkInprogress);

        float MaxWaveDuration = ApplicationSettings.Get().ToolsSettings.LipsyncConverterMaxAudioDuration.Get();
        ImGui.SetNextItemWidth(200);
        if (ImGui.InputFloat("Max audio duration in seconds (0 = no limit)", ref MaxWaveDuration))
        {
            ApplicationSettings.Get().ToolsSettings.LipsyncConverterMaxAudioDuration.Set(MaxWaveDuration);
        }
        
        if (ImGui.Button("Pick Folder...") && !bWorkInprogress)
        {
            using FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
       
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                string FolderPath = FolderDialog.SelectedPath;
                ConverterSequencer.Reset();
                CurrentConversionProgress = 0f;
                ConverterSequencer.AddTask(new LipsyncConverterTask(FolderPath));
                ConverterSequencer.Start();
                
                bWorkInprogress = true;
            }
        }
        ImGui.EndDisabled();

        if (bWorkInprogress)
        {
            CurrentConversionProgress = MathUtils.InterpTo(CurrentConversionProgress, ConverterSequencer.GetProgressRatio(), DeltaTime, 1f);
            ImGui.ProgressBar(CurrentConversionProgress, new Vector2(0.0f, 0.0f));
        }
    }
}