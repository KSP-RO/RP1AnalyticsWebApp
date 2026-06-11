import { reactive } from 'vue';
import type { DateFilterMode, Filters, RecordEligibilityFilter, VersionFilterOp } from 'types';

export function createEmptyFilters(): Filters {
    return {
        players: [],
        races: [],
        careerDateMode: 'All',
        careerDateStart: null,
        careerDateEnd: null,
        lastUpdateMode: 'All',
        lastUpdateStart: null,
        lastUpdateEnd: null,
        rp1VersionOp: 'Any',
        rp1Version: null,
        difficulties: [],
        playstyles: [],
        recordEligibility: 'All'
    };
}

export function normalizeFilters(value: Partial<Filters> & Record<string, unknown> | null | undefined): Filters {
    const empty = createEmptyFilters();
    if (!value) return empty;

    const version = versionFilter(value);

    return {
        players: stringArray(value.players, value.player),
        races: stringArray(value.races, value.race),
        careerDateMode: dateMode(value.careerDateMode, value.ingameDateOp, value.ingameDate),
        careerDateStart: stringValue(value.careerDateStart) ?? legacyDateStart(value.ingameDateOp, value.ingameDate),
        careerDateEnd: stringValue(value.careerDateEnd) ?? legacyDateEnd(value.ingameDateOp, value.ingameDate),
        lastUpdateMode: dateMode(value.lastUpdateMode, value.lastUpdateOp, value.lastUpdate),
        lastUpdateStart: stringValue(value.lastUpdateStart) ?? legacyDateStart(value.lastUpdateOp, value.lastUpdate),
        lastUpdateEnd: stringValue(value.lastUpdateEnd) ?? legacyDateEnd(value.lastUpdateOp, value.lastUpdate),
        rp1VersionOp: version.op,
        rp1Version: version.version,
        difficulties: stringArray(value.difficulties, value.difficulty),
        playstyles: stringArray(value.playstyles, value.playstyle),
        recordEligibility: eligibility(value.recordEligibility)
    };
}

function versionFilter(value: Record<string, unknown>): { op: VersionFilterOp; version: string | null } {
    const op = value.rp1VersionOp;
    if (op === 'Any') {
        return { op: 'Any', version: null };
    }
    if (op === 'Is' || op === 'AtLeast' || op === 'AtMost') {
        const version = stringValue(value.rp1Version);
        return version ? { op, version } : { op: 'Any', version: null };
    }

    // Legacy: original operator + free-text control (rp1verOp / rp1ver).
    if (typeof value.rp1ver === 'string' && value.rp1ver.trim()) {
        const legacyOp: VersionFilterOp = value.rp1verOp === 'le' ? 'AtMost' : value.rp1verOp === 'ge' ? 'AtLeast' : 'Is';
        return { op: legacyOp, version: value.rp1ver.trim() };
    }

    // Legacy: discrete multi-select list. A single value maps cleanly; an
    // arbitrary set cannot be expressed as a range, so it is dropped.
    const list = stringArray(value.rp1Versions);
    return list.length === 1 ? { op: 'Is', version: list[0] } : { op: 'Any', version: null };
}

function stringArray(primary: unknown, legacy?: unknown) {
    const values = Array.isArray(primary) ? primary : legacy ? [legacy] : [];
    return [...new Set(values
        .map(v => typeof v === 'string' ? v.trim() : '')
        .filter(v => v.length > 0))];
}

function stringValue(value: unknown) {
    return typeof value === 'string' && value.trim() ? value : null;
}

function dateMode(value: unknown, legacyOp?: unknown, legacyDate?: unknown): DateFilterMode {
    if (value === 'Range' || value === 'Before' || value === 'After' || value === 'All') {
        return value;
    }

    if (typeof legacyDate === 'string' && legacyDate && legacyDate !== '1951-01-01') {
        return legacyOp === 'le' ? 'Before' : 'After';
    }

    return 'All';
}

function legacyDateStart(op: unknown, date: unknown) {
    return op === 'ge' && typeof date === 'string' && date !== '1951-01-01' ? date : null;
}

function legacyDateEnd(op: unknown, date: unknown) {
    return op === 'le' && typeof date === 'string' && date !== '1951-01-01' ? date : null;
}

function eligibility(value: unknown): RecordEligibilityFilter {
    if (value === 'Eligible' || value === 'Ineligible') return value;
    return 'All';
}

let filters: Filters;
const sFilters = localStorage.getItem('filters');
if (sFilters) {
    filters = normalizeFilters(JSON.parse(sFilters));
}
else {
    filters = createEmptyFilters();
}

export const activeFilters: Filters = reactive(filters);
