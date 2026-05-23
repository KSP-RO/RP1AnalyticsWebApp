<template>
    <ActiveFiltersSummary :filters="filters" />

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

<script setup lang="ts">
    import { ref, watch, useTemplateRef } from 'vue';
    import { activeFilters } from '../utils/activeFilters';
    import type { ProgramRecord, ContractEventWithCareerInfo } from 'types';
    import ProgramRecordTypeSelect from '../components/ProgramRecordTypeSelect.vue';
    import ActiveFiltersSummary from '../components/ActiveFiltersSummary.vue';
    import ProgramRecordsTable from '../components/ProgramRecordsTable.vue';
    import ProgramLeaderboardModal from '../components/ProgramLeaderboardModal.vue';
    import ContractRecordsTable from '../components/ContractRecordsTable.vue';
    import ContractLeaderboardModal from '../components/ContractLeaderboardModal.vue';

    const programsMode = ref('completed');
    const programName = ref<string | null>(null);
    const contractName = ref<string | null>(null);
    const filters = activeFilters;

    const programModal = useTemplateRef<typeof ProgramLeaderboardModal>('programModal');
    const contractModal = useTemplateRef<typeof ContractLeaderboardModal>('contractModal');

    watch(filters, () => {
        contractName.value = null;
    }, { deep: true });

    watch(programsMode, () => {
        programName.value = null;    // prevent dialog showing outdated data when clicking on a program that was already loaded before but in different mode
    });

    function handleChangeActive(tabName: string) {
        programsMode.value = tabName;
    }

    function showProgramLeaderboard(program: ProgramRecord) {
        if (programName.value === program.programName) {
            programModal.value!.show();
        }
        programName.value = program.programName;
    }

    function showContractLeaderboard(contract: ContractEventWithCareerInfo) {
        if (contractName.value === contract.contractInternalName) {
            contractModal.value!.show();
        }
        contractName.value = contract.contractInternalName;
    }
</script>
