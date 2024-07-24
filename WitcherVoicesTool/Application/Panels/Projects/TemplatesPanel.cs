using System.Numerics;
using ImGuiNET;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Application.Settings;

namespace WitcherVoicesTool.Application.Panels;

public class TemplatesPanel : ContentPanel
{
    private TemplateSettings TemplateSettings;
    
    private PopupInvokator CharacterEditorPopup = new PopupInvokator("Pick Character");
    private List<string> SceneCharacters = new List<string>();
    private int PopupCurrentlySelectedCharacterIndex = -1;
    private Template? EditedTemplate;
    
    public TemplatesPanel()
    {
        PanelName = "Templates";
        TemplateSettings = ApplicationSettings.Get().TemplateSettings;
    }


    protected override void DrawContent(float DeltaTime)
    {
        if (ImGui.Button("Add Template"))
        {
            TemplateSettings.AddTemplate(new Template());
        }

        List<Template> Templates = TemplateSettings.GetTemplates();

        ImGui.Dummy(new Vector2(0, 10));


        for (int i = 0; i < Templates.Count; ++i)
        {
            ImGui.PushID(i);
            //SceneLine Line = Scene.Lines[i];
            Template Template = Templates[i];

            if (ImGui.BeginChild($"{Template.Id}", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.Border, ImGuiWindowFlags.None))
            {
                bool bHasCharacter = Template.Character.Length > 0;
                string DisplayedCharacter = bHasCharacter ? Template.Character : "Click to select Character";
                
                if (!bHasCharacter)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1,1,1,0.5f));
                }
                ImGui.SeparatorText($"Template {i + 1}: {DisplayedCharacter}");
                if (!bHasCharacter)
                {
                    ImGui.PopStyleColor();
                }
                
                if (ImGui.IsItemClicked())
                {
                    EditedTemplate = Template;
                    CharacterEditorPopup.RequestPopup();
                }
                
                ImGui.Dummy(new Vector2(0, 5));
                
                
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Name : ");
                ImGui.SameLine();
                
                if (Template.bEditingName)
                {
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 100);
                    ImGui.InputTextWithHint("##Name", "Geralt saying 'So Long'", ref  Template.Name, 256);
                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                    if (ImGui.Button("Validate##Name"))
                    {
                        Template.bEditingName = false;
                        ApplicationSettings.Save();
                    }
                }
                else
                {
                    ImGui.AlignTextToFramePadding();
                    if (Template.Name.Length > 0)
                    {
                        ImGui.Text( Template.Name );
                    }
                    else
                    {
                        ImGui.TextDisabled( "Click to edit!");
                    }
                }
                
                if (ImGui.IsItemClicked())
                {
                    Template.bEditingName = true;
                }
                
                
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Text : ");
                ImGui.SameLine();
                
                if (Template.bEditingText)
                {
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 100);
                    ImGui.InputTextWithHint("##LineText", "Line", ref  Template.LineText, 256);
                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                    if (ImGui.Button("Validate"))
                    {
                        Template.bEditingText = false;
                        ApplicationSettings.Save();
                    }
                }
                else
                {
                    ImGui.AlignTextToFramePadding();
                    if (Template.LineText.Length > 0)
                    {
                        ImGui.Text( Template.LineText);
                    }
                    else
                    {
                        ImGui.TextDisabled( "Click to edit!");
                    }
                }
                
                if (ImGui.IsItemClicked())
                {
                    Template.bEditingText = true;
                }

                if (ImGui.Button("Edit Audio"))
                {
                   ContentPanelManager.GetInstance().AddContentPanel(new DialogueWaveEditorPanel( Template ));
                }
                ImGui.SameLine();
                if (ImGui.Button("Play Audio"))
                {
                    Template.Play();
                }
            }
            
            ImGui.EndChild();

        }
    }
    
     protected override void DrawUnframed(float DeltaTime)
    {
        if (EditedTemplate == null)
        {
            return;
        }
        
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
                            EditedTemplate.Character = SceneCharacters[n];
                        }
                            
                    
                        if (bIsSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                
                    ImGui.EndListBox();
                }
            }
            
            ImGui.Text("Enter manually :");
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputTextWithHint("##Character", "Geralt", ref  EditedTemplate.Character, 256);
            ImGui.PopItemWidth();
            
            ImGui.Dummy(new Vector2(0,8));

            if (ImGui.Button("Validate"))
            {
                ApplicationSettings.Save();
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
        foreach (Template Template in TemplateSettings.GetTemplates())
        {
            if (Template.Character.Length > 0 && SceneCharacters.Contains(Template.Character) == false)
            {
                SceneCharacters.Add(Template.Character);
            }
        }
    }
}