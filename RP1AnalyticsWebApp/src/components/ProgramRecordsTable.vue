<template>
    <table class="table is-bordered is-fullwidth is-hoverable" v-show="!isLoading">
        <thead>
            <tr>
                <th>Program Name</th>
                <th>Date</th>
                <th>User</th>
                <th>Career</th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="r in items">
                <td>
                    <a role="button" class="modal-trigger" @click="programClicked(r)">{{r.programDisplayName}}</a>
                </td>
                <td class="date-col">{{formatDate(r.date)}}</td>
                <td>{{r.userPreferredName}}</td>
                <td>
                    <a :href="getCareerUrl(r)">{{r.careerName}}</a>
                </td>
            </tr>
        </tbody>
    </table>
    <div v-if="isLoading" class="columns mt-4 is-centered is-vcentered">
        <LoadingSpinner />
    </div>
</template>

<script setup lang="ts">
    import { ref, watch, onMounted } from 'vue';
    import type { ProgramRecord, Filters } from 'types';
    import { fetchProgramRecords } from '../utils/api';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    const props = defineProps<{
        mode: string;
        filters: Filters;
    }>();

    const emit = defineEmits<{
        'programClicked': [item: ProgramRecord];
    }>();

    const items = ref<ProgramRecord[] | null>(null);
    const isLoading = ref(false);

    async function queryData() {
        isLoading.value = true;
        try {
            items.value = await fetchProgramRecords(props.mode, props.filters);
        } finally {
            isLoading.value = false;
        }
    }

    function programClicked(item: ProgramRecord) {
        emit('programClicked', item);
    }

    function getCareerUrl(item: ProgramRecord) {
        return `/?careerId=${item.careerId}`;
    }

    function formatDate(date: string) {
        return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
    }

    watch(() => props.mode, () => {
        queryData();
    });

    watch(() => props.filters, () => {
        queryData();
    }, { deep: true });

    onMounted(() => {
        queryData();
    });
</script>
