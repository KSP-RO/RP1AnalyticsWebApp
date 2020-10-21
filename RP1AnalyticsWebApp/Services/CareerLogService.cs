using System.Collections.Generic;
using System.Linq;
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
            return _careerLogs.Find<CareerLog>(entry => entry.exportUuid == id).FirstOrDefault();
        }

        public List<string> GetCareerIDs()
        {
            return _careerLogs.Distinct<string>(nameof(CareerLog.exportUuid), FilterDefinition<CareerLog>.Empty).ToList();
        }

        public CareerLog Create(List<CareerLogDto> careerLogDtos)
        {
            var careerLog = new CareerLog
            {
                exportUuid = careerLogDtos[0].careerUuid,
                epochStart = careerLogDtos[0].epoch,
                epochEnd = careerLogDtos[^1].epoch,
                careerLogEntries = new List<CareerLogDto>(careerLogDtos.Take(careerLogDtos.Count - 1).ToArray())
            };

            _careerLogs.InsertOne(careerLog);

            return careerLog;
        }
    }
}