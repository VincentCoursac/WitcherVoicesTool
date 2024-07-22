namespace WitcherVoicesTool.Application.Panels;

public abstract class SingletonPanel<T> : ContentPanel where T : new()
{
    protected SingletonPanel() { }
        
    private static T? _instance;
        
    public static T GetInstance()
    {
        if (_instance == null)
        {
            _instance = new T();
        }
        return _instance;
    }
}