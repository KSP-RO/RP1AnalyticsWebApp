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
            Speed = (ProgramSpeed)p.Speed;
        }

        /// <summary>
        /// Uses the same heuristic as RP-1 itself: a completed program whose dates are all suspiciously
        /// round was most likely granted by a configurable start scenario instead of being played out.
        /// </summary>
        public bool IsLikelyCompletedByConfigurableStart()
        {
            // Missing dates correspond to UT 0 in the game, which is a round date as well.
            return Completed.HasValue &&
                   IsSuspiciouslyRoundDate(Accepted) &&
                   (!ObjectivesCompleted.HasValue || IsSuspiciouslyRoundDate(ObjectivesCompleted.Value)) &&
                   IsSuspiciouslyRoundDate(Completed.Value);
        }

        private static bool IsSuspiciouslyRoundDate(DateTime dt)
        {
            // Apparently some scenarios assign minutes and hours to dates.
            return dt.Second == 0 && dt.Millisecond == 0;
        }
    }
}
