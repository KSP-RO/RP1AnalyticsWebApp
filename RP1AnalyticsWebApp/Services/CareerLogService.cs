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
        private readonly IContractSettings _contractSettings;

        public CareerLogService(ICareerLogDatabaseSettings dbSettings, IContractSettings contractSettings)
        {
            _contractSettings = contractSettings;

            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            _careerLogs = database.GetCollection<CareerLog>(dbSettings.CareerLogsCollectionName);
        }

        public List<CareerLog> Get()
        {
            return _careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();
        }

        public CareerLog Get(string id)
        {
            return _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
        }

        public List<ContractEvent> GetContractsForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.contractEventEntries == null) return new List<ContractEvent>(0);

            return c.contractEventEntries.Select(ce => new ContractEvent
            {
                ContractInternalName = ce.internalName,
                ContractDisplayName = ResolveContractName(ce.internalName),
                Type = ce.type,
                Date = ce.date
            }).ToList();
        }

        public List<ContractEvent> GetCompletedMilestonesForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.contractEventEntries == null) return new List<ContractEvent>(0);

            return _contractSettings.MilestoneContractNames.Select(name => new ContractEvent
            {
                ContractInternalName = name,
                ContractDisplayName = ResolveContractName(name),
                Type = ContractEventType.Complete,
                Date = c.contractEventEntries.FirstOrDefault(e => e.type == ContractEventType.Complete &&
                                                                  string.Equals(e.internalName, name, StringComparison.OrdinalIgnoreCase))?.date
            }).Where(ce => ce.Date.HasValue).OrderBy(ce => ce.Date).ToList();
        }

        public List<ContractEventWithCareerInfo> GetEventsForContract(string contract, ContractEventType evtType = ContractEventType.Complete)
        {
            var projection = Builders<CareerLog>.Projection.Expression(c => new ContractEventWithCareerInfo
            {
                CareerId = c.Id,
                CareerName = c.name,
                ContractInternalName = contract,
                ContractDisplayName = ResolveContractName(contract),
                Type = evtType,
                Date = c.contractEventEntries.First(ce => ce.internalName == contract && ce.type == evtType).date
            });

            var events = _careerLogs.Find(entry => entry.contractEventEntries.Any(ce => ce.internalName == contract && ce.type == evtType))
                                    .Project(projection)
                                    .ToList();
            // Cannot use server-side sort here. https://stackoverflow.com/q/56988743
            events.Sort((a, b) => a.Date.Value.CompareTo(b.Date.Value));
            return events;
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

        private string ResolveContractName(string name)
        {
            return _contractSettings.ContractNameDict.TryGetValue(name, out string disp) ? disp : name;
        }
    }
}
