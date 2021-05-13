using System;

namespace RP1AnalyticsWebApp.Models
{
    public class FacilityConstructionEventDto
    {
        public DateTime Date { get; set; }
        public SpaceCenterFacility Facility { get; set; }
        public int NewLevel { get; set; }
        public double Cost { get; set; }
        public ConstructionState State { get; set; }
    }
}
