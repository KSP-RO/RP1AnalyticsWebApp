using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class CareerLogMeta
    {
        public string CareerPlaystyle { get; set; }
        public string DifficultyLevel { get; set; }
        public string ConfigurableStart { get; set; }

        public string FailureModel { get; set; }
        public string DescriptionText { get; set; }
    }


    public enum CareerPlaystyle
    {
        Normal,
        Historic,
        Caveman,
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }


    public enum ConfigurableStart
    {
        NONE,
        Y1955,
        Y1957,
        Y1959,
        Y1961,
        Y1963
    }

    public enum FailureModel
    {
        TestFlight,
        TestLite
    }
}