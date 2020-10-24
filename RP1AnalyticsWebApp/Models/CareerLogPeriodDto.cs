using System;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogPeriodDto
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int vabUpgrades { get; set; }
        public int sphUpgrades { get; set; }
        public int rndUpgrades { get; set; }
        public double currentFunds { get; set; }
        public double currentSci { get; set; }
        public double scienceEarned { get; set; }
        public double advanceFunds { get; set; }
        public double rewardFunds { get; set; }
        public double failureFunds { get; set; }
        public double otherFundsEarned { get; set; }
        public double launchFees { get; set; }
        public double maintenanceFees { get; set; }
        public double toolingFees { get; set; }
        public double entryCosts { get; set; }
        public double constructionFees { get; set; }
        public double otherFees { get; set; }
    }
}
