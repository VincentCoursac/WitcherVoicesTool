namespace WitcherVoicesTool.Application;

public interface ISavable
{
    public virtual bool Save()
    {
        return false; 
    }
}