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

<script lang="ts">
    import { defineComponent } from 'vue';
    import { fetchRepeatablesForCareer } from '../utils/api';
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
                    this.items = await fetchRepeatablesForCareer(careerId);
                }
                finally {
                    this.isLoading = false;
                }
            }
        },
        computed: {
            tabName() {
                return 'repeatables';
            }
        }
    });
</script>
