namespace WitcherVoicesTool.Utils;

public class Singleton<T> where T : new()
{
    protected Singleton() { }
        
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