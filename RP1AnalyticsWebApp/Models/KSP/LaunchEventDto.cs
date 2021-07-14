using System;

namespace RP1AnalyticsWebApp.Models
{
    public class LaunchEventDto
    {
        public DateTime Date { get; set; }
        public string VesselName { get; set; }
        public string VesselUID { get; set; }
        public string LaunchID { get; set; }
        public EditorFacility BuiltAt { get; set; }
    }
}
