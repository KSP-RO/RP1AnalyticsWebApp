import { BaseContractEvent, CareerListItem, CareerLog, ContractEventWithCareerInfo, ContractRecord, FacilityEvent, Filters, LaunchEvent, LaunchMetadata, LC, ExtendedLeaderEvent, ProgramItem, ProgramRecord, TechUnlockItem, UserData } from "../types";
import { constructFilterQueryString } from "./constructFilterQueryString";

interface ODateResp {
    value: any;
}

async function get<T>(url: string): Promise<T> {
    const resp = await fetch(url);
    const jsonItems = await resp.json() as T;
    return jsonItems;
}

async function patch(url: string, obj: any): Promise<void> {
    await fetch(url, {
        method: "PATCH",
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(obj)
    });
}


export async function fetchCareerLogList(): Promise<CareerListItem[]> {
    return await get<CareerListItem[]>(`/api/careerLogs/list`);
}

export async function fetchCareerLog(careerId: string): Promise<CareerLog> {
    return await get<CareerLog>(`/api/careerlogs/${careerId}`);
}

export async function assignCareerToRace(careerId: string, race: string): Promise<void> {
    await patch(`/api/careerLogs/${careerId}/Race`, race);
}

export async function fetchFacilitiesForCareer(careerId: string): Promise<FacilityEvent[]> {
    const items = await get<FacilityEvent[]>(`/api/careerlogs/${careerId}/facilities`);
    return items.filter(e => e.state != 'ConstructionCancelled');
}

export async function fetchLCsForCareer(careerId: string): Promise<LC[]> {
    const items = await get<LC[]>(`/api/careerlogs/${careerId}/lcs`);
    return items.filter(e => e.state != 'ConstructionCancelled');
}

export async function fetchLaunchesForCareer(careerId: string): Promise<LaunchEvent[]> {
    return await get<LaunchEvent[]>(`/api/careerlogs/${careerId}/launches`);
}

export async function patchLaunchMeta(careerId: string, launchID: string, meta: LaunchMetadata): Promise<void> {
    await patch(`/api/careerlogs/${careerId}/launches/${launchID}`, meta);
}

export async function fetchLeadersForCareer(careerId: string): Promise<ExtendedLeaderEvent[]> {
    return await get<ExtendedLeaderEvent[]>(`/api/careerlogs/${careerId}/leaders`);
}

export async function fetchUsers(): Promise<UserData[]> {
    return await get<UserData[]>(`/api/users`);
}

export async function fetchRaces(): Promise<string[]> {
    return await get<string[]>(`/api/careerlogs/races`);
}

export async function fetchContractsForCareer(careerId: string): Promise<BaseContractEvent[]> {
    return await get<BaseContractEvent[]>(`/api/careerlogs/${careerId}/contracts`);
}

export async function fetchMilestonesForCareer(careerId: string): Promise<BaseContractEvent[]> {
    return await get<BaseContractEvent[]>(`/api/careerlogs/${careerId}/completedmilestones`);
}

export async function fetchRepeatablesForCareer(careerId: string): Promise<BaseContractEvent[]> {
    return await get<BaseContractEvent[]>(`/api/careerlogs/${careerId}/completedRepeatables`);
}

export async function fetchTechUnlocksForCareer(careerId: string): Promise<TechUnlockItem[]> {
    return await get<TechUnlockItem[]>(`/api/careerlogs/${careerId}/tech`);
}

export async function fetchProgramsForCareer(careerId: string): Promise<ProgramItem[]> {
    return await get<ProgramItem[]>(`/api/careerlogs/${careerId}/programs`);
}

export async function fetchCareerListItems(filters: Filters): Promise<CareerListItem[]> {
    const odata = await get<ODateResp>(`/odata/careerListItems${constructFilterQueryString(filters, false)}`);
    return odata.value as CareerListItem[];
}

export async function fetchContracts(contractName: string, filters: Filters): Promise<ContractEventWithCareerInfo[]> {
    const odata = await get<ODateResp>(`/odata/contracts('${contractName}')${constructFilterQueryString(filters, false)}`);
    return odata.value as ContractEventWithCareerInfo[];
}

export async function fetchContractRecords(filters: Filters): Promise<ContractRecord[]> {
    const odata = await get<ODateResp>(`/odata/contractRecords${constructFilterQueryString(filters, false)}`);
    return odata.value as ContractRecord[];
}

export async function fetchPrograms(programName: string, mode: string, filters: Filters): Promise<ProgramItem[]> {
    const odata = await get<ODateResp>(`/odata/programs('${programName}')?type=${mode}&${constructFilterQueryString(filters, true)}`);
    return odata.value as ProgramItem[];
}

export async function fetchProgramRecords(mode: string, filters: Filters): Promise<ProgramRecord[]> {
    const odata = await get<ODateResp>(`/odata/programRecords?type=${mode}&${constructFilterQueryString(filters, true)}`);
    return odata.value as ProgramRecord[];
}
