<template>
    <div v-show="isVisible">
        <h2 class="subtitle">Leaders</h2>

        <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Added</th>
                    <th>Removed</th>
                    <th>Remove cost</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.title }}</td>
                    <td>{{ getTypeTitle(item.type) }}</td>
                    <td class="date-col">{{ formatDate(item.dateAdd) }}</td>
                    <td class="date-col">{{ formatDate(item.dateRemove) }}</td>
                    <td>{{ item.dateRemove ? formatFloat(item.fireCost) : ''}}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
        <LoadingSpinner />
    </div>
</template>

<script setup lang="ts">
    import { ref } from 'vue';
    import { leaderTypes } from '../utils/leaderTypes';
    import { fetchLeadersForCareer } from '../utils/api';
    import { useDataTab } from '../utils/useDataTab';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    const props = defineProps<{
        careerId?: string;
        activeTab?: string;
    }>();

    const items = ref<any[] | null>(null);
    const isLoading = ref(false);

    async function queryData(careerId: string) {
        try {
            items.value = await fetchLeadersForCareer(careerId);
        } finally {
            isLoading.value = false;
        }
    }

    const { isVisible, isSpinnerShown, formatDate } = useDataTab('leaders', props, items, isLoading, queryData);

    function formatFloat(val: number | null) {
        return typeof val === 'number' ? val.toFixed(1) : '';
    }

    function getTypeTitle(type: keyof typeof leaderTypes) {
        if (type == null) return '';
        return leaderTypes[type];
    }
</script>
