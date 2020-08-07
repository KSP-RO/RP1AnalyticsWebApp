using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using MongoDB.Driver;
using RP1AnalyticsWebApp.Models;

namespace RP1AnalyticsWebApp.Services
{
    public class CareerLogService
    {
        private readonly IMongoCollection<CareerLog> _careerLogs;

        public CareerLogService(ICareerLogDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _careerLogs = database.GetCollection<CareerLog>(settings.CareerLogsCollectionName);
        }

        public List<CareerLog> Get() =>
            _careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();

        public CareerLog Get(string id)
        {
            return _careerLogs.Find<CareerLog>(entry => entry.Id == id).FirstOrDefault();
        }

        public CareerLog Create(CareerLog careerLog)
        {
            _careerLogs.InsertOne(careerLog);
            return careerLog;
        }

        public List<CareerLog> CreateMany(List<CareerLog> careerLogs)
        {
            _careerLogs.InsertMany(careerLogs);
            return careerLogs;
        }
    }
}