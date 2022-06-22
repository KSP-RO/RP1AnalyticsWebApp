namespace RP1AnalyticsWebApp.Models
{
    public enum FacilityConstructionState
    {
        Completed = 1,
        UnderConstruction = 1 << 1,
        ConstructionCancelled = 1 << 2
    }
}
