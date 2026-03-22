<template>
    <div id="modal2" class="modal" :class="{ 'is-active': isVisible }">
        <div class="modal-background" @click="closeModal"></div>
        <div class="modal-content">
            <div id="careerDates" class="contracts-app">
                <CareerDates :items="items" :date-field="dateField" :extra-fields="extraFields" :title="dlgTitle" />
            </div>
        </div>
        <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
    </div>
</template>

<script setup lang="ts">
    import { ref, computed, watch } from 'vue';
    import type { ProgramItem, Filters } from 'types';
    import { fetchPrograms } from '../utils/api';
    import CareerDates from '../components/CareerDates.vue';

    const props = defineProps<{
        programName?: string;
        mode: string;
        filters: Filters;
    }>();

    const items = ref<ProgramItem[] | null>(null);
    const isVisible = ref(false);
    const isLoading = ref(false);
    const extraFields = [{ title: 'Speed', field: 'speed' }];

    const dlgTitle = computed(() => items.value && items.value[0].title);
    const dateField = computed(() => props.mode);

    async function queryData(programName: string) {
        items.value = null;
        if (programName) {
            isLoading.value = true;
            try {
                items.value = await fetchPrograms(programName, props.mode, props.filters);
                isVisible.value = true;
            } finally {
                isLoading.value = false;
            }
        }
    }

    function closeModal() {
        isVisible.value = false;
    }

    watch(() => props.programName, (newProgramName, oldProgramName) => {
        if (newProgramName !== oldProgramName) {
            queryData(newProgramName!);
        }
    });

    function show() {
        isVisible.value = true;
    }

    defineExpose({ show });
</script>
