using System;

namespace RP1AnalyticsWebApp.Models
{
    public class LaunchEvent
    {
        public DateTime Date { get; set; }
        public string VesselName { get; set; }

        public LaunchEvent()
        {
        }

        public LaunchEvent(LaunchEventDto l)
        {
            Date = l.Date;
            VesselName = l.VesselName;
        }
    }
}
