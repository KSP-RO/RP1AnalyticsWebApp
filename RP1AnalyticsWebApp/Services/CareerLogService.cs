using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData.Query;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RP1AnalyticsWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Services
{
    public class CareerLogService
    {
        private readonly IMongoCollection<CareerLog> _careerLogs;
        private readonly IContractSettings _contractSettings;
        private readonly ITechTreeSettings _techTreeSettings;
        private readonly IProgramSettings _programSettings;
        private readonly ILeaderSettings _leaderSettings;
        private readonly UserManager<WebAppUser> _userManager;
        private readonly CacheService _cache;

        private List<WebAppUser> _allUsers;
        private List<WebAppUser> AllUsers => _allUsers ??= _userManager.Users.ToList();

        public CareerLogService(ICareerLogDatabaseSettings dbSettings, IContractSettings contractSettings,
            ITechTreeSettings techTreeSettings, IProgramSettings programSettings, ILeaderSettings leaderSettings,
            UserManager<WebAppUser> userManager, CacheService cache)
        {
            _contractSettings = contractSettings;
            _techTreeSettings = techTreeSettings;
            _programSettings = programSettings;
            _leaderSettings = leaderSettings;
            _userManager = userManager;
            _cache = cache;

            _careerLogs = CreateCareerLogsMongoClient(dbSettings);
        }

        public static IMongoCollection<CareerLog> CreateCareerLogsMongoClient(ICareerLogDatabaseSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            return database.GetCollection<CareerLog>(dbSettings.CareerLogsCollectionName);
        }

        public async Task<List<CareerLog>> GetAsync(ODataQueryOptions<CareerLog> queryOptions = null)
        {
            List<CareerLog> all = await _cache.FetchAllCareersAsync(_careerLogs);
            if (queryOptions != null)
            {
                var q = all.AsQueryable();
                q = (IQueryable<CareerLog>)queryOptions.ApplyTo(q,
                    new ODataQuerySettings
                    {
                        HandleNullPropagation = HandleNullPropagationOption.False
                    },
                    AllowedQueryOptions.Supported ^ AllowedQueryOptions.Filter);
                return q.ToList();
            }

            return all;
        }

        public async Task<CareerLog> GetAsync(string id)
        {
            return await _careerLogs.Find(entry => entry.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<CareerLogPeriod>> GetCareerPeriodsAsync(string id, ODataQueryOptions<CareerLogPeriod> queryOptions = null)
        {
            var q = _careerLogs.AsQueryable()
                               .Where(c => c.Id == id)
                               .SelectMany(c => c.CareerLogEntries);
            if (queryOptions != null)
            {
                q = (IQueryable<CareerLogPeriod>)queryOptions.ApplyTo(q,
                    new ODataQuerySettings
                    {
                        HandleNullPropagation = HandleNullPropagationOption.False
                    },
                    AllowedQueryOptions.Supported ^ AllowedQueryOptions.Filter);
            }
            return await q.ToListAsync();
        }

        public async Task<CareerLogPeriod> GetCareerPeriodAsync(string careerId, DateTime startDate)
        {
            var q = _careerLogs.AsQueryable()
                               .Where(c => c.Id == careerId)
                               .SelectMany(c => c.CareerLogEntries)
                               .Where(p => p.StartDate == startDate);
            return await q.FirstOrDefaultAsync();
        }

        public async Task<List<BaseContractEvent>> GetContractsForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.ContractEventEntries
                })
                .FirstOrDefaultAsync();

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

        public async Task<List<BaseContractEvent>> GetCompletedMilestonesForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.ContractEventEntries
                })
                .FirstOrDefaultAsync();
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

        public async Task<List<ContractEventWithCount>> GetRepeatableContractCompletionCountsForCareerAsync(string id)
        {
            var result = await _careerLogs.AsQueryable()
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
                .ToListAsync();

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

        public async Task<List<ContractRecord>> GetContractRecordsAsync(ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = (await GetAsync(queryOptions)).AsQueryable();

            var result = q
                .Where(c => c.EligibleForRecords && c.ContractEventEntries != null)
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

        public async Task<List<ProgramRecord>> GetProgramRecordsAsync(ProgramRecordType type, ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = (await GetAsync(queryOptions)).AsQueryable();

            var allPrograms = q
                .Where(c => c.EligibleForRecords && c.Programs != null)
                .SelectMany(c => c.Programs, (c, p) => new ProgramRecord
                {
                    CareerId = c.Id,
                    CareerName = c.Name,
                    UserLogin = c.UserLogin,
                    ProgramName = p.Name,
                    Date = type == ProgramRecordType.Accepted ? p.Accepted :
                               type == ProgramRecordType.ObjectivesCompleted ? p.ObjectivesCompleted : p.Completed
                });

            if (type != ProgramRecordType.Accepted)
            {
                allPrograms = allPrograms.Where(p => p.Date.HasValue);
            }

            var result = allPrograms.OrderBy(p => p.Date)
                .GroupBy(p => p.ProgramName)
                .Select(g => new ProgramRecord
                {
                    ProgramName = g.Key,
                    CareerId = g.First().CareerId,
                    UserLogin = g.First().UserLogin,
                    CareerName = g.First().CareerName,
                    Date = g.Min(p => p.Date)
                })
                .OrderBy(c => c.Date)
                .ToList();

            result.ForEach(r =>
            {
                r.ProgramDisplayName = _programSettings.ProgramNameDict.TryGetValue(r.ProgramName, out string disp) ? disp : r.ProgramName;
                r.UserPreferredName = GetUserPreferredName(r.UserLogin);
            });

            return result;
        }

        public async Task<List<ProgramItemWithCareerInfo>> GetProgramRecordsAsync(ProgramRecordType type, string program, ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = (await GetAsync(queryOptions)).AsQueryable();

            var allPrograms = q
                .Where(c => c.EligibleForRecords && c.Programs != null)
                .SelectMany(c => c.Programs, (c, p) => new ProgramItemWithCareerInfo
                {
                    CareerId = c.Id,
                    CareerName = c.Name,
                    UserLogin = c.UserLogin,
                    Name = p.Name,
                    Accepted = p.Accepted,
                    ObjectivesCompleted = p.ObjectivesCompleted,
                    Completed = p.Completed,
                    Speed = p.Speed,
                    TotalFunding = p.TotalFunding,
                    FundsPaidOut = p.FundsPaidOut,
                    RepPenaltyAssessed = p.RepPenaltyAssessed,
                    NominalDurationYears = p.NominalDurationYears
                })
                .Where(p => p.Name == program);

            if (type == ProgramRecordType.ObjectivesCompleted)
            {
                allPrograms = allPrograms.Where(p => p.ObjectivesCompleted.HasValue);
            }
            else if (type == ProgramRecordType.Completed)
            {
                allPrograms = allPrograms.Where(p => p.Completed.HasValue);
            }

            var result = allPrograms
                .OrderBy(c => type == ProgramRecordType.Accepted ? c.Accepted :
                              type == ProgramRecordType.ObjectivesCompleted ? c.ObjectivesCompleted : c.Completed)
                .ToList();

            result.ForEach(r =>
            {
                r.Title = _programSettings.ProgramNameDict.TryGetValue(r.Name, out string disp) ? disp : r.Name;
                r.UserPreferredName = GetUserPreferredName(r.UserLogin);
            });

            return result;
        }

        public async Task<List<string>> GetRacesAsync()
        {
            var q = _careerLogs.AsQueryable();
            var items = await q.Where(c => c.Race != null)
                               .Select(c => c.Race)
                               .Distinct()
                               .OrderBy(r => r)
                               .ToListAsync();
            return items;
        }

        public async Task<List<ContractEventWithCareerInfo>> GetAllContractEventsAsync(ODataQueryOptions<CareerLog> queryOptions = null)
        {
            var q = _careerLogs.AsQueryable();
            if (queryOptions != null)
            {
                q = (IQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False
                });
            }

            var events = await q.Where(c => c.EligibleForRecords)
                                .SelectMany(c => c.ContractEventEntries, (c, e) => new ContractEventWithCareerInfo
            {
                CareerId = c.Id,
                CareerName = c.Name,
                UserLogin = c.UserLogin,
                ContractInternalName = e.InternalName,
                Date = e.Date,
                Type = e.Type
            })
            .ToListAsync();

            events.ForEach(entry =>
            {
                entry.UserPreferredName = GetUserPreferredName(entry.UserLogin);
                entry.ContractDisplayName = ResolveContractName(entry.ContractInternalName);
            });

            return events;
        }

        public async Task<List<ContractEventWithCareerInfo>> GetEventsForContractAsync(string contract,
            ContractEventType evtType = ContractEventType.Complete, ODataQueryOptions<CareerLog> queryOptions = null)
        {
            string contractDispName = ResolveContractName(contract);

            var q = _careerLogs.AsQueryable();
            if (queryOptions != null)
            {
                q = (IQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.False
                });
            }

            var events = await q.Where(entry => entry.EligibleForRecords &&
                                                entry.ContractEventEntries.Any(ce => ce.InternalName == contract && ce.Type == evtType))
                                .Select(c => new ContractEventWithCareerInfo
                                {
                                    CareerId = c.Id,
                                    CareerName = c.Name,
                                    UserLogin = c.UserLogin,
                                    Date = c.ContractEventEntries.Where(ce => ce.InternalName == contract && ce.Type == evtType)
                                                                 .Min(ce => ce.Date)
                                })
                                .ToListAsync();

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

        public async Task<List<TechEvent>> GetTechUnlocksForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.TechEventEntries
                })
                .FirstOrDefaultAsync();

            if (c == null) return null;
            if (c.TechEventEntries == null) return new List<TechEvent>(0);

            return c.TechEventEntries.Select(e => new TechEvent
            {
                NodeInternalName = e.NodeName,
                NodeDisplayName = ResolveTechNodeName(e.NodeName),
                Date = e.Date,
                YearMult = e.YearMult,
                ResearchRate = e.ResearchRate
            }).OrderBy(ce => ce.Date).ToList();
        }

        public async Task<List<LaunchEvent>> GetLaunchesForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.LaunchEventEntries
                })
                .FirstOrDefaultAsync();

            if (c == null) return null;
            if (c.LaunchEventEntries == null) return new List<LaunchEvent>(0);

            return c.LaunchEventEntries.OrderBy(e => e.Date).ToList();
        }

        public async Task<List<FacilityConstruction>> GetFacilityConstructionsForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.FacilityConstructions
                })
                .FirstOrDefaultAsync();

            if (c == null) return null;
            if (c.FacilityConstructions == null) return new List<FacilityConstruction>(0);

            return c.FacilityConstructions.OrderBy(e => e.Started).ToList();
        }

        public async Task<List<LC>> GetLCsForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.LCs
                })
                .FirstOrDefaultAsync();

            if (c == null) return null;
            if (c.LCs == null) return new List<LC>(0);

            return c.LCs.OrderBy(e => e.ConstrStarted).ToList();
        }

        public async Task<List<ProgramItem>> GetProgramsForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Programs
                })
                .FirstOrDefaultAsync();

            if (c == null) return null;
            if (c.Programs == null) return new List<ProgramItem>(0);

            return c.Programs.Select(p => new ProgramItem
            {
                Name = p.Name,
                Title = _programSettings.ProgramNameDict.TryGetValue(p.Name, out string disp) ? disp : p.Name,
                Accepted = p.Accepted,
                ObjectivesCompleted = p.ObjectivesCompleted,
                Completed = p.Completed,
                NominalDurationYears = p.NominalDurationYears,
                TotalFunding = p.TotalFunding,
                FundsPaidOut = p.FundsPaidOut,
                RepPenaltyAssessed = p.RepPenaltyAssessed,
                Speed = p.Speed
            }).OrderBy(p => p.Accepted).ToList();
        }

        public async Task<List<LeaderItem>> GetLeadersForCareerAsync(string id)
        {
            var c = await _careerLogs.AsQueryable()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Leaders
                })
                .FirstOrDefaultAsync();

            if (c == null) return null;
            if (c.Leaders == null) return new List<LeaderItem>(0);

            return c.Leaders.Select(p => new LeaderItem
            {
                Name = p.Name,
                Title = _leaderSettings.LeaderDict.TryGetValue(p.Name, out LeaderDefinitionItem lItem) ? lItem.Title : p.Name,
                Type = lItem?.Type,
                DateAdd = p.DateAdd,
                DateRemove = p.DateRemove,
                FireCost = p.FireCost
            }).OrderBy(p => p.DateAdd).ToList();
        }

        public async Task<List<CareerListItem>> GetCareerListAsync(string userName = null)
        {
            var res = await GetCareerListWithTokensAsync(userName);
            res.ForEach(c => c.Token = null);
            return res;
        }

        public async Task<List<CareerListItem>> GetCareerListWithTokensAsync(string userName = null)
        {
            var q = _careerLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                q = q.Where(c => c.UserLogin == userName);
            }

            var items = await q.Select(c => new CareerListItem
            {
                Id = c.Id,
                Name = c.Name,
                User = c.UserLogin,
                Token = c.Token,
                Race = c.Race
            }).ToListAsync();

            items.ForEach(item =>
            {
                item.UserPreferredName = GetUserPreferredName(item.User);
            });

            return items;
        }

        public async Task<List<CareerListItem>> GetCareerListAsync(ODataQueryOptions<CareerLog> queryOptions)
        {
            var res = await GetCareerListWithTokensAsync(queryOptions);
            res.ForEach(c => c.Token = null);
            return res;
        }

        public async Task<List<CareerListItem>> GetCareerListWithTokensAsync(ODataQueryOptions<CareerLog> queryOptions)
        {
            var q = _careerLogs.AsQueryable();
            q = (IQueryable<CareerLog>)queryOptions.ApplyTo(q, new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            });

            var items = await q.Select(c => new CareerListItem
            {
                Id = c.Id,
                Name = c.Name,
                User = c.UserLogin,
                Token = c.Token,
                Race = c.Race
            }).ToListAsync();

            items.ForEach(item =>
            {
                item.UserPreferredName = GetUserPreferredName(item.User);
            });

            return items;
        }

        public async Task<CareerLog> CreateAsync(CareerLog log)
        {
            var careerLog = new CareerLog
            {
                Token = Guid.NewGuid().ToString("N"),
                Name = log.Name,
                UserLogin = log.UserLogin,
                EligibleForRecords = log.EligibleForRecords,
                StartDate = log.CareerLogEntries?[0].StartDate ?? Constants.CareerEpoch,
                EndDate = log.CareerLogEntries?[^1].EndDate ?? Constants.CareerEpoch,
                LastUpdate = DateTime.UtcNow,
                CareerLogMeta = log.CareerLogMeta
            };

            if (log.CareerLogEntries != null)
            {
                careerLog.CareerLogEntries = log.CareerLogEntries;
            }

            careerLog.ContractEventEntries = log.ContractEventEntries;
            careerLog.LCs = log.LCs;
            careerLog.FacilityConstructions = log.FacilityConstructions;
            careerLog.TechEventEntries = log.TechEventEntries;
            careerLog.LaunchEventEntries = log.LaunchEventEntries;

            await _careerLogs.InsertOneAsync(careerLog);
            await _cache.InvalidateAsync();

            return careerLog;
        }

        public async Task<CareerLog> UpdateAsync(string token, CareerLogDto careerLogDto)
        {
            careerLogDto.TrimEmptyPeriod();

            CareerLog existingItem = await GetByTokenAsync(token);
            if (existingItem == null) return null;

            var periods = careerLogDto.Periods.Select(c => new CareerLogPeriod(c)).ToList();
            var contracts = careerLogDto.ContractEvents.Select(c => new ContractEvent(c)).ToList();
            var facilityConstructions = careerLogDto.FacilityConstructions.Select(fc => new FacilityConstruction(fc, careerLogDto.FacilityEvents));
            var tech = careerLogDto.TechEvents.Select(t => new TechResearchEvent(t)).ToList();

            var launches = careerLogDto.LaunchEvents.Select(l => new LaunchEvent(l)).ToList();
            AddFailuresToLaunches(careerLogDto.FailureEvents, launches);
            AddExistingMetadataToLaunches(launches, existingItem.LaunchEventEntries);

            var programs = careerLogDto.Programs.Select(p => new Models.Program(p)).ToList();
            List<LC> lcs = ParseLCs(careerLogDto);
            List<Leader> leaders = ParseLeaders(careerLogDto);

            var updateDef = Builders<CareerLog>.Update
                .Set(nameof(CareerLog.StartDate), periods.FirstOrDefault()?.StartDate ?? Constants.CareerEpoch)
                .Set(nameof(CareerLog.EndDate), periods.LastOrDefault()?.EndDate ?? Constants.CareerEpoch)
                .Set(nameof(CareerLog.LastUpdate), DateTime.UtcNow)
                .Set(nameof(CareerLog.CareerLogEntries), periods)
                .Set(nameof(CareerLog.ContractEventEntries), contracts)
                .Set(nameof(CareerLog.Programs), programs)
                .Set(nameof(CareerLog.LCs), lcs)
                .Set(nameof(CareerLog.FacilityConstructions), facilityConstructions)
                .Set(nameof(CareerLog.TechEventEntries), tech)
                .Set(nameof(CareerLog.LaunchEventEntries), launches)
                .Set(nameof(CareerLog.Leaders), leaders);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

            var res = await _careerLogs.FindOneAndUpdateAsync(entry => entry.Token == token, updateDef, opts);
            await _cache.InvalidateAsync();
            return res;
        }

        public async Task<CareerLog> GetByTokenAsync(string token)
        {
            return await _careerLogs.Find(entry => entry.Token == token).FirstOrDefaultAsync();
        }

        public async Task DeleteByTokenAsync(string token)
        {
            await _careerLogs.DeleteOneAsync(entry => entry.Token == token);
            await _cache.InvalidateAsync();
        }

        public async Task<CareerLog> UpdateMetaByTokenAsync(string token, string careerName, CareerLogMeta meta)
        {
            var updateDefinition = Builders<CareerLog>.Update.Set(nameof(CareerLog.CareerLogMeta), meta)
                                                             .Set(nameof(CareerLog.Name), careerName);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

            var res = await _careerLogs.FindOneAndUpdateAsync(entry => entry.Token == token, updateDefinition, opts);
            await _cache.InvalidateAsync();
            return res;
        }

        public async Task<CareerLog> UpdateRaceAsync(string careerId, string race)
        {
            var updateDefinition = Builders<CareerLog>.Update.Set(nameof(CareerLog.Race), race);
            var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };

            var res = await _careerLogs.FindOneAndUpdateAsync(entry => entry.Id == careerId, updateDefinition, opts);
            await _cache.InvalidateAsync();
            return res;
        }

        public async Task UpdateLaunchAsync(string careerId, LaunchEvent launch)
        {
            var filterDef = Builders<CareerLog>.Filter.Where(c => c.Id == careerId && 
                                                                  c.LaunchEventEntries.Any(l => l.LaunchID == launch.LaunchID));
            var updateDef = Builders<CareerLog>.Update.Set(c => c.LaunchEventEntries.FirstMatchingElement(), launch);

            await _careerLogs.FindOneAndUpdateAsync(filterDef, updateDef);
            await _cache.InvalidateAsync();
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

        private static List<LC> ParseLCs(CareerLogDto careerLogDto)
        {
            var lcs = careerLogDto.LCs.Select(l => new LC(l, careerLogDto.FacilityEvents))
                                      .OrderBy(l => l.ConstrEnded)
                                      .ToList();
            // Only one LC in a chain of modifications can be active
            var groups = lcs.GroupBy(l => l.Id);
            foreach (IGrouping<Guid, LC> group in groups)
            {
                LC[] arr = group.Where(l => l.State == LCState.Active).ToArray();
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    var state = i == arr.Length - 1 ? LCState.Active : LCState.Modified;
                    arr[i].State = state;
                }
            }

            return lcs;
        }

        private static List<Leader> ParseLeaders(CareerLogDto careerLogDto)
        {
            List<LeaderEventDto> events = careerLogDto.LeaderEvents;
            if (events == null || events.Count == 0)
                return new List<Leader>(0);

            List<Leader> allLeaders = events.Where(l => l.IsAdd)
                                            .Select(l => new Leader(l))
                                            .ToList();
            foreach (Leader leader in allLeaders)
            {
                var removeEvent = events.Where(l => !l.IsAdd && l.LeaderName == leader.Name && l.Date >= leader.DateAdd)
                                        .OrderBy(l => l.Date)
                                        .FirstOrDefault();
                if (removeEvent  != null)
                {
                    leader.DateRemove = removeEvent.Date;
                    leader.FireCost = removeEvent.Cost;
                }
            }

            return allLeaders;
        }

        private static void AddFailuresToLaunches(List<FailureEventDto> failureDtos, List<LaunchEvent> launches)
        {
            foreach (LaunchEvent launch in launches)
            {
                var failures = failureDtos.Where(f => f.LaunchID == launch.LaunchID)
                                          .Select(f => new FailureEvent(f))
                                          .ToList();
                launch.Failures = failures;
            }
        }

        private static void AddExistingMetadataToLaunches(List<LaunchEvent> newLaunches, List<LaunchEvent> existingLaunches)
        {
            if (existingLaunches == null) return;

            foreach (LaunchEvent eLaunch in existingLaunches.Where(l => l.Metadata != null))
            {
                LaunchEvent nLaunch = newLaunches.FirstOrDefault(l => l.LaunchID == eLaunch.LaunchID);
                if (nLaunch != null)
                {
                    nLaunch.Metadata = eLaunch.Metadata;
                }
            }
        }
    }
}