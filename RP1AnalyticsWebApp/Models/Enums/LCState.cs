namespace RP1AnalyticsWebApp.Models
{
    public enum LCState
    {
        // Active version of the LC. Should be the last link in a chain of modifications.
        Active = 1,
        /// <summary>
        /// Modified to another version of LC and thus not active.
        /// </summary>
        Modified = 1 << 1,
        Dismantled = 1 << 2,
        UnderConstruction = 1 << 3,
        ConstructionCancelled = 1 << 4
    }
}
