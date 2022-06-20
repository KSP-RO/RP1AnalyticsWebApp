using System;
using System.Collections.Generic;
using System.Linq;
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

        public FacilityConstructionEvent(FacilityConstructionEventDto f, List<FacilityConstructionDto> facilityConstructions)
        {
            FacilityConstructionDto fc = facilityConstructions.FirstOrDefault(fc => fc.FacilityID == f.FacilityID);

            Date = f.Date;
            Facility = (SpaceCenterFacility)Enum.Parse(typeof(SpaceCenterFacility), f.Facility.ToString());
            NewLevel = fc?.NewLevel ?? 0;
            Cost = fc?.Cost ?? 0;
            State = f.State;
        }
    }
}
