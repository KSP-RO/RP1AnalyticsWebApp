<template>
    <div class="combo-box control dropdown is-expanded is-rounded" :class="{ 'is-active': isOpen, 'is-loading': isLoading }">
        <div class="dropdown-trigger">
            <div class="control has-icons-left has-icons-right">
                <input type="text"
                       v-model="inputText"
                       @focus="isOpen = true"
                       placeholder="Select a career"
                       class="combo-input input is-rounded" />
                <div class="icon is-small is-left">
                    <i class="fas fa-database"></i>
                </div>
                <span v-if="!isLoading" class="icon is-small is-right">
                    <i class="fas fa-angle-down" aria-hidden="true"></i>
                </span>
            </div>
        </div>

        <div v-if="isOpen" class="combo-dropdown dropdown-menu" role="menu">
            <div class="dropdown-content">
                <template v-for="[key, value] in filteredGroups" :key="key">
                    <div class="combo-group-label dropdown-item">
                        {{ key }}
                    </div>

                    <button v-for="option in value"
                            :key="option.id"
                            @click="selectValue(option)"
                            class="combo-option dropdown-item">
                        {{ option.name }}
                    </button>
                </template>

                <div v-if="filteredGroups.size === 0" class="dropdown-item">
                    No results
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .combo-box input {
        min-width: 20rem;
    }

    .combo-group-label {
        font-weight: bold;
    }

    .combo-group-label:not(:first-child) {
        margin-top: 6px;
    }

    .combo-option {
        padding-left: 1.5rem;
    }
</style>

<script lang="ts">
    import { defineComponent } from 'vue';
    import type { PropType } from 'vue'
    import type { CareerListItem, Filters } from 'types';
    import { fetchCareerListItems } from '../utils/api';

    interface ComponentData {
        items: CareerListItem[] | null;
        isLoading: boolean;
        isOpen: boolean;
        inputText: string;
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
                isLoading: false,
                isOpen: false,
                inputText: ''
            };
        },
        methods: {
            async queryData(filters: Filters) {
                this.isLoading = true;
                try {
                    const arr = await fetchCareerListItems(filters);
                    this.items = arr;
                    this.updateInputText();
                }
                finally {
                    this.isLoading = false;
                }
            },
            selectValue(value) {
                this.inputText = value.name;
                this.isOpen = false;
                this.careerChanged(value.id);
            },
            updateInputText() {
                const selItem = this.items.find(i => i.id === this.selectedCareer);
                this.inputText = selItem?.name ?? '';
            },
            onClickOutside(e) {
                if (!e.target.closest(".combo-box")) {
                    this.isOpen = false;
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
        computed: {
            canEdit(): boolean {
                return this.career != null && currentUser != null && this.career.userLogin === currentUser.userName;
            },
            filteredGroups() {
                let items = this.items;
                if (!items) return new Map<string, CareerListItem[]>();

                const text = this.inputText.toLowerCase();
                if (text.length > 0) {
                    items = this.items.filter(i => this.getPlayerName(i).toLowerCase().includes(text) ||
                                                   i.name.toLowerCase().includes(text));
                }

                const groupedMap = items.reduce(
                    (entryMap, e) => entryMap.set(this.getPlayerName(e), [...entryMap.get(this.getPlayerName(e)) || [], e]),
                    new Map<string, CareerListItem[]>()
                );

                return groupedMap;
            }
        },
        mounted() {
            this.$nextTick(function () {
                this.queryData(this.filters);
            });
            document.addEventListener('click', this.onClickOutside);
        },
        beforeUnmount() {
            document.removeEventListener('click', this.onClickOutside);
        },
        watch: {
            selectedCareer() {
                if (this.items) {
                    this.updateInputText();
                }
            },
            filters: {
                handler() {
                    this.queryData(this.filters);
                },
                deep: true
            }
        }
    });
</script>
