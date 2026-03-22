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

<script setup lang="ts">
    import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue';
    import type { CareerListItem, Filters } from 'types';
    import { fetchCareerListItems } from '../utils/api';

    const props = defineProps<{
        careerItems?: CareerListItem[];
        selectedCareer?: string;
        filters: Filters;
    }>();

    const emit = defineEmits<{
        'update:selectedCareer': [value: string];
        'careerChanged': [value: string];
    }>();

    const items = ref<CareerListItem[] | null>(null);
    const isLoading = ref(false);
    const isOpen = ref(false);
    const inputText = ref('');
    let mouseDownInsideInput = false;

    async function queryData(filters: Filters) {
        isLoading.value = true;
        try {
            const arr = await fetchCareerListItems(filters);
            items.value = arr;
            updateInputText();
        } finally {
            isLoading.value = false;
        }
    }

    function selectValue(value: CareerListItem) {
        inputText.value = value.name;
        isOpen.value = false;
        careerChanged(value.id);
    }

    function updateInputText() {
        const selItem = items.value!.find(i => i.id === props.selectedCareer);
        inputText.value = selItem?.name ?? '';
    }

    function onMouseDown(e: MouseEvent) {
        if ((e.target as Element).closest('.combo-box')) {
            mouseDownInsideInput = true;
        }
    }

    function onClickOutside(e: MouseEvent) {
        if (mouseDownInsideInput) {
            mouseDownInsideInput = false;
            return;
        }

        if (!(e.target as Element).closest('.combo-box')) {
            isOpen.value = false;
        }
    }

    function careerChanged(careerId: string) {
        emit('update:selectedCareer', careerId);
        emit('careerChanged', careerId);
    }

    function getPlayerName(entry: CareerListItem) {
        return entry.userPreferredName ? entry.userPreferredName : entry.user;
    }

    const filteredGroups = computed(() => {
        let arr = items.value;
        if (!arr) return new Map<string, CareerListItem[]>();

        const text = inputText.value.toLowerCase();
        if (text.length > 0) {
            arr = arr.filter(i => getPlayerName(i).toLowerCase().includes(text) ||
                                  i.name.toLowerCase().includes(text));
        }

        return arr.reduce(
            (entryMap, e) => entryMap.set(getPlayerName(e), [...entryMap.get(getPlayerName(e)) || [], e]),
            new Map<string, CareerListItem[]>()
        );
    });

    watch(() => props.selectedCareer, () => {
        if (items.value) {
            updateInputText();
        }
    });

    watch(() => props.filters, () => {
        queryData(props.filters);
    }, { deep: true });

    onMounted(() => {
        queryData(props.filters);
        document.addEventListener('mousedown', onMouseDown);
        document.addEventListener('click', onClickOutside);
    });

    onBeforeUnmount(() => {
        document.removeEventListener('mousedown', onMouseDown);
        document.removeEventListener('click', onClickOutside);
    });
</script>
