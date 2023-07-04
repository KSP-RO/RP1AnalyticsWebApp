using System;

namespace RP1AnalyticsWebApp.Models
{
    public class LeaderEventDto
    {
        public DateTime Date { get; set; }
        public string LeaderName { get; set; }
        public double Cost { get; set; }
        public bool IsAdd { get; set; }
    }
}
