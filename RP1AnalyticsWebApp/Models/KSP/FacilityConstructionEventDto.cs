using System;

namespace RP1AnalyticsWebApp.Models
{
    public class FacilityConstructionEventDto
    {
        public DateTime Date { get; set; }
        public FacilityType Facility { get; set; }
        public Guid FacilityID { get; set; }
        public ConstructionState State { get; set; }
    }
}
