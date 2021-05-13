using System;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogPeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int VabUpgrades { get; set; }
        public int SphUpgrades { get; set; }
        public int RndUpgrades { get; set; }
        public double CurrentFunds { get; set; }
        public double CurrentSci { get; set; }
        public double ScienceEarned { get; set; }
        public double AdvanceFunds { get; set; }
        public double RewardFunds { get; set; }
        public double FailureFunds { get; set; }
        public double OtherFundsEarned { get; set; }
        public double LaunchFees { get; set; }
        public double MaintenanceFees { get; set; }
        public double ToolingFees { get; set; }
        public double EntryCosts { get; set; }
        public double ConstructionFees { get; set; }
        public double OtherFees { get; set; }
        public double FundsGainMult { get; set; }
    }
}
