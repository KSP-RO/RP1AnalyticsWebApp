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

<script lang="ts">
    import { defineComponent } from 'vue';
    import { FacilityEvent, LCItem } from '../types';
    import { fetchFacilitiesForCareer, fetchLCsForCareer } from '../utils/api';
    import DataTabMixin from '../components/DataTabMixin.vue';
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

    export default defineComponent({
        mixins: [DataTabMixin],
        components: {
            LoadingSpinner
        },
        methods: {
            async queryData(careerId: string) {
                try {
                    const p1 = fetchFacilitiesForCareer(careerId);
                    const p2 = fetchLCsForCareer(careerId);
                    const facilityItems = await p1;
                    const lcItems = await p2 as LCItem[];

                    const combinedItems = [...facilityItems, ...lcItems];
                    combinedItems.sort((a, b) => {
                        const v1 = isLCItem(a) ? a.constrStarted : a.started;
                        const v2 = isLCItem(b) ? b.constrStarted : b.started;
                        if (v1 < v2) {
                            return -1;
                        }
                        if (v1 > v2) {
                            return 1;
                        }

                        return 0;
                    });
                    this.items = combinedItems;
                }
                finally {
                    this.isLoading = false;
                }
            },
            getLCIcon(lc: LCItem) {
                if (lc.lcType === 'Pad') return 'fa-rocket';
                if (lc.lcType === 'Hangar') return 'fa-plane';
                return '';
            },
            toggleVisibility(item: LCItem) {
                const newState = !item.visible;
                this.items.forEach(i => i.visible = false);
                item.visible = newState;
            },
            formatMass(value: number) {
                if (value > 1e+38) return '∞';
                return value;
            },
            getFacilityTitle(val: keyof typeof facilityDefs) {
                const title = facilityDefs[val];
                return title ? title : val;
            }
        },
        computed: {
            tabName() {
                return 'facilities';
            }
        }
    });
</script>
