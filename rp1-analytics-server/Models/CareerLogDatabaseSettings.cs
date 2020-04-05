namespace rp1_analytics_server.Models
{
    public class CareerLogDatabaseSettings : ICareerLogDatabaseSettings
    {
        public string CareerLogsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ICareerLogDatabaseSettings
    {
        string CareerLogsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}