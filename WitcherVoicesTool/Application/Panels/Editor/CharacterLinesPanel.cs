using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Models;

namespace WitcherVoicesTool.Application.Panels;

public class CharacterLinesPanel : ContentPanel
{
    private readonly Character Character;

    public CharacterLinesPanel(Character InCharacter)
    {
        Character = InCharacter;
        PanelName = "Lines : " + Character.Name;
    }
    
    protected override void DrawContent(float DeltaTime)
    {
        ImGuiTableFlags TableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg |
                                         ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV
                                         | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Hideable;
            
            Vector2 outer_size = new Vector2(0.0f, ImGui.GetContentRegionAvail().Y);

            int CurrentlyPlayedVoiceId = VoiceAudioService.GetInstance().GetCurrentlyPlayedVoiceLineId();

            if (ImGui.BeginTable("Lines", 4, TableFlags, outer_size))
            {
                ImGui.TableSetupScrollFreeze(0, 1);

                ImGui.TableSetupColumn("#", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Character", ImGuiTableColumnFlags.NoResize);
                ImGui.TableSetupColumn("Line", ImGuiTableColumnFlags.NoResize);

                ImGui.TableHeadersRow();

                for (int ResultIndex = 0; ResultIndex < Character.LineIds.Count; ++ResultIndex)
                {
                    VoiceLine Line = VoiceLineService.GetInstance().GetLineById(Character.LineIds[ResultIndex]);

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
                            //bPickingEditor = true;
                            //TODO: manage this
                        }
                    }

                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{Line.Character}");
                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text(Line.TextLine);

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
    }
}