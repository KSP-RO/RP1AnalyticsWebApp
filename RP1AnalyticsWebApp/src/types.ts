import { programSpeeds } from './utils/programSpeeds';

export interface UserData {
    userName: string;
    preferredName: string;
}

export interface CurrentUser {
    userName: string;
    roles: string[];
}

export interface CareerLog {
    id: string;
    name: string;
    userLogin: string;
    token: string;
    eligibleForRecords: boolean;
    startDate: string;
    endDate: string;
    lastUpdate: string;
    race: string;
    careerLogEntries: CareerLogPeriod[];
    contractEventEntries: ContractEvent[];
    lcs: LC[];
    facilityConstructions: FacilityEvent[];
    techEventEntries: TechEventEntry[];
    launchEventEntries: LaunchEvent[];
    programs: ProgramItem[];
    leaders: LeaderEvent[];
    careerLogMeta: CareerLogMeta;
}

export interface CareerLogPeriod {
    startDate: string;
    endDate: string;
    numEngineers: number;
    numResearchers: number;
    efficiencyEngineers: number;
    currentFunds: number;
    currentSci: number;
    rnDQueueLength: number;
    scienceEarned: number;
    salaryEngineers: number;
    salaryResearchers: number;
    salaryCrew: number;
    programFunds: number;
    otherFundsEarned: number;
    launchFees: number;
    vesselPurchase: number;
    vesselRecovery: number;
    lcMaintenance: number;
    facilityMaintenance: number;
    maintenanceFees: number;
    trainingFees: number;
    toolingFees: number;
    entryCosts: number;
    spentUnlockCredit: number;
    constructionFees: number;
    hiringResearchers: number;
    hiringEngineers: number;
    otherFees: number;
    subsidySize: number;
    subsidyPaidOut: number;
    repFromPrograms: number;
    fundsGainMult: number;
    numNautsKilled: number;
    confidence: number;
    reputation: number;
}

export interface CareerLogMeta {
    careerPlaystyle: string;
    difficultyLevel: string;
    configurableStart: string;
    failureModel: string;
    modRecency: string;
    versionTag: string;
    versionSort: number;
    creationDate: string;
    descriptionText: string;
}

export interface ExtendedCareerLogMeta extends CareerLogMeta {
    lastUpdate: string;
}

export interface ContractEvent {
    internalName: string;
    date: string;
    repChange: number;
    type: number;
}

export interface TechEventEntry {
    nodeName: string
    date: string
    yearMult: number
    researchRate: number
}

export interface TechUnlockItem {
    nodeInternalName: string
    nodeDisplayName: string
    date: string
    yearMult: number
    researchRate: number
}

export interface ProgramItem {
    name: string;
    title: string;
    accepted: string;
    objectivesCompleted: string;
    completed: string;
    nominalDurationYears: number;
    totalFunding: number;
    fundsPaidOut: number;
    repPenaltyAssessed: number;
    speed: typeof programSpeeds;
}

export interface BaseContractEvent {
    contractInternalName: string;
    contractDisplayName: string;
    type: number;
    date: string;
}

export interface ContractEventWithCareerInfo extends BaseContractEvent {
    CareerId: string;
    CareerName: string;
    UserLogin: string;
    UserPreferredName: string;
}

export interface LaunchEvent {
    date: string;
    vesselName: string;
    vesselUID: string;
    launchID: string;
    lcID: string;
    lcModID: string;
    builtAt: string;
    vesselMassTons: number | null;
    partCount: number | null;
    crewCount: number | null;
    failures: Failure[];
    metadata: LaunchMetadata;
}

export interface LaunchEventItem extends LaunchEvent {
    visible: boolean;
}

export interface Failure {
    date: string;
    part: string;
    type: string;
}

export interface LaunchMetadata {
    success: boolean | null;
}

export interface LeaderEvent {
    name: string;
    dateAdd: string;
    dateRemove: string;
    fireCost: number;
}

export interface ExtendedLeaderEvent extends LeaderEvent {
    title: string;
    type: string;
}

export interface FacilityEvent {
    id: string;
    facility: string;
    newLevel: number;
    cost: number;
    state: string;
    started: string;
    ended: string;
}

