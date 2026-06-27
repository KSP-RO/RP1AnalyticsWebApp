<template>
    <section class="careers-dashboard">
        <header class="overview-header">
            <div>
                <p class="eyebrow">Career dashboard</p>
                <h1>All Careers</h1>
                <p class="overview-copy">{{ summaryLine }}</p>
            </div>
            <div class="overview-links">
                <a href="/Records" class="overview-link">
                    <i class="fas fa-trophy" aria-hidden="true"></i>
                    Records
                </a>
            </div>
        </header>

        <ActiveFiltersSummary :filters="filters" />

        <div v-if="isLoading && !summarySnapshot && !tableSnapshot" class="overview-loading">
            <LoadingSpinner />
        </div>

        <template v-else>
            <p v-if="!summarySnapshot && !isLoading" class="overview-empty">
                The dashboard summary could not load.
            </p>

            <section v-if="metricTiles.length" class="overview-metrics" aria-label="Career overview metrics">
                <article v-for="metric in metricTiles" :key="metric.key" class="metric-tile">
                    <span>{{ metric.label }}</span>
                    <strong>{{ metric.value }}</strong>
                    <small>{{ metric.detail }}</small>
                </article>
            </section>
            <p v-else-if="!isLoading" class="overview-empty">
                No summary data is available.
            </p>

            <section v-if="tableSnapshot" class="overview-section table-section">
                <div class="section-heading">
                    <h2>Career Table</h2>
                    <span>{{ tableFilteredCareers.length }} of {{ tableSnapshot?.careers.length ?? 0 }} careers</span>
                </div>
                <div class="career-table-wrap">
                    <table class="career-table">
                        <colgroup>
                            <col class="col-career" />
                            <col class="col-player" />
                            <col class="col-version" />
                            <col class="col-difficulty" />
                            <col class="col-playstyle" />
                            <col class="col-career-date" />
                            <col class="col-updated" />
                            <col class="col-launches" />
                            <col class="col-programs" />
                            <col class="col-milestones" />
                            <col class="col-records" />
                        </colgroup>
                        <thead>
                            <tr class="sort-row">
                                <th v-for="column in tableColumns" :key="column.key">
                                    <button type="button" class="sort-button" @click="toggleSort(column.key)">
                                        <span class="sort-button__label">{{ column.label }}</span>
                                        <i class="fas" :class="sortIcon(column.key)" aria-hidden="true"></i>
                                    </button>
                                </th>
                            </tr>
                            <tr class="filter-row">
                                <th class="filter-cell">
                                    <input
                                        v-model="tableFilters.careerSearch"
                                        type="search"
                                        class="column-search"
                                        placeholder="Search careers"
                                        aria-label="Search careers"
                                    />
                                </th>
                                <th class="filter-cell">
                                    <input
                                        v-model="tableFilters.playerSearch"
                                        type="search"
                                        class="column-search"
                                        placeholder="Search players"
                                        aria-label="Search players"
                                    />
                                </th>
                                <th class="filter-cell">
                                    <details class="column-filter">
                                        <summary>{{ selectionSummary(tableFilters.rp1Versions, versionOptions.length) }}</summary>
                                        <div class="filter-menu">
                                            <div class="filter-menu__toolbar">
                                                <span>RP-1 versions</span>
                                                <button type="button" class="link-button" :disabled="tableFilters.rp1Versions.length === 0" @click="clearSelections(tableFilters.rp1Versions)">Clear</button>
                                            </div>
                                            <div class="check-list">
                                                <label v-for="version in versionOptions" :key="version" class="check-row">
                                                    <input type="checkbox" :checked="isSelected(tableFilters.rp1Versions, version)" @change="toggleValue(tableFilters.rp1Versions, version)" />
                                                    <span>{{ version }}</span>
                                                </label>
                                            </div>
                                        </div>
                                    </details>
                                </th>
                                <th class="filter-cell">
                                    <details class="column-filter">
                                        <summary>{{ selectionSummary(tableFilters.difficulties, difficultyOptions.length) }}</summary>
                                        <div class="filter-menu">
                                            <div class="filter-menu__toolbar">
                                                <span>Difficulty</span>
                                                <button type="button" class="link-button" :disabled="tableFilters.difficulties.length === 0" @click="clearSelections(tableFilters.difficulties)">Clear</button>
                                            </div>
                                            <div class="toggle-grid">
                                                <label v-for="difficulty in difficultyOptions" :key="difficulty" class="check-pill">
                                                    <input type="checkbox" :checked="isSelected(tableFilters.difficulties, difficulty)" @change="toggleValue(tableFilters.difficulties, difficulty)" />
                                                    <span>{{ difficulty }}</span>
                                                </label>
                                            </div>
                                        </div>
                                    </details>
                                </th>
                                <th class="filter-cell">
                                    <details class="column-filter">
                                        <summary>{{ selectionSummary(tableFilters.playstyles, playstyleOptions.length) }}</summary>
                                        <div class="filter-menu">
                                            <div class="filter-menu__toolbar">
                                                <span>Playstyle</span>
                                                <button type="button" class="link-button" :disabled="tableFilters.playstyles.length === 0" @click="clearSelections(tableFilters.playstyles)">Clear</button>
                                            </div>
                                            <div class="toggle-grid">
                                                <label v-for="playstyle in playstyleOptions" :key="playstyle" class="check-pill">
                                                    <input type="checkbox" :checked="isSelected(tableFilters.playstyles, playstyle)" @change="toggleValue(tableFilters.playstyles, playstyle)" />
                                                    <span>{{ playstyle }}</span>
                                                </label>
                                            </div>
                                        </div>
                                    </details>
                                </th>
                                <th class="filter-cell">
                                    <details class="column-filter">
                                        <summary>{{ careerDateSummary }}</summary>
                                        <div class="filter-menu date-filter">
                                            <div class="filter-menu__toolbar">
                                                <span>Career date</span>
                                                <button type="button" class="link-button" :disabled="tableFilters.careerDateMode === 'All'" @click="clearCareerDateFilter">Clear</button>
                                            </div>
                                            <div class="segmented-control" role="group" aria-label="Career date filter mode">
                                                <button v-for="mode in dateModes"
                                                        :key="`career-${mode.value}`"
                                                        type="button"
                                                        :class="{ 'is-active': tableFilters.careerDateMode === mode.value }"
                                                        @click="updateCareerDate(mode.value)">
                                                    {{ mode.label }}
                                                </button>
                                            </div>
                                            <div v-if="tableFilters.careerDateMode !== 'All'"
                                                 class="date-fields"
                                                 :class="{ 'date-fields--single': tableFilters.careerDateMode !== 'Range' }">
                                                <label v-if="tableFilters.careerDateMode === 'Range' || tableFilters.careerDateMode === 'After'">
                                                    <span>{{ tableFilters.careerDateMode === 'After' ? 'After' : 'From' }}</span>
                                                    <input type="date"
                                                           :value="tableFilters.careerDateStart ?? ''"
                                                           @input="tableFilters.careerDateStart = inputDateValue($event)" />
                                                </label>
                                                <label v-if="tableFilters.careerDateMode === 'Range' || tableFilters.careerDateMode === 'Before'">
                                                    <span>{{ tableFilters.careerDateMode === 'Before' ? 'Before' : 'To' }}</span>
                                                    <input type="date"
                                                           :value="tableFilters.careerDateEnd ?? ''"
                                                           @input="tableFilters.careerDateEnd = inputDateValue($event)" />
                                                </label>
                                            </div>
                                        </div>
                                    </details>
                                </th>
                                <th>
                                    <details class="column-filter">
                                        <summary>{{ updatedSummary }}</summary>
                                        <div class="filter-menu date-filter">
                                            <div class="filter-menu__toolbar">
                                                <span>Updated</span>
                                                <button type="button" class="link-button" :disabled="tableFilters.updatedMode === 'All'" @click="clearUpdatedFilter">Clear</button>
                                            </div>
                                            <div class="segmented-control" role="group" aria-label="Updated filter mode">
                                                <button v-for="mode in updatedModes"
                                                        :key="`updated-${mode.value}`"
                                                        type="button"
                                                        :class="{ 'is-active': tableFilters.updatedMode === mode.value }"
                                                        @click="updateUpdatedMode(mode.value)">
                                                    {{ mode.label }}
                                                </button>
                                            </div>
                                            <div v-if="tableFilters.updatedMode === 'Range'"
                                                 class="date-fields">
                                                <label>
                                                    <span>From</span>
                                                    <input type="date"
                                                           :value="tableFilters.updatedStart ?? ''"
                                                           @input="tableFilters.updatedStart = inputDateValue($event)" />
                                                </label>
                                                <label>
                                                    <span>To</span>
                                                    <input type="date"
                                                           :value="tableFilters.updatedEnd ?? ''"
                                                           @input="tableFilters.updatedEnd = inputDateValue($event)" />
                                                </label>
                                            </div>
                                        </div>
                                    </details>
                                </th>
                                <th></th>
                                <th></th>
                                <th></th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="career in pagedCareers" :key="career.id" :class="{ 'is-ineligible': !career.eligibleForRecords }">
                                <td>
                                    <a :href="careerUrl(career.id)">{{ career.name }}</a>
                                    <span>{{ career.race || 'No race' }}</span>
                                </td>
                                <td>{{ career.userPreferredName || career.userLogin }}</td>
                                <td>{{ career.rp1Version || '-' }}</td>
                                <td>{{ career.difficultyLevel || '-' }}</td>
                                <td>{{ career.careerPlaystyle || '-' }}</td>
                                <td class="date-col">{{ formatDate(career.endDate) }}</td>
                                <td class="date-col">{{ formatDateLocal(career.lastUpdate) }}</td>
                                <td>{{ formatNumber(career.launchCount) }}</td>
                                <td>{{ formatNumber(career.completedProgramCount) }}</td>
                                <td>{{ formatNumber(career.completedMilestoneCount) }}</td>
                                <td>
                                    <span class="record-count" :title="recordsTitle(career.recordsOwned)">
                                        {{ formatNumber(career.recordCount) }}
                                    </span>
                                </td>
                            </tr>
                            <tr v-if="pagedCareers.length === 0">
                                <td class="table-empty" :colspan="tableColumns.length">No careers match the table filters.</td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <footer class="table-pagination">
                    <span>{{ paginationSummary }}</span>
                    <div class="pagination-controls">
                        <button type="button" :disabled="currentPage === 1" @click="currentPage--">
                            <i class="fas fa-chevron-left" aria-hidden="true"></i>
                        </button>
                        <span>Page {{ currentPage }} of {{ pageCount }}</span>
                        <button type="button" :disabled="currentPage === pageCount" @click="currentPage++">
                            <i class="fas fa-chevron-right" aria-hidden="true"></i>
                        </button>
                    </div>
                    <label>
                        <span>Rows</span>
                        <select v-model="pageSize">
                            <option value="20">20</option>
                            <option value="50">50</option>
                            <option value="100">100</option>
                            <option value="All">All</option>
                        </select>
                    </label>
                </footer>
            </section>
            <p v-else-if="!isLoading" class="overview-empty">
                The career table could not load.
            </p>
        </template>
    </section>
