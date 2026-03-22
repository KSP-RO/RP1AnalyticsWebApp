<template>
    <div v-show="isVisible">
        <h2 class="subtitle">Facility construction and upgrades</h2>

        <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Facility</th>
                    <th>Level / Max tonnage</th>
                    <th>Started</th>
                    <th>Completed</th>
                </tr>
            </thead>
            <tbody>
                <template v-for="item in items">
                    <tr v-if="item.lcType" @click="toggleVisibility(item)" class="clickable" :class="{ 'is-selected': item.visible }">
                        <td>
                            <span class="icon is-small"><i class="fas mr-2" :class="getLCIcon(item)" aria-hidden="true"></i></span>
                            <span>{{ item.name }}</span>
                        </td>
                        <td>{{ formatMass(item.massMax) }}</td>
                        <td class="date-col">{{ formatDate(item.constrStarted) }}</td>
                        <td class="date-col">{{ formatDate(item.constrEnded) }}</td>
                    </tr>
                    <tr v-if="item.lcType && item.visible">
                        <td colspan="3">
                            <div>Cost: √{{item.modCost.toLocaleString('en', { maximumFractionDigits: 0 })}}</div>
                            <div>Height: {{item.sizeMax.y}}m</div>
                            <div>Width: {{item.sizeMax.x > item.sizeMax.z ? item.sizeMax.x : item.sizeMax.z}}m</div>
                            <div>Human-rated: {{item.isHumanRated ? 'Yes' : 'No'}}</div>
                        </td>
                    </tr>
                    <tr v-if="!item.lcType">
                        <td>{{ getFacilityTitle(item.facility) }}</td>
                        <td>{{ item.newLevel + 1 }}</td>
                        <td class="date-col">{{ formatDate(item.started) }}</td>
                        <td class="date-col">{{ formatDate(item.ended) }}</td>
                    </tr>
                </template>
            </tbody>
        </table>
    </div>
    <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
        <LoadingSpinner />
    </div>
</template>

<script setup lang="ts">
    import { ref } from 'vue';
    import { FacilityEvent, LCItem } from '../types';
    import { fetchFacilitiesForCareer, fetchLCsForCareer } from '../utils/api';
    import { useDataTab } from '../utils/useDataTab';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    const facilityDefs = {
        ResearchAndDevelopment: 'R&D',
        Administration: 'Administration',
        MissionControl: 'Mission Control',
        TrackingStation: 'Tracking Station',
        AstronautComplex: 'Astronaut Complex'
    };

    function isLCItem(item: LCItem | FacilityEvent): item is LCItem {
        return (<LCItem>item).constrStarted !== undefined;
    }

    const props = defineProps<{
        careerId?: string;
        activeTab?: string;
    }>();

    const items = ref<any[] | null>(null);
    const isLoading = ref(false);

    async function queryData(careerId: string) {
        try {
            const p1 = fetchFacilitiesForCareer(careerId);
            const p2 = fetchLCsForCareer(careerId);
            const facilityItems = await p1;
            const lcItems = await p2 as LCItem[];

            const combinedItems = [...facilityItems, ...lcItems];
            combinedItems.sort((a, b) => {
                const v1 = isLCItem(a) ? a.constrStarted : a.started;
                const v2 = isLCItem(b) ? b.constrStarted : b.started;
                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
                return 0;
            });
            items.value = combinedItems;
        } finally {
            isLoading.value = false;
        }
    }

    const { isVisible, isSpinnerShown, formatDate } = useDataTab('facilities', props, items, isLoading, queryData);

    function getLCIcon(lc: LCItem) {
        if (lc.lcType === 'Pad') return 'fa-rocket';
        if (lc.lcType === 'Hangar') return 'fa-plane';
        return '';
    }

    function toggleVisibility(item: LCItem) {
        const newState = !item.visible;
        items.value!.forEach(i => i.visible = false);
        item.visible = newState;
    }

    function formatMass(value: number) {
        if (value > 1e+38) return '∞';
        return value;
    }

    function getFacilityTitle(val: keyof typeof facilityDefs) {
        const title = facilityDefs[val];
        return title ? title : val;
    }
</script>
