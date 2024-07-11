using NLog;

static class VoicesToolProgram
{
    public  static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    static void Main()
    {
        Logger.Info("Tool started!");
    }
}
