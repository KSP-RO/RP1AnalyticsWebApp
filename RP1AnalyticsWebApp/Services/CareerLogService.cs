using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using MongoDB.Bson;
using MongoDB.Driver;
using RP1AnalyticsWebApp.Models;

namespace RP1AnalyticsWebApp.Services
{
    public class CareerLogService
    {
        private readonly IMongoCollection<CareerLog> _careerLogs;
        private readonly IContractSettings _contractSettings;
        private readonly ITechTreeSettings _techTreeSettings;

        public CareerLogService(ICareerLogDatabaseSettings dbSettings, IContractSettings contractSettings,
            ITechTreeSettings techTreeSettings)
        {
            _contractSettings = contractSettings;
            _techTreeSettings = techTreeSettings;

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
                                                                  string.Equals(e.internalName, name,
                                                                      StringComparison.OrdinalIgnoreCase))?.date
            }).Where(ce => ce.Date.HasValue).OrderBy(ce => ce.Date).ToList();
        }

        public List<ContractEventWithCount> GetRepeatableContractCompletionCountsForCareer(string id)
        {
            var result = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .SelectMany(c => c.contractEventEntries, (c, e) => new
                {
                    CareerId = c.Id,
                    CareerName = c.name,
                    ContractInternalName = e.internalName,
                    EventType = e.type,
                    EventDate = e.date
                })
                .Where(c => c.EventType == ContractEventType.Complete)
                .GroupBy(c => c.ContractInternalName)
                .Select(g => new ContractEventWithCount
                {
                    ContractInternalName = g.Key,
                    CareerId = g.First().CareerId,
                    CareerName = g.First().CareerName,
                    Date = g.Min(e => e.EventDate),
                    Count = g.Count()
                })
                .ToList();

            var repeatables = result
                .Where(r => _contractSettings.RepeatableContractNames.Contains(r.ContractInternalName))
                .ToList();
            foreach (var item in repeatables)
            {
                item.ContractDisplayName = ResolveContractName(item.ContractInternalName);
                item.Type = ContractEventType.Complete;
            }

            return repeatables;
        }

        public List<ContractEventWithCareerInfo> GetRecords()
        {
            var result = _careerLogs.AsQueryable()
                .Where(c => c.eligibleForRecords)
                .SelectMany(c => c.contractEventEntries, (c, e) => new
                {
                    CareerId = c.Id,
                    CareerName = c.name,
                    ContractInternalName = e.internalName,
                    EventType = e.type,
                    EventDate = e.date
                })
                .Where(c => c.EventType == ContractEventType.Complete)
                .OrderBy(c => c.EventDate)
                .GroupBy(c => c.ContractInternalName)
                .Select(g => new ContractEventWithCareerInfo
                {
                    ContractInternalName = g.Key,
                    CareerId = g.First().CareerId,
                    CareerName = g.First().CareerName,
                    Date = g.Min(e => e.EventDate)
                })
                .OrderBy(c => c.Date)
                .ToList();

            result.ForEach(r =>
            {
                r.ContractDisplayName = ResolveContractName(r.ContractInternalName);
                r.Type = ContractEventType.Complete;
            });

            return result;
        }

        public List<ContractEventWithCareerInfo> GetEventsForContract(string contract,
            ContractEventType evtType = ContractEventType.Complete)
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

            var events = _careerLogs.Find(entry =>
                    entry.contractEventEntries.Any(ce => ce.internalName == contract && ce.type == evtType))
                .Project(projection)
                .ToList();
            // Cannot use server-side sort here. https://stackoverflow.com/q/56988743
            events.Sort((a, b) => a.Date.Value.CompareTo(b.Date.Value));
            return events;
        }

        public List<TechEvent> GetTechUnlocksForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.techEventEntries == null) return new List<TechEvent>(0);

            return c.techEventEntries.Select(e => new TechEvent
            {
                NodeInternalName = e.nodeName,
                NodeDisplayName = ResolveTechNodeName(e.nodeName),
                Date = e.date
            }).OrderBy(ce => ce.Date).ToList();
        }

        public List<LaunchEvent> GetLaunchesForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.launchEventEntries == null) return new List<LaunchEvent>(0);

            return c.launchEventEntries.Select(e => new LaunchEvent
            {
                VesselName = e.vesselName,
                Date = e.date
            }).OrderBy(e => e.Date).ToList();
        }

        public List<CareerListItem> GetCareerList(string userName = null)
        {
            var res = GetCareerListWithTokens(userName);
            res.ForEach(c => c.token = null);
            return res;
        }

        public List<CareerListItem> GetCareerListWithTokens(string userName = null)
        {
            var filter = !string.IsNullOrWhiteSpace(userName)
                ? Builders<CareerLog>.Filter.Where(c => c.userLogin == userName)
                : FilterDefinition<CareerLog>.Empty;
            var p = Builders<CareerLog>.Projection.Expression(c => new CareerListItem(c));
            return _careerLogs.Find(filter).Project(p).ToList();
        }

        public CareerLog Create(CareerLog log)
        {
            var careerLog = new CareerLog
            {
                token = Guid.NewGuid().ToString("N"),
                name = log.name,
                userLogin = log.userLogin,
                eligibleForRecords = log.eligibleForRecords,
                careerLogMeta = log.careerLogMeta
            };

            if (log.careerLogEntries != null)
            {
                careerLog.startDate = log.careerLogEntries[0].startDate;
                careerLog.endDate = log.careerLogEntries[^1].endDate;
                careerLog.careerLogEntries = log.careerLogEntries;
            }

            careerLog.contractEventEntries = log.contractEventEntries;
            careerLog.facilityEventEntries = log.facilityEventEntries;
            careerLog.techEventEntries = log.techEventEntries;
            careerLog.launchEventEntries = log.launchEventEntries;

            _careerLogs.InsertOne(careerLog);

            return careerLog;
        }

        public CareerLog Update(string token, CareerLogDto careerLogDto)
        {
            careerLogDto.TrimEmptyPeriod();
            var updateDef = Builders<CareerLog>.Update
                .Set(nameof(CareerLog.startDate), careerLogDto.periods[0].startDate)
                .Set(nameof(CareerLog.endDate), careerLogDto.periods[^1].endDate)
                .Set(nameof(CareerLog.careerLogEntries), careerLogDto.periods)
                .Set(nameof(CareerLog.contractEventEntries), careerLogDto.contractEvents)
                .Set(nameof(CareerLog.facilityEventEntries), careerLogDto.facilityEvents)
                .Set(nameof(CareerLog.techEventEntries), careerLogDto.techEvents)
                .Set(nameof(CareerLog.launchEventEntries), careerLogDto.launchEvents);
            var opts = new FindOneAndUpdateOptions<CareerLog> {ReturnDocument = ReturnDocument.After};

            return _careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.token == token, updateDef, opts);
        }

        private string ResolveContractName(string name)
        {
            return _contractSettings.ContractNameDict.TryGetValue(name, out string disp) ? disp : name;
        }

        private string ResolveTechNodeName(string id)
        {
            return _techTreeSettings.NodeTitleDict.TryGetValue(id, out string disp) ? disp : id;
        }

        public CareerLog GetByToken(string token)
        {
            return _careerLogs.Find(entry => entry.token == token).FirstOrDefault();
        }

        public void DeleteByToken(string token)
        {
            Console.WriteLine("delete called: " + token);
            _careerLogs.DeleteOne(entry => entry.token == token);
        }

        public CareerLog UpdateMetaByToken(string token, CareerLogMeta meta)
        {
            var updateDefinition = Builders<CareerLog>.Update.Set(nameof(CareerLog.careerLogMeta), meta);
            var opts = new FindOneAndUpdateOptions<CareerLog> {ReturnDocument = ReturnDocument.After};

            return _careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.token == token, updateDefinition, opts);
        }
    }
}