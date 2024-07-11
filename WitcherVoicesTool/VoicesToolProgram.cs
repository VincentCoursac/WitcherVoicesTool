using NLog;
using WitcherVoicesTool.Application;
using WitcherVoicesTool.Application.Panels;

static class VoicesToolProgram
{
    public  static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    static void Main()
    {
        Logger.Info("Tool started!");
        
        ContentPanelManager.GetInstance()?.AddContentPanel(new DemoPanel());
        new ApplicationWindow().Start();
    }
}
