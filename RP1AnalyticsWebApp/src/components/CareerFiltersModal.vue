<template>
    <div class="modal is-active career-filter-modal" v-show="isVisible">
        <div class="modal-background" @click="closeModal"></div>
        <section class="career-filter-panel" role="dialog" aria-modal="true" aria-labelledby="career-filter-title">
            <header class="filter-panel__header">
                <div>
                    <p>Global filters</p>
                    <h2 id="career-filter-title">Career Filters</h2>
                </div>
                <button type="button" class="icon-button" aria-label="Close filters" @click="closeModal">
                    <i class="fas fa-xmark" aria-hidden="true"></i>
                </button>
            </header>

            <div class="filter-panel__body">
                <section class="filter-card">
                    <div class="filter-card__heading">
                        <h3>Player</h3>
                        <span>{{ localFilters.players.length || 'All' }}</span>
                    </div>
                    <label class="filter-search">
                        <i class="fas fa-magnifying-glass" aria-hidden="true"></i>
                        <input v-model="playerSearch" type="search" placeholder="Search players" />
                    </label>
                    <div class="check-list">
                        <label v-for="player in filteredPlayers" :key="player.value" class="check-row">
                            <input type="checkbox" :checked="isSelected(localFilters.players, player.value)" @change="toggle(localFilters.players, player.value)" />
                            <span>{{ player.label }}</span>
                        </label>
                    </div>
                </section>

                <section class="filter-card">
                    <div class="filter-card__heading">
                        <h3>RP-1 Version</h3>
                        <span>{{ localFilters.rp1Versions.length || 'All' }}</span>
                    </div>
                    <label class="filter-search">
                        <i class="fas fa-magnifying-glass" aria-hidden="true"></i>
                        <input v-model="versionSearch" type="search" placeholder="Search versions" />
                    </label>
                    <div class="check-list">
                        <label v-for="version in filteredVersions" :key="version" class="check-row">
                            <input type="checkbox" :checked="isSelected(localFilters.rp1Versions, version)" @change="toggle(localFilters.rp1Versions, version)" />
                            <span>{{ version }}</span>
                        </label>
                    </div>
                </section>

                <section class="filter-card">
                    <div class="filter-card__heading">
                        <h3>Difficulty</h3>
                        <span>{{ localFilters.difficulties.length || 'All' }}</span>
                    </div>
                    <div class="toggle-grid">
                        <label v-for="difficulty in difficultyOptions" :key="difficulty" class="check-pill">
                            <input type="checkbox" :checked="isSelected(localFilters.difficulties, difficulty)" @change="toggle(localFilters.difficulties, difficulty)" />
                            <span>{{ difficulty }}</span>
                        </label>
                    </div>
                </section>

                <section class="filter-card">
                    <div class="filter-card__heading">
                        <h3>Playstyle</h3>
                        <span>{{ localFilters.playstyles.length || 'All' }}</span>
                    </div>
                    <div class="toggle-grid">
                        <label v-for="playstyle in playstyleOptions" :key="playstyle" class="check-pill">
                            <input type="checkbox" :checked="isSelected(localFilters.playstyles, playstyle)" @change="toggle(localFilters.playstyles, playstyle)" />
                            <span>{{ playstyle }}</span>
                        </label>
                    </div>
                </section>

                <section class="filter-card">
                    <div class="filter-card__heading">
                        <h3>Race</h3>
                        <span>{{ localFilters.races.length || 'All' }}</span>
                    </div>
                    <label class="filter-search">
                        <i class="fas fa-magnifying-glass" aria-hidden="true"></i>
                        <input v-model="raceSearch" type="search" placeholder="Search races" />
                    </label>
                    <div class="check-list">
                        <label v-for="race in filteredRaces" :key="race" class="check-row">
                            <input type="checkbox" :checked="isSelected(localFilters.races, race)" @change="toggle(localFilters.races, race)" />
                            <span>{{ race }}</span>
                        </label>
                    </div>
                </section>

                <section class="filter-card filter-card--dates">
                    <div class="date-control">
                        <h3>Career Date</h3>
                        <div class="segmented-control" role="group" aria-label="Career date filter mode">
                            <button v-for="mode in dateModes"
                                    :key="`career-${mode.value}`"
                                    type="button"
                                    :class="{ 'is-active': localFilters.careerDateMode === mode.value }"
                                    @click="updateCareerDate({ mode: mode.value, start: localFilters.careerDateStart, end: localFilters.careerDateEnd })">
                                {{ mode.label }}
                            </button>
                        </div>
                        <div v-if="localFilters.careerDateMode !== 'All'"
                             class="date-fields"
                             :class="{ 'date-fields--single': localFilters.careerDateMode !== 'Range' }">
                            <label v-if="localFilters.careerDateMode === 'Range' || localFilters.careerDateMode === 'After'">
                                <span>{{ localFilters.careerDateMode === 'After' ? 'After' : 'From' }}</span>
                                <input type="date"
                                       :value="localFilters.careerDateStart ?? ''"
                                       @input="updateCareerDate({ mode: localFilters.careerDateMode, start: inputDateValue($event), end: localFilters.careerDateEnd })" />
                            </label>
                            <label v-if="localFilters.careerDateMode === 'Range' || localFilters.careerDateMode === 'Before'">
                                <span>{{ localFilters.careerDateMode === 'Before' ? 'Before' : 'To' }}</span>
                                <input type="date"
                                       :value="localFilters.careerDateEnd ?? ''"
                                       @input="updateCareerDate({ mode: localFilters.careerDateMode, start: localFilters.careerDateStart, end: inputDateValue($event) })" />
                            </label>
                        </div>
                    </div>

                    <div class="date-control">
                        <h3>Last Update</h3>
                        <div class="segmented-control" role="group" aria-label="Last update filter mode">
                            <button v-for="mode in dateModes"
                                    :key="`updated-${mode.value}`"
                                    type="button"
                                    :class="{ 'is-active': localFilters.lastUpdateMode === mode.value }"
                                    @click="updateLastUpdate({ mode: mode.value, start: localFilters.lastUpdateStart, end: localFilters.lastUpdateEnd })">
                                {{ mode.label }}
                            </button>
                        </div>
                        <div v-if="localFilters.lastUpdateMode !== 'All'"
                             class="date-fields"
                             :class="{ 'date-fields--single': localFilters.lastUpdateMode !== 'Range' }">
                            <label v-if="localFilters.lastUpdateMode === 'Range' || localFilters.lastUpdateMode === 'After'">
                                <span>{{ localFilters.lastUpdateMode === 'After' ? 'After' : 'From' }}</span>
                                <input type="date"
                                       :value="localFilters.lastUpdateStart ?? ''"
                                       @input="updateLastUpdate({ mode: localFilters.lastUpdateMode, start: inputDateValue($event), end: localFilters.lastUpdateEnd })" />
                            </label>
                            <label v-if="localFilters.lastUpdateMode === 'Range' || localFilters.lastUpdateMode === 'Before'">
                                <span>{{ localFilters.lastUpdateMode === 'Before' ? 'Before' : 'To' }}</span>
                                <input type="date"
                                       :value="localFilters.lastUpdateEnd ?? ''"
                                       @input="updateLastUpdate({ mode: localFilters.lastUpdateMode, start: localFilters.lastUpdateStart, end: inputDateValue($event) })" />
                            </label>
                        </div>
                    </div>
                    <div class="eligibility-control">
                        <h3>Record Eligibility</h3>
                        <div class="segmented-control">
                            <button v-for="option in eligibilityOptions"
                                    :key="option.value"
                                    type="button"
                                    :class="{ 'is-active': localFilters.recordEligibility === option.value }"
                                    @click="localFilters.recordEligibility = option.value">
                                {{ option.label }}
                            </button>
                        </div>
                    </div>
                </section>
            </div>

            <footer class="filter-panel__footer">
                <span>{{ activeCount }} active {{ activeCount === 1 ? 'filter' : 'filters' }}</span>
                <div>
                    <button type="button" class="button-secondary" @click="clearFilters">Clear all</button>
                    <button type="button" class="button-primary" @click="applyFilters">Apply filters</button>
                </div>
            </footer>
        </section>
    </div>
