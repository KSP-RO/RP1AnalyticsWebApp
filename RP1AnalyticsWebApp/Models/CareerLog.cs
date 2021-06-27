using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class CareerLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string name { get; set; }
        public string userLogin { get; set; }
        public string token { get; set; }
        public bool eligibleForRecords { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<CareerLogPeriodDto> careerLogEntries { get; set; }
        public List<ContractEventDto> contractEventEntries { get; set; }
        public List<FacilityConstructionEventDto> facilityEventEntries { get; set; }
        public List<TechResearchEventDto> techEventEntries { get; set; }
        public List<LaunchEventDto> launchEventEntries { get; set; }
        public CareerLogMeta careerLogMeta { get; set; }

        public void RemoveNonPublicData()
        {
            token = null;
        }
    }
}