export interface LC {
    id: string;
    modId: string;
    modCost: number;
    name: string;
    lcType: string;
    massMax: number;
    massOrig: number;
    sizeMax: Vector3;
    isHumanRated: boolean;
    state: string;
    constrStarted: string;
    constrEnded: string;
}

export interface LCItem extends LC {
    visible: boolean;
}

export interface Vector3 {
    x: number;
    y: number;
    z: number;
}

export type DateFilterMode = 'All' | 'Range' | 'Before' | 'After';
export type RecordEligibilityFilter = 'All' | 'Eligible' | 'Ineligible';
export type VersionFilterOp = 'Any' | 'Is' | 'AtLeast' | 'AtMost';

export interface Filters {
    players: string[];
    races: string[];
    careerDateMode: DateFilterMode;
    careerDateStart: string | null;
    careerDateEnd: string | null;
    lastUpdateMode: DateFilterMode;
    lastUpdateStart: string | null;
    lastUpdateEnd: string | null;
    rp1VersionOp: VersionFilterOp;
    rp1Version: string | null;
    difficulties: string[];
    playstyles: string[];
    recordEligibility: RecordEligibilityFilter;
}

export interface CareerListItem {
    id: string;
    name: string;
    user: string;
    userPreferredName: string;
    race: string;
    token: string;
}

export interface CareerOverviewSnapshot {
    summary: CareerOverviewSummary;
    versionBreakdown: CareerOverviewBreakdownItem[];
    difficultyBreakdown: CareerOverviewBreakdownItem[];
    playstyleBreakdown: CareerOverviewBreakdownItem[];
    recentCareers: CareerOverviewItem[];
    recordLeaders: CareerRecordLeader[];
    careers: CareerOverviewItem[];
}

export interface CareerOverviewSummary {
    totalCareers: number;
    recordsEligibleCareers: number;
    totalLaunches: number;
    totalCompletedContracts: number;
    totalCompletedMilestones: number;
    totalCompletedPrograms: number;
    totalRecordsSet: number;
    latestSaveDate: string | null;
    latestUpdate: string | null;
}

export interface CareerOverviewItem {
    id: string;
    name: string;
    userLogin: string;
    userPreferredName: string;
    race: string | null;
    eligibleForRecords: boolean;
    startDate: string;
    endDate: string;
    lastUpdate: string;
    rp1Version: string | null;
    versionSort: number | null;
    difficultyLevel: string | null;
    careerPlaystyle: string | null;
    launchCount: number;
    completedContractCount: number;
    completedMilestoneCount: number;
    completedProgramCount: number;
    recordCount: number;
    recordsOwned: string[];
}

export interface CareerRecordLeader {
    careerId: string;
    careerName: string;
    userLogin: string;
    userPreferredName: string;
    recordCount: number;
    contractRecordCount: number;
    programRecordCount: number;
    difficultyLevel: string | null;
    careerPlaystyle: string | null;
    endDate: string;
    recordsOwned: string[];
}

export interface CareerOverviewBreakdownItem {
    key: string;
    label: string;
    count: number;
}

export interface RaceManagementCareerListItem extends CareerListItem {
    isUpdating: boolean;
}

export interface ProgramRecord {
    careerId: string;
    programName: string;
    programDisplayName: string;
    date: string;
    careerName: string;
    userLogin: string;
    userPreferredName: string;
    speed: string;
    rank?: number;
    cohortSize?: number;
    versionTag?: string;
    difficultyLevel?: string;
    careerPlaystyle?: string;
}

export interface ContractRecord {
    careerId: string;
    contractInternalName: string;
    contractDisplayName: string;
    date: string;
    careerName: string;
    userLogin: string;
    userPreferredName: string;
    rank?: number;
    cohortSize?: number;
    versionTag?: string;
    difficultyLevel?: string;
    careerPlaystyle?: string;
}

export type ComparisonCohortScope =
    | 'Exact'
    | 'SameVersion'
    | 'SameMinorVersion'
    | 'SameDifficultyAndPlaystyle'
    | 'SamePlaystyle'
    | 'AllEligible';

export type ComparisonEndDateMode =
    | 'All'
    | 'Range'
    | 'Before'
    | 'After';

