using System;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogPeriod
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
        public double fundsGainMult { get; set; }

        public CareerLogPeriod()
        {
        }

        public CareerLogPeriod(CareerLogPeriodDto c)
        {
            StartDate = c.StartDate;
            EndDate = c.EndDate;
            VabUpgrades = c.VabUpgrades;
            SphUpgrades = c.SphUpgrades;
            RndUpgrades = c.RndUpgrades;
            CurrentFunds = c.CurrentFunds;
            CurrentSci = c.CurrentSci;
            ScienceEarned = c.ScienceEarned;
            AdvanceFunds = c.AdvanceFunds;
            RewardFunds = c.RewardFunds;
            FailureFunds = c.FailureFunds;
            OtherFundsEarned = c.OtherFundsEarned;
            LaunchFees = c.LaunchFees;
            MaintenanceFees = c.MaintenanceFees;
            ToolingFees = c.ToolingFees;
            EntryCosts = c.EntryCosts;
            ConstructionFees = c.ConstructionFees;
            OtherFees = c.OtherFees;
            fundsGainMult = c.FundsGainMult;
        }
    }
}
