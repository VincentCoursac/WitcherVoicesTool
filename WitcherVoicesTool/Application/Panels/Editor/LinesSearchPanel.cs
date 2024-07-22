using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Models;

namespace WitcherVoicesTool.Application.Panels;

public class LinesSearchPanel : ContentPanel
{
    private bool bSearchInProgress = false;
    private float SearchProgress;
    private string CurrentSearchText = "";
    private string FinalInputText = "";
    private List<VoiceLine> CurrentVoiceLines = new List<VoiceLine>();
    private List<string> ImportantWords = new List<string>();
    private bool bPickingEditor = false;
    
    public LinesSearchPanel()
    {
        PanelName = "Search Lines";
    }

    protected override void Initialize()
    {
    }

    protected override void DrawContent(float DeltaTime)
    {
        ImGui.BeginDisabled(bSearchInProgress);
        ImGui.SeparatorText("Search for voice lines and add audio to your dialogue");
        
        ImGuiInputTextFlags Flags = ImGuiInputTextFlags.None;
        
        ImGui.InputTextMultiline("##source", ref CurrentSearchText, 256, new Vector2(0, ImGui.GetTextLineHeight() * 3), Flags);

        if (ImGui.Button("Search...") && !bSearchInProgress)
        {
            Search();
        }
        
        ImGui.EndDisabled();

        if (bSearchInProgress)
        {
            ImGui.ProgressBar(SearchProgress, new Vector2(0.0f, 0.0f));
        }

        if (CurrentVoiceLines.Count > 0)
        {
            ImGuiTableFlags TableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg |
                                         ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV
                                         | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Hideable;
            
            Vector2 outer_size = new Vector2(0.0f, ImGui.GetContentRegionAvail().Y);

            int CurrentlyPlayedVoiceId = VoiceAudioService.GetInstance().GetCurrentlyPlayedVoiceLineId();
            
            if (ImGui.BeginTable("Results", 4, TableFlags, outer_size))
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                
                ImGui.TableSetupColumn("#", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Character", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Line", ImGuiTableColumnFlags.NoResize);
                
                ImGui.TableHeadersRow();
                
                for (int ResultIndex = 0; ResultIndex < CurrentVoiceLines.Count; ++ResultIndex)
                {
                    VoiceLine Line = CurrentVoiceLines[ResultIndex];
                    
                    ImGui.PushID(Line.Id);
                    
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text((ResultIndex + 1).ToString());
                    
                    ImGui.TableSetColumnIndex(1);

                    if (Line.Id == CurrentlyPlayedVoiceId)
                    {
                        if (ImGui.SmallButton("Stop"))
                        {
                            //TODO: Stop current audio
                        }
                    }
                    else
                    {
                        if (ImGui.SmallButton("Play"))
                        {
                            VoiceAudioService.GetInstance().PlayVoiceLine(Line);
                        }
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.SmallButton("+ Add"))
                    {
                        if (DialogueWaveEditorPanel.OpenedWaveEditors.Count == 1)
                        {
                            DialogueWaveEditorPanel.OpenedWaveEditors[0].AddVoiceLine(Line);
                        }
                        else
                        {
                            bPickingEditor = true;
                        }
                    }
                   
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{Line.Character}");
                    ImGui.TableSetColumnIndex(3);
                    HighlightText(Line.TextLine);
                    
                    ImGui.PopID();
                }
                
                ImGui.EndTable();
            }
        }
    }

    protected override void DrawUnframed(float DeltaTime)
    {
        if (bPickingEditor)
        {
            ImGui.OpenPopup("Pick Editor");
            bPickingEditor = false;
        }
        
        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        
        if (ImGui.BeginPopupModal("Pick Editor", ImGuiWindowFlags.AlwaysAutoResize /*| ImGuiWindowFlags.NoTitleBar*/))
        {
            ImGui.Text("Pick Editor");

            foreach (DialogueWaveEditorPanel Panel in DialogueWaveEditorPanel.OpenedWaveEditors)
            {
                ImGui.PushID(Panel.GetPanelName());
                ImGui.Text(Panel.GetPanelName());
                ImGui.SameLine(0, 10);
                
                if (ImGui.Button("Add"))
                {
                    //Panel.AddVoiceLine(null);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.PopID();
            }

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
    }

    private void HighlightText(string LineText)
    {
        bool bWordSarted = false;
        string CurrentWord = "";
        
        foreach (char c in LineText)
        {
            if (Char.IsLetter(c) || (bWordSarted && c == '\''))
            {
                bWordSarted = true;
                CurrentWord += c;
            }
            else
            {
                if (bWordSarted)
                {
                    if (CurrentWord.Length > 1 && FinalInputText.Contains(CurrentWord, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ImportantWords.Contains(CurrentWord, StringComparer.OrdinalIgnoreCase))
                        {
                            ImGui.TextColored(new Vector4(1,1,0,1), CurrentWord);
                        }
                        else
                        {
                            ImGui.TextColored(new Vector4(0,1,0,1), CurrentWord);
                        }
                        
                    }
                    else
                    {
                        ImGui.Text(CurrentWord);
                    }
                   
                    ImGui.SameLine(0, 0);
                    
                    CurrentWord = "";
                    bWordSarted = false;
                    
                }
                
                ImGui.Text(c.ToString());
                ImGui.SameLine(0, 0);
            }
        }
        
        ImGui.NewLine();
    }

    async Task Search()
    {
        bSearchInProgress = true;
        SearchProgress = 0f;

        FinalInputText = CurrentSearchText;
        ExtractImportantWords(ref FinalInputText);
        
        CurrentVoiceLines = await VoiceLineService.GetInstance().SearchLines(FinalInputText, ImportantWords, Progress =>
        {
            SearchProgress = Progress;
        });
        
        bSearchInProgress = false;
    }
    
    private void ExtractImportantWords(ref string InputText)
    {
        ImportantWords.Clear();
        
        var Regex = new Regex(@"\*(.*?)\*");
        var Matches = Regex.Matches(InputText);

        foreach (Match Match in Matches)
        {
            ImportantWords.Add(Match.Groups[1].Value);
        }
        
        InputText = Regex.Replace(InputText, "$1");
    }
}