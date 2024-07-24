using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Models;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Panels;


public class DialogueWaveEditorPanel : ContentPanel
{
    public static readonly List<DialogueWaveEditorPanel> OpenedWaveEditors = new List<DialogueWaveEditorPanel>();
    
    private LineAudioEntry? CurrentlyEditingEntryName = null;
    private bool bShouldOpenRenamePopup = false;
    private string CurrentEditedName = "";

    private LineAudioEntry? SelectedAudio = null;

    private ISavable ParentSavable;
    private LineBase TargetLine;

    private PopupInvokator PickTemplatePopup = new PopupInvokator("Select Popup");
    
    public DialogueWaveEditorPanel(Scene InScene, LineBase InTargetLine)
    {
        //ParentScene = InScene;
        ParentSavable = InScene;
        TargetLine = InTargetLine;
       // PanelName = $"{ParentScene.Header.Name} - {TargetLine.Character}";
    }
    
    public DialogueWaveEditorPanel(LineBase InTargetLine)
    {
        ParentSavable = ApplicationSettings.Get().TemplateSettings;
        TargetLine = InTargetLine;
        PanelName = $"Template - {TargetLine.Character}";
    }

    protected override void Initialize()
    {
        OpenedWaveEditors.Add(this);
    }

    protected override void Shutdown()
    {
        OpenedWaveEditors.Remove(this);
    }

