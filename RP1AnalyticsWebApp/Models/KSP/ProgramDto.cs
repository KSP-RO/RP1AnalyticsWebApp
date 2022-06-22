using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ProgramDto
    {
        public string Name { get; set; }
        public DateTime Accepted { get; set; }
        public DateTime ObjectivesCompleted { get; set; }
        public DateTime Completed { get; set; }
        public double NominalDurationYears { get; set; }
        public double TotalFunding { get; set; }
        public double FundsPaidOut { get; set; }
        public double RepPenaltyAssessed { get; set; }
}
}
