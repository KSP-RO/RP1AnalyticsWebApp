<template>
    <div id="modal1" class="modal" :class="{ 'is-active': isVisible }">
        <div class="modal-background" @click="closeModal"></div>
        <div class="modal-content">
            <div id="careerDates" class="contracts-app">
                <CareerDates :items="items" date-field="date" :title="dlgTitle" />
            </div>
        </div>
        <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import type { ContractEventWithCareerInfo, Filters } from 'types';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import { fetchContracts } from '../utils/api';
    import CareerDates from '../components/CareerDates.vue';

    interface ComponentData {
        items: ContractEventWithCareerInfo[] | null;
        isVisible: boolean;
        isLoading: boolean;
    }

    export default defineComponent({
        components: {
            CareerDates
        },
        props: {
            contractName: String,
            filters: {
                type: Object as PropType<Filters>,
                required: true
            }
        },
        data(): ComponentData {
            return {
                items: null,
                isLoading: false,
                isVisible: false
            }
        },
        methods: {
            async queryData(contractName: string) {
                this.items = null;
                if (contractName) {
                    this.isLoading = true;
                    try {
                        this.items = await fetchContracts(contractName, this.filters);
                        this.isVisible = true;
                    }
                    finally {
                        this.isLoading = false;
                    }
                }
            },
            formatDate(date: string) {
                return date ? parseUtcDate(date).toFormat('yyyy-MM-dd') : '';
            },
            closeModal() {
                this.isVisible = false;
            }
        },
        computed: {
            dlgTitle() {
                return this.items && this.items[0].contractDisplayName;
            }
        },
        watch: {
            contractName(newContractName, oldContractName) {
                if (newContractName !== oldContractName) {
                    this.queryData(newContractName);
                }
            }
        },
    });
</script>
