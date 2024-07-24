using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Flow;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Panels.Loading;

public class BuildVoiceLibraryPanel : ContentPanel
{
    private bool bBuildInProgress = false;
    private readonly FlowTaskSequencer BuildVoicesSequencer = new FlowTaskSequencer();

    private float CurrentBuildProgress = 0f;

    private bool bVoiceLinePathValid = false;
    private bool bAudioDirectoryPathValid = false;
    private bool bCanGenerateLibrary = false;
    
    public BuildVoiceLibraryPanel()
    {
        PanelName = "Build Voice Library";

        BuildVoicesSequencer.AddTask(new PopulateVoiceDatabaseTask());
        BuildVoicesSequencer.OnCompleted += OnDatabasePopulated;
    }

    protected override void Initialize()
    {
        CheckPaths();

        if (bVoiceLinePathValid && bAudioDirectoryPathValid)
        {
            bBuildInProgress = true;
            BuildVoicesSequencer.Start();
        }
    }

    private void OnDatabasePopulated(object? sender, EventArgs e)
    {
        CloseAndSummonNextPanels();
    }

    private void CloseAndSummonNextPanels()
    {
        Close();
        ContentPanelManager.GetInstance()?.AddContentPanel(CharactersPanel.GetInstance());
        ContentPanelManager.GetInstance()?.AddContentPanel(new LinesSearchPanel());
        ContentPanelManager.GetInstance()?.AddContentPanel(HomePanel.GetInstance());
    }
    protected override void DrawContent(float DeltaTime)
    {
        ImGui.BeginDisabled(bBuildInProgress);

        DrawFileListSection();
        DrawAudioFilesFolderSection();
        
        ImGui.Dummy(new Vector2(0,15));
        
        ImGui.BeginDisabled(!bCanGenerateLibrary);
        if (ImGui.Button("Generate library") && !bBuildInProgress)
        {
            bBuildInProgress = true;
            BuildVoicesSequencer.Start();
        }
        
        ImGui.EndDisabled();
        ImGui.EndDisabled();

        if (bBuildInProgress)
        {
            ImGui.Dummy(new Vector2(0,5));
            ImGui.Text("Building Library...");
            
            CurrentBuildProgress = MathUtils.InterpTo(CurrentBuildProgress, BuildVoicesSequencer.GetProgressRatio(), DeltaTime, 1f);
            ImGui.ProgressBar(CurrentBuildProgress, new Vector2(0.0f, 0.0f));
        }
    }
    
    void DrawFileListSection()
    {
        ImGui.SeparatorText("1. Locate the file containing the voice lines list (*.xlxx)");
        
        if (ImGui.Button("Locate..."))
        {
            using OpenFileDialog FileDialog = new OpenFileDialog();
            FileDialog.Filter = "Voice Lines (*.xlsx)|*.xlsx";
            FileDialog.RestoreDirectory = true;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesListFileLocation.Set(FileDialog.FileName);
            }
        }

        CheckPaths();
        
        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.Text, bVoiceLinePathValid ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1));

        ImGui.Text(ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesListFileLocation.Get());

        ImGui.PopStyleColor();

    }

    void DrawAudioFilesFolderSection()
    {
        ImGui.SeparatorText("2. Locate the folder containing the audio files (*.ogg)");
        
        if (ImGui.Button("Locate...##"))
        {
            using FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
       
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesAudioFilesLocation.Set(FolderDialog.SelectedPath);
            }
        }

        CheckPaths();
            
        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.Text, bAudioDirectoryPathValid ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1));

        ImGui.Text(ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesAudioFilesLocation.Get());

        ImGui.PopStyleColor();
     
    }

    private void CheckPaths()
    {
        bVoiceLinePathValid = File.Exists(ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesListFileLocation.Get());
        bAudioDirectoryPathValid = Directory.Exists(ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesAudioFilesLocation.Get());
        bCanGenerateLibrary = bVoiceLinePathValid && bAudioDirectoryPathValid;
    }
    
}