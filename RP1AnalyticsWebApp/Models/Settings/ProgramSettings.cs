using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RP1AnalyticsWebApp.Models
{
    public class ProgramSettings : IProgramSettings
    {
        public Dictionary<string, string> ProgramNameDict { get; private set; }

        public ProgramSettings()
        {
            const string _fileName = @"programs.json";
            string jsonString = File.ReadAllText(_fileName);
            var arr = JsonSerializer.Deserialize<ProgramDefinitionItem[]>(jsonString);
            ProgramNameDict = arr.ToDictionary(e => e.Name, e => e.Title);
        }
    }

    public interface IProgramSettings
    {
        public Dictionary<string, string> ProgramNameDict { get; }
    }
}
