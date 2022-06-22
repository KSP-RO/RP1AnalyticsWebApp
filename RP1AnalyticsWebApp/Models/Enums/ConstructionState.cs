namespace RP1AnalyticsWebApp.Models
{
    public enum ConstructionState
    {
        Started = 1,
        Cancelled = 1 << 1,
        Completed = 1 << 2,
        Dismantled = 1 << 3
    }
}
