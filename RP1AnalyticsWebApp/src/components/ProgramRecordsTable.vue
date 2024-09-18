<template>
    <table class="table is-bordered is-fullwidth is-hoverable" v-show="!isLoading">
        <thead>
            <tr>
                <th>Program Name</th>
                <th>Date</th>
                <th>User</th>
                <th>Career</th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="r in items">
                <td>
                    <a role="button" class="modal-trigger" @click="programClicked(r)">{{r.programDisplayName}}</a>
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
    import type { ProgramRecord, Filters } from 'types';
    import { fetchProgramRecords } from '../utils/api';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import LoadingSpinner from '../components/LoadingSpinner.vue';

    interface ComponentData {
        items: ProgramRecord[] | null;
        isLoading: boolean;
    }

    export default defineComponent({
        components: {
            LoadingSpinner
        },
        props: {
            mode: {
                type: String,
                required: true
            },
            filters: {
                type: Object as PropType<Filters>,
                required: true
            }
        },
        emits: ['programClicked'],
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
                    this.items = await fetchProgramRecords(this.mode, this.filters);;
                }
                finally {
                    this.isLoading = false;
                }
            },
            programClicked(item: ProgramRecord) {
                this.$emit('programClicked', item);
            },
            getCareerUrl(item: ProgramRecord) {
                return `/?careerId=${item.careerId}`;
            },
            formatDate(date: string) {
                return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
            }
        },
        watch: {
            mode() {
                this.queryData();
            },
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