</template>

<script setup lang="ts">
    import { computed, ref, watch } from 'vue';
    import type { DateFilterMode, Filters, RecordEligibilityFilter, UserData } from 'types';
    import { fetchCareerOverview, fetchRaces, fetchUsers } from '../utils/api';
    import { createEmptyFilters, normalizeFilters } from '../utils/activeFilters';

    const props = defineProps<{
        filters?: Filters;
        isVisible?: boolean;
    }>();

    const emit = defineEmits<{
        'update:isVisible': [value: boolean];
        'update:filters': [value: Filters];
        'applyFilters': [value: Filters];
    }>();

    const players = ref<UserData[]>([]);
    const races = ref<string[]>([]);
    const rp1Versions = ref<string[]>([]);
    const playerSearch = ref('');
    const raceSearch = ref('');
    const versionSearch = ref('');
    const localFilters = ref<Filters>(makeLocalFilters());
    const difficultyOptions = ['Easy', 'Normal', 'Moderate', 'Hard'];
    const playstyleOptions = ['Normal', 'Historic', 'Caveman'];
    const dateModes: { label: string; value: DateFilterMode }[] = [
        { label: 'All', value: 'All' },
        { label: 'Range', value: 'Range' },
        { label: 'Before', value: 'Before' },
        { label: 'After', value: 'After' }
    ];
    const eligibilityOptions: { label: string; value: RecordEligibilityFilter }[] = [
        { label: 'All careers', value: 'All' },
        { label: 'Eligible only', value: 'Eligible' },
        { label: 'Ineligible only', value: 'Ineligible' }
    ];

    const filteredPlayers = computed(() => {
        const needle = playerSearch.value.trim().toLowerCase();
        return players.value
            .map(player => ({
                value: player.userName,
                label: player.preferredName || player.userName
            }))
            .filter(player => !needle || player.label.toLowerCase().includes(needle) || player.value.toLowerCase().includes(needle))
            .sort((a, b) => a.label.localeCompare(b.label));
    });

    const filteredRaces = computed(() => filterStrings(races.value, raceSearch.value));
    const filteredVersions = computed(() => filterStrings(rp1Versions.value, versionSearch.value));

    const activeCount = computed(() => {
        let count = localFilters.value.players.length +
            localFilters.value.races.length +
            localFilters.value.rp1Versions.length +
            localFilters.value.difficulties.length +
            localFilters.value.playstyles.length;
        if (localFilters.value.recordEligibility !== 'All') count++;
        if (localFilters.value.careerDateMode !== 'All') count++;
        if (localFilters.value.lastUpdateMode !== 'All') count++;
        return count;
    });

    function makeLocalFilters(): Filters {
        return normalizeFilters(props.filters);
    }

    watch(() => props.isVisible, async (newIsVisible) => {
        if (newIsVisible) {
            localFilters.value = makeLocalFilters();
            await queryOptions();
        }
    });

    function closeModal() {
        emit('update:isVisible', false);
    }

    function applyFilters() {
        const filters = normalizeFilters(cleanDateFilters(localFilters.value));
        emit('applyFilters', filters);
        closeModal();
    }

    function clearFilters() {
        emit('applyFilters', createEmptyFilters());
        closeModal();
    }

    function updateCareerDate(value: { mode: DateFilterMode; start: string | null; end: string | null }) {
        localFilters.value.careerDateMode = value.mode;
        localFilters.value.careerDateStart = value.mode === 'All' || value.mode === 'Before' ? null : value.start;
        localFilters.value.careerDateEnd = value.mode === 'All' || value.mode === 'After' ? null : value.end;
    }

    function updateLastUpdate(value: { mode: DateFilterMode; start: string | null; end: string | null }) {
        localFilters.value.lastUpdateMode = value.mode;
        localFilters.value.lastUpdateStart = value.mode === 'All' || value.mode === 'Before' ? null : value.start;
        localFilters.value.lastUpdateEnd = value.mode === 'All' || value.mode === 'After' ? null : value.end;
    }

    function inputDateValue(event: Event) {
        return (event.target as HTMLInputElement).value || null;
    }

    function toggle(list: string[], value: string) {
        const index = list.indexOf(value);
        if (index >= 0) list.splice(index, 1);
        else list.push(value);
    }

    function isSelected(list: string[], value: string) {
        return list.includes(value);
    }

    function filterStrings(values: string[], search: string) {
        const needle = search.trim().toLowerCase();
        return values
            .filter(value => !needle || value.toLowerCase().includes(needle))
            .sort((a, b) => a.localeCompare(b, undefined, { numeric: true }));
    }

    function cleanDateFilters(filters: Filters): Filters {
        const copy = normalizeFilters(filters);
        if (copy.careerDateMode === 'All') {
            copy.careerDateStart = null;
            copy.careerDateEnd = null;
        }
        if (copy.lastUpdateMode === 'All') {
            copy.lastUpdateStart = null;
            copy.lastUpdateEnd = null;
        }
        return copy;
    }

    async function queryOptions() {
        if (players.value.length === 0) {
            players.value = await fetchUsers();
        }

        if (races.value.length === 0) {
            races.value = await fetchRaces();
        }

        if (rp1Versions.value.length === 0) {
            const overview = await fetchCareerOverview(createEmptyFilters());
            rp1Versions.value = overview.versionBreakdown
                .map(item => item.label)
                .filter(label => label && label !== 'Unknown');
        }
    }
