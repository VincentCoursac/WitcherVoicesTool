using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Models;

namespace WitcherVoicesTool.Application.Panels;

public class CharactersPanel : SingletonPanel<CharactersPanel>
{
    private List<Character> Characters = new List<Character>();

    public int CharactersSelectedCount;
    
    protected override void Initialize()
    {
        PanelName = "Characters";
        Characters = VoiceLineService.GetInstance().GetAllCharacters().OrderByDescending(Char => Char.Count).ToList();
    }
    
    protected override void DrawContent(float DeltaTime)
    {
        ImGui.Text("Total Characters : " + Characters?.Count);
 
        ImGuiTableFlags Flags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV 
                                | ImGuiTableFlags.Resizable | ImGuiTableFlags.Hideable
                                | ImGuiTableFlags.Sortable;
        Vector2 outer_size = new Vector2(0.0f, ImGui.GetContentRegionAvail().Y);
        
        if (ImGui.BeginTable("characters", 4, Flags, outer_size))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("##1", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 20);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 180);
            ImGui.TableSetupColumn("Lines", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 50);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 100);
            ImGui.TableHeadersRow();
            
            foreach (Character Character in Characters)
            { 
                ImGui.PushID(Character.Name);
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (ImGui.Checkbox("", ref Character.bSelected))
                {
                    if (Character.bSelected)
                    {
                        ++CharactersSelectedCount;
                    }
                    else
                    {
                        --CharactersSelectedCount;
                    }
                }
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(Character.Name);
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(Character.FormattedCount);
                ImGui.TableSetColumnIndex(3);
                if (ImGui.Button("Lines..."))
                {
                    ContentPanelManager.GetInstance().AddContentPanel(new CharacterLinesPanel(Character));
                }
                ImGui.PopID();
            }
            
            ImGui.EndTable();
        }
    }
}