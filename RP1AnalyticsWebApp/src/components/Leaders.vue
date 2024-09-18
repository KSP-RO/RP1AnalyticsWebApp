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

<script lang="ts">
    import { defineComponent } from 'vue';
    import { leaderTypes } from '../utils/leaderTypes';
    import { fetchLeadersForCareer } from '../utils/api';
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
                    this.items = await fetchLeadersForCareer(careerId);
                }
                finally {
                    this.isLoading = false;
                }
            },
            formatFloat(val: number | null) {
                return typeof val === 'number' ? val.toFixed(1) : '';
            },
            getTypeTitle(type: keyof typeof leaderTypes) {
                if (type == null) return '';
                return leaderTypes[type];
            }
        },
        computed: {
            tabName() {
                return 'leaders';
            }
        }
    });
</script>
