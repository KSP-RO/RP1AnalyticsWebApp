<template>
    <div v-show="isVisible">
        <h2 class="subtitle">Unlocked Technologies</h2>

        <table class="table is-bordered is-fullwidth is-hoverable">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Rate Multiplier</th>
                    <th>Completion Date</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in items">
                    <td>{{ item.nodeDisplayName }}</td>
                    <td>{{ formatFloat(item.yearMult) }}</td>
                    <td class="date-col">{{ formatDate(item.date) }}</td>
                </tr>
            </tbody>
        </table>
    </div>
    <div v-if="isSpinnerShown" class="columns mt-4 is-centered is-vcentered">
        <LoadingSpinner />
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import { fetchTechUnlocksForCareer } from '../utils/api';
    import DataTabMixin from '../components/DataTabMixin.vue';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    export default defineComponent({
        mixins: [DataTabMixin],
        components: {
            LoadingSpinner
        },
        methods: {
            async queryData(careerId: string) {
                try {
                    this.items = await fetchTechUnlocksForCareer(careerId);
                }
                finally {
                    this.isLoading = false;
                }
            },
            formatFloat(val: number | null) {
                return typeof val === 'number' ? val.toFixed(2) : '';
            }
        },
        computed: {
            tabName() {
                return 'tech';
            }
        }
    });
</script>
