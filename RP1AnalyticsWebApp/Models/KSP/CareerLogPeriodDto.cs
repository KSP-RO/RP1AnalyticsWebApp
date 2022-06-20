using System;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogPeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumEngineers { get; set; }
        public int NumResearchers { get; set; }
        public double EfficiencyEngineers { get; set; }
        public double EfficiencyResearchers { get; set; }
        public double CurrentFunds { get; set; }
        public double CurrentSci { get; set; }
        public double ScienceEarned { get; set; }
        public double ProgramFunds { get; set; }
        public double OtherFundsEarned { get; set; }
        public double LaunchFees { get; set; }
        public double MaintenanceFees { get; set; }
        public double ToolingFees { get; set; }
        public double EntryCosts { get; set; }
        public double ConstructionFees { get; set; }
        public double OtherFees { get; set; }
        public double SubsidySize { get; set; }
        public double SubsidyPaidOut { get; set; }
        public double RepFromPrograms { get; set; }
        public double FundsGainMult { get; set; }
        public int NumNautsKilled { get; set; }
        public double Reputation { get; set; }
    }
}
