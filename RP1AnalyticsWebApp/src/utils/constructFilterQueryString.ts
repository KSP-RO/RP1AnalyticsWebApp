import type { Filters } from 'types';

export function constructFilterQueryString(filters: Filters | null, omitSeparator: boolean) {
    if (filters == null) return '';

    const arr: string[] = [];
    const playerFilter = stringOrFilter('UserLogin', filters.players);
    if (playerFilter) arr.push(playerFilter);

    const raceFilter = stringOrFilter('Race', filters.races);
    if (raceFilter) arr.push(raceFilter);

    const careerDateFilter = dateModeFilter('EndDate', filters.careerDateMode, filters.careerDateStart, filters.careerDateEnd);
    if (careerDateFilter) arr.push(careerDateFilter);

    const lastUpdateFilter = dateModeFilter('LastUpdate', filters.lastUpdateMode, filters.lastUpdateStart, filters.lastUpdateEnd);
    if (lastUpdateFilter) arr.push(lastUpdateFilter);

    const versionFilter = stringOrFilter('CareerLogMeta/VersionTag', filters.rp1Versions);
    if (versionFilter) arr.push(versionFilter);

    const difficultyFilter = stringOrFilter('CareerLogMeta/DifficultyLevel', filters.difficulties);
    if (difficultyFilter) arr.push(difficultyFilter);

    const playstyleFilter = stringOrFilter('CareerLogMeta/CareerPlaystyle', filters.playstyles);
    if (playstyleFilter) arr.push(playstyleFilter);

    if (filters.recordEligibility !== 'All') {
        arr.push(`EligibleForRecords eq ${filters.recordEligibility === 'Eligible' ? 'true' : 'false'}`);
    }

    if (arr.length === 0) return '';

    return `${omitSeparator ? '' : '?'}$filter=${arr.join(' and ')}`;
}

function stringOrFilter(property: string, values: string[]) {
    const clean = values.map(escapeValue).filter(Boolean);
    if (clean.length === 0) return '';
    return clean.length === 1
        ? `${property} eq '${clean[0]}'`
        : `(${clean.map(value => `${property} eq '${value}'`).join(' or ')})`;
}

function dateModeFilter(property: string, mode: Filters['careerDateMode'], start: string | null, end: string | null) {
    if (mode === 'All') return '';
    if (mode === 'Before' && end) return `${property} le ${end}T00:00:00.00Z`;
    if (mode === 'After' && start) return `${property} ge ${start}T00:00:00.00Z`;
    if (mode === 'Range') {
        const parts = [];
        if (start) parts.push(`${property} ge ${start}T00:00:00.00Z`);
        if (end) parts.push(`${property} le ${end}T00:00:00.00Z`);
        return parts.join(' and ');
    }

    return '';
}

function escapeValue(value: string) {
    return value.trim().replace(/'/g, "''");
}
