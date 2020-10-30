using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RP1AnalyticsWebApp.Models
{
    public class ContractSettings : IContractSettings
    {
        public Dictionary<string, string> ContractNameDict { get; private set; }
        public HashSet<string> MilestoneContractNames { get; private set; }

        public ContractSettings()
        {
            const string _fileName = @"contractData.json";
            string jsonString = File.ReadAllText(_fileName);
            var arr = JsonSerializer.Deserialize<ContractDefinitionItem[]>(jsonString);
            ContractNameDict = arr.ToDictionary(e => e.Name, e => e.Title);

            const string _milestoneFileName = @"milestoneContracts.json";
            jsonString = File.ReadAllText(_milestoneFileName);
            var arr2 = JsonSerializer.Deserialize<string[]>(jsonString);
            MilestoneContractNames = new HashSet<string>(arr2);
        }
    }

    public interface IContractSettings
    {
        public Dictionary<string, string> ContractNameDict { get; }
        public HashSet<string> MilestoneContractNames { get; }
    }
}
