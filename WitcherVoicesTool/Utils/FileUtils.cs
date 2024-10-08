using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WitcherVoicesTool.Utils;

public static class FileUtils
{
    public static string GetSafeFilename(string OriginalName)
    {
       Regex Regex = new Regex("[^a-zA-Z0-9_]");
       string Transformed = OriginalName.Replace(" ", "_");
       Transformed = Regex.Replace(Transformed, "");
       Transformed = Regex.Replace(Transformed, "_+", "_");
       return Transformed;
    }

    public static void OpenURL(string URL)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = URL,
            UseShellExecute = true 
        });
    }
}