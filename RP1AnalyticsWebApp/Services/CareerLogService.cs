using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData.Query;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

        public List<CareerLog> Get(ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = _careerLogs.AsQueryable();
            if (queryOptions != null)
            {
                q = (IMongoQueryable<CareerLog>)queryOptions.ApplyTo(q,
                    new ODataQuerySettings
                    {
                        HandleNullPropagation = HandleNullPropagationOption.False
                    },
                    AllowedQueryOptions.Supported ^ AllowedQueryOptions.Filter);
            }
            return q.ToList();
        }

        public CareerLog Get(string id)
        {
            return _careerLogs.Find(entry => entry.Id == id).FirstOrDefault();
        }

        public List<CareerLogPeriod> GetCareerPeriods(string id, ODataQueryOptions<CareerLogPeriod> queryOptions = null)
        {
            var q = _careerLogs.AsQueryable()
                               .Where(c => c.Id == id)
                               .SelectMany(c => c.CareerLogEntries);
            if (queryOptions != null)
            {
                q = (IMongoQueryable<CareerLogPeriod>)queryOptions.ApplyTo(q,
                    new ODataQuerySettings
                    {
                        HandleNullPropagation = HandleNullPropagationOption.False
                    },
                    AllowedQueryOptions.Supported ^ AllowedQueryOptions.Filter);
            }
            return q.ToList();
        }

        public CareerLogPeriod GetCareerPeriod(string careerId, DateTime startDate)
        {
            var q = _careerLogs.AsQueryable()
                               .Where(c => c.Id == careerId)
                               .SelectMany(c => c.CareerLogEntries)
                               .Where(p => p.StartDate == startDate);
            return q.FirstOrDefault();
        }

        public List<BaseContractEvent> GetContractsForCareer(string id)
        {
            var c = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.ContractEventEntries
                })
                .FirstOrDefault();

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
            var c = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.ContractEventEntries
                })
                .FirstOrDefault();
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

        public List<ContractRecord> GetRecords(ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = _careerLogs.AsQueryable();
            if (queryOptions != null)
            {
                q = (IMongoQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False
                });
            }

            var result = q
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
                .Select(g => new ContractRecord
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
            });

            return result;
        }

        public List<ContractEventWithCareerInfo> GetAllContractEvents(ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = _careerLogs.AsQueryable();
            if (queryOptions != null)
            {
                q = (IMongoQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False
                });
            }

            var events = q.SelectMany(c => c.ContractEventEntries, (c, e) => new ContractEventWithCareerInfo
            {
                CareerId = c.Id,
                CareerName = c.Name,
                UserLogin = c.UserLogin,
                ContractInternalName = e.InternalName,
                Date = e.Date,
                Type = e.Type
            })
            .ToList();

            events.ForEach(entry =>
            {
                entry.UserPreferredName = GetUserPreferredName(entry.UserLogin);
                entry.ContractDisplayName = ResolveContractName(entry.ContractInternalName);
            });

            return events;
        }

        public List<ContractEventWithCareerInfo> GetEventsForContract(string contract,
            ContractEventType evtType = ContractEventType.Complete, ODataQueryOptions<CareerLog> queryOptions = null)
        {
            string contractDispName = ResolveContractName(contract);

            var q = _careerLogs.AsQueryable();
            if (queryOptions != null)
            {
                q = (IMongoQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False
                });
            }

            var events = q.Where(entry => entry.ContractEventEntries.Any(ce => ce.InternalName == contract && ce.Type == evtType))
                          .Select(c => new ContractEventWithCareerInfo
                          {
                              CareerId = c.Id,
                              CareerName = c.Name,
                              UserLogin = c.UserLogin,
                              Date = c.ContractEventEntries.Where(ce => ce.InternalName == contract && ce.Type == evtType)
                                                           .Min(ce => ce.Date)
                          })
                          .ToList();

            events.ForEach(entry =>
            {
                entry.Type = evtType;
                entry.ContractInternalName = contract;
                entry.UserPreferredName = GetUserPreferredName(entry.UserLogin);
                entry.ContractDisplayName = contractDispName;
            });

            events.Sort((a, b) => a.Date.Value.CompareTo(b.Date.Value));

            return events;
        }

        public List<TechEvent> GetTechUnlocksForCareer(string id)
        {
            var c = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.TechEventEntries
                })
                .FirstOrDefault();

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
            var c = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.LaunchEventEntries
                })
                .FirstOrDefault();

            if (c == null) return null;
            if (c.LaunchEventEntries == null) return new List<LaunchEvent>(0);

            return c.LaunchEventEntries.OrderBy(e => e.Date).ToList();
        }

        public List<FacilityConstructionEvent> GetFacilityConstructionsForCareer(string id)
        {
            var c = _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.FacilityEventEntries
                })
                .FirstOrDefault();

            if (c == null) return null;
            if (c.FacilityEventEntries == null) return new List<FacilityConstructionEvent>(0);

            return c.FacilityEventEntries.OrderBy(e => e.Date).ToList();
        }

        public List<CareerListItem> GetCareerList(string userName = null)
        {
            var res = GetCareerListWithTokens(userName);
            res.ForEach(c => c.Token = null);
            return res;
        }

        public List<CareerListItem> GetCareerListWithTokens(string userName = null)
        {
            var q = _careerLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                q = q.Where(c => c.UserLogin == userName);
            }

            var items = q.Select(c => new CareerListItem
            {
                Id = c.Id,
                Name = c.Name,
                User = c.UserLogin,
                Token = c.Token
            }).ToList();

            items.ForEach(item =>
            {
                item.UserPreferredName = GetUserPreferredName(item.User);
            });

            return items;
        }

        public List<CareerListItem> GetCareerList(ODataQueryOptions<CareerLog> queryOptions)
        {
            var res = GetCareerListWithTokens(queryOptions);
            res.ForEach(c => c.Token = null);
            return res;
        }

        public List<CareerListItem> GetCareerListWithTokens(ODataQueryOptions<CareerLog> queryOptions)
        {
            var q = _careerLogs.AsQueryable();
            q = (IMongoQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            });

            var items = q.Select(c => new CareerListItem
            {
                Id = c.Id,
                Name = c.Name,
                User = c.UserLogin,
                Token = c.Token
            }).ToList();

            items.ForEach(item =>
            {
                item.UserPreferredName = GetUserPreferredName(item.User);
            });

            return items;
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
                .Set(nameof(CareerLog.StartDate), periods.FirstOrDefault()?.StartDate ?? Constants.CareerEpoch)
                .Set(nameof(CareerLog.EndDate), periods.LastOrDefault()?.EndDate ?? Constants.CareerEpoch)
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
            _careerLogs.DeleteOne(entry => entry.Token == token);
        }

        public CareerLog UpdateMetaByToken(string token, string careerName, CareerLogMeta meta)
        {
            var updateDefinition = Builders<CareerLog>.Update.Set(nameof(CareerLog.CareerLogMeta), meta)
                                                             .Set(nameof(CareerLog.Name), careerName);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

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