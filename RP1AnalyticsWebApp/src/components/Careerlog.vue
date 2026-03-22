<template>
    <div class="box columns is-centered">
        <div class="field">
            <CareerSelect :selected-career="careerId" :filters="filters" v-on:career-changed="handleCareerChange" />
        </div>
    </div>

    <div class="pb-5">
        <MetaInformation :title="careerTitle" :meta="careerLogMeta" :is-loading="isLoadingCareerMeta" />
    </div>

    <div id="selection-tab" class="selection-tab">
        <SelectionTab :active="activeTab" v-on:change-active="handleChangeActive" />
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

    <Chart :career="career" :contract-events="contractEvents" :programs="programs" />
</template>

<script setup lang="ts">
    import { ref, computed, onMounted } from 'vue';
    import { fetchCareerLog, fetchContractsForCareer, fetchProgramsForCareer } from '../utils/api';
    import currentUser from '../utils/currentUser';
    import { activeFilters } from '../utils/activeFilters';
    import type { CareerLog, BaseContractEvent, ExtendedCareerLogMeta, ProgramItem } from 'types';
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
    import Chart from './Chart.vue';

    const careerId = ref<string | null>(null);
    const career = ref<CareerLog | null>(null);
    const careerTitle = ref<string | null>(null);
    const careerLogMeta = ref<ExtendedCareerLogMeta | null>(null);
    const isLoadingCareerMeta = ref(false);
    const contractEvents = ref<BaseContractEvent[] | null>(null);
    const programs = ref<ProgramItem[] | null>(null);
    const activeTab = ref('milestones');
    const filters = activeFilters;

    const canEdit = computed(() =>
        career.value != null && currentUser != null && career.value.userLogin === currentUser.userName
    );

    function reset() {
        careerId.value = null;
        career.value = null;
        careerLogMeta.value = null;
        isLoadingCareerMeta.value = false;
        careerTitle.value = null;
        contractEvents.value = null;
        programs.value = null;
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
        window.history.pushState({}, '', url);
        getCareerLogs(id);
    }

    async function getCareerLogs(id: string) {
        console.log(`Getting Logs for ${id}...`);

        reset();

        if (id) {
            careerId.value = id;
            isLoadingCareerMeta.value = true;

            const p1 = fetchCareerLog(id);
            const p2 = fetchContractsForCareer(id);
            const p3 = fetchProgramsForCareer(id);

            const log = await p1;
            const meta = log.careerLogMeta as ExtendedCareerLogMeta;
            meta.lastUpdate = log.lastUpdate;
            isLoadingCareerMeta.value = false;
            careerLogMeta.value = meta;
            careerTitle.value = log.name;
            career.value = log;

            contractEvents.value = await p2;
            programs.value = await p3;
        }
    }

    onMounted(() => {
        const urlParams = new URLSearchParams(window.location.search);
        const initialCareerId = urlParams.get('careerId');
        if (initialCareerId) {
            getCareerLogs(initialCareerId);
        }

        const tabId = urlParams.get('tab');
        if (tabId) {
            activeTab.value = tabId;
        }

        window.onpopstate = () => {
            const params = new URLSearchParams(window.location.search);
            const id = params.get('careerId');
            if (id) {
                getCareerLogs(id);
            } else {
                reset();
            }

            const tab = params.get('tab');
            if (tab) {
                activeTab.value = tab;
            }
        };
    });
</script>
