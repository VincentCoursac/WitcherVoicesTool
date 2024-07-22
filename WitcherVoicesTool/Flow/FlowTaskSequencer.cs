namespace WitcherVoicesTool.Flow;

public class FlowTaskSequencer
{
    private readonly List<FlowTask> Tasks = new List<FlowTask>();
    private readonly bool bRunInParallel;
    

    private bool bInProgress;
    private bool bCompleted;
    private bool bAnyTaskFailed;
    private bool bWasCanceled;
    
    public event EventHandler OnCompleted = null!;
    
    public FlowTaskSequencer(bool bInRunInParallel = false)
    {
        bRunInParallel = bInRunInParallel;
    }
    
    public FlowTaskSequencer AddTask(FlowTask Task)
    {
        Tasks.Add(Task);
        return this;
    }

    public async void Start()
    {
        bInProgress = true;
        
        if (bRunInParallel)
        {
            var TaskList = new List<Task>();
            foreach (var Task in Tasks)
            {
                TaskList.Add(ExecuteTask(Task));
            }
            
            await Task.WhenAll(TaskList);

            if (!bAnyTaskFailed)
            {
                Cancel();
            }
            else
            {
                Complete();
            }
        }
        else
        {
            foreach (FlowTask Task in Tasks)
            {
                if (!await ExecuteTask(Task))
                {
                    Cancel();
                    return;
                }
            }

            Complete();
        }
    }

    private async Task<bool> ExecuteTask(FlowTask Task)
    {
        bool bSucceed = await Task.Start();
        
        if (!bSucceed)
        {
            bAnyTaskFailed = true;
        }

        return bSucceed;
    }

    public float GetProgressRatio()
    {
        float TotalProgress = 0;
        float TotalCost = 0;
        
        foreach (FlowTask Task in Tasks)
        {
            TotalProgress += Task.GetEstimatedProgress();
            TotalCost += Task.GetEstimatedCost();
        }
        return TotalProgress / TotalCost;
    }

    public bool IsCompleted()
    {
        return bCompleted;
    }
    
    public bool WasCanceled()
    {
        return bWasCanceled;
    }

    public bool IsInProgress()
    {
        return bInProgress;
    }
    
    public bool DidAnyTaskFailed()
    {
        return bAnyTaskFailed;
    }

    private void Complete()
    {
        bCompleted = true;
        bInProgress = false;
        OnCompleted?.Invoke(this, EventArgs.Empty);
        
    }

    private void Cancel()
    {
        bInProgress = false;
    }
}