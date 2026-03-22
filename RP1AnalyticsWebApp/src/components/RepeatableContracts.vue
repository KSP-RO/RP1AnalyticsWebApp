<template>
    <div v-show="isVisible">
        <h2 class="subtitle">Completed Repeatables</h2>

        <table class="table is-bordered is-fullwidth">
            <thead>
                <tr>
                    <th>Name</th>
                    <th># of Completions</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.contractDisplayName }}</td>
                    <td>{{ item.count }}</td>
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
    import { fetchRepeatablesForCareer } from '../utils/api';
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
            items.value = await fetchRepeatablesForCareer(careerId);
        } finally {
            isLoading.value = false;
        }
    }

    const { isVisible, isSpinnerShown } = useDataTab('repeatables', props, items, isLoading, queryData);
</script>
