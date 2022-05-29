using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogDto
    {
        public List<CareerLogPeriodDto> Periods { get; set; }
        public List<ContractEventDto> ContractEvents { get; set; }
        public List<FacilityConstructionEventDto> FacilityEvents { get; set; }
        public List<TechResearchEventDto> TechEvents { get; set; }
        public List<LaunchEventDto> LaunchEvents { get; set; }

        public void TrimEmptyPeriod()
        {
            if (Periods == null || Periods.Count == 0) return;
            int idx = Periods.Count - 1;
            var p = Periods[idx];
            if (p.NumEngineers == 0 && p.NumResearchers == 0)    // Probably not the best check if the player can fire all their personnel
            {
                Periods.RemoveAt(idx);
            }
        }
    }
}