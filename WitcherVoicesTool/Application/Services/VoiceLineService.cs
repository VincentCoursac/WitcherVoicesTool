using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FuzzySharp;
using WitcherVoicesTool.Application.Panels;
using WitcherVoicesTool.Models;
using WitcherVoicesTool.Utils;

namespace WitcherVoicesTool.Application.Services;

public class VoiceLineService : Singleton<VoiceLineService>
{
    private readonly List<VoiceLine> VoiceLines = new List<VoiceLine>();
    private readonly Dictionary<string, Character> Characters = new Dictionary<string, Character>();
    
    public void RegisterVoiceLine(VoiceLine Line)
    {
        VoiceLines.Add(Line);
        Characters.TryAdd(Line.Character, new Character
        {
            Name = Line.Character,
            Count = 0
        });

        Character Character = Characters[Line.Character];
        Character.Count++;
        Character.FormattedCount = Character.Count.ToString("#");
        
        Character.LineIds.Add(Line.Id);
    }

    public List<Character> GetAllCharacters()
    {
        return Characters.Values.ToList();
    }

    public VoiceLine GetLineById(int LineId)
    {
        if (LineId > 0 && VoiceLines.Count > LineId)
        {
            return VoiceLines[LineId];
        }

        return new VoiceLine();
    }

    public async Task<List<VoiceLine>> SearchLines(string InputText, List<string> ImportantWords, Action<float> ProgressCallback)
    {
        return await Task.Run(() =>
        {
            var ScoredLines = new ConcurrentBag<(VoiceLine Line, int Score)>();
            int TotalLines = VoiceLines.Count;
            int ProcessedLines = 0;
            object progressLock = new object();
            
            var ParallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            
            string CleanedInput = Regex.Replace(InputText, @"[^a-zA-Z0-9\s]", "");
            string[] Words = CleanedInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            float ImportantWordsRatio = (float) ImportantWords.Count / Words.Length;
            float ImportantScoreAdding = 100 * ImportantWordsRatio;

            Parallel.ForEach(VoiceLines, ParallelOptions, Line =>
            {
                if (CharactersPanel.GetInstance().CharactersSelectedCount > 0 && !Characters[Line.Character].bSelected)
                {
                    return;
                }
                
                int Score = Fuzz.Ratio(InputText, Line.TextLine);
                
                int ImportantWordsMatchingCount = ImportantWords.Count(word => Line.TextLine.Contains(word, StringComparison.OrdinalIgnoreCase));
                float MatchingImportantWordsRatio = ((float) ImportantWordsMatchingCount) / ImportantWords.Count;
                Score += (int)(MatchingImportantWordsRatio * ImportantScoreAdding);
                
                ScoredLines.Add((Line, Score));

                lock (progressLock)
                {
                    ++ProcessedLines;
                    float progress = (float)ProcessedLines / TotalLines;
                    ProgressCallback?.Invoke(progress);
                }
            });
            
            return ScoredLines
                .OrderByDescending(Result => Result.Score)
                .Take(100)
                .Select(result => result.Line)
                .ToList();
        });
    }
}