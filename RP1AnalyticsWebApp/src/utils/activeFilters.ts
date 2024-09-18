import { reactive } from 'vue';
import type { Filters } from 'types';

export function createEmptyFilters(): Filters {
    return {
        player: null,
        race: null,
        ingameDateOp: null,
        ingameDate: null,
        lastUpdateOp: null,
        lastUpdate: null,
        rp1verOp: null,
        rp1ver: null,
        difficulty: null,
        playstyle: null
    };
}

let filters: Filters;
const sFilters = localStorage.getItem('filters');
if (sFilters) {
    filters = JSON.parse(sFilters);
}
else {
    filters = createEmptyFilters();
}

export const activeFilters: Filters = reactive(filters);
