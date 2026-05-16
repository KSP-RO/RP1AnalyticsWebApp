using RP1AnalyticsWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Services
{
    public class CareerComparisonService
    {
        private static readonly List<string[]> MutuallyExclusiveProgramGroups = new()
        {
            new[] { "EarlySatellites", "EarlySatellites-Heavy" }
        };

        private static readonly Dictionary<string, string[]> ProgramMilestoneContracts = new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "EarlySatellites",
                new[] { "FirstSatellite", "FirstScienceSat", "AtmoSat", "FirstPolarSat", "FirstSolarSat" }
            },
            {
                "EarlySatellites-Heavy",
                new[] { "FirstSatellite-Heavy", "FirstBioSat-Heavy", "FirstScienceSat-Heavy", "FirstPolarSat-Heavy" }
            }
        };

        private readonly CareerLogService _careerLogService;
        private readonly IContractSettings _contractSettings;
        private readonly ITechTreeSettings _techTreeSettings;
        private readonly IProgramSettings _programSettings;
        private readonly HistoricalBenchmarkService _historicalBenchmarkService;

        public CareerComparisonService(CareerLogService careerLogService, IContractSettings contractSettings,
            ITechTreeSettings techTreeSettings, IProgramSettings programSettings,
            HistoricalBenchmarkService historicalBenchmarkService)
        {
            _careerLogService = careerLogService;
            _contractSettings = contractSettings;
            _techTreeSettings = techTreeSettings;
            _programSettings = programSettings;
            _historicalBenchmarkService = historicalBenchmarkService;
        }

        public async Task<CareerComparison> GetComparisonAsync(string careerId,
            List<string> players = null, List<string> races = null,
            ComparisonEndDateMode careerDateMode = ComparisonEndDateMode.All,
            DateTime? careerDateStart = null, DateTime? careerDateEnd = null,
            ComparisonEndDateMode lastUpdateMode = ComparisonEndDateMode.All,
            DateTime? lastUpdateStart = null, DateTime? lastUpdateEnd = null,
            List<string> rp1Versions = null, List<string> difficulties = null, List<string> playstyles = null,
            string recordEligibility = "All")
        {
            var careers = await _careerLogService.GetAsync();
            var target = careers.FirstOrDefault(c => c.Id == careerId);
            if (target == null)
            {
                return null;
            }

            var cohort = ApplyGlobalFilters(careers, players, races, careerDateMode, careerDateStart, careerDateEnd,
                lastUpdateMode, lastUpdateStart, lastUpdateEnd, rp1Versions, difficulties, playstyles, recordEligibility);
            var benchmarks = _historicalBenchmarkService.GetHistoricalBenchmarks();
            var milestones = CreateMilestoneComparisons(target, cohort, benchmarks);

            return new CareerComparison
            {
                CareerId = target.Id,
                CareerName = target.Name,
                Cohort = CreateCohortSummary(target, cohort, players, races, careerDateMode, careerDateStart, careerDateEnd,
                    lastUpdateMode, lastUpdateStart, lastUpdateEnd, rp1Versions, difficulties, playstyles, recordEligibility),
                Metrics = CreateMetricComparisons(target, cohort),
                Milestones = milestones,
                Timeline = CreateTimeline(target).OrderBy(e => e.Date).ToList(),
                HistoricalBenchmarks = benchmarks,
                Charts = CreateCharts(target, cohort)
            };
        }

        public async Task<List<CareerTimelineEvent>> GetTimelineAsync(string careerId)
        {
            var career = await _careerLogService.GetAsync(careerId);
            return career == null ? null : CreateTimeline(career).OrderBy(e => e.Date).ToList();
        }

        public async Task<RecordsSnapshot> GetRecordsSnapshotAsync(List<string> players, List<string> races,
            ComparisonEndDateMode careerDateMode, DateTime? careerDateStart, DateTime? careerDateEnd,
            ComparisonEndDateMode lastUpdateMode, DateTime? lastUpdateStart, DateTime? lastUpdateEnd,
            List<string> rp1Versions, List<string> difficulties, List<string> playstyles,
            string recordEligibility, ProgramRecordType programType)
        {
            var careers = await _careerLogService.GetAsync();
            var cohort = ApplyGlobalFilters(careers, players, races, careerDateMode, careerDateStart, careerDateEnd,
                lastUpdateMode, lastUpdateStart, lastUpdateEnd, rp1Versions, difficulties, playstyles, recordEligibility)
                .Where(c => c.EligibleForRecords)
                .ToList();

            return new RecordsSnapshot
            {
                Filters = new RecordFilterSummary
                {
                    Rp1Version = rp1Versions?.Count == 1 ? rp1Versions[0] : null,
                    Difficulty = difficulties?.Count == 1 ? difficulties[0] : null,
                    Playstyle = playstyles?.Count == 1 ? playstyles[0] : null,
                    CareerCount = cohort.Count,
                    EligibleOnly = true
                },
                ContractRecords = CreateContractRecords(cohort),
                ProgramRecords = CreateProgramRecords(cohort, programType)
            };
        }

        public async Task<CareerOverviewSnapshot> GetCareerOverviewAsync(List<string> players = null, List<string> races = null,
            ComparisonEndDateMode careerDateMode = ComparisonEndDateMode.All, DateTime? careerDateStart = null, DateTime? careerDateEnd = null,
            ComparisonEndDateMode lastUpdateMode = ComparisonEndDateMode.All, DateTime? lastUpdateStart = null, DateTime? lastUpdateEnd = null,
            List<string> rp1Versions = null, List<string> difficulties = null, List<string> playstyles = null,
            string recordEligibility = "All")
        {
            var careers = await _careerLogService.GetAsync();
            var filteredCareers = ApplyGlobalFilters(careers, players, races, careerDateMode, careerDateStart, careerDateEnd,
                lastUpdateMode, lastUpdateStart, lastUpdateEnd, rp1Versions, difficulties, playstyles, recordEligibility);
            var eligibleCareers = filteredCareers.Where(c => c.EligibleForRecords).ToList();
            var recordCounts = CreateRecordSummaries(eligibleCareers);

            var items = filteredCareers.Select(c => CreateOverviewItem(c, recordCounts))
                .OrderByDescending(c => c.LastUpdate)
                .ThenBy(c => c.Name)
                .ToList();

            return new CareerOverviewSnapshot
            {
                Summary = new CareerOverviewSummary
                {
                    TotalCareers = items.Count,
                    RecordsEligibleCareers = items.Count(c => c.EligibleForRecords),
                    TotalLaunches = items.Sum(c => c.LaunchCount),
                    TotalCompletedContracts = items.Sum(c => c.CompletedContractCount),
                    TotalCompletedMilestones = items.Sum(c => c.CompletedMilestoneCount),
                    TotalCompletedPrograms = items.Sum(c => c.CompletedProgramCount),
                    TotalRecordsSet = items.Sum(c => c.RecordCount),
                    LatestSaveDate = items.Count > 0 ? items.Max(c => c.EndDate) : null,
                    LatestUpdate = items.Count > 0 ? items.Max(c => c.LastUpdate) : null
                },
                VersionBreakdown = CreateBreakdown(items, c => c.Rp1Version),
                DifficultyBreakdown = CreateBreakdown(items, c => c.DifficultyLevel),
                PlaystyleBreakdown = CreateBreakdown(items, c => c.CareerPlaystyle),
                RecentCareers = items.Take(10).ToList(),
                RecordLeaders = CreateRecordLeaders(items, recordCounts),
                Careers = items
            };
        }

        private static CohortSummary CreateCohortSummary(CareerLog target, List<CareerLog> cohort,
            List<string> players, List<string> races, ComparisonEndDateMode careerDateMode,
            DateTime? careerDateStart, DateTime? careerDateEnd,
            ComparisonEndDateMode lastUpdateMode, DateTime? lastUpdateStart, DateTime? lastUpdateEnd,
            List<string> rp1Versions, List<string> difficulties, List<string> playstyles, string recordEligibility)
        {
            var normalizedStart = NormalizeStartDate(careerDateMode, careerDateStart, careerDateEnd);
            var normalizedEnd = NormalizeEndDate(careerDateMode, careerDateStart, careerDateEnd);
            var normalizedLastUpdateStart = NormalizeStartDate(lastUpdateMode, lastUpdateStart, lastUpdateEnd);
            var normalizedLastUpdateEnd = NormalizeEndDate(lastUpdateMode, lastUpdateStart, lastUpdateEnd);

            return new CohortSummary
            {
                CareerCount = cohort.Count,
                Rp1Version = SingleFilterValue(rp1Versions),
                VersionSort = null,
                Difficulty = SingleFilterValue(difficulties),
                Playstyle = SingleFilterValue(playstyles),
                EligibleOnly = string.Equals(recordEligibility, "Eligible", StringComparison.OrdinalIgnoreCase),
                Scope = "GlobalFilters",
                Description = GlobalFilterDescription(players, races, careerDateMode, normalizedStart, normalizedEnd,
                    lastUpdateMode, normalizedLastUpdateStart, normalizedLastUpdateEnd, rp1Versions, difficulties,
                    playstyles, recordEligibility),
                EndDateFilter = new ComparisonEndDateFilter
                {
                    Mode = careerDateMode.ToString(),
                    TargetEndDate = target.EndDate,
                    StartDate = normalizedStart,
                    EndDate = normalizedEnd,
                    Description = EndDateDescription(careerDateMode, normalizedStart, normalizedEnd)
                }
            };
        }

        private static bool MatchesDateMode(DateTime candidate, ComparisonEndDateMode mode, DateTime? startDate, DateTime? endDate)
        {
            var start = NormalizeStartDate(mode, startDate, endDate);
            var end = NormalizeEndDate(mode, startDate, endDate);
            var candidateDate = candidate.Date;

            return mode switch
            {
                ComparisonEndDateMode.Range => (!start.HasValue || candidateDate >= start.Value.Date) &&
                                               (!end.HasValue || candidateDate <= end.Value.Date),
                ComparisonEndDateMode.Before => !end.HasValue || candidateDate <= end.Value.Date,
                ComparisonEndDateMode.After => !start.HasValue || candidateDate >= start.Value.Date,
                _ => true
            };
        }

        private static DateTime? NormalizeStartDate(ComparisonEndDateMode mode, DateTime? startDate, DateTime? endDate)
        {
            return mode switch
            {
                ComparisonEndDateMode.Range => startDate?.Date,
                ComparisonEndDateMode.After => (startDate ?? endDate)?.Date,
                _ => null
            };
        }

        private static DateTime? NormalizeEndDate(ComparisonEndDateMode mode, DateTime? startDate, DateTime? endDate)
        {
            return mode switch
            {
                ComparisonEndDateMode.Range => endDate?.Date,
                ComparisonEndDateMode.Before => (endDate ?? startDate)?.Date,
                _ => null
            };
        }

        private static string EndDateDescription(ComparisonEndDateMode mode, DateTime? startDate, DateTime? endDate)
        {
            return mode switch
            {
                ComparisonEndDateMode.Range when startDate.HasValue && endDate.HasValue =>
                    $"Career end date from {startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.Range when startDate.HasValue =>
                    $"Career end date on or after {startDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.Range when endDate.HasValue =>
                    $"Career end date on or before {endDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.Before when endDate.HasValue =>
                    $"Career end date on or before {endDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.After when startDate.HasValue =>
                    $"Career end date on or after {startDate.Value:yyyy-MM-dd}",
                _ => "All career end dates"
            };
        }

        private static string GlobalFilterDescription(List<string> players, List<string> races,
            ComparisonEndDateMode careerDateMode, DateTime? careerDateStart, DateTime? careerDateEnd,
            ComparisonEndDateMode lastUpdateMode, DateTime? lastUpdateStart, DateTime? lastUpdateEnd,
            List<string> rp1Versions, List<string> difficulties, List<string> playstyles, string recordEligibility)
        {
            var parts = new List<string>();
            AddFilterList(parts, "Player", players);
            AddFilterList(parts, "Race", races);
            AddFilterList(parts, "RP-1", rp1Versions);
            AddFilterList(parts, "Difficulty", difficulties);
            AddFilterList(parts, "Playstyle", playstyles);

            string careerDateDescription = DateFilterDescription("Career date", careerDateMode, careerDateStart, careerDateEnd);
            if (careerDateDescription != null)
            {
                parts.Add(careerDateDescription);
            }

            string lastUpdateDescription = DateFilterDescription("Last update", lastUpdateMode, lastUpdateStart, lastUpdateEnd);
            if (lastUpdateDescription != null)
            {
                parts.Add(lastUpdateDescription);
            }

            if (string.Equals(recordEligibility, "Eligible", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("Record eligibility: eligible only");
            }
            else if (string.Equals(recordEligibility, "Ineligible", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("Record eligibility: ineligible only");
            }

            return parts.Count == 0 ? "All careers" : string.Join("; ", parts);
        }

        private List<MetricComparison> CreateMetricComparisons(CareerLog target, List<CareerLog> cohort)
        {
            return new List<MetricComparison>
            {
                Metric("milestones", "Milestones completed", "Progression", "count", target, cohort, c => CountCompletedMilestones(c), true,
                    "Completed RP-1 milestone contracts."),
                Metric("programsCompleted", "Programs completed", "Progression", "count", target, cohort, c => CountCompletedPrograms(c), true,
                    "Programs with completion dates."),
                Metric("techUnlocks", "Tech unlocks", "Research", "count", target, cohort, c => c.TechEventEntries?.Count ?? 0, true,
                    "Completed research nodes."),
                Metric("scienceEarned", "Science earned", "Research", "science", target, cohort, c => Last(c)?.ScienceEarned, true,
                    "Total science earned as reported by RP-1."),
                Metric("currentReputation", "Current reputation", "Reputation", "score", target, cohort, c => Last(c)?.Reputation, true,
                    "Reputation at the latest uploaded period."),
                Metric("totalSpending", "Total spending", "Economy", "funds", target, cohort, TotalSpending, true,
                    "Recorded spending across launches, construction, salaries, maintenance, tooling, and other fees."),
                Metric("maintenanceBurden", "Maintenance burden", "Economy", "funds", target, cohort, c => SumPeriods(c, p => p.MaintenanceFees), true,
                    "Maintenance fees after subsidies."),
                Metric("launches", "Launches", "Operations", "count", target, cohort, c => c.LaunchEventEntries?.Count ?? 0, true,
                    "Uploaded launch events."),
                Metric("testFlightFailures", "Recorded failures", "Operations", "count", target, cohort,
                    c => c.LaunchEventEntries?.Sum(l => l.Failures?.Count ?? 0) ?? 0, false,
                    "TestFlight failures attached to launches."),
                Metric("successRate", "Tagged success rate", "Operations", "percent", target, cohort, TaggedSuccessRate, true,
                    "Manual success/failure tags on launches."),
                Metric("launchComplexes", "Launch complexes", "Infrastructure", "count", target, cohort, c => c.LCs?.Count ?? 0, true,
                    "Launch complex builds and modifications."),
                Metric("facilities", "Facility upgrades", "Infrastructure", "count", target, cohort, c => c.FacilityConstructions?.Count ?? 0, true,
                    "KSC facility construction records."),
                Metric("engineers", "Engineers", "Workforce", "count", target, cohort, c => Last(c)?.NumEngineers, true,
                    "Engineers at the latest uploaded period."),
                Metric("researchers", "Researchers", "Workforce", "count", target, cohort, c => Last(c)?.NumResearchers, true,
                    "Researchers at the latest uploaded period."),
                Metric("engineerEfficiency", "Engineer efficiency", "Workforce", "percent", target, cohort, c => Last(c)?.EfficiencyEngineers * 100d, true,
                    "Engineer efficiency at the latest uploaded period."),
                Metric("confidence", "Confidence earned", "Confidence", "score", target, cohort, c => SumPeriods(c, p => p.Confidence), true,
                    "Cumulative confidence earned across uploaded periods.")
            };
        }

        private MetricComparison Metric(string key, string label, string category, string unit, CareerLog target, List<CareerLog> cohort,
            Func<CareerLog, double?> selector, bool higherRanksFirst, string description)
        {
            double? targetValue = selector(target);
            var values = cohort.Select(selector).Where(v => v.HasValue).Select(v => v.Value).OrderBy(v => v).ToList();
            return new MetricComparison
            {
                Key = key,
                Label = label,
                Category = category,
                Unit = unit,
                TargetValue = targetValue,
                CohortMedian = Median(values),
                CohortP25 = PercentileValue(values, 0.25),
                CohortP75 = PercentileValue(values, 0.75),
                Rank = targetValue.HasValue ? Rank(values, targetValue.Value, higherRanksFirst) : null,
                CohortCount = values.Count,
                Percentile = targetValue.HasValue && values.Count > 0 ? PercentilePosition(values, targetValue.Value) : null,
                Description = description
            };
        }

        private List<MilestoneComparison> CreateMilestoneComparisons(CareerLog target, List<CareerLog> cohort, List<HistoricalBenchmark> benchmarks)
        {
            var result = new List<MilestoneComparison>();
            var benchmarkMap = benchmarks.ToDictionary(b => $"{b.Kind}:{b.Key}", StringComparer.OrdinalIgnoreCase);

            var cohortContractDates = cohort.ToDictionary(
                c => c,
                c => c.ContractEventEntries?
                       .Where(e => e.Type == ContractEventType.Complete)
                       .GroupBy(e => e.InternalName, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(g => g.Key, g => (DateTime?)g.Min(e => e.Date), StringComparer.OrdinalIgnoreCase)
                    ?? new Dictionary<string, DateTime?>(StringComparer.OrdinalIgnoreCase));

            var cohortProgramDates = cohort.ToDictionary(
                c => c,
                c => c.Programs?
                       .Where(p => p.Completed.HasValue)
                       .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(g => g.Key, g => g.Min(p => p.Completed), StringComparer.OrdinalIgnoreCase)
                    ?? new Dictionary<string, DateTime?>(StringComparer.OrdinalIgnoreCase));

            foreach (string contract in _contractSettings.MilestoneContractNames)
            {
                var targetDate = GetContractCompletionDate(target, contract);
                bool isExcludedByTargetProgramChoice = IsContractExcludedByTargetProgramChoice(target, contract, out string exclusionReason);
                var completions = cohort.Select(c => new
                    {
                        Career = c,
                        Date = cohortContractDates[c].GetValueOrDefault(contract)
                    })
                    .Where(e => e.Date != default)
                    .OrderBy(e => e.Date)
                    .ToList();
                var days = completions.Select(e => DaysSinceEpoch(e.Date.Value)).ToList();
                var record = completions.FirstOrDefault();

                result.Add(DateMilestone(contract, "Contract", ResolveContractName(contract), targetDate,
                    target.EndDate, cohort.Count, days,
                    CreateMilestoneRecordInfo(record?.Career, record?.Date),
                    benchmarkMap.TryGetValue($"Contract:{contract}", out var bm) ? bm : null,
                    isExcludedByTargetProgramChoice, exclusionReason));
            }

            foreach (string program in _programSettings.ProgramNameDict.Keys.Where(program => !IsDeprecatedProgram(program)))
            {
                var targetDate = GetProgramCompletionDate(target, program);
                bool isExcludedByTargetProgramChoice = IsProgramExcludedByTargetChoice(target, program, out string exclusionReason);
                var completions = cohort.Select(c => new
                    {
                        Career = c,
                        Date = cohortProgramDates[c].GetValueOrDefault(program)
                    })
                    .Where(e => e.Date != default)
                    .OrderBy(e => e.Date)
                    .ToList();
                var days = completions.Select(e => DaysSinceEpoch(e.Date.Value)).ToList();
                var record = completions.FirstOrDefault();

                result.Add(DateMilestone(program, "Program", ResolveProgramName(program), targetDate,
                    target.EndDate, cohort.Count, days,
                    CreateMilestoneRecordInfo(record?.Career, record?.Date),
                    benchmarkMap.TryGetValue($"Program:{program}", out var bm) ? bm : null,
                    isExcludedByTargetProgramChoice, exclusionReason));
            }

            return result.OrderBy(m => m.TargetDate ?? m.CohortMedianDate ?? m.HistoricalBenchmark?.Date).ToList();
        }

        private MilestoneRecordInfo CreateMilestoneRecordInfo(CareerLog career, DateTime? date)
        {
            if (career == null || !date.HasValue)
            {
                return null;
            }

            return new MilestoneRecordInfo
            {
                Date = date.Value,
                CareerId = career.Id,
                CareerName = career.Name,
                UserLogin = career.UserLogin,
                UserPreferredName = _careerLogService.GetUserPreferredName(career.UserLogin)
            };
        }

        private static MilestoneComparison DateMilestone(string key, string kind, string label, DateTime? targetDate,
            DateTime targetEndDate, int cohortCareerCount, List<double> cohortDays, MilestoneRecordInfo record,
            HistoricalBenchmark benchmark,
            bool isExcludedByTargetProgramChoice = false, string exclusionReason = null)
        {
            double? targetDays = targetDate.HasValue ? DaysSinceEpoch(targetDate.Value) : null;
            double? medianDays = Median(cohortDays);
            double targetEndDays = DaysSinceEpoch(targetEndDate);
            int completedByTargetEndDate = cohortDays.Count(d => d <= targetEndDays);
            return new MilestoneComparison
            {
                Key = key,
                Kind = kind,
                Label = label,
                TargetDate = targetDate,
                CohortMedianDate = DateFromDays(medianDays),
                CohortP25Date = DateFromDays(PercentileValue(cohortDays, 0.25)),
                CohortP75Date = DateFromDays(PercentileValue(cohortDays, 0.75)),
                CohortRecordDate = record?.Date,
                CohortRecordCareerId = record?.CareerId,
                CohortRecordCareerName = record?.CareerName,
                CohortRecordUserLogin = record?.UserLogin,
                CohortRecordUserPreferredName = record?.UserPreferredName,
                Rank = targetDays.HasValue ? Rank(cohortDays, targetDays.Value, false) : null,
                CohortCount = cohortDays.Count,
                CohortCareerCount = cohortCareerCount,
                CohortCompletedByTargetDateCount = completedByTargetEndDate,
                CohortCompletedByTargetDatePercent = cohortCareerCount > 0
                    ? Math.Round(completedByTargetEndDate * 100d / cohortCareerCount, 1)
                    : 0,
                DaysFromCohortMedian = targetDays.HasValue && medianDays.HasValue
                    ? (int)Math.Round(targetDays.Value - medianDays.Value)
                    : null,
                DaysFromHistoricalBenchmark = targetDate.HasValue && benchmark != null
                    ? (int)Math.Round(DaysSinceEpoch(targetDate.Value) - DaysSinceEpoch(benchmark.Date))
                    : null,
                IsExcludedByTargetProgramChoice = isExcludedByTargetProgramChoice,
                ExclusionReason = exclusionReason,
                Percentile = targetDays.HasValue && cohortDays.Count > 0
                    ? Math.Round(cohortDays.Count(d => d >= targetDays.Value) * 100d / cohortDays.Count, 1)
                    : null,
                HistoricalBenchmark = benchmark
            };
        }

        private class MilestoneRecordInfo
        {
            public DateTime Date { get; set; }
            public string CareerId { get; set; }
            public string CareerName { get; set; }
            public string UserLogin { get; set; }
            public string UserPreferredName { get; set; }
        }

        private ComparisonCharts CreateCharts(CareerLog target, List<CareerLog> cohort)
        {
            return new ComparisonCharts
            {
                ProgressSeries = CreateProgressSeries(target, cohort),
                LaunchCadence = CreateLaunchCadence(target, cohort),
                ProgramPaths = CreateProgramPaths(target, cohort),
                InfrastructureSeries = CreateInfrastructureSeries(target, cohort),
                InfrastructureEvents = CreateInfrastructureEvents(target),
                EconomySeries = CreateEconomySeries(target, cohort)
            };
        }

        private List<ComparisonBandSeries> CreateProgressSeries(CareerLog target, List<CareerLog> cohort)
        {
            return new List<ComparisonBandSeries>
            {
                BandSeriesFromFactory("milestones", "Milestones", "count", target, cohort, BuildMilestoneSelector),
                BandSeriesFromFactory("programs", "Programs", "count", target, cohort, BuildProgramsSelector),
                BandSeriesFromFactory("techUnlocks", "Tech Unlocks", "count", target, cohort, BuildTechUnlocksSelector),
                BandSeriesFromFactory("launches", "Launches", "count", target, cohort, BuildLaunchesSelector)
            };
        }

        private List<ComparisonBandSeries> CreateInfrastructureSeries(CareerLog target, List<CareerLog> cohort)
        {
            return new List<ComparisonBandSeries>
            {
                BandSeries("infrastructureEvents", "Completed Builds", "count", target, cohort, CountInfrastructureCompletionsByDate),
                BandSeries("maxLaunchMass", "Max LC Mass", "mass", target, cohort, MaxLaunchComplexMassByDate)
            };
        }

        private List<ComparisonBandSeries> CreateEconomySeries(CareerLog target, List<CareerLog> cohort)
        {
            return new List<ComparisonBandSeries>
            {
                BandSeries("totalSpending", "Total Spending", "funds", target, cohort, TotalSpendingByDate),
                BandSeries("confidence", "Confidence Earned", "score", target, cohort, ConfidenceEarnedByDate)
            };
        }

        private ComparisonBandSeries BandSeries(string key, string label, string unit, CareerLog target, List<CareerLog> cohort,
            Func<CareerLog, DateTime, double?> selector)
        {
            return new ComparisonBandSeries
            {
                Key = key,
                Label = label,
                Unit = unit,
                Points = ChartDates(target).Select(date => BandPoint(date, target, cohort, selector)).ToList()
            };
        }

        private static ComparisonBandPoint BandPoint(DateTime date, CareerLog target, List<CareerLog> cohort,
            Func<CareerLog, DateTime, double?> selector)
        {
            var values = cohort
                .Where(c => c.StartDate.Date <= date.Date && c.EndDate.Date >= date.Date)
                .Select(c => selector(c, date))
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .OrderBy(v => v)
                .ToList();

            return new ComparisonBandPoint
            {
                Date = date.Date,
                TargetValue = target.EndDate.Date >= date.Date ? selector(target, date) : null,
                Median = Median(values),
                P25 = PercentileValue(values, 0.25),
                P75 = PercentileValue(values, 0.75),
                ComparisonCount = values.Count
            };
        }

        private static ComparisonBandSeries BandSeriesFromFactory(string key, string label, string unit,
            CareerLog target, List<CareerLog> cohort, Func<CareerLog, Func<DateTime, double?>> selectorFactory)
        {
            var targetSelector = selectorFactory(target);
            var cohortSelectors = cohort.Select(c => (career: c, selector: selectorFactory(c))).ToList();
            var dates = ChartDates(target);

            return new ComparisonBandSeries
            {
                Key = key,
                Label = label,
                Unit = unit,
                Points = dates.Select(date => BandPointPrecomputed(date, target, targetSelector, cohortSelectors)).ToList()
            };
        }

        private static ComparisonBandPoint BandPointPrecomputed(DateTime date, CareerLog target,
            Func<DateTime, double?> targetSelector,
            List<(CareerLog career, Func<DateTime, double?> selector)> cohortSelectors)
        {
            var values = cohortSelectors
                .Where(cs => cs.career.StartDate.Date <= date.Date && cs.career.EndDate.Date >= date.Date)
                .Select(cs => cs.selector(date))
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .OrderBy(v => v)
                .ToList();

            return new ComparisonBandPoint
            {
                Date = date.Date,
                TargetValue = target.EndDate.Date >= date.Date ? targetSelector(date) : null,
                Median = Median(values),
                P25 = PercentileValue(values, 0.25),
                P75 = PercentileValue(values, 0.75),
                ComparisonCount = values.Count
            };
        }

        private static List<DateTime> ChartDates(CareerLog target)
        {
            var end = target.EndDate.Date;
            var dates = new List<DateTime> { Constants.CareerEpoch.Date };

            for (int year = Constants.CareerEpoch.Year + 1; year <= end.Year; year++)
            {
                dates.Add(new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            }

            if (dates[^1] != end)
            {
                dates.Add(DateTime.SpecifyKind(end, DateTimeKind.Utc));
            }

            return dates.Distinct().OrderBy(d => d).ToList();
        }

        private List<LaunchCadencePoint> CreateLaunchCadence(CareerLog target, List<CareerLog> cohort)
        {
            var points = new List<LaunchCadencePoint>();
            for (int year = Constants.CareerEpoch.Year; year <= target.EndDate.Year; year++)
            {
                var targetLaunches = LaunchesInYear(target, year);
                var values = cohort
                    .Where(c => c.StartDate.Year <= year && c.EndDate.Year >= year)
                    .Select(c => (double)LaunchesInYear(c, year).Count)
                    .OrderBy(v => v)
                    .ToList();

                points.Add(new LaunchCadencePoint
                {
                    Year = year,
                    TargetSuccesses = targetLaunches.Count(l => l.Metadata?.Success == true),
                    TargetFailures = targetLaunches.Count(l => l.Metadata?.Success == false),
                    TargetPartialFailures = targetLaunches.Count(l => l.Metadata?.Success == null && (l.Failures?.Count ?? 0) > 0),
                    TargetUntagged = targetLaunches.Count(l => l.Metadata?.Success == null && (l.Failures?.Count ?? 0) == 0),
                    MedianLaunches = Median(values),
                    P25Launches = PercentileValue(values, 0.25),
                    P75Launches = PercentileValue(values, 0.75),
                    ComparisonCount = values.Count
                });
            }

            return points;
        }

        private List<ProgramPathChoice> CreateProgramPaths(CareerLog target, List<CareerLog> cohort)
        {
            int comparisonCount = cohort.Count;
            return _programSettings.ProgramNameDict.Keys
                .Select(program =>
                {
                    var selectedCount = cohort.Count(c => HasProgramEntry(c, program));
                    var completedCount = cohort.Count(c => GetProgramCompletionDate(c, program).HasValue);
                    var exclusiveGroup = ExclusiveGroupForProgram(program);
                    return new ProgramPathChoice
                    {
                        Key = program,
                        Label = ResolveProgramName(program),
                        TargetAcceptedDate = GetProgramAcceptedDate(target, program),
                        TargetCompletedDate = GetProgramCompletionDate(target, program),
                        TargetSelected = HasProgramEntry(target, program),
                        TargetCompleted = GetProgramCompletionDate(target, program).HasValue,
                        SelectedCount = selectedCount,
                        CompletedCount = completedCount,
                        ComparisonCount = comparisonCount,
                        SelectedPercent = comparisonCount > 0 ? Math.Round(selectedCount * 100d / comparisonCount, 1) : 0,
                        CompletedPercent = comparisonCount > 0 ? Math.Round(completedCount * 100d / comparisonCount, 1) : 0,
                        ExclusiveGroup = exclusiveGroup != null ? string.Join("|", exclusiveGroup) : null,
                        MutuallyExclusiveKeys = exclusiveGroup?.Where(p => !string.Equals(p, program, StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<string>()
                    };
                })
                .OrderByDescending(p => p.TargetSelected)
                .ThenByDescending(p => p.CompletedPercent)
                .ThenByDescending(p => p.SelectedPercent)
                .ThenBy(p => p.Label)
                .ToList();
        }

        private List<InfrastructureChartEvent> CreateInfrastructureEvents(CareerLog target)
        {
            var events = new List<InfrastructureChartEvent>();

            foreach (var lc in target.LCs ?? new List<LC>())
            {
                AddInfrastructureEvent(events, lc.ConstrStarted, "Launch Complex", lc.Name,
                    $"{lc.LcType}, {FormatMass(lc.MassMax)} t", false, lc.MassMax);
                AddInfrastructureEvent(events, lc.ConstrEnded, "Launch Complex", lc.Name,
                    $"{lc.State}, {FormatMass(lc.MassMax)} t", lc.State == LCState.Active, lc.MassMax);
            }

            foreach (var facility in target.FacilityConstructions ?? new List<FacilityConstruction>())
            {
                string label = FormatFacilityName(facility.Facility);
                AddInfrastructureEvent(events, facility.Started, "Facility", label,
                    $"Level {facility.NewLevel + 1} started", false);
                AddInfrastructureEvent(events, facility.Ended, "Facility", label,
                    $"Level {facility.NewLevel + 1}, {facility.State}", facility.State == FacilityConstructionState.Completed);
            }

            return events.OrderBy(e => e.Date).ToList();
        }

        private static void AddInfrastructureEvent(List<InfrastructureChartEvent> events, DateTime? date,
            string type, string label, string detail, bool completed, float? massTons = null)
        {
            if (!date.HasValue)
            {
                return;
            }

            events.Add(new InfrastructureChartEvent
            {
                Date = date.Value,
                Type = type,
                Label = label,
                Detail = detail,
                MassTons = massTons.HasValue && IsFiniteMass(massTons.Value) ? massTons.Value : null,
                Completed = completed
            });
        }

        private List<CareerTimelineEvent> CreateTimeline(CareerLog career)
        {
            var events = new List<CareerTimelineEvent>();

            foreach (var launch in career.LaunchEventEntries ?? new List<LaunchEvent>())
            {
                AddTimelineEvent(events, launch.Date, "Launch", launch.LaunchID, launch.VesselName,
                    launch.BuiltAt?.ToString(), GetLaunchOutcome(launch));
            }

            foreach (var contract in career.ContractEventEntries ?? new List<ContractEvent>())
            {
                AddTimelineEvent(events, contract.Date, "Contract", contract.InternalName, ResolveContractName(contract),
                    outcome: contract.Type.ToString());
            }

            foreach (var program in career.Programs ?? new List<RP1AnalyticsWebApp.Models.Program>())
            {
                string title = ResolveProgramName(program.Name);
                AddTimelineEvent(events, program.Accepted, "Program", program.Name, title, outcome: "Accepted");
                AddTimelineEvent(events, program.Completed, "Program", program.Name, title, outcome: "Completed");
            }

            foreach (var tech in career.TechEventEntries ?? new List<TechResearchEvent>())
            {
                AddTimelineEvent(events, tech.Date, "Research", tech.NodeName, ResolveTechNodeName(tech.NodeName),
                    $"Rate {tech.ResearchRate:N2}, year multiplier {tech.YearMult:N2}", "Unlocked");
            }

            foreach (var lc in career.LCs ?? new List<LC>())
            {
                string key = lc.ModId.ToString();
                string detail = $"{lc.LcType}, {FormatMass(lc.MassMax)} t";
                AddTimelineEvent(events, lc.ConstrStarted, "Infrastructure", key, lc.Name, detail, "Started");
                AddTimelineEvent(events, lc.ConstrEnded, "Infrastructure", key, lc.Name, detail, lc.State.ToString());
            }

            foreach (var facility in career.FacilityConstructions ?? new List<FacilityConstruction>())
            {
                string key = facility.Id.ToString();
                string title = FormatFacilityName(facility.Facility);
                string detail = $"Level {facility.NewLevel + 1}";
                AddTimelineEvent(events, facility.Started, "Infrastructure", key, title, detail, "Started");
                AddTimelineEvent(events, facility.Ended, "Infrastructure", key, title, detail, facility.State.ToString());
            }

            foreach (var leader in career.Leaders ?? new List<Leader>())
            {
                AddTimelineEvent(events, leader.DateAdd, "Leader", leader.Name, leader.Name, outcome: "Added");
                AddTimelineEvent(events, leader.DateRemove, "Leader", leader.Name, leader.Name, outcome: "Removed");
            }

            return events;
        }

        private static void AddTimelineEvent(List<CareerTimelineEvent> events, DateTime? date, string type, string key,
            string title, string detail = null, string outcome = null)
        {
            if (!date.HasValue)
            {
                return;
            }

            events.Add(new CareerTimelineEvent
            {
                Date = date.Value,
                Type = type,
                Key = key,
                Title = title,
                Detail = detail,
                Outcome = outcome
            });
        }

        private static string GetLaunchOutcome(LaunchEvent launch)
        {
            if (launch.Metadata?.Success == true)
            {
                return "Success";
            }

            if (launch.Metadata?.Success == false)
            {
                return "Failure";
            }

            return launch.Failures?.Count > 0 ? "Had failures" : null;
        }

        private static List<CareerLog> ApplyGlobalFilters(List<CareerLog> careers, List<string> players, List<string> races,
            ComparisonEndDateMode careerDateMode, DateTime? careerDateStart, DateTime? careerDateEnd,
            ComparisonEndDateMode lastUpdateMode, DateTime? lastUpdateStart, DateTime? lastUpdateEnd,
            List<string> rp1Versions, List<string> difficulties, List<string> playstyles, string recordEligibility)
        {
            IEnumerable<CareerLog> q = careers;
            var playerSet = StringSet(players);
            var raceSet = StringSet(races);
            var versionSet = StringSet(rp1Versions);
            var difficultySet = StringSet(difficulties);
            var playstyleSet = StringSet(playstyles);

            if (playerSet.Count > 0)
            {
                q = q.Where(c => !string.IsNullOrWhiteSpace(c.UserLogin) && playerSet.Contains(c.UserLogin));
            }

            if (raceSet.Count > 0)
            {
                q = q.Where(c => !string.IsNullOrWhiteSpace(c.Race) && raceSet.Contains(c.Race));
            }

            if (careerDateMode != ComparisonEndDateMode.All)
            {
                q = q.Where(c => MatchesDateMode(c.EndDate, careerDateMode, careerDateStart, careerDateEnd));
            }

            if (lastUpdateMode != ComparisonEndDateMode.All)
            {
                q = q.Where(c => MatchesDateMode(c.LastUpdate, lastUpdateMode, lastUpdateStart, lastUpdateEnd));
            }

            if (versionSet.Count > 0)
            {
                q = q.Where(c => !string.IsNullOrWhiteSpace(c.CareerLogMeta?.VersionTag) &&
                                 versionSet.Contains(c.CareerLogMeta.VersionTag));
            }

            if (difficultySet.Count > 0)
            {
                q = q.Where(c => c.CareerLogMeta != null &&
                                 difficultySet.Contains(c.CareerLogMeta.DifficultyLevel.ToString()));
            }

            if (playstyleSet.Count > 0)
            {
                q = q.Where(c => c.CareerLogMeta != null &&
                                 playstyleSet.Contains(c.CareerLogMeta.CareerPlaystyle.ToString()));
            }

            if (string.Equals(recordEligibility, "Eligible", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(c => c.EligibleForRecords);
            }
            else if (string.Equals(recordEligibility, "Ineligible", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(c => !c.EligibleForRecords);
            }

            return q.ToList();
        }

        private static HashSet<string> StringSet(IEnumerable<string> values)
        {
            return values == null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : values.Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static int CountValues(IEnumerable<string> values)
        {
            return values?.Count(v => !string.IsNullOrWhiteSpace(v)) ?? 0;
        }

        private static string SingleFilterValue(IEnumerable<string> values)
        {
            var normalized = FilterValues(values).ToList();
            return normalized.Count == 1 ? normalized[0] : null;
        }

        private static void AddFilterList(List<string> parts, string label, IEnumerable<string> values)
        {
            var normalized = FilterValues(values).ToList();
            if (normalized.Count == 0)
            {
                return;
            }

            parts.Add(normalized.Count <= 3
                ? $"{label}: {string.Join(", ", normalized)}"
                : $"{label}: {normalized.Count} selected");
        }

        private static IEnumerable<string> FilterValues(IEnumerable<string> values)
        {
            return values == null
                ? Enumerable.Empty<string>()
                : values.Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string DateFilterDescription(string label, ComparisonEndDateMode mode, DateTime? startDate, DateTime? endDate)
        {
            return mode switch
            {
                ComparisonEndDateMode.Range when startDate.HasValue && endDate.HasValue =>
                    $"{label}: {startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.Range when startDate.HasValue =>
                    $"{label}: after {startDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.Range when endDate.HasValue =>
                    $"{label}: before {endDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.Before when endDate.HasValue =>
                    $"{label}: before {endDate.Value:yyyy-MM-dd}",
                ComparisonEndDateMode.After when startDate.HasValue =>
                    $"{label}: after {startDate.Value:yyyy-MM-dd}",
                _ => null
            };
        }

        private CareerOverviewItem CreateOverviewItem(CareerLog career,
            Dictionary<string, CareerRecordSummary> recordCounts)
        {
            recordCounts.TryGetValue(career.Id, out var records);
            return new CareerOverviewItem
            {
                Id = career.Id,
                Name = career.Name,
                UserLogin = career.UserLogin,
                UserPreferredName = _careerLogService.GetUserPreferredName(career.UserLogin),
                Race = career.Race,
                EligibleForRecords = career.EligibleForRecords,
                StartDate = career.StartDate,
                EndDate = career.EndDate,
                LastUpdate = career.LastUpdate,
                Rp1Version = career.CareerLogMeta?.VersionTag,
                VersionSort = career.CareerLogMeta?.VersionSort,
                DifficultyLevel = career.CareerLogMeta?.DifficultyLevel.ToString(),
                CareerPlaystyle = career.CareerLogMeta?.CareerPlaystyle.ToString(),
                LaunchCount = career.LaunchEventEntries?.Count ?? 0,
                CompletedContractCount = career.ContractEventEntries?.Count(e => e.Type == ContractEventType.Complete) ?? 0,
                CompletedMilestoneCount = CountCompletedMilestones(career),
                CompletedProgramCount = CountCompletedPrograms(career),
                RecordCount = records?.RecordCount ?? 0,
                RecordsOwned = records?.Names.OrderBy(n => n).ToList() ?? new List<string>()
            };
        }

        private Dictionary<string, CareerRecordSummary> CreateRecordSummaries(List<CareerLog> eligibleCareers)
        {
            var counts = new Dictionary<string, CareerRecordSummary>(StringComparer.OrdinalIgnoreCase);

            foreach (var record in CreateContractRecords(eligibleCareers))
            {
                var current = GetOrCreateRecordSummary(counts, record.CareerId);
                current.ContractRecords++;
                current.Names.Add(record.ContractDisplayName);
            }

            foreach (var record in CreateProgramRecords(eligibleCareers, ProgramRecordType.Completed))
            {
                var current = GetOrCreateRecordSummary(counts, record.CareerId);
                current.ProgramRecords++;
                current.Names.Add(record.ProgramDisplayName);
            }

            return counts;
        }

        private static List<CareerRecordLeader> CreateRecordLeaders(List<CareerOverviewItem> items,
            Dictionary<string, CareerRecordSummary> recordCounts)
        {
            return items.Where(i => i.RecordCount > 0)
                .OrderByDescending(i => i.RecordCount)
                .ThenByDescending(i => i.LastUpdate)
                .ThenBy(i => i.Name)
                .Take(10)
                .Select(i =>
                {
                    recordCounts.TryGetValue(i.Id, out var counts);
                    return new CareerRecordLeader
                    {
                        CareerId = i.Id,
                        CareerName = i.Name,
                        UserLogin = i.UserLogin,
                        UserPreferredName = i.UserPreferredName,
                        RecordCount = i.RecordCount,
                        ContractRecordCount = counts?.ContractRecords ?? 0,
                        ProgramRecordCount = counts?.ProgramRecords ?? 0,
                        DifficultyLevel = i.DifficultyLevel,
                        CareerPlaystyle = i.CareerPlaystyle,
                        EndDate = i.EndDate,
                        RecordsOwned = counts?.Names.OrderBy(n => n).ToList() ?? new List<string>()
                    };
                })
                .ToList();
        }

        private static CareerRecordSummary GetOrCreateRecordSummary(Dictionary<string, CareerRecordSummary> counts, string careerId)
        {
            if (!counts.TryGetValue(careerId, out var summary))
            {
                summary = new CareerRecordSummary();
                counts[careerId] = summary;
            }

            return summary;
        }

        private static List<CareerOverviewBreakdownItem> CreateBreakdown(List<CareerOverviewItem> items,
            Func<CareerOverviewItem, string> selector)
        {
            return items.Select(i => string.IsNullOrWhiteSpace(selector(i)) ? "Unknown" : selector(i))
                .GroupBy(v => v)
                .Select(g => new CareerOverviewBreakdownItem
                {
                    Key = g.Key,
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(i => i.Count)
                .ThenBy(i => i.Label)
                .ToList();
        }

        private class CareerRecordSummary
        {
            public int ContractRecords { get; set; }
            public int ProgramRecords { get; set; }
            public int RecordCount => ContractRecords + ProgramRecords;
            public List<string> Names { get; } = new();
        }

        private static bool MatchesDateFilter(DateTime candidate, string op, DateTime filterDate)
        {
            var left = candidate.Date;
            var right = filterDate.Date;
            return op switch
            {
                "le" => left <= right,
                "ge" => left >= right,
                _ => left == right
            };
        }

        private static bool VersionMatches(int? candidateVersionSort, int targetVersionSort, string op, bool minorOnly)
        {
            if (!candidateVersionSort.HasValue)
            {
                return false;
            }

            return op switch
            {
                "le" => candidateVersionSort.Value <= targetVersionSort,
                "ge" => candidateVersionSort.Value >= targetVersionSort,
                _ when minorOnly => candidateVersionSort.Value >= targetVersionSort &&
                                    candidateVersionSort.Value < targetVersionSort + 1000,
                _ => candidateVersionSort.Value == targetVersionSort
            };
        }

        private List<CareerLog> ApplyRecordFilters(List<CareerLog> careers, string rp1Version, string rp1VersionOp,
            DifficultyLevel? difficulty, CareerPlaystyle? playstyle, bool includeIneligible)
        {
            var q = careers.Where(c => c.CareerLogMeta != null && (includeIneligible || c.EligibleForRecords));

            if (!string.IsNullOrWhiteSpace(rp1Version) && TryCreateSortableVersion(rp1Version, out int versionSort, out bool minorOnly))
            {
                string op = string.IsNullOrWhiteSpace(rp1VersionOp) ? "eq" : rp1VersionOp;
                q = op switch
                {
                    "le" => q.Where(c => c.CareerLogMeta.VersionSort <= versionSort),
                    "ge" => q.Where(c => c.CareerLogMeta.VersionSort >= versionSort),
                    _ when minorOnly => q.Where(c => c.CareerLogMeta.VersionSort >= versionSort &&
                                                     c.CareerLogMeta.VersionSort < versionSort + 1000),
                    _ => q.Where(c => c.CareerLogMeta.VersionSort == versionSort)
                };
            }

            if (difficulty.HasValue)
            {
                q = q.Where(c => c.CareerLogMeta.DifficultyLevel == difficulty.Value);
            }

            if (playstyle.HasValue)
            {
                q = q.Where(c => c.CareerLogMeta.CareerPlaystyle == playstyle.Value);
            }

            return q.ToList();
        }

        private List<ContractRecord> CreateContractRecords(List<CareerLog> cohort)
        {
            return cohort.Where(c => c.ContractEventEntries != null)
                .SelectMany(c => c.ContractEventEntries
                    .Where(e => e.Type == ContractEventType.Complete)
                    .GroupBy(e => e.InternalName)
                    .Select(g => new
                    {
                        Career = c,
                        Contract = g.Key,
                        Date = g.Min(e => e.Date)
                    }))
                .GroupBy(e => e.Contract)
                .SelectMany(g => g.OrderBy(e => e.Date).Select((e, i) => new ContractRecord
                {
                    Rank = i + 1,
                    CohortSize = g.Count(),
                    ContractInternalName = g.Key,
                    ContractDisplayName = ResolveContractName(g.Key),
                    Date = e.Date,
                    CareerId = e.Career.Id,
                    CareerName = e.Career.Name,
                    UserLogin = e.Career.UserLogin,
                    UserPreferredName = e.Career.UserLogin,
                    VersionTag = e.Career.CareerLogMeta?.VersionTag,
                    DifficultyLevel = e.Career.CareerLogMeta?.DifficultyLevel.ToString(),
                    CareerPlaystyle = e.Career.CareerLogMeta?.CareerPlaystyle.ToString()
                }))
                .Where(r => r.Rank == 1)
                .OrderBy(r => r.Date)
                .ToList();
        }

        private List<ProgramRecord> CreateProgramRecords(List<CareerLog> cohort, ProgramRecordType type)
        {
            return cohort.Where(c => c.Programs != null)
                .SelectMany(c => c.Programs.Select(p => new
                {
                    Career = c,
                    Program = p.Name,
                    Date = type == ProgramRecordType.Accepted ? p.Accepted :
                        type == ProgramRecordType.ObjectivesCompleted ? p.ObjectivesCompleted : p.Completed
                }))
                .Where(e => e.Date.HasValue)
                .GroupBy(e => e.Program)
                .SelectMany(g => g.OrderBy(e => e.Date).Select((e, i) => new ProgramRecord
                {
                    Rank = i + 1,
                    CohortSize = g.Count(),
                    ProgramName = g.Key,
                    ProgramDisplayName = ResolveProgramName(g.Key),
                    Date = e.Date,
                    CareerId = e.Career.Id,
                    CareerName = e.Career.Name,
                    UserLogin = e.Career.UserLogin,
                    UserPreferredName = e.Career.UserLogin,
                    VersionTag = e.Career.CareerLogMeta?.VersionTag,
                    DifficultyLevel = e.Career.CareerLogMeta?.DifficultyLevel.ToString(),
                    CareerPlaystyle = e.Career.CareerLogMeta?.CareerPlaystyle.ToString()
                }))
                .Where(r => r.Rank == 1)
                .OrderBy(r => r.Date)
                .ToList();
        }

        private int CountCompletedMilestones(CareerLog career)
        {
            return _contractSettings.MilestoneContractNames.Count(name => GetContractCompletionDate(career, name).HasValue);
        }

        private Func<DateTime, double?> BuildMilestoneSelector(CareerLog career)
        {
            var completionDates = _contractSettings.MilestoneContractNames
                .Select(name => GetContractCompletionDate(career, name))
                .Where(d => d.HasValue)
                .Select(d => d.Value.Date)
                .OrderBy(d => d)
                .ToList();
            return date => (double?)CountUpToDate(completionDates, date.Date);
        }

        private static Func<DateTime, double?> BuildProgramsSelector(CareerLog career)
            {
            var completionDates = career.Programs?
                .Where(p => p.Completed.HasValue)
                .Select(p => p.Completed.Value.Date)
                .OrderBy(d => d)
                .ToList() ?? [];
            return date => (double?)CountUpToDate(completionDates, date.Date);
        }

        private static Func<DateTime, double?> BuildTechUnlocksSelector(CareerLog career)
        {
            var dates = career.TechEventEntries?
                .Select(e => e.Date.Date)
                .OrderBy(d => d)
                .ToList() ?? [];
            return date => (double?)CountUpToDate(dates, date.Date);
        }

        private static Func<DateTime, double?> BuildLaunchesSelector(CareerLog career)
        {
            var dates = career.LaunchEventEntries?
                .Select(l => l.Date.Date)
                .OrderBy(d => d)
                .ToList() ?? [];
            return date => (double?)CountUpToDate(dates, date.Date);
        }

        private static int CountUpToDate(List<DateTime> sortedDates, DateTime date)
        {
            int lo = 0, hi = sortedDates.Count;
            while (lo < hi)
        {
                int mid = (lo + hi) >> 1;
                if (sortedDates[mid] <= date) lo = mid + 1;
                else hi = mid;
            }
            return lo;
        }

        private static int CountCompletedPrograms(CareerLog career)
        {
            return career.Programs?.Count(p => p.Completed.HasValue) ?? 0;
        }

        private static double? CountInfrastructureCompletionsByDate(CareerLog career, DateTime date)
        {
            int lcCount = career.LCs?.Count(l => l.ConstrEnded.HasValue && l.ConstrEnded.Value.Date <= date.Date) ?? 0;
            int facilityCount = career.FacilityConstructions?.Count(f => f.Ended.HasValue && f.Ended.Value.Date <= date.Date) ?? 0;
            return lcCount + facilityCount;
        }

        private static double? MaxLaunchComplexMassByDate(CareerLog career, DateTime date)
        {
            var values = career.LCs?
                .Where(l => IsFiniteMass(l.MassMax) &&
                            l.ConstrEnded.HasValue &&
                            l.ConstrEnded.Value.Date <= date.Date &&
                            l.State != LCState.Dismantled &&
                            l.State != LCState.ConstructionCancelled)
                .Select(l => (double)l.MassMax)
                .OrderBy(v => v)
                .ToList();
            return values == null || values.Count == 0 ? 0 : values[^1];
        }

        private static CareerLogPeriod Last(CareerLog career)
        {
            return career.CareerLogEntries?.OrderBy(p => p.EndDate).LastOrDefault();
        }

        private static double? TotalSpending(CareerLog career)
        {
            return SumPeriods(career, p =>
                SpendingForPeriod(p));
        }

        private static double? TotalSpendingByDate(CareerLog career, DateTime date)
        {
            if (career.CareerLogEntries == null || career.CareerLogEntries.Count == 0)
            {
                return null;
            }

            return career.CareerLogEntries
                .Where(p => p.EndDate.Date <= date.Date)
                .Sum(SpendingForPeriod);
        }

        private static double? ConfidenceEarnedByDate(CareerLog career, DateTime date)
        {
            if (career.CareerLogEntries == null || career.CareerLogEntries.Count == 0)
            {
                return null;
            }

            return career.CareerLogEntries
                .Where(p => p.EndDate.Date <= date.Date)
                .Sum(p => p.Confidence);
        }

        private static double SpendingForPeriod(CareerLogPeriod p)
        {
            return p.SalaryEngineers + p.SalaryResearchers + p.SalaryCrew +
                   p.LaunchFees + p.VesselPurchase + p.LCMaintenance + p.FacilityMaintenance +
                   p.MaintenanceFees + p.TrainingFees + p.ToolingFees + p.EntryCosts +
                   p.SpentUnlockCredit + p.ConstructionFees + p.HiringResearchers +
                   p.HiringEngineers + p.OtherFees;
        }

        private static List<LaunchEvent> LaunchesInYear(CareerLog career, int year)
        {
            return career.LaunchEventEntries?
                .Where(l => l.Date.Year == year)
                .ToList() ?? new List<LaunchEvent>();
        }

        private static double? SumPeriods(CareerLog career, Func<CareerLogPeriod, double> selector)
        {
            if (career.CareerLogEntries == null || career.CareerLogEntries.Count == 0)
            {
                return null;
            }

            return career.CareerLogEntries.Sum(selector);
        }

        private static double? TaggedSuccessRate(CareerLog career)
        {
            var launches = career.LaunchEventEntries?.Where(l => l.Metadata?.Success != null).ToList();
            if (launches == null || launches.Count == 0)
            {
                return null;
            }

            return launches.Count(l => l.Metadata.Success == true) * 100d / launches.Count;
        }

        private static DateTime? GetContractCompletionDate(CareerLog career, string contract)
        {
            return career.ContractEventEntries?
                .Where(e => e.Type == ContractEventType.Complete &&
                            string.Equals(e.InternalName, contract, StringComparison.OrdinalIgnoreCase))
                .Select(e => (DateTime?)e.Date)
                .DefaultIfEmpty()
                .Min();
        }

        private bool IsProgramExcludedByTargetChoice(CareerLog target, string program, out string exclusionReason)
        {
            exclusionReason = null;

            foreach (string[] group in MutuallyExclusiveProgramGroups)
            {
                if (!group.Contains(program, StringComparer.OrdinalIgnoreCase) || HasProgramEntry(target, program))
                {
                    continue;
                }

                string selectedProgram = group.FirstOrDefault(p => HasProgramEntry(target, p));
                if (selectedProgram == null)
                {
                    continue;
                }

                exclusionReason = $"{ResolveProgramName(program)} is mutually exclusive with {ResolveProgramName(selectedProgram)}.";
                return true;
            }

            return false;
        }

        private bool IsContractExcludedByTargetProgramChoice(CareerLog target, string contract, out string exclusionReason)
        {
            exclusionReason = null;

            foreach (string[] group in MutuallyExclusiveProgramGroups)
            {
                string owningProgram = group.FirstOrDefault(program =>
                    ProgramMilestoneContracts.TryGetValue(program, out string[] contracts) &&
                    contracts.Contains(contract, StringComparer.OrdinalIgnoreCase));

                if (owningProgram == null || HasProgramEntry(target, owningProgram))
                {
                    continue;
                }

                string selectedProgram = group.FirstOrDefault(p => HasProgramEntry(target, p));
                if (selectedProgram == null)
                {
                    continue;
                }

                exclusionReason =
                    $"{ResolveContractName(contract)} belongs to {ResolveProgramName(owningProgram)}, which is mutually exclusive with {ResolveProgramName(selectedProgram)}.";
                return true;
            }

            return false;
        }

        private static bool HasProgramEntry(CareerLog career, string program)
        {
            return career.Programs?.Any(p => string.Equals(p.Name, program, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private static DateTime? GetProgramAcceptedDate(CareerLog career, string program)
        {
            return career.Programs?
                .Where(p => string.Equals(p.Name, program, StringComparison.OrdinalIgnoreCase))
                .Select(p => (DateTime?)p.Accepted)
                .DefaultIfEmpty()
                .Min();
        }

        private static DateTime? GetProgramCompletionDate(CareerLog career, string program)
        {
            return career.Programs?
                .Where(p => string.Equals(p.Name, program, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Completed)
                .Where(d => d.HasValue)
                .DefaultIfEmpty()
                .Min();
        }

        private string ResolveContractName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name ?? string.Empty;
            }

            return _contractSettings.ContractNameDict.TryGetValue(name, out string disp) ? disp : HumanizeIdentifier(name);
        }

        private string ResolveContractName(ContractEvent contract)
        {
            if (string.IsNullOrWhiteSpace(contract?.DisplayName) ||
                LooksLikeInternalIdentifier(contract.DisplayName))
            {
                return ResolveContractName(contract?.InternalName ?? contract?.DisplayName);
            }

            return contract.DisplayName;
        }

        private string ResolveTechNodeName(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return id ?? string.Empty;
            }

            return _techTreeSettings.NodeTitleDict.TryGetValue(id, out string disp) ? disp : id;
        }

        private string ResolveProgramName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name ?? string.Empty;
            }

            return _programSettings.ProgramNameDict.TryGetValue(name, out string disp) ? disp : name;
        }

        private static string FormatFacilityName(SpaceCenterFacility facility)
        {
            return facility switch
            {
                SpaceCenterFacility.ResearchAndDevelopment => "R&D",
                SpaceCenterFacility.AstronautComplex => "Astronaut Complex",
                SpaceCenterFacility.LaunchPad => "Launch Pad",
                SpaceCenterFacility.MissionControl => "Mission Control",
                SpaceCenterFacility.TrackingStation => "Tracking Station",
                SpaceCenterFacility.SpaceplaneHangar => "Spaceplane Hangar",
                SpaceCenterFacility.VehicleAssemblyBuilding => "Vehicle Assembly Building",
                _ => HumanizeIdentifier(facility.ToString())
            };
        }

        private static bool LooksLikeInternalIdentifier(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   (value.Contains('_') || value.Contains('-'));
        }

        private static string HumanizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            var chars = new List<char>();
            char previous = '\0';

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                char next = i + 1 < value.Length ? value[i + 1] : '\0';

                if (current == '_' || current == '-')
                {
                    chars.Add(' ');
                }
                else
                {
                    bool startsWord = i > 0 &&
                                      char.IsUpper(current) &&
                                      (char.IsLower(previous) || (char.IsUpper(previous) && char.IsLower(next)));
                    if (startsWord)
                    {
                        chars.Add(' ');
                    }

                    chars.Add(current);
                }

                previous = current;
            }

            return string.Join(" ", new string(chars.ToArray())
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(FormatIdentifierWord));
        }

        private static string FormatIdentifierWord(string word)
        {
            string lower = word.ToLowerInvariant();
            return lower switch
            {
                "eva" => "EVA",
                "geo" => "GEO",
                "gto" => "GTO",
                "iss" => "ISS",
                "lc" => "LC",
                "leo" => "LEO",
                "r&d" => "R&D",
                "rp" => "RP",
                "ssto" => "SSTO",
                "tv" => "TV",
                "us" => "US",
                "usa" => "USA",
                "usaf" => "USAF",
                _ => char.ToUpperInvariant(lower[0]) + lower[1..]
            };
        }

        private bool IsDeprecatedProgram(string name)
        {
            return ResolveProgramName(name).Contains("(Deprecated)", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] ExclusiveGroupForProgram(string program)
        {
            return MutuallyExclusiveProgramGroups.FirstOrDefault(group =>
                group.Contains(program, StringComparer.OrdinalIgnoreCase));
        }

        private static double DaysSinceEpoch(DateTime date)
        {
            return (date - Constants.CareerEpoch).TotalDays;
        }

        private static DateTime? DateFromDays(double? days)
        {
            return days.HasValue ? Constants.CareerEpoch.AddDays(days.Value) : null;
        }

        private static double? Median(List<double> values)
        {
            return PercentileValue(values, 0.5);
        }

        private static double? PercentileValue(List<double> values, double percentile)
        {
            if (values.Count == 0)
            {
                return null;
            }

            if (values.Count == 1)
            {
                return values[0];
            }

            double position = (values.Count - 1) * percentile;
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);
            if (lower == upper)
            {
                return values[lower];
            }

            double weight = position - lower;
            return values[lower] * (1 - weight) + values[upper] * weight;
        }

        private static int Rank(List<double> sortedAscending, double value, bool descending)
        {
            var sorted = descending ? sortedAscending.OrderByDescending(v => v).ToList() : sortedAscending;
            int idx = sorted.FindIndex(v => Math.Abs(v - value) < 0.0001);
            if (idx >= 0)
            {
                return idx + 1;
            }

            int countAhead = descending ? sorted.Count(v => v > value) : sorted.Count(v => v < value);
            return countAhead + 1;
        }

        private static double PercentilePosition(List<double> sortedAscending, double value)
        {
            return Math.Round(sortedAscending.Count(v => v <= value) * 100d / sortedAscending.Count, 1);
        }

        private static bool TryCreateSortableVersion(string version, out int value, out bool minorOnly)
        {
            value = 0;
            minorOnly = false;
            string[] split = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
            {
                return false;
            }

            int[] mults = { 1000000, 1000, 1 };
            for (int i = 0; i < Math.Min(3, split.Length); i++)
            {
                if (!int.TryParse(split[i], out int component))
                {
                    return false;
                }

                value += component * mults[i];
            }

            minorOnly = split.Length == 2;
            return true;
        }

        private static string FormatMass(float value)
        {
            return value > 1e+38 ? "unlimited" : value.ToString("N0");
        }

        private static bool IsFiniteMass(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value < 1e+38;
        }
    }
}
