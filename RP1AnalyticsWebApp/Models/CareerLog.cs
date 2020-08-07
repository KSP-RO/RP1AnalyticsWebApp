using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string EpochStartDate { get; set; }
        public string EpochEndDate { get; set; }
        public int VabUpgrades { get; set; }
        public int SphUpgrades { get; set; }
        public int RndUpgrades { get; set; }
        public double CurrentFunds { get; set; }
        public double CurrentSci { get; set; }
        public double ScienceEarned { get; set; }
        public double AdditionalFunds { get; set; }
        public double LaunchFees { get; set; }
        public double MaintenanceFees { get; set; }
        public double ToolingFees { get; set; }
        public double EntryCosts { get; set; }
        public double OtherFees { get; set; }
        public string[] LaunchedVessels { get; set; }
        public string[] ContractEvents { get; set; }
        public string[] TechEvents { get; set; }
        public string[] FacilityConstructions { get; set; }
    }
}