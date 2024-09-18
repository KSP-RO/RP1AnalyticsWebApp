using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RP1AnalyticsWebApp.Models
{
    public class LeaderSettings : ILeaderSettings
    {
        public Dictionary<string, LeaderDefinitionItem> LeaderDict { get; private set; }

        public LeaderSettings()
        {
            const string _fileName = @"Configs/leaders.json";
            string jsonString = File.ReadAllText(_fileName);
            var arr = JsonSerializer.Deserialize<LeaderDefinitionItem[]>(jsonString);
            LeaderDict = arr.ToDictionary(e => e.Name);
        }
    }

    public interface ILeaderSettings
    {
        public Dictionary<string, LeaderDefinitionItem> LeaderDict { get; }
    }
}
