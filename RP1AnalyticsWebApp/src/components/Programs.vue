<template>
    <div v-show="isVisible">
        <h2 class="subtitle">Programs</h2>

        <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Speed</th>
                    <th>Accepted</th>
                    <th>Objectives completed</th>
                    <th>Completed</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.title }}</td>
                    <td>{{ item.speed }}</td>
                    <td class="date-col">{{ formatDate(item.accepted) }}</td>
                    <td class="date-col">{{ formatDate(item.objectivesCompleted) }}</td>
                    <td class="date-col">{{ formatDate(item.completed) }}</td>
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
    import { fetchProgramsForCareer } from '../utils/api';
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
            items.value = await fetchProgramsForCareer(careerId);
        } finally {
            isLoading.value = false;
        }
    }

    const { isVisible, isSpinnerShown, formatDate } = useDataTab('programs', props, items, isLoading, queryData);
</script>
