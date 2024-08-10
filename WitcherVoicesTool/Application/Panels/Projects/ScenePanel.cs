using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;

namespace WitcherVoicesTool.Application.Panels;

public class ScenePanel : ContentPanel
{
    private Scene Scene;
    private PopupInvokator CharacterEditorPopup = new PopupInvokator("Pick Character");
    private int EditedLineIndex = -1;

    private List<string> SceneCharacters = new List<string>();
    private int PopupCurrentlySelectedCharacterIndex = -1;
    
    public ScenePanel(Scene InScene)
    {
        Scene = InScene;
        PanelName = $"{Scene.Header.Name} - {Scene.ParentProject.Name}###{Scene.Header.Id}";
    }

    protected override void Initialize()
    {
        RefreshCharacters();
    }

    protected override void DrawContent(float DeltaTime)
    {
        if (ImGui.Button("Add Line"))
        {
            SceneLine Line = new SceneLine();
            Line.Id = Guid.NewGuid().ToString();
            
            Scene.Lines.Add(Line);
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Save"))
        {
            Scene.Save();
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Play Scene"))
        {
            Scene.Play();
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Export All"))
        {
            foreach (SceneLine Line in Scene.Lines)
            {
                Scene.ExportLine(Line);
            }
        }
        
        
        ImGui.Dummy(new Vector2(0, 10));
        
        
        for (int i = 0; i < Scene.Lines.Count; ++i)
        {
            ImGui.PushID(i);
            SceneLine Line = Scene.Lines[i];
            
            if (ImGui.BeginChild($"{Line.Id}",  new Vector2(ImGui.GetContentRegionAvail().X, 0), ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.Border, ImGuiWindowFlags.None))
            {
                bool bHasCharacter = Line.Character.Length > 0;
                string DisplayedCharacter = bHasCharacter ? Line.Character : "Click to select Character";

                if (!bHasCharacter)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1,1,1,0.5f));
                }
                ImGui.SeparatorText($"Line {i + 1}: {DisplayedCharacter}");
                if (!bHasCharacter)
                {
                    ImGui.PopStyleColor();
                }
                
                if (ImGui.IsItemClicked())
                {
                    EditedLineIndex = i;
                    CharacterEditorPopup.RequestPopup();
                }
                
                ImGui.Dummy(new Vector2(0, 5));
                
                if (Line.bEditingText)
                {
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 100);
                    ImGui.InputTextWithHint("##LineText", "Line", ref  Line.LineText, 256);
                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                    if (ImGui.Button("Validate"))
                    {
                        Line.bEditingText = false;
                        Scene.Save();
                    }
                }
                else
                {
                    ImGui.AlignTextToFramePadding();
                    if (Line.LineText.Length > 0)
                    {
                        ImGui.Text( Line.LineText);
                    }
                    else
                    {
                        ImGui.TextDisabled( "Click to edit!");
                    }
                    
                }
                
                if (ImGui.IsItemClicked())
                {
                    Line.bEditingText = true;
                }

                if (ImGui.Button("Edit Audio"))
                {
                    ContentPanelManager.GetInstance().AddContentPanel(new DialogueWaveEditorPanel(Scene, Line));
                }
                ImGui.SameLine();
                if (ImGui.Button("Play Audio"))
                {
                    Line.Play();
                }
                ImGui.SameLine();
        
                if (ImGui.Button("Export"))
                {
                    Scene.ExportLine(Line);
                }
                
            }
            ImGui.EndChild();
            ImGui.PopStyleColor(2);
            ImGui.PopID();
        }
    }

    protected override void DrawUnframed(float DeltaTime)
    {
        CharacterEditorPopup.TryOpenIfNeeded();

        if (Widgets.ModalPopup(CharacterEditorPopup.GetPopupName()))
        {
            if (SceneCharacters.Count > 0)
            {
                ImGui.Text("Pick existing character from scene : ");
                if (ImGui.BeginListBox("###Characters", new Vector2(-1, Math.Min(SceneCharacters.Count * 18, 250))))
                {
                    for (int n = 0; n < SceneCharacters.Count; n++)
                    {
                        bool bIsSelected = (PopupCurrentlySelectedCharacterIndex == n);
                        if (ImGui.Selectable(SceneCharacters[n], bIsSelected))
                        {
                            PopupCurrentlySelectedCharacterIndex = n;
                            Scene.Lines[EditedLineIndex].Character = SceneCharacters[n];
                        }
                            
                    
                        if (bIsSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                
                    ImGui.EndListBox();
                }
            }
            
            ImGui.Text("Enter manually :");
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputTextWithHint("##Character", "Geralt", ref  Scene.Lines[EditedLineIndex].Character, 256);
            ImGui.PopItemWidth();
            
            ImGui.Dummy(new Vector2(0,8));

            if (ImGui.Button("Validate"))
            {
                Scene.Save();
                RefreshCharacters();
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

    void RefreshCharacters()
    {
        SceneCharacters.Clear();
        foreach (SceneLine Line in Scene.Lines)
        {
            if (Line.Character.Length > 0 && SceneCharacters.Contains(Line.Character) == false)
            {
                SceneCharacters.Add(Line.Character);
            }
        }
    }

}