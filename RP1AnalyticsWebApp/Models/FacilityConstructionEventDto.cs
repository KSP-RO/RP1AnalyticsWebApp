using System;

namespace RP1AnalyticsWebApp.Models
{
    public class FacilityConstructionEventDto
    {
        public DateTime date { get; set; }
        public SpaceCenterFacility facility { get; set; }
        public int newLevel { get; set; }
        public double cost { get; set; }
        public ConstructionState state { get; set; }
    }

    public enum ConstructionState
    {
        Started, Completed
    }

    public enum SpaceCenterFacility
    {
        Administration = 0,
        AstronautComplex = 1,
        LaunchPad = 2,
        MissionControl = 3,
        ResearchAndDevelopment = 4,
        Runway = 5,
        TrackingStation = 6,
        SpaceplaneHangar = 7,
        VehicleAssemblyBuilding = 8
    }
}
