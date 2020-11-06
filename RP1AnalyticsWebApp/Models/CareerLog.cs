using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string name { get; set; }
        public string userLogin { get; set; }
        public string token { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<CareerLogPeriodDto> careerLogEntries { get; set; }
        public List<ContractEventDto> contractEventEntries { get; set; }
        public List<FacilityConstructionEventDto> facilityEventEntries { get; set; }

        public void RemoveNonPublicData()
        {
            token = null;
        }
    }
}