namespace WitcherVoicesTool.Utils;

public static class Smoothing
{
    // Similar to Unreal Engine FMath::FInterpTo
    public static float InterpTo( float Current, float Target, float DeltaTime, float Speed = 1f) 
    {
        float Remaining = Target - Current;

        if (Math.Abs(Remaining) <= 0.001f)
        {
            return Target;
        }
        
        return Current + Remaining *  Math.Clamp(DeltaTime * Speed, 0, 1f);
    }
}