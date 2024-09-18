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

<script lang="ts">
    import { defineComponent } from 'vue';
    import { fetchCareerLog, fetchContractsForCareer, fetchProgramsForCareer } from '../utils/api';
    import { currentUser } from '../utils/currentUser';
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

    interface ComponentData {
        careerId: string | null;
        career: CareerLog | null;
        careerTitle: string | null;
        careerLogMeta: ExtendedCareerLogMeta | null;
        isLoadingCareerMeta: boolean;
        contractEvents: BaseContractEvent[] | null;
        programs: ProgramItem[] | null;
        activeTab: string;
        filters: typeof activeFilters;
    }

    export default defineComponent({
        components: {
            CareerSelect,
            MetaInformation,
            SelectionTab,
            MilestoneContracts,
            RepeatableContracts,
            Programs,
            Launches,
            Facilities,
            TechUnlocks,
            Leaders,
            Chart
        },
        data(): ComponentData {
            return {
                careerId: null,
                career: null,
                careerTitle: null,
                careerLogMeta: null,
                programs: null,
                isLoadingCareerMeta: false,
                contractEvents: null,
                activeTab: 'milestones',
                filters: activeFilters
            };
        },
        methods: {
            reset() {
                this.careerId = null;
                this.career = null;
                this.careerLogMeta = null;
                this.isLoadingCareerMeta = false;
                this.careerTitle = null;
            },
            handleChangeActive(tabName: string) {
                this.activeTab = tabName;
                const url = new URL(window.location.href);
                url.searchParams.set('tab', tabName);
                window.history.replaceState({}, '', url);
            },
            handleCareerChange(careerId: string) {
                const url = new URL(window.location.href);
                url.searchParams.set('careerId', careerId);
                window.history.pushState({}, '', url);
                this.getCareerLogs(careerId);
            },
            async getCareerLogs(careerId: string) {
                console.log(`Getting Logs for ${careerId}...`);

                if (!careerId) {
                    this.contractEvents = null;
                    this.programs = null;
                    this.reset();
                }
                else {
                    this.careerId = careerId;
                    this.isLoadingCareerMeta = true;

                    const p1 = fetchCareerLog(careerId);
                    const p2 = fetchContractsForCareer(careerId);
                    const p3 = fetchProgramsForCareer(careerId);

                    const log = await p1;
                    const meta = log.careerLogMeta as ExtendedCareerLogMeta;
                    meta.lastUpdate = log.lastUpdate;
                    this.isLoadingCareerMeta = false;
                    this.careerLogMeta = meta;
                    this.careerTitle = log.name;
                    this.career = log;
                    
                    this.contractEvents = await p2;
                    this.programs = await p3;
                }
            }
        },
        computed: {
            canEdit(): boolean {
                return this.career != null && currentUser != null && this.career.userLogin === currentUser.userName;
            }
        },
        mounted() {
            this.$nextTick(function () {
                const urlParams = new URLSearchParams(window.location.search);
                const initialCareerId = urlParams.get('careerId');
                if (initialCareerId) {
                    this.getCareerLogs(initialCareerId);
                }

                const tabId = urlParams.get('tab');
                if (tabId) {
                    this.activeTab = tabId;
                }
            });

            window.onpopstate = () => {
                const urlParams = new URLSearchParams(window.location.search);
                const initialCareerId = urlParams.get('careerId');
                if (initialCareerId) {
                    this.getCareerLogs(initialCareerId);
                }
                else {
                    this.reset();
                }

                const tabId = urlParams.get('tab');
                if (tabId) {
                    this.activeTab = tabId;
                }
            }
        }
    });
</script>
