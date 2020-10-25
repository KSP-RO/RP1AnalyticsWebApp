using System;
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

        public List<CareerLog> Get()
        {
            return _careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();
        }

        public CareerLog Get(string id)
        {
            return _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
        }

        public List<ContractEvent> GetRecords(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;

            var contractDict = new Dictionary<string, string>
            {
                { "first_KarmanUncrewed", "Karman Line" },
                { "SuborbitalReturn", "Reach a Suborbital Trajectory & Return (uncrewed)" },
                { "BreakSoundBarrier", "Break the Sound Barrier (Crewed)" },
                { "first_KarmanCrewed", "Pass the Karman Line (Crewed)" },
                { "first_Downrange", "Downrange Milestone (3000km)" },
                { "first_OrbitUncrewed", "First Artificial Satellite" },
                { "first_OrbitScience", "First Scientific Satellite" },
                { "first_MoonFlybyUncrewed", "Lunar Flyby (Uncrewed)" },
                { "first_MoonImpact", "Lunar Impactor (Uncrewed)" },
                { "first_MoonOrbitUncrewed", "First Crewed Lunar Orbit" },
                { "landingMoon", "Lunar Landing (Uncrewed)" },
                { "first_OrbitRecover", "Reach Orbital Speed & Return Safely to Earth" },
                { "first_OrbitCrewed", "First Orbital Flight (Crewed)" },
                // TODO: other interesting milestones
            };

            return contractDict.Select(kvp => new ContractEvent
            {
                ContractInternalName = kvp.Key,
                ContractDisplayName = kvp.Value,
                Date = c.contractEventEntries.FirstOrDefault(e => e.type == ContractEventType.Complete &&
                                                                  string.Equals(e.internalName, kvp.Key, StringComparison.OrdinalIgnoreCase))?.date
            }).ToList();
        }

        public List<CareerListItem> GetCareerList()
        {
            var p = Builders<CareerLog>.Projection.Expression(c => new CareerListItem(c));
            return _careerLogs.Find(FilterDefinition<CareerLog>.Empty).Project(p).ToList();
        }

        public CareerLog Create(CareerLog log)
        {
            var careerLog = new CareerLog
            {
                token = Guid.NewGuid().ToString("N"),
                name = log.name,
            };

            if (log.careerLogEntries != null)
            {
                careerLog.startDate = log.careerLogEntries[0].startDate;
                careerLog.endDate = log.careerLogEntries[^1].endDate;
                careerLog.careerLogEntries = log.careerLogEntries;
            }

            careerLog.contractEventEntries = log.contractEventEntries;
            careerLog.facilityEventEntries = log.facilityEventEntries;

            _careerLogs.InsertOne(careerLog);

            return careerLog;
        }

        public CareerLog Update(string token, CareerLogDto careerLogDto)
        {
            careerLogDto.TrimEmptyPeriod();
            var updateDef = Builders<CareerLog>.Update.Set(nameof(CareerLog.startDate), careerLogDto.periods[0].startDate)
                                                      .Set(nameof(CareerLog.endDate), careerLogDto.periods[^1].endDate)
                                                      .Set(nameof(CareerLog.careerLogEntries), careerLogDto.periods)
                                                      .Set(nameof(CareerLog.contractEventEntries), careerLogDto.contractEvents)
                                                      .Set(nameof(CareerLog.facilityEventEntries), careerLogDto.facilityEvents);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

            return _careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.token == token, updateDef, opts);
        }
    }
}
