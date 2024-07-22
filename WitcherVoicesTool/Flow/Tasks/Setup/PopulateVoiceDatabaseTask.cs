using System.Text.RegularExpressions;
using OfficeOpenXml;
using WitcherVoicesTool.Application.Services;
using WitcherVoicesTool.Application.Settings;
using WitcherVoicesTool.Models;

namespace WitcherVoicesTool.Flow;

public class PopulateVoiceDatabaseTask : FlowTask
{
    private float Progress;
    private int TotalRows;
    private int TotalColumns;
    
    protected override bool Execute()
    {
        string? CurrentFilePath = ApplicationSettings.Get()?.VoiceLibrarySettings.VoicesListFileLocation.Get();
        if (!File.Exists(CurrentFilePath))
        {
            VoicesToolProgram.Logger.Error("Voices file path invalid");
            return false;
        }

        Dictionary<string, int> Mapping = new Dictionary<string, int>();
        
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage Package = new ExcelPackage(new FileInfo(CurrentFilePath));
        var VoicesSheet = Package.Workbook.Worksheets.First();
        
        TotalRows = VoicesSheet.Dimension.End.Row;
        TotalColumns = VoicesSheet.Dimension.End.Column;
        Console.WriteLine(VoicesSheet.Dimension.End.Column);
      
        VoicesToolProgram.Logger.Info($"Found {TotalRows} rows, {TotalColumns} columns");

        Progress += 250;

        int CurrentLineId = 0;
        
        for (int i = 1; i < TotalRows; ++i)
        {
            ++Progress;
            
            ExcelRange Cell = VoicesSheet.Cells[i, 1];
            if (Cell == null)
                continue;

            object RawCellValue = Cell.Value;
            if (RawCellValue == null)
                continue;
            
            string RowValue =  RawCellValue.ToString()!;
            string RegexPattern = @"^\s*(\d+)\s+(0x[0-9a-fA-F]+)\s+(.*?):\s+(.*)$";
            Regex Regex = new Regex(RegexPattern);
            Match Match = Regex.Match(RowValue);

            if (!Match.Success)
            {
                continue;
            }
            
            VoiceLine Line = new VoiceLine
            {
                Id = CurrentLineId,
                LineId = Match.Groups[2].Value,
                Character = Match.Groups[3].Value,
                TextLine = Match.Groups[4].Value
            };

            if (Line.Character.Contains("Geralt choice", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            ++CurrentLineId;

            VoiceLineService.GetInstance().RegisterVoiceLine(Line);
        }
        
        return true;
    }

    public override float GetEstimatedCost()
    {
        return 250 + TotalRows;
    }

    protected override float GetOngoingProgress()
    {
        return Progress;
    }
}