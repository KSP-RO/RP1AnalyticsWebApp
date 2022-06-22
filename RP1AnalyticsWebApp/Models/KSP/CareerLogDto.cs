using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogDto
    {
        public string JsonVer { get; set; }
        public string RP1Ver { get; set; }
        public List<CareerLogPeriodDto> Periods { get; set; }
        public List<ContractEventDto> ContractEvents { get; set; }
        public List<FacilityConstructionDto> FacilityConstructions { get; set; }
        public List<LCDto> LCs { get; set; }
        public List<LPConstructionDto> LPConstructions { get; set; }
        public List<FacilityConstructionEventDto> FacilityEvents { get; set; }
        public List<TechResearchEventDto> TechEvents { get; set; }
        public List<LaunchEventDto> LaunchEvents { get; set; }
        public List<ProgramDto> Programs { get; set; }

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