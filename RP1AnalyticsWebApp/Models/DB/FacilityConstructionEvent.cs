using System;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    public class FacilityConstructionEvent
    {
        public DateTime Date { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpaceCenterFacility Facility { get; set; }
        public int NewLevel { get; set; }
        public double Cost { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConstructionState State { get; set; }

        public FacilityConstructionEvent()
        {
        }

        public FacilityConstructionEvent(FacilityConstructionEventDto f)
        {
            Date = f.Date;
            Facility = f.Facility;
            NewLevel = f.NewLevel;
            Cost = f.Cost;
            State = f.State;
        }
    }
}
