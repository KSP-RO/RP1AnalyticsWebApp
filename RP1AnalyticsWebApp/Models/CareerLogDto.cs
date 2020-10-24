using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogDto
    {
        public List<CareerLogPeriodDto> periods { get; set; }
        public List<ContractEventDto> contractEvents { get; set; }
        public List<FacilityConstructionEventDto> facilityEvents { get; set; }
        public List<TechResearchEventDto> techEvents { get; set; }
        public List<LaunchEventDto> launchEvents { get; set; }

        public void TrimEmptyPeriod()
        {
            if (periods == null || periods.Count == 0) return;
            int idx = periods.Count - 1;
            var p = periods[idx];
            if (p.vabUpgrades == 0 && p.sphUpgrades == 0 && p.rndUpgrades == 0)
            {
                periods.RemoveAt(idx);
            }
        }
    }
}