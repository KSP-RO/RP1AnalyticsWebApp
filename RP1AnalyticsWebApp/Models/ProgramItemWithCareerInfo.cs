using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ProgramItemWithCareerInfo
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public DateTime Accepted { get; set; }
        public DateTime? ObjectivesCompleted { get; set; }
        public DateTime? Completed { get; set; }
        public double NominalDurationYears { get; set; }
        public double TotalFunding { get; set; }
        public double FundsPaidOut { get; set; }
        public double RepPenaltyAssessed { get; set; }
        public ProgramSpeed Speed { get; set; }
        public string CareerId { get; set; }
        public string CareerName { get; set; }
        public string UserLogin { get; set; }
        public string UserPreferredName { get; set; }
    }
}
