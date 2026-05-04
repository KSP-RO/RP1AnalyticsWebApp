using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    public class LaunchEvent
    {
        public DateTime Date { get; set; }
        public string VesselName { get; set; }
        public string VesselUID { get; set; }
        public string LaunchID { get; set; }
        public string LCID { get; set; }
        public string LCModID { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EditorFacility? BuiltAt { get; set; }
        public double? VesselMassTons { get; set; }
        public int? PartCount { get; set; }
        public int? CrewCount { get; set; }
        public List<FailureEvent> Failures { get; set; }
        public LaunchMeta Metadata { get; set; }

        public LaunchEvent()
        {
        }

        public LaunchEvent(LaunchEventDto l)
        {
            Date = l.Date;
            VesselName = l.VesselName;
            VesselUID = l.VesselUID;
            LaunchID = l.LaunchID;
            LCID = l.LCID;
            LCModID = l.LCModID;
            BuiltAt = l.BuiltAt == EditorFacility.None ? null : l.BuiltAt;
            VesselMassTons = l.VesselMassTons;
            PartCount = l.PartCount;
            CrewCount = l.CrewCount;
        }
    }
}
