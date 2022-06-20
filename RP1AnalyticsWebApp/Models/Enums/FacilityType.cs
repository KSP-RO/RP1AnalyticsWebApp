namespace RP1AnalyticsWebApp.Models
{
    /// <summary>
    /// RP-1 custom facility type enum. Has all the KSP builtin facilities + our custom ones.
    /// Supports bitwise operations.
    /// </summary>
    public enum FacilityType
    {
        Administration = 1,
        AstronautComplex = 1 << 1,
        LaunchPad = 1 << 2,
        MissionControl = 1 << 3,
        ResearchAndDevelopment = 1 << 4,
        Runway = 1 << 5,
        TrackingStation = 1 << 6,
        SpaceplaneHangar = 1 << 7,
        VehicleAssemblyBuilding = 1 << 8,
        LaunchComplex = 1 << 9
    }
}
