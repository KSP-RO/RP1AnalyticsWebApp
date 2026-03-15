using System;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    public class ProgramItem
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
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProgramSpeed Speed { get; set; }
    }
}
