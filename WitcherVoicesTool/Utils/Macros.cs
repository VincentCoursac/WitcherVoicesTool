using NLog;

namespace WitcherVoicesTool.Utils;

using static VoicesToolProgram;

public class Macros
{
    public static bool ENSURE(bool bExpression, string OptionalText = "")
    {
        if (!bExpression)
        {
            System.Diagnostics.StackTrace Trace = new System.Diagnostics.StackTrace();
            Logger.Error($"Expression not true. {OptionalText} \n {Trace.ToString()}");
        }
        
        return bExpression;
    }
}