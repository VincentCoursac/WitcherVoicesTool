using System.Numerics;
using ImGuiNET;

namespace WitcherVoicesTool.Application;

public static class Widgets
{
    public static bool ColoredButton(string Label, ThemeColor Color, bool bIsDisabled = false)
    {
        ImGui.BeginDisabled(bIsDisabled);
        ImGui.PushStyleColor(ImGuiCol.Button,  Color.Main);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Color.Active);
        ImGui.PushStyleColor(ImGuiCol.Text, Color.Content);
        
        bool bClicked = ImGui.Button(Label);
        
        ImGui.PopStyleColor(4);
        ImGui.EndDisabled();
        return bClicked;
    }

    public static bool ModalPopup(string PopupName)
    {
        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));

        return ImGui.BeginPopupModal(PopupName,
            ImGuiWindowFlags.AlwaysAutoResize /*| ImGuiWindowFlags.NoTitleBar*/);
    }
}