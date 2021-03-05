using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class ContractEventWithCount : ContractEventWithCareerInfo
    {
        public int Count { get; set; }

        public override string ToString()
        {
            return $"{ContractInternalName}: {Count}";
        }
    }
}
