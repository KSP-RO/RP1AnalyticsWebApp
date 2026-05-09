<template>
    <CareersDashboard v-if="!careerId" :filters="filters" />

    <template v-else>
        <div class="career-workspace">
            <div class="career-selector">
                <CareerSelect :selected-career="careerId" :filters="filters" v-on:career-changed="handleCareerChange" />
                <ActiveFiltersSummary :filters="filters" />
            </div>
        </div>

        <div class="career-summary-wrap">
            <MetaInformation
                :title="careerTitle"
                :meta="careerLogMeta"
                :current-in-game-date="career?.endDate"
                :is-loading="isLoadingCareerMeta" />
        </div>

        <div id="selection-tab" class="selection-tab">
            <SelectionTab :active="activeTab" v-on:change-active="handleChangeActive" />
        </div>

    <div id="comparison" class="comparison-tab" v-if="activeTab === 'comparison'">
        <ComparisonDashboard
            :comparison="comparison"
            :is-loading="isLoadingComparison" />
    </div>

        <div id="milestones" class="contracts-app">
            <MilestoneContracts :active-tab="activeTab" :career-id="careerId" />
        </div>

        <div id="repeatables" class="contracts-app">
            <RepeatableContracts :active-tab="activeTab" :career-id="careerId" />
        </div>

        <div id="programs" class="contracts-app">
            <Programs :active-tab="activeTab" :career-id="careerId" />
        </div>

        <div id="tech" class="contracts-app">
            <TechUnlocks :active-tab="activeTab" :career-id="careerId" />
        </div>

        <div id="launches" class="contracts-app">
            <Launches :active-tab="activeTab" :career-id="careerId" :can-edit="canEdit" />
        </div>

        <div id="facilities" class="contracts-app">
            <Facilities :active-tab="activeTab" :career-id="careerId" />
        </div>

        <div id="leaders" class="contracts-app">
            <Leaders :active-tab="activeTab" :career-id="careerId" />
        </div>

        <div v-if="activeTab !== 'comparison'">
            <Chart :career="career" :contract-events="contractEvents" :programs="programs" />
        </div>
    </template>
</template>

