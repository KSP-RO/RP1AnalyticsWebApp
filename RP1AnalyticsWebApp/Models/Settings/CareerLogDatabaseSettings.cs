namespace RP1AnalyticsWebApp.Models
{
    public class CareerLogDatabaseSettings : ICareerLogDatabaseSettings
    {
        public string CareerLogsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionString2 { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseName2 { get; set; }
    }

    public interface ICareerLogDatabaseSettings
    {
        string CareerLogsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string ConnectionString2 { get; set; }
        string DatabaseName { get; set; }
        string DatabaseName2 { get; set; }
    }
}