namespace WitcherVoicesTool.Utils;

public static class MathUtils
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

    public static float RandomFloatInRange(float Min, float Max)
    {
        Random Random = new Random();
        double Val = Random.NextDouble() * (Max - Min) + Min;
        return (float)Val;
    }
    
    //Safe Clamp method, templated
    public static T Clamp<T>(T CurrentValue, T MinValue, T MaxValue) where T : IComparable<T>
    {
        if (MinValue.CompareTo(MaxValue) > 0)
        {
            return CurrentValue;
        }

        if (CurrentValue.CompareTo(MinValue) < 0)
        {
            return MinValue;
        }
        else if (CurrentValue.CompareTo(MaxValue) > 0)
        {
            return MaxValue;
        }

        return CurrentValue;
    }
}