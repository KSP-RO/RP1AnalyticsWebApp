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
    speed: keyof typeof programSpeeds;
    speedText: string;
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
    builtAt: string;
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

export interface Filters {
    player: string | null;
    race: string | null;
    ingameDateOp: string | null;
    ingameDate: string | null;
    lastUpdateOp: string | null;
    lastUpdate: string | null;
    rp1verOp: string | null;
    rp1ver: string | null;
    difficulty: string | null;
    playstyle: string | null;
}

export interface CareerListItem {
    id: string;
    name: string;
    user: string;
    userPreferredName: string;
    race: string;
    token: string;
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
}

export interface ContractRecord {
    careerId: string;
    contractInternalName: string;
    contractDisplayName: string;
    date: string;
    careerName: string;
    userLogin: string;
    userPreferredName: string;
}
