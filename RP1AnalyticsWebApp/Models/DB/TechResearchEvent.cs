using System;

namespace RP1AnalyticsWebApp.Models
{
    public class TechResearchEvent
    {
        public DateTime Date { get; set; }
        public string NodeName { get; set; }
        public double? YearMult { get; set; }
        public double? ResearchRate { get; set; }

        public TechResearchEvent()
        {
        }

        public TechResearchEvent(TechResearchEventDto t)
        {
            Date = t.Date;
            NodeName = t.NodeName;
            YearMult = t.YearMult == default ? null : t.YearMult;
            ResearchRate = t.ResearchRate == default ? null : t.ResearchRate;
        }
    }
}
