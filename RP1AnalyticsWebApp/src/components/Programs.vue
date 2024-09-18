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
                    <td>{{ mapSpeedToText(item.speed) }}</td>
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

<script lang="ts">
    import { defineComponent } from 'vue';
    import { fetchProgramsForCareer } from '../utils/api';
    import { programSpeeds } from '../utils/programSpeeds';
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
                    this.items = await fetchProgramsForCareer(careerId);
                }
                finally {
                    this.isLoading = false;
                }
            },
            mapSpeedToText(speed: keyof typeof programSpeeds) {
                if (speed == null) return '';
                return programSpeeds[speed];
            }
        },
        computed: {
            tabName() {
                return 'programs';
            }
        }
    });
</script>
