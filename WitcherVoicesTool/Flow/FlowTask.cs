using static VoicesToolProgram;

namespace WitcherVoicesTool.Flow;

public abstract class FlowTask
{
    private bool bCompleted = false;
    private bool bSucceed = false;
    public async Task<bool> Start()
    {
        Logger.Info($"Starting Task : <{GetType().Name}>");
        bSucceed = await Task.Run(TriggerExecute);
        bCompleted = true;
        
        return bSucceed;
    }

    bool TriggerExecute()
    {
        try
        {
            return Execute();
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return false;
        }
    }

    protected abstract bool Execute();
    
    public float GetEstimatedProgress()
    {
        return bSucceed ? GetEstimatedCost() : GetOngoingProgress();
    }
    
    public virtual float GetEstimatedCost()
    {
        return 1f;
    }

    protected virtual float GetOngoingProgress()
    {
        return 0f;
    }
}