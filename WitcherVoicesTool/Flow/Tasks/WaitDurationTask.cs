namespace WitcherVoicesTool.Flow;

public class WaitDurationTask : FlowTask
{
    private readonly int WaitDuration;
    
    public WaitDurationTask(float DurationInSeconds = 1)
    {
        WaitDuration = (int)(DurationInSeconds * 1000);
    }
    
    protected override bool Execute()
    {
        Thread.Sleep(WaitDuration);
        return true;
    }

    public override float GetEstimatedCost()
    {
        return WaitDuration / 1000f;
    }
}