using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using RP1AnalyticsWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP1AnalyticsWebApp.Services
{
    public class CareerLogService
    {
        private readonly IMongoCollection<CareerLog> _careerLogs;
        private readonly IContractSettings _contractSettings;
        private readonly ITechTreeSettings _techTreeSettings;
        private readonly UserManager<WebAppUser> _userManager;

        private List<WebAppUser> _allUsers;
        private List<WebAppUser> AllUsers => _allUsers ??= _userManager.Users.ToList();

        public CareerLogService(ICareerLogDatabaseSettings dbSettings, IContractSettings contractSettings,
            ITechTreeSettings techTreeSettings, UserManager<WebAppUser> userManager)
        {
            _contractSettings = contractSettings;
            _techTreeSettings = techTreeSettings;
            _userManager = userManager;

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

        public List<BaseContractEvent> GetContractsForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.ContractEventEntries == null) return new List<BaseContractEvent>(0);

            return c.ContractEventEntries.Select(ce => new BaseContractEvent
            {
                ContractInternalName = ce.InternalName,
                ContractDisplayName = ResolveContractName(ce.InternalName),
                Type = ce.Type,
                Date = ce.Date
            }).ToList();
        }

        public List<BaseContractEvent> GetCompletedMilestonesForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.ContractEventEntries == null) return new List<BaseContractEvent>(0);

            return _contractSettings.MilestoneContractNames.Select(name => new BaseContractEvent
            {
                ContractInternalName = name,
                ContractDisplayName = ResolveContractName(name),
                Type = ContractEventType.Complete,
                Date = c.ContractEventEntries.FirstOrDefault(e => e.Type == ContractEventType.Complete &&
                                                                  string.Equals(e.InternalName, name,
                                                                      StringComparison.OrdinalIgnoreCase))?.Date
            }).Where(ce => ce.Date.HasValue).OrderBy(ce => ce.Date).ToList();
        }

        public List<ContractEventWithCount> GetRepeatableContractCompletionCountsForCareer(string id)
        {
            var result = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .SelectMany(c => c.ContractEventEntries, (c, e) => new
                {
                    CareerId = c.Id,
                    CareerName = c.Name,
                    ContractInternalName = e.InternalName,
                    EventType = e.Type,
                    EventDate = e.Date
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
                .Where(c => c.EligibleForRecords)
                .SelectMany(c => c.ContractEventEntries, (c, e) => new
                {
                    CareerId = c.Id,
                    CareerName = c.Name,
                    UserLogin = c.UserLogin,
                    ContractInternalName = e.InternalName,
                    EventType = e.Type,
                    EventDate = e.Date
                })
                .Where(c => c.EventType == ContractEventType.Complete)
                .OrderBy(c => c.EventDate)
                .GroupBy(c => c.ContractInternalName)
                .Select(g => new ContractEventWithCareerInfo
                {
                    ContractInternalName = g.Key,
                    CareerId = g.First().CareerId,
                    UserLogin = g.First().UserLogin,
                    CareerName = g.First().CareerName,
                    Date = g.Min(e => e.EventDate)
                })
                .OrderBy(c => c.Date)
                .ToList();

            result.ForEach(r =>
            {
                r.ContractDisplayName = ResolveContractName(r.ContractInternalName);
                r.UserPreferredName = GetUserPreferredName(r.UserLogin);
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
                CareerName = c.Name,
                UserLogin = c.UserLogin,
                UserPreferredName = GetUserPreferredName(c.UserLogin),
                ContractInternalName = contract,
                ContractDisplayName = ResolveContractName(contract),
                Type = evtType,
                Date = c.ContractEventEntries.First(ce => ce.InternalName == contract && ce.Type == evtType).Date
            });

            var events = _careerLogs.Find(entry =>
                    entry.ContractEventEntries.Any(ce => ce.InternalName == contract && ce.Type == evtType))
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
            if (c.TechEventEntries == null) return new List<TechEvent>(0);

            return c.TechEventEntries.Select(e => new TechEvent
            {
                NodeInternalName = e.NodeName,
                NodeDisplayName = ResolveTechNodeName(e.NodeName),
                Date = e.Date
            }).OrderBy(ce => ce.Date).ToList();
        }

        public List<LaunchEvent> GetLaunchesForCareer(string id)
        {
            var c = _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
            if (c == null) return null;
            if (c.LaunchEventEntries == null) return new List<LaunchEvent>(0);

            return c.LaunchEventEntries.OrderBy(e => e.Date).ToList();
        }

        public List<CareerListItem> GetCareerList(string userName = null)
        {
            var res = GetCareerListWithTokens(userName);
            res.ForEach(c => c.Token = null);
            return res;
        }

        public List<CareerListItem> GetCareerListWithTokens(string userName = null)
        {
            var filter = !string.IsNullOrWhiteSpace(userName)
                ? Builders<CareerLog>.Filter.Where(c => c.UserLogin == userName)
                : FilterDefinition<CareerLog>.Empty;
            var p = Builders<CareerLog>.Projection.Expression(c => new CareerListItem(c));
            return _careerLogs.Find(filter).Project(p).ToList();
        }

        public CareerLog Create(CareerLog log)
        {
            var careerLog = new CareerLog
            {
                Token = Guid.NewGuid().ToString("N"),
                Name = log.Name,
                UserLogin = log.UserLogin,
                EligibleForRecords = log.EligibleForRecords,
                CareerLogMeta = log.CareerLogMeta
            };

            if (log.CareerLogEntries != null)
            {
                careerLog.StartDate = log.CareerLogEntries[0].StartDate;
                careerLog.EndDate = log.CareerLogEntries[^1].EndDate;
                careerLog.CareerLogEntries = log.CareerLogEntries;
            }

            careerLog.ContractEventEntries = log.ContractEventEntries;
            careerLog.FacilityEventEntries = log.FacilityEventEntries;
            careerLog.TechEventEntries = log.TechEventEntries;
            careerLog.LaunchEventEntries = log.LaunchEventEntries;

            _careerLogs.InsertOne(careerLog);

            return careerLog;
        }

        public CareerLog Update(string token, CareerLogDto careerLogDto)
        {
            careerLogDto.TrimEmptyPeriod();

            var periods = careerLogDto.Periods.Select(c => new CareerLogPeriod(c)).ToList();
            var contracts = careerLogDto.ContractEvents.Select(c => new ContractEvent(c)).ToList();
            var facilities = careerLogDto.FacilityEvents.Select(f => new FacilityConstructionEvent(f)).ToList();
            var tech = careerLogDto.TechEvents.Select(t => new TechResearchEvent(t)).ToList();
            var launches = careerLogDto.LaunchEvents.Select(l => new LaunchEvent(l)).ToList();

            var updateDef = Builders<CareerLog>.Update
                .Set(nameof(CareerLog.StartDate), periods[0].StartDate)
                .Set(nameof(CareerLog.EndDate), periods[^1].EndDate)
                .Set(nameof(CareerLog.CareerLogEntries), periods)
                .Set(nameof(CareerLog.ContractEventEntries), contracts)
                .Set(nameof(CareerLog.FacilityEventEntries), facilities)
                .Set(nameof(CareerLog.TechEventEntries), tech)
                .Set(nameof(CareerLog.LaunchEventEntries), launches);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

            return _careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.Token == token, updateDef, opts);
        }

        public CareerLog GetByToken(string token)
        {
            return _careerLogs.Find(entry => entry.Token == token).FirstOrDefault();
        }

        public void DeleteByToken(string token)
        {
            Console.WriteLine("delete called: " + token);
            _careerLogs.DeleteOne(entry => entry.Token == token);
        }

        public CareerLog UpdateMetaByToken(string token, CareerLogMeta meta)
        {
            var updateDefinition = Builders<CareerLog>.Update.Set(nameof(CareerLog.CareerLogMeta), meta);
            var opts = new FindOneAndUpdateOptions<CareerLog> {ReturnDocument = ReturnDocument.After};

            return _careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.Token == token, updateDefinition, opts);
        }

        private string ResolveContractName(string name)
        {
            return _contractSettings.ContractNameDict.TryGetValue(name, out string disp) ? disp : name;
        }

        private string ResolveTechNodeName(string id)
        {
            return _techTreeSettings.NodeTitleDict.TryGetValue(id, out string disp) ? disp : id;
        }

        private string GetUserPreferredName(string username)
        {
            return AllUsers.FirstOrDefault(u => u.UserName == username)?.PreferredName ?? username;
        }
    }
}