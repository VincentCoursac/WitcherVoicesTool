using ImGuiNET;

namespace WitcherVoicesTool.Application;

public class PopupInvokator
{
    private readonly string PopupName;
    private bool bPopupRequested;
    
    public PopupInvokator(string InPopupName)
    {
        PopupName = InPopupName;
    }

    public void RequestPopup()
    {
        bPopupRequested = true;
    }

    public void TryOpenIfNeeded()
    {
        if (bPopupRequested)
        {
            bPopupRequested = false;
            ImGui.OpenPopup(PopupName);
        }
    }

    public string GetPopupName()
    {
        return PopupName;
    }
}