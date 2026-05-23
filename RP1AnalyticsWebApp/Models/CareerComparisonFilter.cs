using System;
using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerComparisonFilter
    {
        public List<string> Players { get; set; }
        public List<string> Races { get; set; }
        public ComparisonEndDateMode CareerDateMode { get; set; } = ComparisonEndDateMode.All;
        public DateTime? CareerDateStart { get; set; }
        public DateTime? CareerDateEnd { get; set; }
        public ComparisonEndDateMode LastUpdateMode { get; set; } = ComparisonEndDateMode.All;
        public DateTime? LastUpdateStart { get; set; }
        public DateTime? LastUpdateEnd { get; set; }
        public List<string> Rp1Versions { get; set; }
        public List<string> Difficulties { get; set; }
        public List<string> Playstyles { get; set; }
        public string RecordEligibility { get; set; } = "All";
    }
}
