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

        public string exportUuid { get; set; }
        public string epochStart { get; set; }
        public string epochEnd { get; set; }
        public List<CareerLogDto> careerLogEntries { get; set; }
    }
}