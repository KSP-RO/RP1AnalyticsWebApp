<template>
    <h2 class="subtitle">Program Records</h2>
    <div id="selection-tab" class="selection-tab mb-1">
        <ProgramRecordTypeSelect :active="programsMode" v-on:change-active="handleChangeActive" />
    </div>
    <ProgramRecordsTable :mode="programsMode" :filters="filters" v-on:program-clicked="showProgramLeaderboard" />
    <ProgramLeaderboardModal ref="programModal" :program-name="programName" :mode="programsMode" :filters="filters" />

    <h2 class="subtitle">Contract Records</h2>
    <ContractRecordsTable :filters="filters" v-on:contract-clicked="showContractLeaderboard" />
    <ContractLeaderboardModal ref="contractModal" :contract-name="contractName" :filters="filters" />
</template>

<script lang="ts">
    import { defineComponent, useTemplateRef } from 'vue';
    import { activeFilters } from '../utils/activeFilters';
    import type { ProgramRecord, ContractEventWithCareerInfo } from 'types';
    import ProgramRecordTypeSelect from '../components/ProgramRecordTypeSelect.vue';
    import ProgramRecordsTable from '../components/ProgramRecordsTable.vue';
    import ProgramLeaderboardModal from '../components/ProgramLeaderboardModal.vue';
    import ContractRecordsTable from '../components/ContractRecordsTable.vue';
    import ContractLeaderboardModal from '../components/ContractLeaderboardModal.vue';

    interface ComponentData {
        programsMode: string;
        programName: string | null;
        contractName: string | null;
        filters: typeof activeFilters;
    }

    export default defineComponent({
        components: {
            ProgramRecordTypeSelect,
            ProgramRecordsTable,
            ProgramLeaderboardModal,
            ContractRecordsTable,
            ContractLeaderboardModal
        },
        data(): ComponentData {
            return {
                programsMode: 'completed',
                programName: null,
                contractName: null,
                filters: activeFilters
            }
        },
        setup() {
            const programModal = useTemplateRef<typeof ProgramLeaderboardModal | null>('programModal');
            const contractModal = useTemplateRef<typeof ContractLeaderboardModal | null>('contractModal');
            return {
                programModal, contractModal
            }
        },
        watch: {
            filters: {
                handler() {
                    this.contractName = null;
                },
                deep: true
            },
            programsMode() {
                this.programName = null;    // to prevent dialog showing outdated data when clicking on a program that was already loaded before but in different mode
            }
        },
        methods: {
            handleChangeActive(tabName: string) {
                this.programsMode = tabName;
            },
            showProgramLeaderboard(program: ProgramRecord) {
                if (this.programName === program.programName) {
                    this.programModal!.isVisible = true;
                }
                this.programName = program.programName;
            },
            showContractLeaderboard(contract: ContractEventWithCareerInfo) {
                if (this.contractName === contract.contractInternalName) {
                    this.contractModal!.isVisible = true;
                }
                this.contractName = contract.contractInternalName;
            }
        }
    });
</script>
