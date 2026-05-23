<template>
    <div class="combo-box control dropdown is-expanded is-rounded" :class="{ 'is-active': isOpen, 'is-loading': isLoading }">
        <div class="dropdown-trigger">
            <div class="control has-icons-left has-icons-right">
                <input ref="inputRef"
                       type="text"
                       v-model="inputText"
                       @focus="openDropdown"
                       @input="isSearching = true"
                       @keydown="onKeyDown"
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
            <div ref="dropdownContentRef" class="dropdown-content">
                <template v-for="[key, value] in filteredGroups" :key="key">
                    <div class="combo-group-label dropdown-item">
                        {{ key }}
                    </div>

                    <button v-for="option in value"
                            :key="option.id"
                            @click="selectValue(option)"
                            class="combo-option dropdown-item"
                            :class="{ 'is-active': flatIndexMap.get(option.id) === highlightedIndex }">
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
    import currentUser from '../utils/currentUser';

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
    const isSearching = ref(false);
    const inputText = ref('');
    const highlightedIndex = ref(-1);
    const dropdownContentRef = ref<HTMLElement | null>(null);
    const inputRef = ref<HTMLInputElement | null>(null);
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
        isSearching.value = false;
        highlightedIndex.value = -1;
        careerChanged(value.id);
    }

    function openDropdown() {
        isOpen.value = true;
        isSearching.value = false;
        inputRef.value?.select();
    }

    function onKeyDown(e: KeyboardEvent) {
        if (!isOpen.value) {
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                isOpen.value = true;
            }
            return;
        }

        const flat = flatFilteredItems.value;
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            highlightedIndex.value = highlightedIndex.value < flat.length - 1
                ? highlightedIndex.value + 1
                : 0;
            scrollHighlightedIntoView();
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            highlightedIndex.value = highlightedIndex.value > 0
                ? highlightedIndex.value - 1
                : flat.length - 1;
            scrollHighlightedIntoView();
        } else if (e.key === 'Enter' && highlightedIndex.value >= 0) {
            e.preventDefault();
            selectValue(flat[highlightedIndex.value]);
            inputRef.value?.blur();
        }
    }

    function scrollHighlightedIntoView() {
        const el = dropdownContentRef.value?.querySelector<HTMLElement>('.combo-option.is-active');
        el?.scrollIntoView({ block: 'nearest' });
    }

    function updateInputText() {
        const selItem = items.value!.find(i => i.id === props.selectedCareer);
        inputText.value = selItem?.name ?? '';
        isSearching.value = false;
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

    const flatFilteredItems = computed(() => {
        const result: CareerListItem[] = [];
        for (const [, value] of filteredGroups.value) {
            result.push(...value);
        }
        return result;
    });

    const flatIndexMap = computed(() => {
        const map = new Map<string, number>();
        flatFilteredItems.value.forEach((item, i) => map.set(item.id, i));
        return map;
    });

    const filteredGroups = computed(() => {
        let arr = items.value;
        if (!arr) return new Map<string, CareerListItem[]>();

        const text = isSearching.value ? inputText.value.toLowerCase() : '';
        if (text.length > 0) {
            arr = arr.filter(i => getPlayerName(i).toLowerCase().includes(text) ||
                                  i.name.toLowerCase().includes(text));
        }

        const groups = arr.reduce(
            (entryMap, e) => entryMap.set(getPlayerName(e), [...entryMap.get(getPlayerName(e)) || [], e]),
            new Map<string, CareerListItem[]>()
        );

        // Make sure entries for currently logged in user appear first
        if (currentUser) {
            const currentUserItem = arr.find(i => i.user === currentUser.userName);
            if (currentUserItem) {
                const currentUserKey = getPlayerName(currentUserItem);
                const currentUserGroup = groups.get(currentUserKey);
                if (currentUserGroup) {
                    const sorted = new Map<string, CareerListItem[]>();
                    sorted.set(currentUserKey, currentUserGroup);
                    for (const [key, value] of groups) {
                        if (key !== currentUserKey) sorted.set(key, value);
                    }
                    return sorted;
                }
            }
        }

        return groups;
    });

    watch(inputText, () => {
        highlightedIndex.value = -1;
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