export interface CareerComparison {
    careerId: string;
    careerName: string;
    cohort: CohortSummary;
    metrics: MetricComparison[];
    milestones: MilestoneComparison[];
    timeline: CareerTimelineEvent[];
    historicalBenchmarks: HistoricalBenchmark[];
    charts: ComparisonCharts;
}

export interface CohortSummary {
    careerCount: number;
    rp1Version: string;
    versionSort: number | null;
    difficulty: string;
    playstyle: string;
    eligibleOnly: boolean;
    scope: ComparisonCohortScope;
    description: string;
    endDateFilter: ComparisonEndDateFilter;
}

export interface ComparisonEndDateFilter {
    mode: ComparisonEndDateMode;
    targetEndDate: string;
    startDate: string | null;
    endDate: string | null;
    description: string;
}

export interface MetricComparison {
    key: string;
    label: string;
    category: string;
    unit: string;
    targetValue: number | null;
    cohortMedian: number | null;
    cohortP25: number | null;
    cohortP75: number | null;
    rank: number | null;
    cohortCount: number;
    percentile: number | null;
    description: string;
}

export interface MilestoneComparison {
    key: string;
    kind: string;
    label: string;
    targetDate: string | null;
    cohortMedianDate: string | null;
    cohortP25Date: string | null;
    cohortP75Date: string | null;
    cohortRecordDate: string | null;
    cohortRecordCareerId: string | null;
    cohortRecordCareerName: string | null;
    cohortRecordUserLogin: string | null;
    cohortRecordUserPreferredName: string | null;
    rank: number | null;
    cohortCount: number;
    cohortCareerCount: number;
    cohortCompletedByTargetDateCount: number;
    cohortCompletedByTargetDatePercent: number;
    daysFromCohortMedian: number | null;
    daysFromHistoricalBenchmark: number | null;
    isExcludedByTargetProgramChoice: boolean;
    exclusionReason: string | null;
    percentile: number | null;
    historicalBenchmark: HistoricalBenchmark | null;
}

export interface HistoricalBenchmark {
    key: string;
    kind: string;
    title: string;
    date: string;
    agency: string;
    sourceUrl: string;
    note: string;
}

export interface CareerTimelineEvent {
    date: string;
    type: string;
    key: string;
    title: string;
    detail: string;
    outcome: string;
}

export interface ComparisonCharts {
    progressSeries: ComparisonBandSeries[];
    launchCadence: LaunchCadencePoint[];
    programPaths: ProgramPathChoice[];
    infrastructureSeries: ComparisonBandSeries[];
    infrastructureEvents: InfrastructureChartEvent[];
    economySeries: ComparisonBandSeries[];
}

export interface ComparisonBandSeries {
    key: string;
    label: string;
    unit: string;
    points: ComparisonBandPoint[];
}

export interface ComparisonBandPoint {
    date: string;
    targetValue: number | null;
    median: number | null;
    p25: number | null;
    p75: number | null;
    comparisonCount: number;
}

export interface LaunchCadencePoint {
    year: number;
    targetSuccesses: number;
    targetFailures: number;
    targetPartialFailures: number;
    targetUntagged: number;
    medianLaunches: number | null;
    p25Launches: number | null;
    p75Launches: number | null;
    comparisonCount: number;
}

export interface ProgramPathChoice {
    key: string;
    label: string;
    targetAcceptedDate: string | null;
    targetCompletedDate: string | null;
    targetSelected: boolean;
    targetCompleted: boolean;
    selectedCount: number;
    completedCount: number;
    comparisonCount: number;
    selectedPercent: number;
    completedPercent: number;
    exclusiveGroup: string | null;
    mutuallyExclusiveKeys: string[];
}

export interface InfrastructureChartEvent {
    date: string;
    type: string;
    label: string;
    detail: string;
    massTons: number | null;
    completed: boolean;
}

export interface RecordsSnapshot {
    filters: RecordFilterSummary;
    contractRecords: ContractRecord[];
    programRecords: ProgramRecord[];
}

export interface RecordFilterSummary {
    rp1Version: string;
    difficulty: string;
    playstyle: string;
    careerCount: number;
    eligibleOnly: boolean;
}