</template>

<script setup lang="ts">
    import { computed, onMounted, reactive, ref, watch } from 'vue';
    import type { CareerOverviewBreakdownItem, CareerOverviewItem, CareerOverviewSnapshot, DateFilterMode, Filters } from '../types';
    import { fetchCareerOverview } from '../utils/api';
    import { createEmptyFilters } from '../utils/activeFilters';
    import { parseUtcDate } from '../utils/parseUtcDate';
    import ActiveFiltersSummary from './ActiveFiltersSummary.vue';
    import LoadingSpinner from './LoadingSpinner.vue';

    type PageSize = '20' | '50' | '100' | 'All';
    type SortDirection = 'asc' | 'desc';
    type SortKey =
        | 'name'
        | 'userPreferredName'
        | 'rp1Version'
        | 'difficultyLevel'
        | 'careerPlaystyle'
        | 'endDate'
        | 'lastUpdate'
        | 'launchCount'
        | 'completedProgramCount'
        | 'completedMilestoneCount'
        | 'recordCount';
    type UpdatedFilterMode = 'All' | 'Last30' | 'Last60' | 'ThisYear' | 'Range';

    interface TableFilters {
        careerSearch: string;
        playerSearch: string;
        rp1Versions: string[];
        difficulties: string[];
        playstyles: string[];
        careerDateMode: DateFilterMode;
        careerDateStart: string | null;
        careerDateEnd: string | null;
        updatedMode: UpdatedFilterMode;
        updatedStart: string | null;
        updatedEnd: string | null;
    }

    const props = defineProps<{
        filters: Filters;
    }>();

    const summarySnapshot = ref<CareerOverviewSnapshot | null>(null);
    const tableSnapshot = ref<CareerOverviewSnapshot | null>(null);
    const summaryLoading = ref(false);
    const tableLoading = ref(false);
    const sortKey = ref<SortKey>('lastUpdate');
    const sortDirection = ref<SortDirection>('desc');
    const currentPage = ref(1);
    const pageSize = ref<PageSize>('50');
    const tableFilters = reactive<TableFilters>({
        careerSearch: '',
        playerSearch: '',
        rp1Versions: [],
        difficulties: [],
        playstyles: [],
        careerDateMode: 'All',
        careerDateStart: null,
        careerDateEnd: null,
        updatedMode: 'All',
        updatedStart: null,
        updatedEnd: null
    });
    let summaryRequestId = 0;
    let tableRequestId = 0;

    const tableColumns: { key: SortKey; label: string }[] = [
        { key: 'name', label: 'Career' },
        { key: 'userPreferredName', label: 'Player' },
        { key: 'rp1Version', label: 'RP-1' },
        { key: 'difficultyLevel', label: 'Difficulty' },
        { key: 'careerPlaystyle', label: 'Playstyle' },
        { key: 'endDate', label: 'Career Date' },
        { key: 'lastUpdate', label: 'Updated' },
        { key: 'launchCount', label: 'Launches' },
        { key: 'completedProgramCount', label: 'Programs' },
        { key: 'completedMilestoneCount', label: 'Milestones' },
        { key: 'recordCount', label: 'Records' }
    ];

    const difficultyOptions = ['Easy', 'Normal', 'Moderate', 'Hard'];
    const playstyleOptions = ['Normal', 'Historic', 'Caveman'];
    const dateModes: { label: string; value: DateFilterMode }[] = [
        { label: 'All', value: 'All' },
        { label: 'Range', value: 'Range' },
        { label: 'Before', value: 'Before' },
        { label: 'After', value: 'After' }
    ];
    const updatedModes: { label: string; value: UpdatedFilterMode }[] = [
        { label: 'All', value: 'All' },
        { label: '30d', value: 'Last30' },
        { label: '60d', value: 'Last60' },
        { label: 'This year', value: 'ThisYear' },
        { label: 'Range', value: 'Range' }
    ];

    const versionOptions = computed(() => {
        const careers = tableSnapshot.value?.careers ?? [];
        return uniqueStrings(careers.map(career => career.rp1Version).filter((value): value is string => !!value && value !== '-'));
    });

    const metricTiles = computed(() => {
        const summary = summarySnapshot.value?.summary;
        if (!summary) return [];

        return [
            {
                key: 'careers',
                label: 'Careers',
                value: formatNumber(summary.totalCareers),
                detail: `${formatNumber(summary.recordsEligibleCareers)} records eligible`
            },
            {
                key: 'launches',
                label: 'Launches',
                value: formatNumber(summary.totalLaunches),
                detail: `${formatAverage(summary.totalLaunches, summary.totalCareers)} per career`
            },
            {
                key: 'contracts',
                label: 'Completed Contracts',
                value: formatNumber(summary.totalCompletedContracts),
                detail: `${formatNumber(summary.totalCompletedMilestones)} milestones`
            },
            {
                key: 'programs',
                label: 'Completed Programs',
                value: formatNumber(summary.totalCompletedPrograms),
                detail: `${formatAverage(summary.totalCompletedPrograms, summary.totalCareers)} per career`
            },
            {
                key: 'records',
                label: 'Records Set',
                value: formatNumber(summary.totalRecordsSet),
                detail: 'Contract and completed-program firsts'
            },
            {
                key: 'careerDate',
                label: 'Latest Career Date',
                value: formatDate(summary.latestSaveDate),
                detail: 'Most advanced in-game date'
            },
            {
                key: 'updateDate',
                label: 'Latest Upload',
                value: formatDateTimeLocal(summary.latestUpdate),
                detail: 'Local time'
            }
        ];
    });

    const summaryLine = computed(() => {
        const summary = summarySnapshot.value?.summary;
        if (!summary) return 'Loading career metrics.';
        return `${formatNumber(summary.totalCareers)} careers, ${formatNumber(summary.totalLaunches)} launches, ${formatNumber(summary.totalRecordsSet)} first-place records.`;
    });

    const careerDateSummary = computed(() => dateFilterSummary(tableFilters.careerDateMode, tableFilters.careerDateStart, tableFilters.careerDateEnd));
    const updatedSummary = computed(() => updatedFilterSummary(tableFilters.updatedMode, tableFilters.updatedStart, tableFilters.updatedEnd));

    const tableFilteredCareers = computed(() => {
        const careers = tableSnapshot.value?.careers ?? [];
        const careerNeedle = tableFilters.careerSearch.trim().toLowerCase();
        const playerNeedle = tableFilters.playerSearch.trim().toLowerCase();
        return careers.filter(career => {
            if (careerNeedle && !career.name.toLowerCase().includes(careerNeedle)) return false;
            if (playerNeedle) {
                const playerText = `${career.userPreferredName || ''} ${career.userLogin || ''}`.trim().toLowerCase();
                if (!playerText.includes(playerNeedle)) return false;
            }
            if (tableFilters.rp1Versions.length && !tableFilters.rp1Versions.includes(career.rp1Version || '')) return false;
            if (tableFilters.difficulties.length && !tableFilters.difficulties.includes(career.difficultyLevel || '')) return false;
            if (tableFilters.playstyles.length && !tableFilters.playstyles.includes(career.careerPlaystyle || '')) return false;
            if (!matchesDateFilter(career.endDate, tableFilters.careerDateMode, tableFilters.careerDateStart, tableFilters.careerDateEnd)) return false;
            if (!matchesUpdatedFilter(career.lastUpdate, tableFilters.updatedMode, tableFilters.updatedStart, tableFilters.updatedEnd)) return false;
            return true;
        });
    });

    const sortedCareers = computed(() => {
        const items = [...tableFilteredCareers.value];
        const direction = sortDirection.value === 'asc' ? 1 : -1;
        return items.sort((a, b) => {
            const primary = compareCareerValues(a, b, sortKey.value) * direction;
            if (primary !== 0) return primary;
            return a.name.localeCompare(b.name);
        });
    });

    const pageCount = computed(() => {
        if (pageSize.value === 'All') return 1;
        return Math.max(1, Math.ceil(sortedCareers.value.length / Number(pageSize.value)));
    });

    const pagedCareers = computed(() => {
        if (pageSize.value === 'All') return sortedCareers.value;
        const size = Number(pageSize.value);
        const start = (currentPage.value - 1) * size;
        return sortedCareers.value.slice(start, start + size);
    });

    const paginationSummary = computed(() => {
        const total = sortedCareers.value.length;
        if (total === 0) return 'Showing 0 careers';
        if (pageSize.value === 'All') return `Showing all ${formatNumber(total)} careers`;
        const size = Number(pageSize.value);
        const start = (currentPage.value - 1) * size + 1;
        const end = Math.min(start + size - 1, total);
        return `Showing ${formatNumber(start)}-${formatNumber(end)} of ${formatNumber(total)} careers`;
    });

    const isLoading = computed(() => summaryLoading.value || tableLoading.value);

    watch(() => props.filters, () => {
        querySummaryData();
    }, { deep: true, immediate: true });

    watch(tableFilters, () => {
        currentPage.value = 1;
    }, { deep: true });

    watch(pageSize, () => {
        currentPage.value = 1;
    });

    watch(pageCount, () => {
        if (currentPage.value > pageCount.value) {
            currentPage.value = pageCount.value;
        }
    });

    onMounted(() => {
        queryTableData();
    });

    async function querySummaryData() {
        const currentRequest = ++summaryRequestId;
        summaryLoading.value = true;
        try {
            const result = await fetchCareerOverview(props.filters);
            if (currentRequest === summaryRequestId) {
                summarySnapshot.value = result;
            }
        } finally {
            if (currentRequest === summaryRequestId) {
                summaryLoading.value = false;
            }
        }
    }

    async function queryTableData() {
        const currentRequest = ++tableRequestId;
        tableLoading.value = true;
        try {
            const result = await fetchCareerOverview(createEmptyFilters());
            if (currentRequest === tableRequestId) {
                tableSnapshot.value = result;
            }
        } finally {
            if (currentRequest === tableRequestId) {
                tableLoading.value = false;
            }
        }
    }

    function toggleSort(key: SortKey) {
        if (sortKey.value === key) {
            sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc';
            return;
        }

        sortKey.value = key;
        sortDirection.value = defaultSortDirection(key);
    }

    function sortIcon(key: SortKey) {
        if (sortKey.value !== key) return 'fa-sort';
        return sortDirection.value === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }

    function defaultSortDirection(key: SortKey): SortDirection {
        if (key === 'name' || key === 'userPreferredName' || key === 'rp1Version' ||
            key === 'difficultyLevel' || key === 'careerPlaystyle') {
            return 'asc';
        }

        return 'desc';
    }

    function compareCareerValues(a: CareerOverviewItem, b: CareerOverviewItem, key: SortKey) {
        if (key === 'endDate' || key === 'lastUpdate') {
            return dateValue(a[key]) - dateValue(b[key]);
        }

        if (key === 'launchCount' || key === 'completedProgramCount' ||
            key === 'completedMilestoneCount' || key === 'recordCount') {
            return a[key] - b[key];
        }

        return stringValue(a[key]).localeCompare(stringValue(b[key]));
    }

    function careerUrl(careerId: string) {
        return `/?careerId=${careerId}&tab=comparison`;
    }

    function uniqueStrings(values: string[]) {
        return [...new Set(values)]
            .filter(value => value && value !== '-')
            .sort((a, b) => a.localeCompare(b, undefined, { numeric: true }));
    }

    function selectionSummary(values: string[], totalOptions: number) {
        if (values.length === 0) return 'All';
        if (values.length === totalOptions) return 'All';
        return `${values.length} selected`;
    }

    function updateCareerDate(mode: DateFilterMode) {
        tableFilters.careerDateMode = mode;
        if (mode === 'All') {
            tableFilters.careerDateStart = null;
            tableFilters.careerDateEnd = null;
        } else if (mode === 'Before') {
            tableFilters.careerDateStart = null;
        } else if (mode === 'After') {
            tableFilters.careerDateEnd = null;
        }
    }

    function clearCareerDateFilter() {
        tableFilters.careerDateMode = 'All';
        tableFilters.careerDateStart = null;
        tableFilters.careerDateEnd = null;
    }

    function updateUpdatedMode(mode: UpdatedFilterMode) {
        tableFilters.updatedMode = mode;
        if (mode !== 'Range') {
            tableFilters.updatedStart = null;
            tableFilters.updatedEnd = null;
        }
    }

    function clearUpdatedFilter() {
        tableFilters.updatedMode = 'All';
        tableFilters.updatedStart = null;
        tableFilters.updatedEnd = null;
    }

    function toggleValue(list: string[], value: string) {
        const index = list.indexOf(value);
        if (index >= 0) list.splice(index, 1);
        else list.push(value);
    }

    function clearSelections(list: string[]) {
        list.splice(0, list.length);
    }

    function isSelected(list: string[], value: string) {
        return list.includes(value);
    }

    function matchesDateFilter(value?: string | null, mode: DateFilterMode = 'All', start?: string | null, end?: string | null) {
        if (mode === 'All') return true;
        const current = dateValue(value);
        if (mode === 'Before') {
            return !!end && current > 0 ? current <= dateValue(end) : false;
        }
        if (mode === 'After') {
            return !!start && current > 0 ? current >= dateValue(start) : false;
        }
        if (!start || !end || current <= 0) return false;
        return current >= dateValue(start) && current <= dateValue(end);
    }

    function matchesUpdatedFilter(value?: string | null, mode: UpdatedFilterMode = 'All', start?: string | null, end?: string | null) {
        if (mode === 'All') return true;
        const current = dateValue(value);
        if (current <= 0) return false;

        const day = 24 * 60 * 60 * 1000;
        const now = new Date();
        const todayUtc = Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate());

        if (mode === 'Last30') return current >= todayUtc - (30 * day);
        if (mode === 'Last60') return current >= todayUtc - (60 * day);
        if (mode === 'ThisYear') return current >= Date.UTC(now.getUTCFullYear(), 0, 1);
        if (!start || !end) return false;
        return current >= dateValue(start) && current <= dateValue(end);
    }

    function dateFilterSummary(mode: DateFilterMode, start: string | null, end: string | null) {
        if (mode === 'All') return 'All';
        if (mode === 'Range') {
            if (start && end) return `${formatDate(start)} to ${formatDate(end)}`;
            if (start) return `From ${formatDate(start)}`;
            if (end) return `To ${formatDate(end)}`;
            return 'Range';
        }
        if (mode === 'Before') {
            return end ? `Before ${formatDate(end)}` : 'Before';
        }
        if (mode === 'After') {
            return start ? `After ${formatDate(start)}` : 'After';
        }
        return 'All';
    }

    function updatedFilterSummary(mode: UpdatedFilterMode, start: string | null, end: string | null) {
        if (mode === 'All') return 'All';
        if (mode === 'Last30') return 'Last 30 days';
        if (mode === 'Last60') return 'Last 60 days';
        if (mode === 'ThisYear') return 'This year';
        if (start && end) return `${formatDate(start)} to ${formatDate(end)}`;
        if (start) return `From ${formatDate(start)}`;
        if (end) return `To ${formatDate(end)}`;
        return 'Range';
    }

    function topBreakdown(items: CareerOverviewBreakdownItem[]) {
        return items.slice(0, 5);
    }

    function breakdownStyle(count: number, items: CareerOverviewBreakdownItem[]) {
        const max = Math.max(...items.map(i => i.count), 1);
        return { width: `${Math.max(6, Math.round(count * 100 / max))}%` };
    }

    function recordsTitle(recordsOwned?: string[]) {
        if (!recordsOwned || recordsOwned.length === 0) return 'No first-place records';
        return recordsOwned.map(cleanRecordName).join(', ');
    }

    function cleanRecordName(value: string) {
        return value
            .replace(/^contract:/i, '')
            .replace(/^program:/i, '')
            .replace(/_/g, ' ')
            .replace(/\s+/g, ' ')
            .trim();
    }

    function formatNumber(value: number) {
        return value.toLocaleString();
    }

    function formatAverage(total: number, count: number) {
        if (!count) return '0';
        return (total / count).toFixed(1);
    }

    function formatDate(value?: string | null) {
        if (!value) return '-';
        return parseUtcDate(value).toFormat('yyyy-MM-dd');
    }

    function formatDateLocal(value?: string | null) {
        if (!value) return '-';
        return parseUtcDate(value).toLocal().toFormat('yyyy-MM-dd');
    }

    function formatDateTimeLocal(value?: string | null) {
        if (!value) return '-';
        return parseUtcDate(value).toLocal().toFormat('yyyy-MM-dd HH:mm');
    }

    function dateValue(value?: string | null) {
        return value ? parseUtcDate(value).toMillis() : 0;
    }

    function stringValue(value?: string | null) {
        return value || '';
    }

    function inputDateValue(event: Event) {
        return (event.target as HTMLInputElement).value || null;
    }
