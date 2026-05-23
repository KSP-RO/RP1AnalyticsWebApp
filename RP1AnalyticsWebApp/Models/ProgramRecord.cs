using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ProgramRecord
    {
        public string ProgramName { get; set; }
        public string ProgramDisplayName { get; set; }
        public DateTime? Date { get; set; }
        public string CareerId { get; set; }
        public string CareerName { get; set; }
        public string UserLogin { get; set; }
        public string UserPreferredName { get; set; }
        public int? Rank { get; set; }
        public int? CohortSize { get; set; }
        public string VersionTag { get; set; }
        public string DifficultyLevel { get; set; }
        public string CareerPlaystyle { get; set; }
    }
}
