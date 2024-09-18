<template>
    <div class="control has-icons-left">
        <div class="select is-rounded">
            <select @change="careerChanged($event.target.value)" class="browser-default" :value="selectedCareer">
                <option value="Select a career" disabled="" selected="">Select a career</option>
                <optgroup v-for="[key, value] in items" :label="key">
                    <option v-for="option in value" :value="option.id">
                        {{ option.name }}
                    </option>
                </optgroup>
            </select>
        </div>
        <div class="icon is-small is-left">
            <i class="fas fa-database"></i>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import type { CareerListItem, Filters } from 'types';
    import { fetchCareerListItems } from '../utils/api';

    interface ComponentData {
        items: Map<string, CareerListItem[]> | null;
        isLoading: boolean;
    }
    
    export default defineComponent({
        props: {
            careerItems: Object as PropType<CareerListItem[]>,
            selectedCareer: String,
            filters: {
                type: Object as PropType<Filters>,
                required: true
            }
        },
        emits: ['update:selectedCareer', 'careerChanged'],
        data(): ComponentData {
            return {
                items: null,
                isLoading: false
            };
        },
        methods: {
            async queryData(filters: Filters) {
                this.isLoading = true;
                try {
                    const arr = await fetchCareerListItems(filters);
                    const groupedMap = arr.reduce(
                        (entryMap, e) => entryMap.set(this.getPlayerName(e), [...entryMap.get(this.getPlayerName(e)) || [], e]),
                        new Map<string, CareerListItem[]>()
                    );

                    this.items = groupedMap;
                }
                finally {
                    this.isLoading = false;
                }
            },
            careerChanged(careerId: string) {
                this.$emit('update:selectedCareer', careerId);
                this.$emit('careerChanged', careerId);
            },
            getPlayerName(entry: CareerListItem) {
                return entry.userPreferredName ? entry.userPreferredName : entry.user;
            }
        },
        mounted() {
            this.$nextTick(function () {
                this.queryData(this.filters);
            });
        },
        watch: {
            filters: {
                handler() {
                    this.queryData(this.filters);
                },
                deep: true
            }
        }
    });
</script>
