using System;

namespace RP1AnalyticsWebApp.Models
{
    public class FacilityConstructionDto
    {
        public Guid FacilityID { get; set; }
        public SpaceCenterFacility Facility { get; set; }
        public int NewLevel { get; set; }
        public double Cost { get; set; }
    }
}