</script>

<style scoped>
    .careers-dashboard {
        --rp-ink: #17212e;
        --rp-muted: #65717f;
        --rp-line: rgba(36, 48, 63, 0.16);
        --rp-panel: #ffffff;
        --rp-panel-solid: #ffffff;
        --rp-accent: #1d8f8a;
        --rp-soft: rgba(29, 143, 138, 0.08);
        padding: 0.35rem 0.35rem 0.5rem;
    }

    .overview-header {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: flex-start;
        padding: 0.25rem 0 1rem;
    }

    .eyebrow {
        margin: 0 0 0.3rem;
        color: var(--rp-accent);
        font-size: 0.72rem;
        font-weight: 800;
        letter-spacing: 0.05em;
        text-transform: uppercase;
    }

    .overview-header h1 {
        margin: 0;
        font-size: 2rem;
        line-height: 1.05;
    }

    .overview-copy {
        margin: 0.55rem 0 0;
        color: var(--rp-muted);
        font-size: 0.94rem;
    }

    .overview-links {
        display: flex;
        justify-content: flex-end;
        flex-wrap: wrap;
        gap: 0.65rem;
    }

    .overview-link {
        display: inline-flex;
        gap: 0.45rem;
        align-items: center;
        border: 1px solid var(--rp-line);
        border-radius: 999px;
        padding: 0.45rem 0.8rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font-size: 0.84rem;
        font-weight: 700;
    }

    .overview-loading {
        display: grid;
        place-items: center;
        min-height: 40vh;
    }

    .overview-empty {
        margin: 0.75rem 0 0;
        color: var(--rp-muted);
        font-size: 0.9rem;
    }

    .overview-metrics {
        display: grid;
        grid-template-columns: repeat(7, minmax(0, 1fr));
        gap: 0.8rem;
        padding: 0.35rem 0 1rem;
    }

    .metric-tile {
        display: grid;
        gap: 0.35rem;
        border: 1px solid var(--rp-line);
        border-radius: 10px;
        padding: 0.85rem 0.9rem;
        background: var(--rp-panel);
    }

    .metric-tile span {
        color: var(--rp-muted);
        font-size: 0.82rem;
    }

    .metric-tile strong {
        font-size: 1.35rem;
        line-height: 1;
    }

    .metric-tile small {
        color: var(--rp-muted);
        font-size: 0.75rem;
    }

    .overview-section {
        border-top: 1px solid var(--rp-line);
        padding-top: 0.9rem;
    }

    .section-heading {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: flex-end;
        margin-bottom: 0.8rem;
    }

    .section-heading h2 {
        margin: 0;
        font-size: 1.08rem;
    }

    .section-heading span {
        color: var(--rp-muted);
        font-size: 0.84rem;
        white-space: nowrap;
    }

    .career-table-wrap {
        overflow-x: auto;
    }

    .career-table {
        width: 100%;
        min-width: 68rem;
        border-collapse: collapse;
        table-layout: fixed;
        font-size: 0.82rem;
    }

    .career-table col.col-career { width: 16%; }
    .career-table col.col-player { width: 10%; }
    .career-table col.col-version { width: 8%; }
    .career-table col.col-difficulty { width: 7%; }
    .career-table col.col-playstyle { width: 7%; }
    .career-table col.col-career-date { width: 9%; }
    .career-table col.col-updated { width: 9%; }
    .career-table col.col-launches { width: 7%; }
    .career-table col.col-programs { width: 7%; }
    .career-table col.col-milestones { width: 7%; }
    .career-table col.col-records { width: 7%; }

    .career-table th,
    .career-table td {
        border-bottom: 1px solid var(--rp-line);
        padding: 0.5rem 0.42rem;
        vertical-align: top;
    }

    .career-table th {
        position: relative;
        overflow: visible;
    }

    .career-table thead th {
        border-bottom-color: color-mix(in srgb, var(--rp-line) 82%, var(--rp-ink));
    }

    .career-table thead tr.filter-row th {
        border-bottom: 1px solid var(--rp-line);
        padding-top: 0.5rem;
        padding-bottom: 2.15rem;
    }

    .sort-button {
        display: inline-flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 0.1rem;
        border: 0;
        padding: 0;
        background: transparent;
        color: var(--rp-muted);
        cursor: pointer;
        font: inherit;
        font-size: 0.68rem;
        font-weight: 800;
        text-transform: uppercase;
        line-height: 1.08;
        white-space: nowrap;
        word-break: normal;
        text-align: left;
        width: 100%;
    }

    .sort-button__label {
        display: block;
        max-width: 100%;
        white-space: nowrap;
    }

    .career-table thead tr.sort-row th:nth-last-child(-n + 4) .sort-button {
        font-size: 0.64rem;
    }

    .column-search {
        width: 100%;
        border: 1px solid var(--rp-line);
        border-radius: 7px;
        padding: 0.3rem 0.42rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font: inherit;
        font-size: 0.7rem;
    }

    .column-filter {
        display: block;
        width: 100%;
    }

    .column-filter > summary {
        display: inline-flex;
        align-items: center;
        gap: 0.3rem;
        width: 100%;
        border: 1px solid var(--rp-line);
        border-radius: 8px;
        padding: 0.26rem 0.38rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        cursor: pointer;
        font-size: 0.62rem;
        font-weight: 700;
        list-style: none;
        white-space: nowrap;
    }

    .column-filter > summary::-webkit-details-marker {
        display: none;
    }

    .column-filter[open] > summary {
        border-color: color-mix(in srgb, var(--rp-accent) 36%, var(--rp-line));
        background: var(--rp-soft);
    }

    .filter-menu {
        position: absolute;
        top: calc(100% + 0.35rem);
        left: 0;
        z-index: 4;
        width: 100%;
        min-width: 9rem;
        display: grid;
        gap: 0.5rem;
        border: 1px solid var(--rp-line);
        border-radius: 8px;
        padding: 0.6rem;
        background: var(--rp-panel-solid);
        box-shadow: 0 0.7rem 1.5rem rgba(12, 18, 26, 0.08);
    }

    .filter-menu__toolbar {
        display: flex;
        justify-content: space-between;
        gap: 0.75rem;
        align-items: center;
        color: var(--rp-muted);
        font-size: 0.6rem;
        font-weight: 700;
        text-transform: uppercase;
    }

    .link-button {
        border: 0;
        padding: 0;
        background: transparent;
        color: var(--rp-accent);
        cursor: pointer;
        font: inherit;
        font-size: 0.58rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .link-button:disabled {
        cursor: not-allowed;
        opacity: 0.45;
    }

    .check-list {
        display: grid;
        gap: 0.25rem;
        max-height: 8rem;
        overflow: auto;
    }

    .check-row {
        display: flex;
        gap: 0.35rem;
        align-items: center;
        color: var(--rp-ink);
        font-size: 0.66rem;
    }

    .toggle-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
    }

    .check-pill {
        display: inline-flex;
        align-items: center;
        gap: 0.3rem;
        border: 1px solid var(--rp-line);
        border-radius: 999px;
        padding: 0.18rem 0.42rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font-size: 0.64rem;
    }

    .check-pill input,
    .check-row input {
        margin: 0;
    }

    .date-filter {
        min-width: 13rem;
    }

    .segmented-control {
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
    }

    .segmented-control button {
        border: 1px solid var(--rp-line);
        border-radius: 999px;
        padding: 0.22rem 0.42rem;
        background: transparent;
        color: var(--rp-muted);
        cursor: pointer;
        font: inherit;
        font-size: 0.66rem;
        font-weight: 800;
    }

    .segmented-control button.is-active {
        border-color: color-mix(in srgb, var(--rp-accent) 48%, var(--rp-line));
        background: var(--rp-soft);
        color: var(--rp-accent);
    }

    .date-fields {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 0.45rem;
    }

    .date-fields--single {
        grid-template-columns: 1fr;
    }

    .date-fields label {
        display: grid;
        gap: 0.22rem;
        color: var(--rp-muted);
        font-size: 0.62rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .date-fields input[type="date"] {
        border: 1px solid var(--rp-line);
        border-radius: 7px;
        padding: 0.24rem 0.38rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font: inherit;
        font-size: 0.62rem;
    }

    .career-table td:first-child {
        min-width: 0;
    }

    .career-table td:first-child a {
        display: block;
        min-width: 0;
        overflow-wrap: anywhere;
        color: var(--rp-ink);
        font-weight: 750;
        line-height: 1.25;
    }

    .career-table td:first-child span {
        display: block;
        margin-top: 0.08rem;
        color: var(--rp-muted);
        font-size: 0.73rem;
    }

    .career-table .date-col {
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
        white-space: nowrap;
    }

    .career-table tr.is-ineligible {
        color: color-mix(in srgb, var(--rp-muted) 82%, var(--rp-ink));
    }

    .career-table tr.is-ineligible td:first-child a {
        color: color-mix(in srgb, var(--rp-muted) 72%, var(--rp-ink));
    }

    .record-count {
        text-decoration: underline dotted color-mix(in srgb, var(--rp-muted) 70%, transparent);
        text-underline-offset: 0.18rem;
        cursor: help;
    }

    .eligibility-pill {
        display: inline-flex;
        border: 1px solid color-mix(in srgb, var(--rp-accent) 45%, var(--rp-line));
        border-radius: 999px;
        padding: 0.18rem 0.48rem;
        color: var(--rp-accent);
        font-size: 0.72rem;
        font-weight: 800;
        white-space: nowrap;
    }

    .eligibility-pill.is-muted {
        border-color: color-mix(in srgb, var(--rp-muted) 45%, var(--rp-line));
        color: var(--rp-muted);
    }

    .table-empty {
        color: var(--rp-muted);
        text-align: center;
    }

    .table-pagination {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        gap: 0.75rem;
        align-items: center;
        padding: 0.85rem 0 0;
        color: var(--rp-muted);
        font-size: 0.82rem;
    }

    .pagination-controls,
    .table-pagination label {
        display: inline-flex;
        gap: 0.5rem;
        align-items: center;
    }

    .pagination-controls button {
        display: inline-grid;
        place-items: center;
        width: 1.8rem;
        height: 1.8rem;
        border: 1px solid var(--rp-line);
        border-radius: 999px;
        background: transparent;
        color: var(--rp-ink);
    }

    .pagination-controls button:disabled {
        cursor: not-allowed;
        opacity: 0.45;
    }

    .table-pagination select {
        border: 1px solid var(--rp-line);
        border-radius: 6px;
        padding: 0.28rem 0.45rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font: inherit;
    }

    @media (max-width: 1400px) {
        .overview-metrics {
            grid-template-columns: repeat(4, minmax(0, 1fr));
        }

        .career-table {
            min-width: 64rem;
            font-size: 0.78rem;
        }

        .career-table th,
        .career-table td {
            padding: 0.42rem 0.34rem;
        }
    }

    @media (max-width: 1120px) {
        .overview-metrics {
            grid-template-columns: repeat(2, minmax(0, 1fr));
        }

        .overview-header {
            display: block;
        }

        .overview-links {
            justify-content: flex-start;
            margin-top: 0.85rem;
        }

        .career-table {
            min-width: 60rem;
            font-size: 0.75rem;
        }

        .column-search,
        .column-filter > summary,
        .segmented-control button,
        .date-fields input[type="date"],
        .check-row,
        .check-pill {
            font-size: 0.64rem;
        }
    }

    @media (max-width: 760px) {
        .overview-metrics {
            grid-template-columns: 1fr;
        }

        .overview-copy {
            font-size: 0.88rem;
        }

        .table-pagination {
            justify-content: flex-start;
        }

        .career-table {
            min-width: 56rem;
        }
    }

    @media (prefers-color-scheme: dark) {
        .careers-dashboard {
            --rp-ink: #edf3fb;
            --rp-muted: #a6b1bd;
            --rp-line: rgba(255, 255, 255, 0.13);
            --rp-panel: rgba(255, 255, 255, 0.045);
            --rp-panel-solid: #111820;
            --rp-accent: #5ed0c9;
            --rp-soft: rgba(94, 208, 201, 0.12);
        }
    }
</style>
