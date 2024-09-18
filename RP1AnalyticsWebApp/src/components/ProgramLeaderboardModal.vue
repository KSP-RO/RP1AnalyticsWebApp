<template>
    <div id="modal2" class="modal" :class="{ 'is-active': isVisible }">
        <div class="modal-background" @click="closeModal"></div>
        <div class="modal-content">
            <div id="careerDates" class="contracts-app">
                <CareerDates :items="items" :date-field="dateField" :extra-fields="extraFields" :title="dlgTitle" />
            </div>
        </div>
        <button @click="closeModal" class="modal-close is-large" aria-label="close"></button>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import type { ProgramItem, Filters } from 'types';
    import { fetchPrograms } from '../utils/api';
    import { programSpeeds } from '../utils/programSpeeds';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import CareerDates from '../components/CareerDates.vue';

    interface ComponentData {
        items: ProgramItem[] | null;
        isVisible: boolean;
        isLoading: boolean;
        extraFields: [{
            title: string,
            field: string
        }];
    }

    export default defineComponent({
        components: {
            CareerDates
        },
        props: {
            programName: String,
            mode: {
                type: String,
                required: true
            },
            filters: {
                type: Object as PropType<Filters>,
                required: true
            }
        },
        data(): ComponentData {
            return {
                items: null,
                isLoading: false,
                isVisible: false,
                extraFields: [{
                    title: 'Speed',
                    field: 'speedText'
                }]
            }
        },
        methods: {
            async queryData(programName: string) {
                this.items = null;
                if (programName) {
                    this.isLoading = true;
                    try {
                        const items = await fetchPrograms(programName, this.mode, this.filters);
                        items.forEach((i) => i.speedText = this.mapSpeedToText(i.speed));
                        this.items = items;
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
            },
            mapSpeedToText(speed: keyof typeof programSpeeds) {
                if (speed == null) return '';
                return programSpeeds[speed];
            }
        },
        computed: {
            dlgTitle() {
                return this.items && this.items[0].title;
            },
            dateField() {
                return this.mode;
            }
        },
        watch: {
            programName(newProgramName: string, oldProgramName: string) {
                if (newProgramName !== oldProgramName) {
                    this.queryData(newProgramName);
                }
            }
        },
    });
</script>
