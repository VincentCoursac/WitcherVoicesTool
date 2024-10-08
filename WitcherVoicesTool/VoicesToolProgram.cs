using NLog;
using WitcherVoicesTool.Application;
using WitcherVoicesTool.Application.Panels;
using WitcherVoicesTool.Application.Panels.Loading;
using WitcherVoicesTool.Application.Settings;

static class VoicesToolProgram
{
    public  static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    [STAThread]
    static void Main()
    {
        Logger.Info("Tool started!");
        
        ApplicationSettings.Load();

        ContentPanelManager.GetInstance()?.AddContentPanel(new LoadApplicationPanel());
        new ApplicationWindow(1280, 720, "WitcherVoicesTool by Roi_Jean - v" + GetVersion()).Start();
    }

    static string GetVersion()
    {
        return "0.1";
    }
}
