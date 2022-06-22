using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    public class LC
    {
        public Guid Id { get; set; }
        public Guid ModId { get; set; }
        public double ModCost { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LaunchComplexType LcType { get; set; }
        public float MassMax { get; set; }
        public float MassOrig { get; set; }
        public Vector3 SizeMax { get; set; }
        public bool IsHumanRated { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LCState State { get; set; }
        public DateTime? ConstrStarted { get; set; }
        public DateTime? ConstrEnded { get; set; }

        public LC()
        {
        }

        public LC(LCDto lc, List<FacilityConstructionEventDto> constrEvents)
        {
            Id = lc.Id;
            ModId = lc.ModId;
            ModCost = lc.ModCost;
            Name = lc.Name;
            LcType = lc.LcType;
            MassMax = lc.MassMax;
            MassOrig = lc.MassOrig;
            SizeMax = lc.SizeMax;
            IsHumanRated = lc.IsHumanRated;

            var currentEvents = constrEvents.Where(e => e.FacilityID == ModId);
            ConstrStarted = currentEvents.FirstOrDefault(e => e.State == ConstructionState.Started)?.Date;
            ConstrEnded = currentEvents.FirstOrDefault(e => e.State == ConstructionState.Completed || e.State == ConstructionState.Cancelled)?.Date;

            if (currentEvents.Any(e => e.State == ConstructionState.Dismantled))
            {
                State = LCState.Dismantled;
            }
            else if (currentEvents.Any(e => e.State == ConstructionState.Completed))
            {
                State = LCState.Active;
            }
            else if (currentEvents.Any(e => e.State == ConstructionState.Cancelled))
            {
                State = LCState.ConstructionCancelled;
            }
            else if (currentEvents.Any(e => e.State == ConstructionState.Started))
            {
                State = LCState.UnderConstruction;
            }
            else
            {
                // No event. Probably active then?
                State = LCState.Active;
            }

            // Modifications can only be determined after all the LCs have been parsed
        }
    }
}
