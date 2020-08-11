namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogDto
    {
        public string careerUuid;
        public string epoch;
        public int vabUpgrades;
        public int sphUpgrades;
        public int rndUpgrades;
        public double currentFunds;
        public double currentSci;
        public double scienceEarned;
        public double advanceFunds;
        public double rewardFunds;
        public double failureFunds;
        public double otherFundsEarned;
        public double launchFees;
        public double maintenanceFees;
        public double toolingFees;
        public double entryCosts;
        public double constructionFees;
        public double otherFees;

        public string[] launchedVessels;
        public string[] contractEvents;
        public string[] techEvents;
        public string[] facilityConstructions;
    }
}