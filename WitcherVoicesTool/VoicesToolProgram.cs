using NLog;
using WitcherVoicesTool.Application;

static class VoicesToolProgram
{
    public  static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    static void Main()
    {
        Logger.Info("Tool started!");
        new ApplicationWindow().Start();
    }
}
