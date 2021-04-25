using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class CareerLogMeta
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public CareerPlaystyle CareerPlaystyle { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public DifficultyLevel DifficultyLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public ConfigurableStart ConfigurableStart { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public FailureModel FailureModel { get; set; }

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
        Normal,
        Moderate,
        Hard
    }


    public enum ConfigurableStart
    {
        None,
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