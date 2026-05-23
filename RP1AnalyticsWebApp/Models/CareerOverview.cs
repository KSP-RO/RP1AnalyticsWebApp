using System;
using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public class CareerOverviewSnapshot
    {
        public CareerOverviewSummary Summary { get; set; } = new();
        public List<CareerOverviewBreakdownItem> VersionBreakdown { get; set; } = new();
        public List<CareerOverviewBreakdownItem> DifficultyBreakdown { get; set; } = new();
        public List<CareerOverviewBreakdownItem> PlaystyleBreakdown { get; set; } = new();
        public List<CareerOverviewItem> RecentCareers { get; set; } = new();
        public List<CareerRecordLeader> RecordLeaders { get; set; } = new();
        public List<CareerOverviewItem> Careers { get; set; } = new();
    }

    public class CareerOverviewSummary
    {
        public int TotalCareers { get; set; }
        public int RecordsEligibleCareers { get; set; }
        public int TotalLaunches { get; set; }
        public int TotalCompletedContracts { get; set; }
        public int TotalCompletedMilestones { get; set; }
        public int TotalCompletedPrograms { get; set; }
        public int TotalRecordsSet { get; set; }
        public DateTime? LatestSaveDate { get; set; }
        public DateTime? LatestUpdate { get; set; }
    }

    public class CareerOverviewItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserLogin { get; set; }
        public string UserPreferredName { get; set; }
        public string Race { get; set; }
        public bool EligibleForRecords { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Rp1Version { get; set; }
        public int? VersionSort { get; set; }
        public string DifficultyLevel { get; set; }
        public string CareerPlaystyle { get; set; }
        public int LaunchCount { get; set; }
        public int CompletedContractCount { get; set; }
        public int CompletedMilestoneCount { get; set; }
        public int CompletedProgramCount { get; set; }
        public int RecordCount { get; set; }
        public List<string> RecordsOwned { get; set; } = new();
    }

    public class CareerRecordLeader
    {
        public string CareerId { get; set; }
        public string CareerName { get; set; }
        public string UserLogin { get; set; }
        public string UserPreferredName { get; set; }
        public int RecordCount { get; set; }
        public int ContractRecordCount { get; set; }
        public int ProgramRecordCount { get; set; }
        public string DifficultyLevel { get; set; }
        public string CareerPlaystyle { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> RecordsOwned { get; set; } = new();
    }

    public class CareerOverviewBreakdownItem
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }
}
