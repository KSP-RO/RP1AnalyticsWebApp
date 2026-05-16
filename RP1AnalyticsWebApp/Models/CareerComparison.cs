using System;
using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Models
{
    public enum ComparisonCohortScope
    {
        Exact,
        SameVersion,
        SameMinorVersion,
        SameDifficultyAndPlaystyle,
        SamePlaystyle,
        AllEligible
    }

    public enum ComparisonEndDateMode
    {
        All,
        Range,
        Before,
        After
    }

    public class CareerComparison
    {
        public string CareerId { get; set; }
        public string CareerName { get; set; }
        public CohortSummary Cohort { get; set; }
        public List<MetricComparison> Metrics { get; set; } = new();
        public List<MilestoneComparison> Milestones { get; set; } = new();
        public List<CareerTimelineEvent> Timeline { get; set; } = new();
        public List<HistoricalBenchmark> HistoricalBenchmarks { get; set; } = new();
        public ComparisonCharts Charts { get; set; } = new();
    }

    public class CohortSummary
    {
        public int CareerCount { get; set; }
        public string Rp1Version { get; set; }
        public int? VersionSort { get; set; }
        public string Difficulty { get; set; }
        public string Playstyle { get; set; }
        public bool EligibleOnly { get; set; }
        public string Scope { get; set; }
        public string Description { get; set; }
        public ComparisonEndDateFilter EndDateFilter { get; set; }
    }

    public class ComparisonEndDateFilter
    {
        public string Mode { get; set; }
        public DateTime TargetEndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
    }

    public class MetricComparison
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public double? TargetValue { get; set; }
        public double? CohortMedian { get; set; }
        public double? CohortP25 { get; set; }
        public double? CohortP75 { get; set; }
        public int? Rank { get; set; }
        public int CohortCount { get; set; }
        public double? Percentile { get; set; }
        public string Description { get; set; }
    }

    public class MilestoneComparison
    {
        public string Key { get; set; }
        public string Kind { get; set; }
        public string Label { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? CohortMedianDate { get; set; }
        public DateTime? CohortP25Date { get; set; }
        public DateTime? CohortP75Date { get; set; }
        public DateTime? CohortRecordDate { get; set; }
        public string CohortRecordCareerId { get; set; }
        public string CohortRecordCareerName { get; set; }
        public string CohortRecordUserLogin { get; set; }
        public string CohortRecordUserPreferredName { get; set; }
        public int? Rank { get; set; }
        public int CohortCount { get; set; }
        public int CohortCareerCount { get; set; }
        public int CohortCompletedByTargetDateCount { get; set; }
        public double CohortCompletedByTargetDatePercent { get; set; }
        public int? DaysFromCohortMedian { get; set; }
        public int? DaysFromHistoricalBenchmark { get; set; }
        public bool IsExcludedByTargetProgramChoice { get; set; }
        public string ExclusionReason { get; set; }
        public double? Percentile { get; set; }
        public HistoricalBenchmark HistoricalBenchmark { get; set; }
    }

    public class CareerTimelineEvent
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Outcome { get; set; }
    }

    public class ComparisonCharts
    {
        public List<ComparisonBandSeries> ProgressSeries { get; set; } = new();
        public List<LaunchCadencePoint> LaunchCadence { get; set; } = new();
        public List<ProgramPathChoice> ProgramPaths { get; set; } = new();
        public List<ComparisonBandSeries> InfrastructureSeries { get; set; } = new();
        public List<InfrastructureChartEvent> InfrastructureEvents { get; set; } = new();
        public List<ComparisonBandSeries> EconomySeries { get; set; } = new();
    }

    public class ComparisonBandSeries
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Unit { get; set; }
        public List<ComparisonBandPoint> Points { get; set; } = new();
    }

    public class ComparisonBandPoint
    {
        public DateTime Date { get; set; }
        public double? TargetValue { get; set; }
        public double? Median { get; set; }
        public double? P25 { get; set; }
        public double? P75 { get; set; }
        public int ComparisonCount { get; set; }
    }

    public class LaunchCadencePoint
    {
        public int Year { get; set; }
        public int TargetSuccesses { get; set; }
        public int TargetFailures { get; set; }
        public int TargetPartialFailures { get; set; }
        public int TargetUntagged { get; set; }
        public double? MedianLaunches { get; set; }
        public double? P25Launches { get; set; }
        public double? P75Launches { get; set; }
        public int ComparisonCount { get; set; }
    }

    public class ProgramPathChoice
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public DateTime? TargetAcceptedDate { get; set; }
        public DateTime? TargetCompletedDate { get; set; }
        public bool TargetSelected { get; set; }
        public bool TargetCompleted { get; set; }
        public int SelectedCount { get; set; }
        public int CompletedCount { get; set; }
        public int ComparisonCount { get; set; }
        public double SelectedPercent { get; set; }
        public double CompletedPercent { get; set; }
        public string ExclusiveGroup { get; set; }
        public List<string> MutuallyExclusiveKeys { get; set; } = new();
    }

    public class InfrastructureChartEvent
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Detail { get; set; }
        public double? MassTons { get; set; }
        public bool Completed { get; set; }
    }
}
