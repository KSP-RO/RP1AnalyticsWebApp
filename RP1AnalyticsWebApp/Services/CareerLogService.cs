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

        public CareerLogService(ICareerLogDatabaseSettings dbSettings, IContractSettings contractSettings, ITechTreeSettings techTreeSettings)
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
                                                                  string.Equals(e.internalName, name, StringComparison.OrdinalIgnoreCase))?.date
            }).Where(ce => ce.Date.HasValue).OrderBy(ce => ce.Date).ToList();
        }

        public List<ContractEventWithCount> GetRepeatableContractCompletionCountsForCareer(string id)
        {
            var proj = BsonDocument.Parse("{ contractEventEntries: 1, name: 1, _id: 1 }");
            var gProj = BsonDocument.Parse("{ _id: \"$contractEventEntries.internalName\", ContractInternalName: { $first: \"$contractEventEntries.internalName\" }, " +
                                           "CareerId: { $first: \"$_id\" }, CareerName: { $first: \"$name\" }, Date: { $min: \"$contractEventEntries.date\" }, Count: { $sum: 1 } }");

            var result = _careerLogs
                .Aggregate()
                .Match(BsonDocument.Parse($"{{ \"_id\": ObjectId(\"{id}\") }}"))
                .Project(proj)
                .Unwind(nameof(CareerLog.contractEventEntries))
                .Match(BsonDocument.Parse("{ \"contractEventEntries.type\": 1 }"))
                .Group(gProj)
                .As<ContractEventWithCount>()
                .ToList();

            var repeatables = result.Where(r => _contractSettings.RepeatableContractNames.Contains(r.ContractInternalName))
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
            var proj = BsonDocument.Parse("{ contractEventEntries: 1, name: 1 }");
            var gProj = BsonDocument.Parse("{ _id: \"$contractEventEntries.internalName\", ContractInternalName: { $first: \"$contractEventEntries.internalName\" }, " +
                                           "CareerId: { $first: \"$_id\" }, CareerName: { $first: \"$name\" }, Date: { $min: \"$contractEventEntries.date\" } }");

            var result = _careerLogs
                .Aggregate()
                .Project(proj)
                .Unwind(nameof(CareerLog.contractEventEntries))
                .Sort(BsonDocument.Parse("{ \"contractEventEntries.date\": 1 }"))
                .Match(BsonDocument.Parse("{ \"contractEventEntries.type\": 1 }"))
                .Group(gProj)
                .Sort(BsonDocument.Parse("{ \"Date\": 1 }"))
                .As<ContractEventWithCareerInfo>()
                .ToList();

            result.ForEach(r =>
            {
                r.ContractDisplayName = ResolveContractName(r.ContractInternalName);
                r.Type = ContractEventType.Complete;
            });

            return result;
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

        public List<CareerListItem> GetCareerList(string userName = null)
        {
            var res = GetCareerListWithTokens(userName);
            res.ForEach(c => c.token = null);
            return res;
        }

        public List<CareerListItem> GetCareerListWithTokens(string userName = null)
        {
            var filter = !string.IsNullOrWhiteSpace(userName) ? Builders<CareerLog>.Filter.Where(c => c.userLogin == userName) :
                                                                FilterDefinition<CareerLog>.Empty;
            var p = Builders<CareerLog>.Projection.Expression(c => new CareerListItem(c));
            return _careerLogs.Find(filter).Project(p).ToList();
        }

        public CareerLog Create(CareerLog log)
        {
            var careerLog = new CareerLog
            {
                token = Guid.NewGuid().ToString("N"),
                name = log.name,
                userLogin = log.userLogin
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
                                                      .Set(nameof(CareerLog.facilityEventEntries), careerLogDto.facilityEvents)
                                                      .Set(nameof(CareerLog.techEventEntries), careerLogDto.techEvents);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

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
    }
}
