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

        public string Name { get; set; }
        public string UserLogin { get; set; }
        public string Token { get; set; }
        public bool EligibleForRecords { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<CareerLogPeriod> CareerLogEntries { get; set; }
        public List<ContractEvent> ContractEventEntries { get; set; }
        public List<LC> LCs { get; set; }
        public List<FacilityConstruction> FacilityConstructions { get; set; }
        public List<TechResearchEvent> TechEventEntries { get; set; }
        public List<LaunchEvent> LaunchEventEntries { get; set; }
        public CareerLogMeta CareerLogMeta { get; set; }

        public void RemoveNonPublicData()
        {
            Token = null;
        }
    }
}