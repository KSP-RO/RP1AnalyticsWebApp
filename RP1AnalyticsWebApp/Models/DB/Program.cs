using System;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    public class Program
    {
        public string Name { get; set; }
        public DateTime Accepted { get; set; }
        public DateTime? ObjectivesCompleted { get; set; }
        public DateTime? Completed { get; set; }
        public double NominalDurationYears { get; set; }
        public double TotalFunding { get; set; }
        public double FundsPaidOut { get; set; }
        public double RepPenaltyAssessed { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProgramSpeed Speed { get; set; }

        public Program()
        {
        }

        public Program(ProgramDto p)
        {
            Name = p.Name;
            Accepted = p.Accepted;
            ObjectivesCompleted = p.ObjectivesCompleted == DateTime.MinValue ? null : p.ObjectivesCompleted;
            Completed = p.Completed == DateTime.MinValue ? null : p.Completed;
            NominalDurationYears = p.NominalDurationYears;
            TotalFunding = p.TotalFunding;
            FundsPaidOut = p.FundsPaidOut;
            RepPenaltyAssessed = p.RepPenaltyAssessed;
            Speed = p.Speed;
        }
    }
}