<script setup lang="ts">
    import { ref, computed, onMounted, watch, defineAsyncComponent } from 'vue';
    import { fetchCareerComparison, fetchCareerLog, fetchContractsForCareer, fetchProgramsForCareer } from '../utils/api';
    import currentUser from '../utils/currentUser';
    import { activeFilters } from '../utils/activeFilters';
    import type { CareerComparison, CareerLog, BaseContractEvent, ExtendedCareerLogMeta, ProgramItem } from 'types';
    import ActiveFiltersSummary from './ActiveFiltersSummary.vue';
    import CareersDashboard from './CareersDashboard.vue';
    import CareerSelect from './CareerSelect.vue';
    import MetaInformation from './MetaInformation.vue';
    import SelectionTab from './SelectionTab.vue';
    import MilestoneContracts from './MilestoneContracts.vue';
    import RepeatableContracts from './RepeatableContracts.vue';
    import Programs from './Programs.vue';
    import Launches from './Launches.vue';
    import Facilities from './Facilities.vue';
    import TechUnlocks from './TechUnlocks.vue';
    import Leaders from './Leaders.vue';

    const ComparisonDashboard = defineAsyncComponent(() => import('./ComparisonDashboard.vue'));
    const Chart = defineAsyncComponent(() => import('./Chart.vue'));

    const initialCareerIdFromUrl = new URLSearchParams(window.location.search).get('careerId');
    const careerId = ref<string | null>(initialCareerIdFromUrl);
    const career = ref<CareerLog | null>(null);
    const careerTitle = ref<string | null>(null);
    const careerLogMeta = ref<ExtendedCareerLogMeta | null>(null);
    const isLoadingCareerMeta = ref(false);
    const isLoadingComparison = ref(false);
    const contractEvents = ref<BaseContractEvent[] | null>(null);
    const programs = ref<ProgramItem[] | null>(null);
    const comparison = ref<CareerComparison | null>(null);
    const activeTab = ref('milestones');
    const filters = activeFilters;
    const loadedChartCareerId = ref<string | null>(null);
    const loadedComparisonKey = ref<string | null>(null);
    let loadSerial = 0;

    const canEdit = computed(() =>
        career.value != null && currentUser != null && career.value.userLogin === currentUser.userName
    );

    function reset() {
        loadSerial += 1;
        careerId.value = null;
        career.value = null;
        careerLogMeta.value = null;
        isLoadingCareerMeta.value = false;
        isLoadingComparison.value = false;
        careerTitle.value = null;
        contractEvents.value = null;
        programs.value = null;
        comparison.value = null;
        loadedChartCareerId.value = null;
        loadedComparisonKey.value = null;
    }

    function handleChangeActive(tabName: string) {
        activeTab.value = tabName;
        const url = new URL(window.location.href);
        url.searchParams.set('tab', tabName);
        window.history.replaceState({}, '', url);
    }

    function handleCareerChange(id: string) {
        const url = new URL(window.location.href);
        url.searchParams.set('careerId', id);
        url.searchParams.set('tab', 'comparison');
        activeTab.value = 'comparison';
        window.history.pushState({}, '', url);
        getCareerLogs(id);
    }

    async function getCareerComparison(id: string) {
        const key = `${id}:${JSON.stringify(filters)}`;
        if (comparison.value && loadedComparisonKey.value === key) {
            return;
        }

        const serial = loadSerial;
        isLoadingComparison.value = true;
        try {
            const nextComparison = await fetchCareerComparison(id, filters);
            if (serial !== loadSerial) return;
            comparison.value = nextComparison;
            loadedComparisonKey.value = key;
        } finally {
            if (serial === loadSerial) {
                isLoadingComparison.value = false;
            }
        }
    }

    async function getCareerTabData(id: string) {
        if (loadedChartCareerId.value === id && contractEvents.value != null && programs.value != null) {
            return;
        }

        const serial = loadSerial;
        const [nextContracts, nextPrograms] = await Promise.all([
            fetchContractsForCareer(id),
            fetchProgramsForCareer(id)
        ]);

        if (serial !== loadSerial) return;

        contractEvents.value = nextContracts;
        programs.value = nextPrograms;
        loadedChartCareerId.value = id;
    }

    async function loadActiveTabData(id: string) {
        if (activeTab.value === 'comparison') {
            await getCareerComparison(id);
            return;
        }

        void getCareerTabData(id);
    }

    async function getCareerLogs(id: string) {
        console.log(`Getting Logs for ${id}...`);

        reset();
        const serial = loadSerial;

        if (id) {
            careerId.value = id;
            isLoadingCareerMeta.value = true;

            const log = await fetchCareerLog(id);
            if (serial !== loadSerial) return;
            const meta = log.careerLogMeta as ExtendedCareerLogMeta;
            meta.lastUpdate = log.lastUpdate;
            isLoadingCareerMeta.value = false;
            careerLogMeta.value = meta;
            careerTitle.value = log.name;
            career.value = log;

            await loadActiveTabData(id);
        }
    }

    onMounted(() => {
        const urlParams = new URLSearchParams(window.location.search);
        const initialCareerId = urlParams.get('careerId');
        const tabId = urlParams.get('tab');

        if (tabId) {
            activeTab.value = tabId;
        }

        if (initialCareerId) {
            getCareerLogs(initialCareerId);
        }

        window.onpopstate = () => {
            const params = new URLSearchParams(window.location.search);
            const id = params.get('careerId');
            const tab = params.get('tab');
            if (tab) {
                activeTab.value = tab;
            }
            if (id) {
                getCareerLogs(id);
            } else {
                reset();
            }
        };
    });

    watch(activeTab, (newTab, oldTab) => {
        if (newTab === oldTab || !careerId.value) return;

        if (newTab === 'comparison') {
            void getCareerComparison(careerId.value);
            return;
        }

        if (!contractEvents.value || !programs.value || loadedChartCareerId.value !== careerId.value) {
            void getCareerTabData(careerId.value);
        }
    });

    watch(filters, () => {
        if (careerId.value && activeTab.value === 'comparison') {
            getCareerComparison(careerId.value);
        }
    }, { deep: true });
</script>

<style scoped>
    .career-workspace {
        margin-top: 1rem;
    }

    .career-selector {
        border: 1px solid rgba(36, 48, 63, 0.14);
        border-radius: 8px;
        padding: 1rem 1rem 0.1rem;
        background: rgba(255, 255, 255, 0.72);
    }

    .career-summary-wrap {
        margin: 1rem 0;
    }

    .selection-tab {
        position: sticky;
        top: 0;
        z-index: 5;
        margin: 1rem 0;
        padding: 0.35rem 0;
        background: color-mix(in srgb, canvas 88%, transparent);
        backdrop-filter: blur(10px);
    }

    .comparison-tab {
        margin-bottom: 1.5rem;
    }

    @media (prefers-color-scheme: dark) {
        .career-selector {
            border-color: rgba(255, 255, 255, 0.12);
            background: rgba(255, 255, 255, 0.04);
        }

        .selection-tab {
            background: rgba(16, 20, 27, 0.82);
        }
    }
</style>
