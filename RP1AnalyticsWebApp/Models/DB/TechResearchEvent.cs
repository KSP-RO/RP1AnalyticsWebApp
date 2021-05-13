using System;

namespace RP1AnalyticsWebApp.Models
{
    public class TechResearchEvent
    {
        public DateTime Date { get; set; }
        public string NodeName { get; set; }

        public TechResearchEvent()
        {
        }

        public TechResearchEvent(TechResearchEventDto t)
        {
            Date = t.Date;
            NodeName = t.NodeName;
        }
    }
}
