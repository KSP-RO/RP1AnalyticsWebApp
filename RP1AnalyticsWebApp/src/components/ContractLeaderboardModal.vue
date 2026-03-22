<template>
    <div id="modal1" class="modal" :class="{ 'is-active': isVisible }">
        <div class="modal-background" @click="closeModal"></div>
        <div class="modal-content">
            <div id="careerDates" class="contracts-app">
                <CareerDates :items="items" date-field="date" :title="dlgTitle" />
            </div>
        </div>
        <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
    </div>
</template>

<script setup lang="ts">
    import { ref, computed, watch } from 'vue';
    import type { ContractEventWithCareerInfo, Filters } from 'types';
    import { fetchContracts } from '../utils/api';
    import CareerDates from '../components/CareerDates.vue';

    const props = defineProps<{
        contractName?: string;
        filters: Filters;
    }>();

    const items = ref<ContractEventWithCareerInfo[] | null>(null);
    const isVisible = ref(false);
    const isLoading = ref(false);

    const dlgTitle = computed(() => items.value && items.value[0].contractDisplayName);

    async function queryData(contractName: string) {
        items.value = null;
        if (contractName) {
            isLoading.value = true;
            try {
                items.value = await fetchContracts(contractName, props.filters);
                isVisible.value = true;
            } finally {
                isLoading.value = false;
            }
        }
    }

    function closeModal() {
        isVisible.value = false;
    }

    watch(() => props.contractName, (newContractName, oldContractName) => {
        if (newContractName !== oldContractName) {
            queryData(newContractName!);
        }
    });

    function show() {
        isVisible.value = true;
    }

    defineExpose({ show });
</script>
