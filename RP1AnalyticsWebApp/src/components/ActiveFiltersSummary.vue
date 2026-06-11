<template>
    <div class="filter-summary" :class="{ 'is-empty': chips.length === 0 }">
        <span class="filter-summary__label">Active filters</span>
        <span v-if="chips.length === 0" class="filter-summary__chip">All careers</span>
        <span v-for="chip in chips" :key="chip" class="filter-summary__chip">{{ chip }}</span>
    </div>
</template>

<script setup lang="ts">
    import { computed } from 'vue';
    import type { Filters } from '../types';
    import { versionFilterLabel } from '../utils/versionFilter';

    const props = defineProps<{
        filters: Filters;
    }>();

    const chips = computed(() => {
        const arr: string[] = [];
        pushList(arr, 'Player', props.filters.players);
        pushList(arr, 'Race', props.filters.races);
        const versionLabel = versionFilterLabel(props.filters.rp1VersionOp, props.filters.rp1Version);
        if (versionLabel) arr.push(`RP-1: ${versionLabel}`);
        pushList(arr, 'Difficulty', props.filters.difficulties);
        pushList(arr, 'Playstyle', props.filters.playstyles);
        if (props.filters.recordEligibility !== 'All') {
            arr.push(props.filters.recordEligibility === 'Eligible' ? 'Records eligible' : 'Records ineligible');
        }
        pushDate(arr, 'Career date', props.filters.careerDateMode, props.filters.careerDateStart, props.filters.careerDateEnd);
        pushDate(arr, 'Last update', props.filters.lastUpdateMode, props.filters.lastUpdateStart, props.filters.lastUpdateEnd);
        return arr;
    });

    function pushList(arr: string[], label: string, values: string[]) {
        if (values.length === 0) return;
        if (values.length <= 2) {
            arr.push(`${label}: ${values.join(', ')}`);
            return;
        }

        arr.push(`${label}: ${values.length} selected`);
    }

    function pushDate(arr: string[], label: string, mode: Filters['careerDateMode'], start: string | null, end: string | null) {
        if (mode === 'All') return;
        if (mode === 'Before') arr.push(`${label}: before ${end || '-'}`);
        else if (mode === 'After') arr.push(`${label}: after ${start || '-'}`);
        else arr.push(`${label}: ${start || '-'} to ${end || '-'}`);
    }
</script>

<style scoped>
    .filter-summary {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 0.45rem;
        margin: 0.75rem 0 1rem;
        color: var(--rp-muted, #65717f);
    }

    .filter-summary__label {
        font-size: 0.72rem;
        font-weight: 700;
        letter-spacing: 0;
        text-transform: uppercase;
    }

    .filter-summary__chip {
        border: 1px solid rgba(50, 64, 80, 0.18);
        border-radius: 999px;
        padding: 0.25rem 0.6rem;
        background: rgba(255, 255, 255, 0.72);
        color: #27313f;
        font-size: 0.82rem;
        line-height: 1.2;
    }

    @media (prefers-color-scheme: dark) {
        .filter-summary__chip {
            background: rgba(255, 255, 255, 0.06);
            border-color: rgba(255, 255, 255, 0.16);
            color: #e8edf5;
        }
    }
</style>
