using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RP1AnalyticsWebApp.Models
{
    public class TechTreeSettings : ITechTreeSettings
    {
        public Dictionary<string, string> NodeTitleDict { get; private set; }

        public TechTreeSettings()
        {
            const string _fileName = @"Configs/techTree.json";
            string jsonString = File.ReadAllText(_fileName);
            var arr = JsonSerializer.Deserialize<TechTreeNode[]>(jsonString);
            NodeTitleDict = arr.ToDictionary(e => e.ID, e => e.Title);
        }
    }

    public interface ITechTreeSettings
    {
        public Dictionary<string, string> NodeTitleDict { get; }
    }

    public class TechTreeNode
    {
        public string ID { get; set; }
        public string Title { get; set; }
    }
}