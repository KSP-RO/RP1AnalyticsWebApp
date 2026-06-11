import type { VersionFilterOp } from 'types';

// Mirrors the backend CareerLogMeta.CreateSortableVersion:
//   sort = major * 1_000_000 + minor * 1_000 + build
const MAJOR_SCALE = 1_000_000;
const MINOR_SCALE = 1_000;

interface ParsedVersion {
    sort: number;
    // Size of the version "bucket" the value addresses: a bare major spans a
    // whole major line (x.*), major.minor spans a minor line, a full version
    // spans a single build.
    span: number;
}

function parseVersionValue(value: string): ParsedVersion | null {
    const parts = value.split('.').map(part => part.trim());
    if (parts.length === 0 || parts.length > 3 || parts.some(part => !/^\d+$/.test(part))) {
        return null;
    }

    const major = Number(parts[0]);
    const minor = parts.length > 1 ? Number(parts[1]) : 0;
    const build = parts.length > 2 ? Number(parts[2]) : 0;
    const sort = major * MAJOR_SCALE + minor * MINOR_SCALE + build;
    const span = parts.length >= 3 ? 1 : parts.length === 2 ? MINOR_SCALE : MAJOR_SCALE;
    return { sort, span };
}

export interface VersionSortBounds {
    min: number | null;
    max: number | null;
}

// Resolves an operator + value into inclusive-min / exclusive-max VersionSort
// bounds. Returns null when no version filter is active or the value is unparseable.
export function versionSortBounds(op: VersionFilterOp, value: string | null | undefined): VersionSortBounds | null {
    if (op === 'Any' || !value) return null;

    const parsed = parseVersionValue(value);
    if (!parsed) return null;

    const { sort, span } = parsed;
    switch (op) {
        case 'Is': return { min: sort, max: sort + span };
        case 'AtLeast': return { min: sort, max: null };
        case 'AtMost': return { min: null, max: sort + span };
        default: return null;
    }
}

// "3" -> "3.x", "3.1" -> "3.1.x", "3.1.0" -> "3.1.0"
export function formatVersionValue(value: string): string {
    const parts = value.split('.');
    return parts.length >= 3 ? value : `${value}.x`;
}

// Human-readable label for chips and cohort descriptions, e.g. "3.x" or "≥ 3.x".
export function versionFilterLabel(op: VersionFilterOp, value: string | null | undefined): string | null {
    if (op === 'Any' || !value) return null;

    const display = formatVersionValue(value);
    switch (op) {
        case 'Is': return display;
        case 'AtLeast': return `≥ ${display}`;
        case 'AtMost': return `≤ ${display}`;
        default: return null;
    }
}
