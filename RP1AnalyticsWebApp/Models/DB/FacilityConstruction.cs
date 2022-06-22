using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    public class FacilityConstruction
    {
        public Guid Id { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpaceCenterFacility Facility { get; set; }
        public int NewLevel { get; set; }
        public double Cost { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FacilityConstructionState State { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }

        public FacilityConstruction()
        {
        }

        public FacilityConstruction(FacilityConstructionDto fc, List<FacilityConstructionEventDto> constrEvents)
        {
            Id = fc.FacilityID;
            Facility = fc.Facility;
            NewLevel = fc.NewLevel;
            Cost = fc.Cost;

            var currentEvents = constrEvents.Where(e => e.FacilityID == Id);
            Started = currentEvents.FirstOrDefault(e => e.State == ConstructionState.Started)?.Date;
            Ended = currentEvents.FirstOrDefault(e => e.State == ConstructionState.Completed || e.State == ConstructionState.Cancelled)?.Date;

            if (currentEvents.Any(e => e.State == ConstructionState.Completed))
            {
                State = FacilityConstructionState.Completed;
            }
            else if (currentEvents.Any(e => e.State == ConstructionState.Cancelled))
            {
                State = FacilityConstructionState.ConstructionCancelled;
            }
            else if (currentEvents.Any(e => e.State == ConstructionState.Started))
            {
                State = FacilityConstructionState.UnderConstruction;
            }
            else
            {
                State = FacilityConstructionState.Completed;
            }
        }
    }
}
