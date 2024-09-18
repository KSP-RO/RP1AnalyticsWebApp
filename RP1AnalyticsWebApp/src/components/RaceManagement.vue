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

<script lang="ts">
    import { defineComponent } from 'vue';
    import { fetchCareerLogList, assignCareerToRace } from '../utils/api';
    import type { RaceManagementCareerListItem } from 'types';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    interface ComponentData {
        items: RaceManagementCareerListItem[] | null;
        isLoading: boolean;
    }

    export default defineComponent({
        components: {
            LoadingSpinner
        },
        data(): ComponentData {
            return {
                items: null,
                isLoading: false
            }
        },
        methods: {
            async queryData() {
                this.isLoading = true;
                try {
                    const arr = await fetchCareerLogList() as RaceManagementCareerListItem[];
                    arr.forEach(i => i.isUpdating = false);
                    this.items = arr;
                }
                finally {
                    this.isLoading = false;
                }
            },
            async saveData(item: RaceManagementCareerListItem) {
                item.isUpdating = true;
                try {
                    await assignCareerToRace(item.id, item.race);
                }
                finally {
                    item.isUpdating = false;
                }
            },
            getCareerUrl(c: RaceManagementCareerListItem) {
                return `/?careerId=${c.id}`;
            }
        },
        mounted() {
            this.queryData();
        }
    });
</script>
