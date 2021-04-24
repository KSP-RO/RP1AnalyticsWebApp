using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RP1AnalyticsWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class CareerLogMeta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public CareerPlaystyle CareerPlaystyle { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public ConfigurableStart ConfigurableStart { get; set; }

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
}