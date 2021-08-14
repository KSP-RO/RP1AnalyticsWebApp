using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class ContractEventWithCareerInfo : BaseContractEvent
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string CareerId { get; set; }
        public string CareerName { get; set; }
        public string UserLogin { get; set; }
        public string UserPreferredName { get; set; }
    }
}
