using System.Numerics;
using ImGuiNET;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WitcherVoicesTool.Models;

namespace WitcherVoicesTool.Application.Panels;

enum AudioSequenceEntryType
{
    Audio,
    Silence
}

class AudioSequenceEntry
{
    public AudioSequenceEntryType Type;
    public LineAudioEntry? Audio;
    public float Duration = 0f;

    public static AudioSequenceEntry FromAudio(LineAudioEntry InAudio)
    {
        AudioSequenceEntry Entry = new AudioSequenceEntry();
        Entry.Type = AudioSequenceEntryType.Audio;
        Entry.Audio = InAudio;
        Entry.Duration = InAudio.TotalDuration;
        return Entry;
    }
    
    public static AudioSequenceEntry FromSilence(float SilenceDuration)
    {
        AudioSequenceEntry Entry = new AudioSequenceEntry();
        Entry.Type = AudioSequenceEntryType.Silence;
        Entry.Duration = SilenceDuration;
        return Entry;
    }

    public string GetName()
    {
        switch (Type)
        {
            case AudioSequenceEntryType.Audio:
                return Audio?.Name ?? string.Empty;
            case AudioSequenceEntryType.Silence:
                return "[Silence]";
        }

        return "";
    }
}
public class DialogueWaveEditorPanel : ContentPanel
{
    public static readonly List<DialogueWaveEditorPanel> OpenedWaveEditors = new List<DialogueWaveEditorPanel>();

    private int CurrentUniqueId = 0;

    private readonly List<AudioSequenceEntry> Sequence = new List<AudioSequenceEntry>();
    
    private readonly List<LineAudioEntry> AudioEntries = new List<LineAudioEntry>();

    private LineAudioEntry? CurrentlyEditingEntryName = null;
    private bool bShouldOpenRenamePopup = false;
    private string CurrentEditedName = "";

    private LineAudioEntry? SelectedAudio = null;
    
    private WaveOut? WaveOut;
    
    public DialogueWaveEditorPanel()
    {
        PanelName = $"Dialogue Editor #{OpenedWaveEditors.Count + 1}";
    }

    protected override void Initialize()
    {
        OpenedWaveEditors.Add(this);
    }

