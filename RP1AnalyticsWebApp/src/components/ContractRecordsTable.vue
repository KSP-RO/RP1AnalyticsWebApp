<template>
    <table class="table is-bordered is-fullwidth is-hoverable" v-show="!isLoading">
        <thead>
            <tr>
                <th>Contract Name</th>
                <th>Completion Date</th>
                <th>User</th>
                <th>Career</th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="r in items">
                <td>
                    <a role="button" class="modal-trigger" @click="contractClicked(r)">{{r.contractDisplayName}}</a>
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
    import type { ContractRecord, Filters } from 'types';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import { fetchContractRecords } from '../utils/api';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    const props = defineProps<{
        filters: Filters;
    }>();

    const emit = defineEmits<{
        'contractClicked': [item: ContractRecord];
    }>();

    const items = ref<ContractRecord[] | null>(null);
    const isLoading = ref(false);

    async function queryData() {
        isLoading.value = true;
        try {
            items.value = await fetchContractRecords(props.filters);
        } finally {
            isLoading.value = false;
        }
    }

    function contractClicked(item: ContractRecord) {
        emit('contractClicked', item);
    }

    function getCareerUrl(item: ContractRecord) {
        return `/?careerId=${item.careerId}`;
    }

    function formatDate(date: string) {
        return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
    }

    watch(() => props.filters, () => {
        queryData();
    }, { deep: true });

    onMounted(() => {
        queryData();
    });
</script>
