using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public class RecordsSnapshot
    {
        public RecordFilterSummary Filters { get; set; }
        public List<ContractRecord> ContractRecords { get; set; } = new();
        public List<ProgramRecord> ProgramRecords { get; set; } = new();
    }

    public class RecordFilterSummary
    {
        public string Rp1Version { get; set; }
        public string Difficulty { get; set; }
        public string Playstyle { get; set; }
        public int CareerCount { get; set; }
        public bool EligibleOnly { get; set; }
    }
}
