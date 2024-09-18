import type { Filters } from 'types';

export function constructFilterQueryString(filters: Filters | null, omitSeparator: boolean) {
    if (filters == null) return '';

    const arr = [];
    if (filters.player) {
        arr.push(`UserLogin eq '${filters.player}'`);
    }

    if (filters.race) {
        arr.push(`Race eq '${filters.race}'`);
    }

    if (filters.ingameDateOp && filters.ingameDate && filters.ingameDate !== '1951-01-01') {
        arr.push(`EndDate ${filters.ingameDateOp} ${filters.ingameDate}T00:00:00.00Z`);
    }

    if (filters.lastUpdateOp && filters.lastUpdate) {
        arr.push(`LastUpdate ${filters.lastUpdateOp} ${filters.lastUpdate}T00:00:00.00Z`);
    }

    if (filters.rp1verOp && filters.rp1ver) {
        const split = filters.rp1ver.split('.');
        const mults = [1000000, 1000, 1];

        if (filters.rp1verOp === 'eq' && split.length === 2) {
            const num1 = parseInt(split[0]);
            const num2 = parseInt(split[1]);
            const verLowerBound = num1 * mults[0] + num2 * mults[1];
            const verUpperBound = num1 * mults[0] + (num2 + 1) * mults[1];

            arr.push(`CareerLogMeta/VersionSort ge ${verLowerBound} and CareerLogMeta/VersionSort lt ${verUpperBound}`);
        }
        else {
            let sortableVer = 0;
            for (let i = 0; i < 3; i++) {
                if (i >= split.length) break;
                const num = parseInt(split[i]);
                if (!isNaN(num)) {
                    sortableVer += num * mults[i];
                }
            }

            arr.push(`CareerLogMeta/VersionSort ${filters.rp1verOp} ${sortableVer}`);
        }
    }

    if (filters.difficulty) {
        arr.push(`CareerLogMeta/DifficultyLevel eq '${filters.difficulty}'`);
    }

    if (filters.playstyle) {
        arr.push(`CareerLogMeta/CareerPlaystyle eq '${filters.playstyle}'`);
    }

    if (arr.length === 0) return '';

    return `${omitSeparator ? '' : '?'}$filter=${arr.join(' and ')}`;
}