    protected override void DrawContent(float DeltaTime)
    {
        if (ImGui.BeginChild("Final Wave",  new Vector2(ImGui.GetContentRegionAvail().X, 170), ImGuiChildFlags.Border, ImGuiWindowFlags.None))
        {
            ImGui.BeginChild("left pane", new Vector2(350, 0), ImGuiChildFlags.AlwaysUseWindowPadding);

            if (AudioEntries.Count > 0)
            {
                if (ImGui.BeginCombo("Pick Audio", SelectedAudio?.FormattedName()))
                {
                    foreach (var Entry in AudioEntries)
                    {
                        bool bIsSelected = Entry == SelectedAudio;

                        if (ImGui.Selectable(Entry.FormattedName(), bIsSelected))
                        {
                            SelectedAudio = Entry;
                        }

                        if (bIsSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else
            {
                ImGui.TextColored(new Vector4(1,0,0,1), "Add Audio First");
            }

            ImGui.BeginDisabled(AudioEntries.Count == 0);
            if (ImGui.Button("Add audio") && SelectedAudio != null)
            {
                Sequence.Add(AudioSequenceEntry.FromAudio(SelectedAudio));
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Add silence"))
            {
                Sequence.Add(AudioSequenceEntry.FromSilence(1));
            }
          
            ImGui.Dummy(new Vector2(0,1));
            
            ImGuiTableFlags TableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg |
                                         ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV
                                         | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Hideable;
            
            Vector2 outer_size = new Vector2(0.0f, ImGui.GetContentRegionAvail().Y);

            if (ImGui.BeginTable("Audios", 3, TableFlags, outer_size))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.NoResize, 100);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.NoResize);
                
                ImGui.TableHeadersRow();

                List<int> IndexesToRemove = new List<int>();
                
                for (int i = 0; i < Sequence.Count; ++i)
                {
                    AudioSequenceEntry Entry = Sequence[i];
                    
                    ImGui.PushID(i);
                    
                    ImGui.TableNextRow();
        
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(Entry.GetName());       
                    ImGui.TableSetColumnIndex(1);
                    
                    switch (Entry.Type)
                    {
                        case AudioSequenceEntryType.Audio:
                            ImGui.Text(Entry.Duration.ToString("n1")  + "s");        
                            break;
                        case AudioSequenceEntryType.Silence:
                            
                            ImGui.SetNextItemWidth(100);
                            ImGui.InputFloat("", ref Entry.Duration, 0.01f, 1.0f, "%.3f"); 
                            
                            break;
                        default:
                            break;
                    }
                    
                   
                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Remove"))
                    {
                        IndexesToRemove.Add(i);
                    }
                    
                    ImGui.PopID();
                }
             
                ImGui.EndTable();

                for (int i = IndexesToRemove.Count - 1; i >= 0; --i)
                {
                    Sequence.RemoveAt(i);
                }
            }


            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild("right pane", new Vector2(0, 0), ImGuiChildFlags.Border);

           
            
            if (ImGui.Button("Play"))
            {
                WaveFormat? Format = null;
                List<ISampleProvider> Providers = new List<ISampleProvider>();
                
                foreach (var Entry in Sequence)
                {
                    if (Entry.Type == AudioSequenceEntryType.Audio)
                    {
                        if (Entry.Audio != null)
                        {
                            Format = Entry.Audio.BuildSampleProvider()?.WaveFormat;
                            break;
                        }
                    }
                }

                if (Format == null)
                {
                    return;
                }
                
                foreach (var Entry in Sequence)
                {
                    if (Entry.Type == AudioSequenceEntryType.Audio)
                    {
                        if (Entry.Audio != null)
                        {
                            ISampleProvider? BuiltProvider = Entry.Audio.BuildSampleProvider();
                            if (BuiltProvider != null)
                            {
                                Providers.Add(BuiltProvider);
                            }
                        }
                    }
                    else if (Entry.Type == AudioSequenceEntryType.Silence)
                    {
                        Providers.Add(new SilenceProvider (Format).ToSampleProvider().Take(TimeSpan.FromSeconds(Entry.Duration)));
                    }
                   
                }
                
                var playlist = new ConcatenatingSampleProvider(Providers.ToArray());
               
                WaveOut = new WaveOut();
                WaveOut.Init(playlist);
                WaveOut.Play();
            }
            
            ImGui.EndChild();
        }
        
        ImGui.EndChild();
        ImGui.Dummy(new Vector2(0, 10));
        DrawEntries();
    }

    protected override void DrawUnframed(float DeltaTime)
    {
        if (bShouldOpenRenamePopup)
        {
            bShouldOpenRenamePopup = false;
            CurrentEditedName = CurrentlyEditingEntryName?.Name ?? string.Empty;
            ImGui.OpenPopup("Rename");
        }
        
        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        
        if (ImGui.BeginPopupModal("Rename", ImGuiWindowFlags.AlwaysAutoResize /*| ImGuiWindowFlags.NoTitleBar*/))
        {
            ImGui.Text("Enter new name");

            ImGui.InputText("", ref CurrentEditedName, 128);

            if (ImGui.Button("Rename"))
            {
                if (CurrentlyEditingEntryName != null) CurrentlyEditingEntryName.Name = CurrentEditedName;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
    }

    void DrawEntries()
    {
        ImGuiWindowFlags WindowsFlags = ImGuiWindowFlags.MenuBar;

        int EntryIndexToRemove = -1;
        
        for (int EntryIndex = 0; EntryIndex < AudioEntries.Count; ++EntryIndex)
        {
            LineAudioEntry Entry = AudioEntries[EntryIndex];
            
            /*ImVec4* colors = ImGui::GetStyle().Colors;
            colors[ImGuiCol_Border]                 = ImVec4(0.27f, 0.59f, 0.39f, 0.50f);
            colors[ImGuiCol_MenuBarBg]              = ImVec4(0.56f, 0.20f, 0.20f, 1.00f);*/

            ImGui.PushStyleColor(ImGuiCol.MenuBarBg, Entry.Color);
            ImGui.PushStyleColor(ImGuiCol.Border, Entry.Color);
            
            if (ImGui.BeginChild($"{Entry.UniqueId}",  new Vector2(ImGui.GetContentRegionAvail().X, 170), ImGuiChildFlags.Border, WindowsFlags))
            {
                ImGuiStylePtr style = ImGui.GetStyle();
                float PreviousAlpha = style.DisabledAlpha;
                style.DisabledAlpha = 1;
                ImGui.BeginDisabled();
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu($"[{Entry.UniqueId}] {Entry.Name}"))
                    {
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }
                
                ImGui.EndDisabled();
                style.DisabledAlpha = PreviousAlpha;

                ImGui.BeginChild("Left", new Vector2(250, 0));

                ImGui.Text($"Audio Duration : {Entry.TotalDuration}s");

                if (ImGui.SmallButton("R##1"))
                {
                    Entry.StartTime = 0;
                }
                ImGui.SameLine();
                ImGui.InputFloat("Start Time", ref Entry.StartTime, 0.01f, 1.0f, "%.3f");
                
                if (ImGui.SmallButton("R##2"))
                {
                    Entry.EndTime = Entry.TotalDuration;
                }
                ImGui.SameLine();
                ImGui.InputFloat("End Time", ref Entry.EndTime, 0.01f, 1.0f, "%.3f");

                Entry.StartTime = Math.Clamp(Entry.StartTime, 0, Entry.EndTime - 0.01f);
                Entry.EndTime = Math.Clamp(Entry.EndTime, Entry.StartTime + 0.01f, Entry.TotalDuration);
                
                if (ImGui.SmallButton("R##3"))
                {
                    Entry.FadeInDuration = 0;
                }
                ImGui.SameLine();
                ImGui.InputFloat("Fade In", ref Entry.FadeInDuration, 0.01f, 1.0f, "%.3f");
                
                if (ImGui.SmallButton("R##4"))
                {
                    Entry.FadeOutDuration = 0;
                }
                ImGui.SameLine();
                ImGui.InputFloat("Fade Out", ref Entry.FadeOutDuration, 0.01f, 1.0f, "%.3f");
                
                if (ImGui.Button("Play"))
                {
                    Entry.Play();
                }
                
                ImGui.SameLine();

                if (ImGui.Button("Rename"))
                {
                    CurrentlyEditingEntryName = Entry;
                    bShouldOpenRenamePopup = true;
                }
                
                ImGui.SameLine();

                if (ImGui.Button("Remove"))
                {
                    EntryIndexToRemove = EntryIndex;
                }
                
                ImGui.EndChild();
                
                ImGui.SameLine();
                
                DrawCurve(Entry);
                
                
            }
            ImGui.EndChild();
            ImGui.PopStyleColor(2);
        }

        if (EntryIndexToRemove != -1)
        {
            AudioEntries.RemoveAt(EntryIndexToRemove);
        }
    }

    private void DrawCurve(LineAudioEntry Entry)
    {
        if (ImGui.BeginChild("Right", new Vector2(0, 0)))
        {
            Vector2 canvas_p0 = ImGui.GetCursorScreenPos();      // ImDrawList API uses screen coordinates!
            Vector2 canvas_sz = ImGui.GetContentRegionAvail();   // Resize canvas to what's available
            Vector2 canvas_p1 = new Vector2(canvas_p0.X + canvas_sz.X, canvas_p0.Y + 90);
                
            ImDrawListPtr draw_list = ImGui.GetWindowDrawList();
            
            float TotalGraphWidth = ImGui.GetContentRegionAvail().X;
                
            float StartTimePercent = Entry.StartTime / Entry.TotalDuration;
            float EndTimePercent = (Entry.TotalDuration - Entry.EndTime) / Entry.TotalDuration;
                
            draw_list.AddRectFilled(canvas_p0 + new Vector2(TotalGraphWidth * StartTimePercent,0), 
                canvas_p1 - new Vector2(TotalGraphWidth * EndTimePercent,0), ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.8f, 1f,0.4f)));

            if (Entry.FadeInDuration > 0)
            {
                Vector2 FadeInP1 = canvas_p0 + new Vector2(TotalGraphWidth * StartTimePercent, 0);
                Vector2 FadeInP2 = FadeInP1 + new Vector2(TotalGraphWidth * (Entry.FadeInDuration / Entry.TotalDuration), 90);
                    
                draw_list.AddRectFilled(FadeInP1, FadeInP2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 1, 0.2f, 0.8f)));
            }
                
            if (Entry.FadeOutDuration > 0)
            {
                Vector2 FadeOutP1 = canvas_p1 - new Vector2(TotalGraphWidth * EndTimePercent, 0);
                Vector2 FadeOutP2 = FadeOutP1 - new Vector2(TotalGraphWidth * (Entry.FadeOutDuration / Entry.TotalDuration), 90);
                    
                draw_list.AddRectFilled(FadeOutP1, FadeOutP2, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.2f, 0.2f, 0.8f)));
            }
            
            ImGui.PlotHistogram("", ref Entry.Samples.ToArray()[0], 
                Entry.Samples.Count, 0, null, -1, 1, new Vector2(ImGui.GetContentRegionAvail().X, 90));
            
            ImGui.Text(Entry.Line.TextLine);
        }

        ImGui.EndChild();
    }

    public void AddVoiceLine(VoiceLine Line)
    {
        LineAudioEntry NewEntry = new LineAudioEntry(CurrentUniqueId++, Line);
        
        AudioEntries.Add(NewEntry);
        SelectedAudio ??= NewEntry;
    }
}