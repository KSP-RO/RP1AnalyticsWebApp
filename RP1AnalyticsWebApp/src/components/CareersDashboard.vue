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

        <div v-if="isLoading && !snapshot" class="overview-loading">
            <LoadingSpinner />
        </div>

        <template v-else-if="snapshot">
            <section class="overview-metrics" aria-label="Career overview metrics">
                <article v-for="metric in metricTiles" :key="metric.key" class="metric-tile">
                    <span>{{ metric.label }}</span>
                    <strong>{{ metric.value }}</strong>
                    <small>{{ metric.detail }}</small>
                </article>
            </section>

            <div class="overview-grid">
                <section class="overview-section">
                    <div class="section-heading">
                        <h2>Recently Updated</h2>
                        <span>{{ snapshot.recentCareers.length }}</span>
                    </div>
                    <ol class="summary-list">
                        <li v-for="career in snapshot.recentCareers" :key="career.id" class="summary-row">
                            <div class="summary-row__main">
                                <a :href="careerUrl(career.id)">{{ career.name }}</a>
                                <span>{{ career.userPreferredName || career.userLogin }}</span>
                            </div>
                            <time>{{ formatDateTimeGmt(career.lastUpdate) }}</time>
                        </li>
                    </ol>
                </section>

                <section class="overview-section">
                    <div class="section-heading">
                        <h2>Most Records</h2>
                        <span>{{ snapshot.recordLeaders.length }}</span>
                    </div>
                    <ol v-if="snapshot.recordLeaders.length" class="summary-list">
                        <li v-for="leader in snapshot.recordLeaders" :key="leader.careerId" class="summary-row summary-row--records">
                            <div class="summary-row__main">
                                <a :href="careerUrl(leader.careerId)">{{ leader.careerName }}</a>
                                <span>{{ leader.userPreferredName || leader.userLogin }}</span>
                                <small>
                                    {{ leader.difficultyLevel || 'Unknown difficulty' }},
                                    {{ leader.careerPlaystyle || 'Unknown playstyle' }},
                                    career date {{ formatDate(leader.endDate) }}
                                </small>
                            </div>
                            <div class="summary-row__side">
                                <strong :title="recordsTitle(leader.recordsOwned)">{{ leader.recordCount }} records</strong>
                                <small>{{ leader.contractRecordCount }} contracts, {{ leader.programRecordCount }} programs</small>
                            </div>
                        </li>
                    </ol>
                    <p v-else class="empty-state">No first-place records in the current filter.</p>
                </section>

                <section class="overview-section overview-breakdowns">
                    <div class="section-heading">
                        <h2>Career Mix</h2>
                        <span>{{ snapshot.summary.totalCareers }}</span>
                    </div>
                    <div class="breakdown-group">
                        <h3>RP-1 Version</h3>
                        <div v-for="item in topBreakdown(snapshot.versionBreakdown)" :key="`version-${item.key}`" class="breakdown-row">
                            <span>{{ item.label }}</span>
                            <div class="breakdown-track">
                                <div :style="breakdownStyle(item.count, snapshot.versionBreakdown)"></div>
                            </div>
                            <strong>{{ item.count }}</strong>
                        </div>
                    </div>
                    <div class="breakdown-columns">
                        <div class="breakdown-group">
                            <h3>Difficulty</h3>
                            <div v-for="item in topBreakdown(snapshot.difficultyBreakdown)" :key="`difficulty-${item.key}`" class="breakdown-row">
                                <span>{{ item.label }}</span>
                                <div class="breakdown-track">
                                    <div :style="breakdownStyle(item.count, snapshot.difficultyBreakdown)"></div>
                                </div>
                                <strong>{{ item.count }}</strong>
                            </div>
                        </div>
                        <div class="breakdown-group">
                            <h3>Playstyle</h3>
                            <div v-for="item in topBreakdown(snapshot.playstyleBreakdown)" :key="`playstyle-${item.key}`" class="breakdown-row">
                                <span>{{ item.label }}</span>
                                <div class="breakdown-track">
                                    <div :style="breakdownStyle(item.count, snapshot.playstyleBreakdown)"></div>
                                </div>
                                <strong>{{ item.count }}</strong>
                            </div>
                        </div>
                    </div>
                </section>
            </div>

            <section class="overview-section table-section">
                <div class="section-heading">
                    <h2>Career Table</h2>
                    <span>{{ tableFilteredCareers.length }} of {{ snapshot.careers.length }} careers</span>
                </div>
                <div class="career-table-wrap">
                    <table class="career-table">
                        <thead>
                            <tr>
                                <th v-for="column in tableColumns" :key="column.key">
                                    <button type="button" class="sort-button" @click="toggleSort(column.key)">
                                        {{ column.label }}
                                        <i class="fas" :class="sortIcon(column.key)" aria-hidden="true"></i>
                                    </button>
                                </th>
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
                                <td class="date-col">{{ formatDate(career.lastUpdate) }}</td>
                                <td>{{ formatNumber(career.launchCount) }}</td>
                                <td>{{ formatNumber(career.completedProgramCount) }}</td>
                                <td>{{ formatNumber(career.completedMilestoneCount) }}</td>
                                <td>
                                    <span class="record-count" :title="recordsTitle(career.recordsOwned)">
                                        {{ formatNumber(career.recordCount) }}
                                    </span>
                                </td>
                                <td>
                                    <span class="eligibility-pill" :class="{ 'is-muted': !career.eligibleForRecords }">
                                        {{ career.eligibleForRecords ? 'Eligible' : 'Not eligible' }}
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
        </template>
    </section>
