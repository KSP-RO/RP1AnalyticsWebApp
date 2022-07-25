using System;

namespace RP1AnalyticsWebApp.Models
{
    public class FailureEventDto
    {
        public DateTime Date { get; set; }
        public string VesselUID { get; set; }
        public string LaunchID { get; set; }
        public string Part { get; set; }
        public string Type { get; set; }
    }
}
