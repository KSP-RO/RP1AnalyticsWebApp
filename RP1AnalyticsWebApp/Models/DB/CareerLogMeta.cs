using System;
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

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public ModRecency ModRecency { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string VersionTag { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int? VersionSort { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? CreationDate { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string DescriptionText { get; set; }

        public static int? CreateSortableVersion(Version version)
        {
            if (version == null) return null;

            // 3 digits per component. Thus version 1.22.333 becomes 001022333
            return version.Major * 1000000 + version.Minor * 1000 + version.Build;
        }
    }
}