</script>

<style scoped>
    .career-filter-modal {
        --filter-ink: #17212e;
        --filter-muted: #65717f;
        --filter-line: rgba(36, 48, 63, 0.16);
        --filter-panel: #ffffff;
        --filter-soft: rgba(29, 143, 138, 0.08);
        --filter-accent: #1d8f8a;
        align-items: center;
        justify-content: center;
    }

    .career-filter-panel {
        position: relative;
        display: grid;
        grid-template-rows: auto minmax(0, 1fr) auto;
        width: min(78rem, calc(100vw - 3rem));
        max-height: calc(100vh - 4rem);
        overflow: hidden;
        border-radius: 8px;
        background: var(--filter-panel);
        color: var(--filter-ink);
        box-shadow: 0 1.6rem 4rem rgba(9, 14, 22, 0.35);
    }

    .filter-panel__header,
    .filter-panel__footer {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: center;
        padding: 1rem 1.25rem;
        border-bottom: 1px solid var(--filter-line);
    }

    .filter-panel__footer {
        border-top: 1px solid var(--filter-line);
        border-bottom: 0;
        color: var(--filter-muted);
        font-size: 0.86rem;
    }

    .filter-panel__footer div {
        display: flex;
        gap: 0.55rem;
    }

    .filter-panel__header p,
    .filter-panel__header h2 {
        margin: 0;
    }

    .filter-panel__header p {
        color: var(--filter-accent);
        font-size: 0.72rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .filter-panel__header h2 {
        font-size: 1.3rem;
        line-height: 1.1;
    }

    .icon-button,
    .button-secondary,
    .button-primary {
        border: 1px solid var(--filter-line);
        border-radius: 7px;
        background: transparent;
        color: var(--filter-ink);
        font: inherit;
        font-weight: 750;
        cursor: pointer;
    }

    .icon-button {
        display: inline-grid;
        place-items: center;
        width: 2rem;
        height: 2rem;
    }

    .button-secondary,
    .button-primary {
        padding: 0.52rem 0.8rem;
    }

    .button-primary {
        border-color: var(--filter-accent);
        background: var(--filter-accent);
        color: #fff;
    }

    .filter-panel__body {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 0.85rem;
        overflow: auto;
        padding: 1rem 1.25rem;
    }

    .filter-card {
        min-width: 0;
        border: 1px solid var(--filter-line);
        border-radius: 8px;
        padding: 0.85rem;
        background: rgba(247, 249, 251, 0.72);
    }

    .filter-card--dates {
        display: grid;
        gap: 1rem;
        align-content: start;
    }

    .filter-card__heading {
        display: flex;
        justify-content: space-between;
        gap: 0.75rem;
        align-items: center;
        margin-bottom: 0.65rem;
    }

    .filter-card h3,
    .date-control h3,
    .eligibility-control h3 {
        margin: 0;
        font-size: 0.84rem;
        line-height: 1.2;
    }

    .filter-card__heading span {
        color: var(--filter-muted);
        font-size: 0.72rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .filter-search {
        display: grid;
        grid-template-columns: auto minmax(0, 1fr);
        gap: 0.4rem;
        align-items: center;
        border: 1px solid var(--filter-line);
        border-radius: 7px;
        padding: 0.42rem 0.55rem;
        background: #fff;
        color: var(--filter-muted);
    }

    .filter-search input,
    .date-fields input {
        min-width: 0;
        border: 0;
        background: transparent;
        color: var(--filter-ink);
        font: inherit;
        outline: 0;
    }

    .check-list {
        display: grid;
        gap: 0.12rem;
        max-height: 13.5rem;
        overflow: auto;
        margin-top: 0.65rem;
        padding-right: 0.25rem;
    }

    .check-row,
    .check-pill {
        display: flex;
        gap: 0.45rem;
        align-items: center;
        min-width: 0;
        color: var(--filter-ink);
        font-size: 0.84rem;
        line-height: 1.25;
    }

    .check-row {
        padding: 0.25rem 0.15rem;
    }

    .check-row span {
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .toggle-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 0.45rem;
    }

    .check-pill {
        position: relative;
    }

    .check-pill input {
        position: absolute;
        opacity: 0;
    }

    .check-pill span {
        border: 1px solid var(--filter-line);
        border-radius: 999px;
        padding: 0.34rem 0.62rem;
        background: #fff;
    }

    .check-pill input:checked + span {
        border-color: color-mix(in srgb, var(--filter-accent) 60%, var(--filter-line));
        background: var(--filter-soft);
        color: var(--filter-accent);
    }

    .date-control,
    .eligibility-control {
        display: grid;
        gap: 0.55rem;
    }

    .date-control + .date-control,
    .eligibility-control {
        border-top: 1px solid var(--filter-line);
        padding-top: 0.85rem;
    }

    .segmented-control {
        display: flex;
        flex-wrap: wrap;
        gap: 0.35rem;
    }

    .segmented-control button {
        border: 1px solid var(--filter-line);
        border-radius: 999px;
        padding: 0.32rem 0.62rem;
        background: #fff;
        color: var(--filter-muted);
        font: inherit;
        font-size: 0.8rem;
        font-weight: 800;
        cursor: pointer;
    }

    .segmented-control button.is-active {
        border-color: color-mix(in srgb, var(--filter-accent) 60%, var(--filter-line));
        background: var(--filter-soft);
        color: var(--filter-accent);
    }

    .date-fields {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 0.5rem;
    }

    .date-fields--single {
        grid-template-columns: minmax(0, 1fr);
    }

    .date-fields label {
        display: grid;
        gap: 0.2rem;
        color: var(--filter-muted);
        font-size: 0.72rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .date-fields input {
        border: 1px solid var(--filter-line);
        border-radius: 7px;
        padding: 0.42rem 0.5rem;
        background: #fff;
    }

    @media (max-width: 980px) {
        .career-filter-panel {
            width: min(42rem, calc(100vw - 1rem));
            max-height: calc(100vh - 1rem);
        }

        .filter-panel__body {
            grid-template-columns: 1fr;
        }
    }
</style>
