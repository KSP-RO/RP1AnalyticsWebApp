using System;

namespace RP1AnalyticsWebApp.Models
{
    public class TechResearchEventDto
    {
        public DateTime Date { get; set; }
        public string NodeName { get; set; }
        public double YearMult { get; set; }
        public double ResearchRate { get; set; }
    }
}