</template>

<script setup lang="ts">
    import { computed, ref, watch } from 'vue';
    import type { CareerOverviewBreakdownItem, CareerOverviewItem, CareerOverviewSnapshot, Filters } from '../types';
    import { fetchCareerOverview } from '../utils/api';
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
        | 'recordCount'
        | 'eligibleForRecords';

    const props = defineProps<{
        filters: Filters;
    }>();

    const snapshot = ref<CareerOverviewSnapshot | null>(null);
    const isLoading = ref(false);
    const sortKey = ref<SortKey>('lastUpdate');
    const sortDirection = ref<SortDirection>('desc');
    const currentPage = ref(1);
    const pageSize = ref<PageSize>('50');
    let requestId = 0;

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
        { key: 'recordCount', label: 'Records' },
        { key: 'eligibleForRecords', label: 'Eligibility' }
    ];

    const metricTiles = computed(() => {
        const summary = snapshot.value?.summary;
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
                value: formatDateTimeGmt(summary.latestUpdate),
                detail: 'GMT'
            }
        ];
    });

    const summaryLine = computed(() => {
        const summary = snapshot.value?.summary;
        if (!summary) return 'Loading career metrics.';
        return `${formatNumber(summary.totalCareers)} careers, ${formatNumber(summary.totalLaunches)} launches, ${formatNumber(summary.totalRecordsSet)} first-place records.`;
    });

    const tableFilteredCareers = computed(() => snapshot.value?.careers ?? []);

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

    watch(() => props.filters, () => {
        queryData();
    }, { deep: true, immediate: true });

    watch(pageSize, () => {
        currentPage.value = 1;
    });

    watch(pageCount, () => {
        if (currentPage.value > pageCount.value) {
            currentPage.value = pageCount.value;
        }
    });

    async function queryData() {
        const currentRequest = ++requestId;
        isLoading.value = true;
        try {
            const result = await fetchCareerOverview(props.filters);
            if (currentRequest === requestId) {
                snapshot.value = result;
                currentPage.value = 1;
            }
        } finally {
            if (currentRequest === requestId) {
                isLoading.value = false;
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

        if (key === 'eligibleForRecords') {
            return Number(a.eligibleForRecords) - Number(b.eligibleForRecords);
        }

        return stringValue(a[key]).localeCompare(stringValue(b[key]));
    }

    function careerUrl(careerId: string) {
        return `/?careerId=${careerId}`;
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
        return value.replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').trim();
    }

    function formatNumber(value: number) {
        return new Intl.NumberFormat('en', { maximumFractionDigits: 0 }).format(value);
    }

    function formatAverage(total: number, count: number) {
        if (count === 0) return '0';
        return (total / count).toFixed(1);
    }

    function formatDate(value?: string | null) {
        if (!value) return '-';
        return parseUtcDate(value).toFormat('yyyy-MM-dd');
    }

    function formatDateTimeGmt(value?: string | null) {
        if (!value) return '-';
        return parseUtcDate(value).toUTC().toFormat("yyyy-MM-dd HH:mm 'GMT'");
    }

    function dateValue(value?: string | null) {
        return value ? parseUtcDate(value).toMillis() : 0;
    }

    function stringValue(value?: string | null) {
        return value || '';
    }
</script>

<style scoped>
    .careers-dashboard {
        --rp-ink: #17212e;
        --rp-muted: #65717f;
        --rp-line: rgba(36, 48, 63, 0.14);
        --rp-panel: rgba(255, 255, 255, 0.78);
        --rp-accent: #1d8f8a;
        margin: 1rem 0 2.5rem;
        color: var(--rp-ink);
    }

    .overview-header {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: end;
        padding: 1.25rem 0;
        border-bottom: 1px solid var(--rp-line);
    }

    .eyebrow {
        margin: 0 0 0.25rem;
        color: var(--rp-accent);
        font-size: 0.76rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .overview-header h1 {
        margin: 0;
        font-size: clamp(1.9rem, 4vw, 3.25rem);
        line-height: 1;
    }

    .overview-copy {
        margin: 0.55rem 0 0;
        color: var(--rp-muted);
    }

    .overview-links {
        display: flex;
        justify-content: flex-end;
    }

    .overview-link {
        display: inline-flex;
        align-items: center;
        gap: 0.45rem;
        border: 1px solid color-mix(in srgb, var(--rp-accent) 45%, var(--rp-line));
        border-radius: 999px;
        padding: 0.45rem 0.8rem;
        color: var(--rp-ink);
        font-size: 0.85rem;
        font-weight: 750;
    }

    .overview-link:hover {
        background: color-mix(in srgb, var(--rp-accent) 11%, transparent);
    }

    .overview-loading {
        display: flex;
        justify-content: center;
        padding: 3rem 0;
    }

    .overview-metrics {
        display: grid;
        grid-template-columns: repeat(7, minmax(0, 1fr));
        gap: 0.65rem;
        margin: 1rem 0;
    }

    .metric-tile {
        min-width: 0;
        border: 1px solid var(--rp-line);
        border-radius: 8px;
        padding: 0.8rem;
        background: var(--rp-panel);
    }

    .metric-tile span,
    .metric-tile small {
        display: block;
        color: var(--rp-muted);
        font-size: 0.74rem;
        line-height: 1.25;
    }

    .metric-tile span {
        font-weight: 800;
        text-transform: uppercase;
    }

    .metric-tile strong {
        display: block;
        margin: 0.35rem 0 0.2rem;
        overflow-wrap: anywhere;
        font-size: 1.25rem;
        line-height: 1.05;
    }

    .overview-grid {
        display: grid;
        grid-template-columns: minmax(0, 1fr) minmax(0, 1fr) minmax(18rem, 0.9fr);
        gap: 1rem;
        align-items: start;
    }

    .overview-section {
        border-top: 1px solid var(--rp-line);
        padding: 1rem 0;
    }

    .section-heading {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
        align-items: baseline;
        margin-bottom: 0.75rem;
    }

    .section-heading h2 {
        margin: 0;
        font-size: 1.1rem;
    }

    .section-heading span {
        color: var(--rp-muted);
        font-size: 0.78rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .summary-list {
        display: grid;
        gap: 0;
        list-style: none;
        margin: 0;
        padding: 0;
    }

    .summary-row {
        display: grid;
        grid-template-columns: minmax(0, 1fr) max-content;
        gap: 0.65rem;
        align-items: start;
        min-height: 4rem;
        border-bottom: 1px solid var(--rp-line);
        padding: 0.62rem 0;
    }

    .summary-row__main {
        display: grid;
        gap: 0.15rem;
        min-width: 0;
    }

    .summary-row a {
        min-width: 0;
        overflow-wrap: anywhere;
        color: var(--rp-ink);
        font-weight: 750;
        line-height: 1.2;
    }

    .summary-row span,
    .summary-row time,
    .summary-row small {
        color: var(--rp-muted);
        font-size: 0.78rem;
        line-height: 1.3;
    }

    .summary-row time {
        font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
        white-space: nowrap;
    }

    .summary-row__side {
        display: grid;
        gap: 0.15rem;
        justify-items: end;
        text-align: right;
    }

    .summary-row__side strong {
        color: var(--rp-ink);
        font-size: 0.95rem;
        line-height: 1.2;
        white-space: nowrap;
    }

    .summary-row__side small {
        white-space: nowrap;
    }

    .empty-state {
        margin: 0;
        color: var(--rp-muted);
        font-size: 0.85rem;
    }

    .overview-breakdowns {
        display: grid;
        gap: 0.75rem;
    }

    .breakdown-columns {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 0.75rem;
    }

    .breakdown-group {
        display: grid;
        gap: 0.4rem;
    }

    .breakdown-group h3 {
        margin: 0;
        color: var(--rp-muted);
        font-size: 0.74rem;
        font-weight: 800;
        text-transform: uppercase;
    }

    .breakdown-row {
        display: grid;
        grid-template-columns: minmax(4rem, 7rem) minmax(5rem, 1fr) 2rem;
        gap: 0.5rem;
        align-items: center;
        color: var(--rp-muted);
        font-size: 0.78rem;
    }

    .breakdown-row span {
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .breakdown-row strong {
        color: var(--rp-ink);
        text-align: right;
    }

    .breakdown-track {
        height: 0.42rem;
        overflow: hidden;
        border-radius: 999px;
        background: color-mix(in srgb, var(--rp-muted) 15%, transparent);
    }

    .breakdown-track div {
        height: 100%;
        border-radius: inherit;
        background: var(--rp-accent);
    }

    .table-section {
        margin-top: 0.25rem;
    }

    .career-table-wrap {
        overflow-x: auto;
    }

    .career-table {
        width: 100%;
        min-width: 92rem;
        border-collapse: collapse;
        font-size: 0.84rem;
    }

    .career-table th {
        border-bottom: 1px solid var(--rp-line);
        padding: 0.5rem 0.55rem;
        text-align: left;
        vertical-align: top;
    }

    .sort-button {
        display: inline-flex;
        align-items: center;
        gap: 0.3rem;
        border: 0;
        padding: 0;
        background: transparent;
        color: var(--rp-muted);
        cursor: pointer;
        font: inherit;
        font-size: 0.72rem;
        font-weight: 800;
        text-transform: uppercase;
        white-space: nowrap;
    }

    .column-filter {
        position: relative;
        margin-top: 0.45rem;
    }

    .column-filter select,
    .column-filter input[type="date"],
    .column-filter summary {
        width: 100%;
        max-width: 10rem;
        border: 1px solid var(--rp-line);
        border-radius: 6px;
        padding: 0.28rem 0.4rem;
        background: var(--rp-panel);
        color: var(--rp-ink);
        font: inherit;
        font-size: 0.74rem;
        line-height: 1.2;
    }

    .column-filter summary {
        cursor: pointer;
        list-style-position: outside;
    }

    .filter-menu {
        position: absolute;
        z-index: 5;
        display: grid;
        gap: 0.25rem;
        max-height: 18rem;
        min-width: 12rem;
        overflow: auto;
        border: 1px solid var(--rp-line);
        border-radius: 8px;
        padding: 0.5rem;
        background: var(--rp-panel-solid, #ffffff);
        box-shadow: 0 0.7rem 1.5rem rgba(12, 18, 26, 0.16);
    }

    .filter-menu label {
        display: flex;
        gap: 0.35rem;
        align-items: center;
        color: var(--rp-ink);
        font-size: 0.78rem;
        white-space: nowrap;
    }

    .date-filter {
        display: grid;
        gap: 0.3rem;
    }

    .career-table td {
        border-bottom: 1px solid var(--rp-line);
        padding: 0.62rem 0.55rem;
        vertical-align: top;
    }

    .career-table td:first-child {
        min-width: 14rem;
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
        margin-top: 0.1rem;
        color: var(--rp-muted);
        font-size: 0.76rem;
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

    @media (max-width: 1180px) {
        .overview-metrics {
            grid-template-columns: repeat(4, minmax(0, 1fr));
        }

        .overview-grid {
            grid-template-columns: 1fr 1fr;
        }

        .overview-breakdowns {
            grid-column: 1 / -1;
        }
    }

    @media (max-width: 760px) {
        .overview-header {
            display: block;
        }

        .overview-links {
            justify-content: flex-start;
            margin-top: 0.85rem;
        }

        .overview-metrics,
        .overview-grid,
        .breakdown-columns {
            grid-template-columns: 1fr;
        }

        .summary-row {
            grid-template-columns: 1fr;
        }

        .summary-row__side {
            justify-items: start;
            text-align: left;
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
        }
    }
</style>