    protected override void DrawContent(float DeltaTime)
    {
        if (ImGui.BeginChild("Final Wave",  new Vector2(ImGui.GetContentRegionAvail().X, 170), ImGuiChildFlags.Border, ImGuiWindowFlags.None))
        {
            ImGui.BeginChild("left pane", new Vector2(ImGui.GetContentRegionAvail().X * 0.4f, 0), ImGuiChildFlags.AlwaysUseWindowPadding);

            if (TargetLine.AudioEntries.Count > 0)
            {
                if (SelectedAudio != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, SelectedAudio.Color);
                }
                if (ImGui.BeginCombo("Pick Audio", SelectedAudio?.FormattedName()))
                {
                    foreach (var Entry in TargetLine.AudioEntries)
                    {
                        bool bIsSelected = Entry == SelectedAudio;
                        
                        ImGui.PushStyleColor(ImGuiCol.Text, Entry.Color);

                        if (ImGui.Selectable(Entry.FormattedName(), bIsSelected))
                        {
                            SelectedAudio = Entry;
                        }

                        ImGui.PopStyleColor();

                        if (bIsSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }
                
                if (SelectedAudio != null)
                {
                    ImGui.PopStyleColor();
                }
            }
            else
            {
                ImGui.TextColored(new Vector4(1,0,0,1), "Add Audio First");
            }

            ImGui.BeginDisabled(TargetLine.AudioEntries.Count == 0);
            if (ImGui.Button("Add audio") && SelectedAudio != null)
            {
                TargetLine.Sequence.Add(AudioSequenceEntry.FromAudio(SelectedAudio));
                ParentSavable.Save();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Add silence"))
            {
                TargetLine.Sequence.Add(AudioSequenceEntry.FromSilence(1));
                ParentSavable.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Add template"))
            {
                PickTemplatePopup.RequestPopup();
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
                List<(int, int)> IndexesToSwap = new List<(int, int)>();
                
                for (int i = 0; i < TargetLine.Sequence.Count; ++i)
                {
                    AudioSequenceEntry Entry = TargetLine.Sequence[i];
                    
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
                        case AudioSequenceEntryType.Template:
                            
                            ImGui.SetNextItemWidth(100);
                            ImGui.Text("WIP");
                            
                            break;
                        default:
                            break;
                    }
                    
                   
                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Remove"))
                    {
                        IndexesToRemove.Add(i);
                    }
                    
                    ImGui.SameLine();
                    
                    ImGui.BeginDisabled(i == 0);
                    if (ImGui.Button("/\\"))
                    {
                        IndexesToSwap.Add((i, i - 1));
                    }
                    ImGui.EndDisabled();
                    
                    ImGui.SameLine();
                    
                    ImGui.BeginDisabled(i == TargetLine.Sequence.Count - 1);
                    if (ImGui.Button("\\/"))
                    {
                        IndexesToSwap.Add((i, i + 1));
                    }
                    ImGui.EndDisabled();
                    
                    ImGui.PopID();
                }
             
                ImGui.EndTable();

                if (IndexesToRemove.Count > 0)
                {
                    for (int i = IndexesToRemove.Count - 1; i >= 0; --i)
                    {
                        TargetLine.Sequence.RemoveAt(IndexesToRemove[i]);
                    }

                    ParentSavable.Save();
                }
                
                if (IndexesToSwap.Count > 0)
                {
                    foreach (var Swap in IndexesToSwap)
                    {
                        (TargetLine.Sequence[Swap.Item1], TargetLine.Sequence[Swap.Item2]) = (TargetLine.Sequence[Swap.Item2], TargetLine.Sequence[Swap.Item1]);
                    }

                    ParentSavable.Save();
                }             
            }

            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.BeginChild("right pane", new Vector2(0, 0), ImGuiChildFlags.Border);

           
            ImGui.SeparatorText($"Character : {TargetLine.Character}");
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputTextWithHint("##LineText", "Hello my dear Geralt !", ref  TargetLine.LineText, 256);
            ImGui.PopItemWidth();
            ImGui.Separator();
            
            ImGui.Dummy(new Vector2(0,10));
            
            if (ImGui.Button("Play"))
            {
                TargetLine.Play();
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
                if (CurrentlyEditingEntryName != null)
                {
                    CurrentlyEditingEntryName.Name = CurrentEditedName;
                    ImGui.CloseCurrentPopup();
                    ParentSavable.Save();
                }
                    
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
        
        PickTemplatePopup.TryOpenIfNeeded();

        if (Widgets.ModalPopup(PickTemplatePopup.GetPopupName()))
        {
            List<Template> Templates = ApplicationSettings.Get().TemplateSettings.GetTemplates();

            ImGuiTableFlags TableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg |
                                         ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV
                                         | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Hideable;
            ImGui.Dummy(new Vector2(600,0));
            Vector2 outer_size = new Vector2(0.0f, 250);

            if (ImGui.BeginTable("Audios", 3, TableFlags, outer_size))
            {
                ImGui.TableSetupColumn("Character", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Text", ImGuiTableColumnFlags.NoResize, 100);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.NoResize);
                
                ImGui.TableHeadersRow();

              
                foreach (var Template in Templates)
                {
                    ImGui.PushID(Template.Id);
                    
                    ImGui.TableNextRow();
        
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(Template.Character);       
                    ImGui.TableSetColumnIndex(1);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(Template.LineText);
                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Play"))
                    {
                        Template.Play();
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Add"))
                    {
                        TargetLine.Sequence.Add(AudioSequenceEntry.FromTemplate(Template));
                        ParentSavable.Save();
                        ImGui.CloseCurrentPopup();
                    }
                    
                    ImGui.PopID();
                }
             
                ImGui.EndTable();
            }

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
        
        for (int EntryIndex = 0; EntryIndex < TargetLine.AudioEntries.Count; ++EntryIndex)
        {
            LineAudioEntry Entry = TargetLine.AudioEntries[EntryIndex];
            
            /*ImVec4* colors = ImGui::GetStyle().Colors;
            colors[ImGuiCol_Border]                 = ImVec4(0.27f, 0.59f, 0.39f, 0.50f);
            colors[ImGuiCol_MenuBarBg]              = ImVec4(0.56f, 0.20f, 0.20f, 1.00f);*/

            ImGui.PushStyleColor(ImGuiCol.MenuBarBg, Entry.Color);
            ImGui.PushStyleColor(ImGuiCol.Border, Entry.Color);
            
            if (ImGui.BeginChild($"{Entry.Id}",  new Vector2(ImGui.GetContentRegionAvail().X, 170), ImGuiChildFlags.Border, WindowsFlags))
            {
                ImGuiStylePtr style = ImGui.GetStyle();
                float PreviousAlpha = style.DisabledAlpha;
                style.DisabledAlpha = 1;
                ImGui.BeginDisabled();
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu(Entry.FormattedName()))
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

                Entry.StartTime = MathUtils.Clamp(Entry.StartTime, 0, Entry.EndTime - 0.01f);
                Entry.EndTime = MathUtils.Clamp(Entry.EndTime, Entry.StartTime + 0.01f, Entry.TotalDuration);
                
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
            TargetLine.AudioEntries.RemoveAt(EntryIndexToRemove);
            ParentSavable.Save();
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
        LineAudioEntry NewEntry = new LineAudioEntry(Line);
        
        TargetLine.AudioEntries.Add(NewEntry);
        SelectedAudio ??= NewEntry;

        ParentSavable.Save();
    }
}