<template>
    <h2 class="subtitle">Careers</h2>
    <table class="table is-bordered is-fullwidth" v-show="!isLoading">
        <thead>
            <tr>
                <th>User</th>
                <th>Career</th>
                <th>Race</th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="item in items">
                <td class="is-vcentered">{{item.userPreferredName}}</td>
                <td class="is-vcentered"><a :href="getCareerUrl(item)">{{item.name}}</a></td>
                <td class="is-vcentered">
                    <form>
                        <div class="field has-addons">
                            <div class="control is-expanded">
                                <input class="input" type="text" name="ris-name" autocomplete="on" v-model="item.race" />
                            </div>
                            <div class="control">
                                <button type="button" class="button is-primary" v-on:click="saveData(item)" :class="{ 'is-loading': item.isUpdating }">
                                    <span class="icon is-small"><i class="far fa-save fa-lg"></i></span>
                                </button>
                            </div>
                        </div>
                    </form>
                </td>
            </tr>
        </tbody>
    </table>
    <div v-if="isLoading" class="columns mt-4 is-centered is-vcentered">
        <LoadingSpinner />
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import { fetchCareerLogList, assignCareerToRace } from '../utils/api';
    import type { RaceManagementCareerListItem } from 'types';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    const items = ref<RaceManagementCareerListItem[] | null>(null);
    const isLoading = ref(false);

    async function queryData() {
        isLoading.value = true;
        try {
            const arr = await fetchCareerLogList() as RaceManagementCareerListItem[];
            arr.forEach(i => i.isUpdating = false);
            items.value = arr;
        } finally {
            isLoading.value = false;
        }
    }

    async function saveData(item: RaceManagementCareerListItem) {
        item.isUpdating = true;
        try {
            await assignCareerToRace(item.id, item.race);
        } finally {
            item.isUpdating = false;
        }
    }

    function getCareerUrl(c: RaceManagementCareerListItem) {
        return `/?careerId=${c.id}`;
    }

    onMounted(() => {
        queryData();
    });
</script>
