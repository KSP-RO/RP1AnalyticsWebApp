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

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import type { ContractRecord, Filters } from 'types';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import { fetchContractRecords } from '../utils/api';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    interface ComponentData {
        items: ContractRecord[] | null;
        isLoading: boolean;
    }

    export default defineComponent({
        components: {
            LoadingSpinner
        },
        props: {
            filters: {
                type: Object as PropType<Filters>,
                required: true
            }
        },
        emits: ['contractClicked'],
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
                    this.items = await fetchContractRecords(this.filters);
                }
                finally {
                    this.isLoading = false;
                }
            },
            contractClicked(item: ContractRecord) {
                this.$emit('contractClicked', item);
            },
            getCareerUrl(item: ContractRecord) {
                return `/?careerId=${item.careerId}`;
            },
            formatDate(date: string) {
                return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
            }
        },
        watch: {
            filters: {
                handler() {
                    this.queryData();
                },
                deep: true
            }
        },
        mounted() {
            this.$nextTick(function () {
                this.queryData();
            });
        }
    });
</script>
