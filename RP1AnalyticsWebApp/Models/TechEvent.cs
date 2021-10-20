using System;

namespace RP1AnalyticsWebApp.Models
{
    public class TechEvent
    {
        public string NodeInternalName { get; set; }
        public string NodeDisplayName { get; set; }
        public DateTime Date { get; set; }
        public double? YearMult { get; set; }
        public double? ResearchRate { get; set; }
    }
}
